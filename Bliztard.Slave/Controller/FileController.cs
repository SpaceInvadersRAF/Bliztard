using Bliztard.Application.Configurations;
using Bliztard.Application.Extension;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Slave.Service.File;
using Bliztard.Slave.Service.Network;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Bliztard.Slave.Controller;

[ApiController]
public class FileController(MachineInfo machineInfo, IFileService fileService, INetworkService networkService, ILogger<MachineController> logger) : ControllerBase
{
    private readonly IFileService               m_FileService    = fileService;
    private readonly INetworkService            m_NetworkService = networkService;
    private readonly ILogger<MachineController> m_Logger         = logger;
    private readonly MachineInfo                m_MachineInfo    = machineInfo;

    [HttpPost(Configuration.Endpoint.Files.Upload)]
    public async Task<IActionResult> Upload()
    {
        var boundary = MediaTypeHeaderValue.Parse(Request.ContentType)
                                           .Boundary.Value;

        if (boundary == null)
            return BadRequest();

        var             reader      = new MultipartReader(boundary, HttpContext.Request.Body);
        var             formData    = new Dictionary<string, string>();
        var             contentType = "";
        await using var stream      = m_FileService.CreateStream(out var pathId);

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | PathId: {PathId} | Start Upload", DateTime.Now, m_MachineInfo.Id, pathId);

        while (await reader.ReadNextSectionAsync() is { } section)
        {
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

            if (!hasContentDispositionHeader || contentDisposition == null || !contentDisposition.DispositionType.Equals("form-data"))
                continue;

            if (!string.IsNullOrEmpty(contentDisposition.FileName.Value))
            {
                contentType = section.ContentType ?? "";

                await section.Body.CopyToAsync(stream);
            }
            else if (!string.IsNullOrEmpty(contentDisposition.Name.Value))
            {
                using var dataStream = new StreamReader(section.Body);

                formData[contentDisposition.Name.Value] = await dataStream.ReadToEndAsync();
            }
        }

        var saveFileInfo = new SaveFileInfo(m_MachineInfo, pathId, formData, contentType, stream.Length);

        m_Logger.LogInformation("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Resource: {Resource} | PathId: {PathId} | Save File", DateTime.Now, m_MachineInfo.Id,
                                saveFileInfo.Resource, pathId);

        if (!await m_FileService.Save(saveFileInfo))
            return BadRequest();

        _ = Task.Run(() => m_NetworkService.NotifyUpload(m_MachineInfo, saveFileInfo));

        return Created(saveFileInfo.Location, null);
    }

    [HttpPost(Configuration.Endpoint.Files.Twincate)]
    public async Task<IActionResult> Twincate([FromBody] TwincateFileRequest twincateFile)
    {
        await using var stream = m_FileService.Read($"{twincateFile.Resource}");

        if (stream == null)
            return NotFound();

        var content = new MultipartFormDataContent();

        content.AddTwincate(twincateFile, stream);

        var locationsResponse = await m_NetworkService.TwincateData(m_MachineInfo, twincateFile, content);
        locationsResponse.EnsureSuccessStatusCode();

        return Ok();
    }

    [HttpGet(Configuration.Endpoint.Files.Download)]
    public async Task<IActionResult> Download([FromForm(Name = "username")] string username, [FromForm(Name = "path")] string path)
    {
        await using var stream = m_FileService.Read($"{username}/{path}");

        if (stream == null)
            return NotFound();

        var reader = new StreamReader(stream);

        return Ok(await reader.ReadToEndAsync());
    }

    [HttpPost(Configuration.Endpoint.Files.NotifyDelete)]
    public async Task<IActionResult> NotifyDelete(NotifyDeleteRequest request)
    {
        var result = await m_FileService.Remove(request.Resource, request.PathId);

        if (!result)
            return BadRequest();

        return Ok();
    }
    
    [HttpPost(Configuration.Endpoint.Files.Stats)]
    public IActionResult Stats()
    {
        m_FileService.Stats();

        return Ok();
    }
}

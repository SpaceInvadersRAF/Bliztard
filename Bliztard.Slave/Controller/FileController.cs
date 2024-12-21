using System.Text.Json;
using Bliztard.Application.Model;
using Bliztard.Application.Service.File;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Bliztard.Slave.Controller;

[ApiController]
public class FileController(IHttpClientFactory httpClientFactory, MachineInfo machineInfo, IFileService fileService, ILogger<MachineController> logger) : ControllerBase
{
    private readonly IHttpClientFactory         m_HttpClientFactory = httpClientFactory;
    private readonly IFileService               m_FileService       = fileService;
    private readonly ILogger<MachineController> m_Logger            = logger;
    private readonly MachineInfo                m_MachineInfo       = machineInfo;
    private const    int                        c_Replications      = 3;
    
    [HttpPost("files/upload")]
    public async Task<IActionResult> Upload()
    {
        var boundary = MediaTypeHeaderValue.Parse(Request.ContentType).Boundary.Value;

        if (boundary == null)
            return BadRequest();
        
        var             reader   = new MultipartReader(boundary, HttpContext.Request.Body);
        var             formData = new Dictionary<string, string>();
        await using var stream   = m_FileService.CreateStream(out var pathId);
        var             fileName = "";

        while (await reader.ReadNextSectionAsync() is { } section)
        {
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
        
            if (!hasContentDispositionHeader || contentDisposition == null || !contentDisposition.DispositionType.Equals("form-data"))
                continue;

            if (!string.IsNullOrEmpty(contentDisposition.FileName.Value))
            {
                fileName = contentDisposition.FileName.Value;
                await section.Body.CopyToAsync(stream);
            }
            else if (!string.IsNullOrEmpty(contentDisposition.Name.Value))
            {
                using var dataStream = new StreamReader(section.Body);
                formData[contentDisposition.Name.Value] = await dataStream.ReadToEndAsync();
            }
        }

        var httpClient = m_HttpClientFactory.CreateClient();

        if (!int.TryParse(formData["replications"], out int replications) || replications > c_Replications)
            replications = c_Replications;

        if (--replications > 0)
        {
            var machineResponse = await httpClient.PostAsync("http://localhost:5259/machines/upload", null);
            machineResponse.EnsureSuccessStatusCode();
            var json    = await machineResponse.Content.ReadAsStringAsync();
            var machineInfo  = JsonSerializer.Deserialize<IEnumerable<MachineInfo>>(json);

            m_Logger.LogInformation("Slave with {slaveId} is replicating to {slaveId2}", m_MachineInfo.Id, machineInfo.First().Id);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(formData["username"]), "username");
            content.Add(new StringContent(formData["path"]), "path");
            content.Add(new StringContent(replications.ToString()), "replications");

            stream.Position = 0;
            using var streamCopy = new MemoryStream();
            await stream.CopyToAsync(streamCopy);
            streamCopy.Position = 0;

            var streamContent = new StreamContent(stream)
            {
                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream")}
            };

            content.Add(streamContent, "file", fileName);

            var saveResponse = await httpClient.PostAsync($"{machineInfo.First().Resource.BaseUrl}/files/upload", content);
            saveResponse.EnsureSuccessStatusCode();
        }

        if (!m_FileService.Save(formData, pathId))
            return BadRequest();

        m_Logger.LogInformation("Slave with {slaveId} saved file with path {path}", m_MachineInfo.Id,$"{formData["username"]}/{formData["path"]}");

        return Created($"{formData["username"]} {formData["path"]}", null);
    }
    
    [HttpGet("files/download")]
    public async Task<IActionResult> Download([FromForm(Name = "username")] string username, [FromForm(Name = "path")] string path)
    {
        await using var stream = m_FileService.Read($"{username}/{path}");
        
        if (stream == null)
            return NotFound();
        
        using var reader = new StreamReader(stream);
        
        return Ok(await reader.ReadToEndAsync());
    }
}

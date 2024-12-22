using Bliztard.Application.Mapper;
using Bliztard.Application.Model;
using Bliztard.Slave.Service.File;
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
    
    [HttpPost("files/upload")]
    public async Task<IActionResult> Upload()
    {
        var boundary = MediaTypeHeaderValue.Parse(Request.ContentType).Boundary.Value;

        if (boundary == null)
            return BadRequest();
        
        var             reader   = new MultipartReader(boundary, HttpContext.Request.Body);
        var             formData = new Dictionary<string, string>();
        await using var stream   = m_FileService.CreateStream(out var pathId);

        while (await reader.ReadNextSectionAsync() is { } section)
        {
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
        
            if (!hasContentDispositionHeader || contentDisposition == null || !contentDisposition.DispositionType.Equals("form-data"))
                continue;

            if (!string.IsNullOrEmpty(contentDisposition.FileName.Value))
            {
                await section.Body.CopyToAsync(stream);
            }
            else if (!string.IsNullOrEmpty(contentDisposition.Name.Value))
            {
                using var dataStream = new StreamReader(section.Body);

                formData[contentDisposition.Name.Value] = await dataStream.ReadToEndAsync();
            }
        }
        
        var saveUploadInfo = new SaveFileInfo(m_MachineInfo, pathId, formData);
        
        if (!m_FileService.Save(saveUploadInfo)) 
            return BadRequest();
        
        var httpClient = m_HttpClientFactory.CreateClient();

        await httpClient.PostAsJsonAsync("http://localhost:5259/files/notify-upload", saveUploadInfo.ToRequest());

        return Created(saveUploadInfo.Location, null); 
    }
    
    [HttpGet("files/download")]
    public async Task<IActionResult> Download([FromForm(Name = "username")] string username, [FromForm(Name = "path")] string path)
    {
        var stream = m_FileService.Read($"{username}/{path}");
        
        if (stream == null)
            return NotFound();
        
        var reader = new StreamReader(stream);
        
        return Ok(await reader.ReadToEndAsync());
    }
}

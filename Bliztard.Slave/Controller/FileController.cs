using Bliztard.Application.Model;
using Bliztard.Application.Service.File;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Bliztard.Slave.Controller;

[ApiController]
public class FileController(IHttpClientFactory httpClientFactory, MachineInfo machineInfo, IFileService fileService) : ControllerBase
{
    private readonly IFileService m_FileService = fileService;
    
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

        if (!m_FileService.Save(formData, pathId))
            return BadRequest();

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

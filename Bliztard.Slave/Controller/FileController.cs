using System.Net;
using Bliztard.Application.Configuration;
using Bliztard.Application.Extension;
using Bliztard.Application.Mapper;
using Bliztard.Application.Model;
using Bliztard.Contract.Response;
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
    
    [HttpPost(Configurations.Endpoint.Files.Upload)]
    public async Task<IActionResult> Upload()
    {
        var boundary = MediaTypeHeaderValue.Parse(Request.ContentType).Boundary.Value;

        if (boundary == null)
            return BadRequest();
        
        var             reader      = new MultipartReader(boundary, HttpContext.Request.Body);
        var             formData    = new Dictionary<string, string>();
        var             contentType = "";
        await using var stream      = m_FileService.CreateStream(out var pathId);
        
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
        
        if (!m_FileService.Save(saveFileInfo)) 
            return BadRequest();
        
        _ = Task.Run(() => NotifyUpload(saveFileInfo));
        _ = Task.Run(() => Twincate(saveFileInfo));
        
        return Created(saveFileInfo.Location, null);
    }

    private async Task<HttpResponseMessage> NotifyUpload(SaveFileInfo saveFileInfo)
    {
        var httpClient = m_HttpClientFactory.CreateClient(Configurations.HttpClient.FileNotifyUpload);
        
        m_Logger.LogDebug("Notify Master - Dusan about upload of '{resource}' on machine {machineId}.", saveFileInfo.Resource, saveFileInfo.MachineInfo.Id);
        
        return await httpClient.PostAsJsonAsync(Configurations.Endpoint.Files.NotifyUpload, saveFileInfo.ToRequest());
    }

    private async Task<HttpResponseMessage> Twincate(SaveFileInfo saveFileInfo)
    {
        if (saveFileInfo.Replication < 2)
            return new HttpResponseMessage(HttpStatusCode.OK);
        
        var httpClient        = m_HttpClientFactory.CreateClient(Configurations.HttpClient.FileTwincateData);
        var locationsResponse = await httpClient.PostAsJsonAsync(Configurations.Endpoint.Machine.UploadLocations, saveFileInfo.ToUploadLocationsRequest());
        
        locationsResponse.EnsureSuccessStatusCode();
        
        var machineInfo = (await locationsResponse.Content.ReadFromJsonAsync<UploadLocationsResponse>()).MachineInfos.First(); 
        
        var content = new MultipartFormDataContent();

        var stream = m_FileService.Read(saveFileInfo.Resource);
        
        if (stream == null)
            return new HttpResponseMessage(HttpStatusCode.BadRequest);

        content.AddTwincate(saveFileInfo, stream);
        
        m_Logger.LogDebug("Twincate resource '{resource}' from machine {machineId} to machine {machineId}.", saveFileInfo.Resource, saveFileInfo.MachineInfo.Id, machineInfo.Id);
        
        return await httpClient.PostAsync($"{machineInfo.BaseUrl}/{Configurations.Endpoint.Files.Upload}", content);
    }
    
    [HttpGet(Configurations.Endpoint.Files.Download)]
    public async Task<IActionResult> Download([FromForm(Name = "username")] string username, [FromForm(Name = "path")] string path)
    {
        var stream = m_FileService.Read($"{username}/{path}");
        
        if (stream == null)
            return NotFound();
        
        var reader = new StreamReader(stream);
        
        return Ok(await reader.ReadToEndAsync());
    }
}

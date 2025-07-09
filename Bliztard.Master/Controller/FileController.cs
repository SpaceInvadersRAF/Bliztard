using Bliztard.Application.Configurations;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Master.Service.File;

using Microsoft.AspNetCore.Mvc;

namespace Bliztard.Master.Controller;

[ApiController]
public class FileController(MachineInfo machineInfo, IFileService fileService, ILogger<MachineController> logger) : ControllerBase
{
    private readonly MachineInfo                m_MachineInfo = machineInfo;
    private readonly IFileService               m_FileService = fileService;
    private readonly ILogger<MachineController> m_Logger      = logger;

    [HttpPost(Configuration.Endpoint.Files.NotifyUpload)]
    public IActionResult NotifyUpload([FromBody] NotifySaveRequest request)
    {
        if (!m_FileService.RegisterFile(request))
        {
            m_Logger.LogError("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | MachineId: {MachineId} | Resource: {Resource} | File Registration Failed", DateTime.Now, request.MachineInfo.Id,
                              request.Resource);

            return BadRequest();
        }

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | MachineId: {MachineId} | Resource: {Resource} | File Registration Succeeded", DateTime.Now, request.MachineInfo.Id,
                          request.Resource);

        return Ok();
    }

    [HttpPost(Configuration.Endpoint.Files.Locate)]
    public IActionResult Locate([FromForm(Name = "username")] string username, [FromForm(Name = "path")] string path)
    {
        var machineInfo = m_FileService.LocateFile($"{username}/{path}");

        if (machineInfo == null)
            return BadRequest();

        return Ok(machineInfo);
    }

    [HttpPost(Configuration.Endpoint.Files.NotifyLogContent)]
    public IActionResult NotifyLogContent(NotifyLogContentRequest request)
    {
        if (!m_FileService.RegisterLog(request))
        {
            m_Logger.LogError("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | MachineId: {MachineId} | Resources: {Resources} | Notify Log Content Failed", DateTime.Now, request.MachineInfo.Id,
                              string.Join(", ", request.SaveFileRequest.Select(fileInfo => fileInfo.Resource)));

            return BadRequest();
        }

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | MachineId: {MachineId} | Resource: {Resource} | Notify Log Content Succeeded", DateTime.Now, request.MachineInfo.Id,
                          string.Join(", ", request.SaveFileRequest.Select(fileInfo => fileInfo.Resource)));

        return Ok();
    }

    [HttpPost(Configuration.Endpoint.Files.Delete)]
    public IActionResult Delete([FromForm(Name = "username")] string username, [FromForm(Name = "path")] string path)
    {
        var result = m_FileService.DegenerateFile($"{username}/{path}");

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

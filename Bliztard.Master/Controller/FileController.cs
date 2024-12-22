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

    [HttpPost("files/notify-upload")]
    public IActionResult NotifyUpload([FromBody] NotifySaveRequest request)
    {
        if (!m_FileService.RegisterFile(request))  
            return BadRequest();
        
        return Ok();
    }
    
    [HttpPost("files/locate")]
    public IActionResult Locate([FromForm(Name = "username")] string username, [FromForm(Name = "path")] string path)
    {
        var machineInfo = m_FileService.LocateFile($"{username}/{path}");
        
        if (machineInfo == null)  
            return BadRequest();
        
        return Ok(machineInfo);
    }
}

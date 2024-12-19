using System.Security.Cryptography;
using Bliztard.Application.Model;
using Bliztard.Application.Service.Machine;
using Microsoft.AspNetCore.Mvc;

namespace Bliztard.Master.Controllers;

[ApiController]
public class MachineController(IMachineService machineService, IHttpClientFactory httpClient, ILogger<MachineController> logger, MachineInfo machineInfo) : ControllerBase
{
    private readonly IMachineService            m_MachineService = machineService;
    private readonly IHttpClientFactory         m_HttpClient     = httpClient;
    private readonly ILogger<MachineController> m_Logger         = logger;
    private readonly MachineInfo                m_MachineInfo    = machineInfo;

    private const int c_MaxFileSize = 1024 * 1024 * 1024;

    [HttpPost("machines/upload")]
    public IActionResult UploadLocations() // Round Robin Hood (ide maca oko tebe / vruci krompirici)
    {
        // vraca slejvove na koje ovaj moze da upload-uje fajl
        return Ok(m_MachineService.AllSlavesWillingToAcceptFile());
    }

    /// <summary>
    /// When slave is spawned, to tell Master that it is ready to use
    /// </summary>
    /// <param name="machineInfo">All info about that slave(id, isAlive, resources, ...)</param>
    /// <returns>Http status code</returns>
    [HttpPost("machines/register")]
    public IActionResult Register([FromBody] MachineInfo machineInfo)
    {
        if (!m_MachineService.Register(machineInfo))
        {
            m_Logger.LogInformation("Failed to register machine. Machine with id: '{machineId}' already exists.", machineInfo.Id);
            return BadRequest();
        }
        
        m_Logger.LogInformation("Successful machine registration. Machine id: '{machineId}'.", machineInfo.Id);
        return Ok(m_MachineInfo);
    }
    
    /// <summary>
    /// Heartbeat from slave to say he is alive
    /// </summary>
    /// <param name="slaveId">Unique identifier of a slave that master is calling</param>
    /// <returns>Http status code</returns>
    [HttpGet("machines/heartbeat/{slaveId}")]
    public IActionResult AcceptHeartbeat([FromRoute] Guid slaveId)
    {
        if (!m_MachineService.Uroshbeat(slaveId))
        {
            m_Logger.LogDebug("Cannot process heartbeat. Machine with id: '{machineId}' is not registered.", slaveId);
            return BadRequest();
        }
        
        m_Logger.LogDebug("Heartbeat accepted. Machine id: '{machineId}'.", slaveId);
        return Ok();
    }

    /// <summary>
    /// Get all slaves of a master
    /// </summary>
    /// <returns></returns>
    [HttpGet("machines")]
    public IActionResult List()
    {
        return Ok(m_MachineService.GetAll());
    }

    [HttpPost("upload")]
    public IActionResult Upload(IFormFile file)
    {
        List<string> validExtensions = new List<string> { ".jpg", ".png", ".gif", ".jpeg", ".docx", ".txt" };
        string extension = Path.GetExtension(file.FileName);
        if (!validExtensions.Contains(extension))
            return BadRequest($"File extension '{extension}' is not supported.");

        if (file.Length > c_MaxFileSize)
            return BadRequest("File size is too big.");

        using (var stream = file.OpenReadStream())
        {
            m_MachineService.UploadFile(stream, file.FileName, extension);
        }

        return Ok(m_MachineService.GetAll());
    }
}

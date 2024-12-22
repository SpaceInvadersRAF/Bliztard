using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Master.Service.Machine;
using Microsoft.AspNetCore.Mvc;

namespace Bliztard.Master.Controllers;

[ApiController]
public class MachineController(IMachineService machineService, IHttpClientFactory httpClient, ILogger<MachineController> logger, MachineInfo machineInfo) : ControllerBase
{
    private readonly IMachineService            m_MachineService = machineService;
    private readonly IHttpClientFactory         m_HttpClient     = httpClient;
    private readonly ILogger<MachineController> m_Logger         = logger;
    private readonly MachineInfo                m_MachineInfo    = machineInfo;
    
    [HttpPost("machines/upload")]
    public IActionResult UploadLocations() // Round Robin Hood (ide maca oko tebe / vruci krompirici)
    {
        // vraca slejvove na koje ovaj moze da upload-uje fajl 
        return Ok(m_MachineService.AllSlavesWillingToAcceptFile());
    }
    
    /// <summary>
    /// kad se slave spawnuje da kaze Bliztard.Master-u da je spreman da se koristi
    /// </summary>
    /// <param name="machineInfo"></param>
    /// <returns></returns>
    [HttpPost("machines/register")]
    public IActionResult Register([FromBody] MachineInfoRequest machineInfo)
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
    /// heartbeat na 4 sekundi salje slave, a na 8 sekundi proverava master
    /// </summary>
    /// <param name="slaveId"></param>
    /// <returns></returns>
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
    
    [HttpGet("machines")]
    public IActionResult List()
    {
        return Ok();
    }
}

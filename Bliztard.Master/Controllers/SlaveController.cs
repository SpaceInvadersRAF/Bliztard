using Bliztard.Application.Model;
using Bliztard.Application.Service.Machine;
using Microsoft.AspNetCore.Mvc;

namespace Bliztard.Master.Controllers;

[ApiController]
public class SlaveController(IMachineService machineService, IHttpClientFactory httpClient, ILogger<SlaveController> logger, MachineInfo machineInfo) : ControllerBase
{
    private readonly IMachineService          m_MachineService = machineService;
    private readonly IHttpClientFactory       m_HttpClient     = httpClient;
    private readonly ILogger<SlaveController> m_Logger         = logger;
    private readonly MachineInfo              m_MachineInfo    = machineInfo;
    
    /// <summary>
    /// kad se slave spawnuje da kaze Bliztard.Master-u da je spreman da se koristi
    /// </summary>
    /// <param name="machineInfo"></param>
    /// <returns></returns>
    [HttpPost("slaves/register")]
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
    /// heartbeat na 4 sekundi salje slave, a na 8 sekundi proverava master
    /// </summary>
    /// <param name="slaveId"></param>
    /// <returns></returns>
    [HttpGet("slaves/heartbeat/{slaveId}")]
    public IActionResult Heartbeat([FromRoute] Guid slaveId)
    {
        if (!m_MachineService.Uroshbeat(slaveId))
        {
            m_Logger.LogDebug("Cannot process heartbeat. Machine with id: '{machineId}' is not registered.", slaveId);
            return BadRequest();
        }
        
        m_Logger.LogDebug("Heartbeat accepted. Machine id: '{machineId}'.", slaveId);
        return Ok();
    }
    
    [HttpGet("slaves")]
    public IActionResult List()
    {
        return Ok();
    }

}

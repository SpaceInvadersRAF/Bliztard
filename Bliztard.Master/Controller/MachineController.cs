using Bliztard.Application.Configuration;
using Bliztard.Application.Mapper;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Contract.Response;
using Bliztard.Master.Service.Machine;
using Microsoft.AspNetCore.Mvc;

namespace Bliztard.Master.Controller;

[ApiController]
public class MachineController(IMachineService machineService, IHttpClientFactory httpClient, ILogger<MachineController> logger, MachineInfo machineInfo) : ControllerBase
{
    private readonly IMachineService            m_MachineService = machineService;
    private readonly IHttpClientFactory         m_HttpClient     = httpClient;
    private readonly ILogger<MachineController> m_Logger         = logger;
    private readonly MachineInfo                m_MachineInfo    = machineInfo;
    
    [HttpPost(Configurations.Endpoint.Machine.UploadLocations)]
    public IActionResult UploadLocations([FromBody] UploadLocationsRequest request)
    {
        var machineInfos = m_MachineService.AllSlavesWillingToAcceptFile(request).ToList();
        
        m_Logger.LogDebug("Upload locations for resource '{resource}' are {machineIds}", request.Resource, string.Join(", ", machineInfos.Select(machineInfo => machineInfo.Id)));
        
        return Ok(new UploadLocationsResponse() { MachineInfos = machineInfos.ToResponse() });
    }
    
    /// <summary>
    /// kad se slave spawnuje da kaze Bliztard.Master-u da je spreman da se koristi
    /// </summary>
    /// <param name="machineInfo"></param>
    /// <returns></returns>
    [HttpPost(Configurations.Endpoint.Machine.Register)]
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
    /// <param name="machineId"></param>
    /// <returns></returns>
    [HttpGet(Configurations.Endpoint.Machine.AcceptHeartbeat)]   
    public IActionResult AcceptHeartbeat([FromRoute] Guid machineId)
    {
        if (!m_MachineService.Uroshbeat(machineId))
        {
            m_Logger.LogDebug("Cannot process heartbeat. Machine with id: '{machineId}' is not registered.", machineId);
            return BadRequest();
        }
        
        m_Logger.LogDebug("Heartbeat accepted. Machine id: '{machineId}'.", machineId);
        return Ok();
    }
    
    [HttpGet("machines")]
    public IActionResult List()
    {
        return Ok();
    }
}

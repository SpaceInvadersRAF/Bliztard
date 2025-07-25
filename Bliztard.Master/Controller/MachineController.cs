﻿using Bliztard.Application.Configurations;
using Bliztard.Application.Mapper;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Contract.Response;
using Bliztard.Master.Service.Machine;
using Microsoft.AspNetCore.Mvc;

namespace Bliztard.Master.Controller;

[ApiController]
public class MachineController(IMachineService machineService, ILogger<MachineController> logger, MachineInfo machineInfo) : ControllerBase
{
    private readonly IMachineService            m_MachineService = machineService;
    private readonly ILogger<MachineController> m_Logger         = logger;
    private readonly MachineInfo                m_MachineInfo    = machineInfo; // localhost:4030 - [::]:8080
    
    [HttpPost(Configuration.Endpoint.Machine.UploadLocations)]
    public IActionResult UploadLocations([FromBody] UploadLocationsRequest request)
    {
        var machineInfos = m_MachineService.GetUploadLocations(request).ToList();
        
        return Ok(new UploadLocationsResponse { MachineInfos = machineInfos.ToResponse() });
    }
    
    /// <summary>
    /// kad se slave spawnuje da kaze Bliztard.Master-u da je spreman da se koristi
    /// </summary>
    /// <param name="machineInfoRequest"></param>
    /// <returns></returns>
    [HttpPost(Configuration.Endpoint.Machine.Register)]
    public IActionResult Register([FromBody] MachineInfoRequest machineInfoRequest)
    {
        if (!m_MachineService.Register(machineInfoRequest))
        {
            m_Logger.LogWarning("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | MachineId: {MachineId} | Machine Registration Failed", DateTime.Now, machineInfoRequest.Id);
            return BadRequest();
        }
        
        m_Logger.LogInformation("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | MachineId: {MachineId} | MachineUrl: {MachineBaseUrl} | Machine Registration Succeeded", DateTime.Now, machineInfoRequest.Id, machineInfoRequest.BaseUrl);

        return Ok(m_MachineInfo);
    }
    
    /// <summary>
    /// heartbeat na 4 sekunde salje slave, a na 8 sekundi proverava master
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    [HttpGet(Configuration.Endpoint.Machine.AcceptHeartbeat)]   
    public IActionResult AcceptHeartbeat([FromRoute] Guid machineId)
    {
        if (!m_MachineService.Uroshbeat(machineId))
        {
            m_Logger.LogWarning("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | MachineId: {MachineId} | Heartbeat Rejected", DateTime.Now, machineId);
            
            return BadRequest();
        }

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | MachineId: {MachineId} | Heartbeat Accepted", DateTime.Now, machineId);

        return Ok();
    }
    
    [HttpGet("machines")]
    public IActionResult List()
    {
        return Ok();
    }
}

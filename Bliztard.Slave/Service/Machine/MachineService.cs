﻿using Bliztard.Application.Configurations;
using Bliztard.Application.Mapper;
using Bliztard.Application.Model;

namespace Bliztard.Slave.Service.Machine;

public class MachineService(MachineInfo machineInfo, ILogger<MachineService> logger, IHttpClientFactory httpClientFactory) : IMachineService
{
    private readonly ILogger<MachineService> m_Logger            = logger; 
    private readonly IHttpClientFactory      m_HttpClientFactory = httpClientFactory;
    private readonly MachineInfo             m_MachineInfo       = machineInfo;
    private readonly CancellationTokenSource m_CancellationToken = new();
    private          Timer?                  m_HeartbeatTimer;
    
    public void OnStart()
    {
        m_Logger.LogDebug("Start {service} service.", nameof(MachineService));
        
        Task.Run(NotifyMasterAsync);
    }

    public void OnStop()
    {
        m_Logger.LogDebug("Stop {service} service.", nameof(MachineService));

        m_CancellationToken.Cancel();
        
        m_HeartbeatTimer?.Change(Timeout.Infinite, 0);
        m_HeartbeatTimer?.Dispose();
    }

    public async Task<HttpResponseMessage> NotifyMasterAsync()
    {
        m_Logger.LogDebug("Machine with id '{machineId}' has notified the master.", m_MachineInfo.Id);

        var httpClient = m_HttpClientFactory.CreateClient(Configuration.HttpClient.MachineNotifyMaster);
        
        var response = await httpClient.PostAsJsonAsync(Configuration.Endpoint.Machine.Register, m_MachineInfo.ToRequest(), m_CancellationToken.Token);
        
        response.EnsureSuccessStatusCode();

        m_HeartbeatTimer = new Timer((_ => Task.Run(MetroOnTheHeartBeatAsync)), this, TimeSpan.Zero, Configuration.Interval.UroshbeatDelay);
        
        return response;
    }

    private async Task<HttpResponseMessage> MetroOnTheHeartBeatAsync()
    {
        m_Logger.LogDebug("Machine with id '{machineId}' has sent a heartbeat.", m_MachineInfo.Id);

        var httpClient = m_HttpClientFactory.CreateClient(Configuration.HttpClient.MachineSendUroshbeat);
        
        var response = await httpClient.GetAsync(Configuration.Endpoint.Machine.AcceptHeartbeat.Replace("{machineId}", m_MachineInfo.Id.ToString()), m_CancellationToken.Token);
        
        response.EnsureSuccessStatusCode();
        
        return response;
    }
}

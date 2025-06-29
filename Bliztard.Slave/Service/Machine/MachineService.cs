using System.ComponentModel;
using Bliztard.Application.Configurations;
using Bliztard.Application.Mapper;
using Bliztard.Application.Model;
using Bliztard.Slave.Service.Network;

namespace Bliztard.Slave.Service.Machine;

public class MachineService(MachineInfo machineInfo, ILogger<MachineService> logger, INetworkService networkService) : IMachineService
{
    private readonly INetworkService         m_NetworkService    = networkService;
    private readonly ILogger<MachineService> m_Logger            = logger; 
    private readonly MachineInfo             m_MachineInfo       = machineInfo;
    private readonly CancellationTokenSource m_CancellationToken = new();
    private          Timer?                  m_HeartbeatTimer;
    
    public void OnStart()
    {
        Task.Run(NotifyMasterAsync);
    }

    public void OnStop()
    {
        m_CancellationToken.Cancel();
        
        m_HeartbeatTimer?.Change(Timeout.Infinite, 0);
        m_HeartbeatTimer?.Dispose();
    }

    public async Task<HttpResponseMessage> NotifyMasterAsync()
    {
        
        var response = await m_NetworkService.NotifyMaster(m_MachineInfo, m_CancellationToken);
        response.EnsureSuccessStatusCode();
        
        m_HeartbeatTimer = new Timer((_ => Task.Run(MetroOnTheHeartBeatAsync)), this, TimeSpan.Zero, Configuration.Interval.UroshbeatDelay);
        
        return response;
    }

    private async Task<HttpResponseMessage> MetroOnTheHeartBeatAsync()
    {
        var response = await m_NetworkService.MetroOnTheHeartBeat(m_MachineInfo, m_CancellationToken);
        response.EnsureSuccessStatusCode();
        
        return response;
    }
}

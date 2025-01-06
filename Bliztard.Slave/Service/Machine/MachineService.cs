using Bliztard.Application.Configurations;
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
        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Notify Deployment", DateTime.Now, m_MachineInfo.Id);

        var httpClient = m_HttpClientFactory.CreateClient(Configuration.HttpClient.MachineNotifyMaster);
        
        var response = await httpClient.PostAsJsonAsync(Configuration.Endpoint.Machine.Register, m_MachineInfo.ToRequest(), m_CancellationToken.Token);
        
        response.EnsureSuccessStatusCode();

        m_HeartbeatTimer = new Timer((_ => Task.Run(MetroOnTheHeartBeatAsync)), this, TimeSpan.Zero, Configuration.Interval.UroshbeatDelay);
        
        return response;
    }

    private async Task<HttpResponseMessage> MetroOnTheHeartBeatAsync()
    {
        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId}' | Send Heartbeat", DateTime.Now, m_MachineInfo.Id);

        var httpClient = m_HttpClientFactory.CreateClient(Configuration.HttpClient.MachineSendUroshbeat);
        
        var response = await httpClient.GetAsync(Configuration.Endpoint.Machine.AcceptHeartbeat.Replace("{machineId}", m_MachineInfo.Id.ToString()), m_CancellationToken.Token);
        
        response.EnsureSuccessStatusCode();
        
        return response;
    }
}

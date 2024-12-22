using Bliztard.Application.Mapper;
using Bliztard.Application.Model;

namespace Bliztard.Slave.Service;

public class InitializationService(MachineInfo machineInfo, IHttpClientFactory httpClientFactory, ILogger<InitializationService> logger)
{
    private readonly IHttpClientFactory             m_HttpClientFactory = httpClientFactory;
    private readonly MachineInfo                    m_MachineInfo       = machineInfo;
    private readonly ILogger<InitializationService> m_Logger            = logger;

    public async Task StartAsync()
    {
        await Task.Run(NotifyMaster);
        await Task.Run(() => MetroOnTheHeartBeat(CancellationToken.None));
    }

    public async Task NotifyMaster()
    {
        var httpClient = m_HttpClientFactory.CreateClient();
        
        var response = await httpClient.PostAsJsonAsync("http://localhost:5259/machines/register", m_MachineInfo.ToRequest(), CancellationToken.None);
        
        response.EnsureSuccessStatusCode();
        
        m_Logger.LogDebug("Machine with id '{machineId}' has notified the master.", m_MachineInfo.Id);
    }
    
    private Task MetroOnTheHeartBeat(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var httpClient = m_HttpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(4);

            var task = httpClient.GetAsync($"http://localhost:5259/machines/heartbeat/{m_MachineInfo.Id}", CancellationToken.None);

            m_Logger.LogDebug("Machine with id '{machineId}' has sent a heartbeat.", m_MachineInfo.Id);

            Task.WaitAll([Task.Delay(4000, token), task], CancellationToken.None);
        }

        return Task.CompletedTask;
    }
    
    // public async Task NotifyMasterAboutFileUpload(string filePath, string username)
    // {
    //     var httpClient = m_HttpClientFactory.CreateClient();
    //     
    //     var content    = new StringContent(JsonSerializer.Serialize(m_MachineInfo), Encoding.UTF8, "application/json");
    //     
    //     var response = await httpClient.PostAsync("http://localhost:5259/files/notify-upload", content);
    //     
    //     response.EnsureSuccessStatusCode();
    //     
    //     m_Logger.LogDebug("Machine with id '{machineId}' has notified the master about file upload. File path: {filePath}", m_MachineInfo.Id, filePath);
    // }

    public async Task<HttpContent> AskMasterForNextSlave()
    {
        var httpClient = m_HttpClientFactory.CreateClient();

        var response = await httpClient.PostAsync("http://localhost:5259/machines/upload", null);

        response.EnsureSuccessStatusCode();
        m_Logger.LogDebug("Slave with {slaveId} is asking master for next slave", m_MachineInfo.Id);

        return response.Content;
    }
}

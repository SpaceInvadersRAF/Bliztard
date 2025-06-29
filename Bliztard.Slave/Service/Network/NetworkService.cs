using Bliztard.Application.Configurations;
using Bliztard.Application.Mapper;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;

namespace Bliztard.Slave.Service.Network;

public class NetworkService(IHttpClientFactory httpClientFactory, ILogger<NetworkService> logger) : INetworkService
{
    private readonly IHttpClientFactory      m_HttpClientFactory = httpClientFactory;
    private readonly ILogger<NetworkService> m_Logger            = logger;

    public async Task<HttpResponseMessage> NotifyUpload(MachineInfo currMachineInfo, SaveFileInfo saveFileInfo)
    {
        var httpClient = m_HttpClientFactory.CreateClient(Configuration.HttpClient.FileNotifyUpload);

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Resource: {Resource} | Notify Master", DateTime.Now, currMachineInfo.Id, saveFileInfo.Resource);

        return await httpClient.PostAsJsonAsync(Configuration.Endpoint.Files.NotifyUpload, saveFileInfo.ToRequest());
    }
    
    public async Task<HttpResponseMessage> TwincateData(MachineInfo currMachineInfo, TwincateFileRequest twincateFile, MultipartFormDataContent content)
    {
        var httpClient = m_HttpClientFactory.CreateClient(Configuration.HttpClient.FileTwincateData);
        
        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Resource: {Resource} | ReplicationId: {ReplicationId} | Twincate", DateTime.Now, currMachineInfo.Id, twincateFile.Resource, twincateFile.MachineInfo.Id);
        
        return await httpClient.PostAsync($"{twincateFile.MachineInfo.BaseUrl}/{Configuration.Endpoint.Files.Upload}", content);
    }
    
    public async Task<HttpResponseMessage> NotifyMaster(MachineInfo currMachineInfo, CancellationTokenSource cancellationToken)
    {
        var httpClient = m_HttpClientFactory.CreateClient(Configuration.HttpClient.MachineNotifyMaster);
        
        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Notify Deployment", DateTime.Now, currMachineInfo.Id);
        
        return await httpClient.PostAsJsonAsync(Configuration.Endpoint.Machine.Register, currMachineInfo.ToRequest(), cancellationToken.Token);
    }

    public async Task<HttpResponseMessage> MetroOnTheHeartBeat(MachineInfo  currMachineInfo, CancellationTokenSource cancellationToken)
    {
        var httpClient = m_HttpClientFactory.CreateClient(Configuration.HttpClient.MachineSendUroshbeat);
        
        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Send Heartbeat", DateTime.Now, currMachineInfo.Id);
        
        return await httpClient.GetAsync(Configuration.Endpoint.Machine.AcceptHeartbeat.Replace("{machineId}", currMachineInfo.Id.ToString()), cancellationToken.Token);
    }
}


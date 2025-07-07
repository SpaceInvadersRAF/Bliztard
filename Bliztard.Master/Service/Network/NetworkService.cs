using Bliztard.Application.Configurations;
using Bliztard.Application.Mapper;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;

using FileInfo = Bliztard.Application.Model.FileInfo;

namespace Bliztard.Master.Service.Network;

public class NetworkService(ILogger<NetworkService> logger, IHttpClientFactory httpClientFactory) : INetworkService
{
    private readonly IHttpClientFactory      m_HttpClientFactory = httpClientFactory;
    private readonly ILogger<NetworkService> m_Logger            = logger;

    public async Task<HttpResponseMessage> TwincateFileToMachine(FileInfo fileInfo, MachineInfo resourceMachine, MachineInfo uploadMachine)
    {
        var httpClient = m_HttpClientFactory.CreateClient(); //todo add config, talk

        return await httpClient.PostAsJsonAsync($"{resourceMachine.Resource.BaseUrl}/{Configuration.Endpoint.Files.Twincate}", fileInfo.ToTwincateFileRequest(uploadMachine));
    }

    public async Task<HttpResponseMessage> NotifyDelete(MachineInfo machineInfo, NotifyDeleteRequest request)
    {
        var httpClient = m_HttpClientFactory.CreateClient(); //todo add config, talk

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Resource: {Resource} | PathId: {PathId} | Notify Delete", DateTime.Now, machineInfo.Id, request.Resource,
                          request.PathId);

        return await httpClient.PostAsJsonAsync($"{machineInfo.Resource.BaseUrl}/{Configuration.Endpoint.Files.NotifyDelete}", request);
    }
}

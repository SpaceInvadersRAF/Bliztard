using Bliztard.Application.Configurations;
using Bliztard.Application.Mapper;
using Bliztard.Application.Model;

using FileInfo = Bliztard.Application.Model.FileInfo;

namespace Bliztard.Master.Service.Network;

public class NetworkService(IHttpClientFactory httpClientFactory) : INetworkService
{
    private readonly IHttpClientFactory m_HttpClientFactory = httpClientFactory;

    public async Task<HttpResponseMessage> TwincateFileToMachine(FileInfo fileInfo, MachineInfo resourceMachine, MachineInfo uploadMachine)
    {
        var httpClient = m_HttpClientFactory.CreateClient(); //todo add config, talk
        
        return await httpClient.PostAsJsonAsync($"{resourceMachine.Resource.BaseUrl}/{Configuration.Endpoint.Files.Twincate}", fileInfo.ToTwincateFileRequest(uploadMachine));
    }
}
    
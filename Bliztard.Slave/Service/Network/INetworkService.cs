using Bliztard.Application.Core;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;

namespace Bliztard.Slave.Service.Network;

public interface INetworkService : ILifecycle
{
    public Task<HttpResponseMessage> NotifyUpload(MachineInfo currMachineInfo, SaveFileInfo saveFileInfo);

    public Task<HttpResponseMessage> TwincateData(MachineInfo currMachineInfo, TwincateFileRequest twincateFile, MultipartFormDataContent content);

    public Task<HttpResponseMessage> NotifyMaster(MachineInfo currMachineInfo, CancellationTokenSource cancellationToken);

    public Task<HttpResponseMessage> MetroOnTheHeartBeat(MachineInfo currMachineInfo, CancellationTokenSource cancellationToken);

    public Task<HttpResponseMessage> NotifyLogContent(MachineInfo currMachineInfo, NotifyLogContentRequest request, CancellationToken cancellationToken);
}

using Bliztard.Application.Core;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;

using FileInfo = Bliztard.Application.Model.FileInfo;

namespace Bliztard.Master.Service.Network;

public interface INetworkService : ILifecycle
{
    public Task<HttpResponseMessage> TwincateFileToMachine(FileInfo fileInfo, MachineInfo resourceMachine, MachineInfo uploadMachine);

    public Task<HttpResponseMessage> NotifyDelete(MachineInfo machineInfo, NotifyDeleteRequest request);
    
    public Task<HttpResponseMessage> Stats(MachineInfo machineInfo);
}

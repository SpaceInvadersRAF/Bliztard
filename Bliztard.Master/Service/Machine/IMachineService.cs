using Bliztard.Application.Core;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;

namespace Bliztard.Master.Service.Machine;

public interface IMachineService : ILifecycle 
{
    public bool Register(MachineInfoRequest machineInfo);
    
    public bool Unregister(Guid machineId);

    public MachineInfo? Retrieve(Guid machineId);

    public IEnumerable<MachineInfo> GetUploadLocations(UploadLocationsRequest request);

    public bool Uroshbeat(Guid machineId);
}

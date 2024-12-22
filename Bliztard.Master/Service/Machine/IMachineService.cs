using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Master.Repository.Machine;

namespace Bliztard.Master.Service.Machine;

public interface IMachineService
{
    public IMachineRepository Repository  { get; }
    public MachineInfo        MachineInfo { get; }

    public bool Register(MachineInfoRequest machineInfo);
    
    public bool Unregister(Guid machineId);

    public MachineInfo? Retrieve(Guid machineId);

    public IEnumerable<MachineInfo> AllSlavesWillingToAcceptFile();

    public bool Uroshbeat(Guid machineId);
}

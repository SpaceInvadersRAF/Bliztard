using Bliztard.Application.Core;
using Bliztard.Application.Model;

namespace Bliztard.Master.Repository.Machine;

public interface IMachineRepository : ILifecycle
{
    public bool Add(MachineInfo machineInfo);

    public bool Remove(Guid machineId);

    public MachineInfo? Get(Guid machineId);

    public List<MachineInfo> GetAll(List<Guid> machineIds);

    public IEnumerable<MachineInfo> Machines();

    public IDictionary<Guid, MachineInfo> GetAll();
}

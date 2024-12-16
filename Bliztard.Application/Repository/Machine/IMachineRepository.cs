using System.Collections.Concurrent;
using Bliztard.Application.Model;

namespace Bliztard.Application.Repository.Machine;

public interface IMachineRepository
{
    public ConcurrentDictionary<Guid, MachineInfo> Machines { get; }

    public bool Add(MachineInfo machineInfo);
    
    public bool Remove(Guid machineId);

    public MachineInfo? Get(Guid machineId);

    public IEnumerable<MachineInfo> GetAll();
}

using System.Collections.Concurrent;
using Bliztard.Application.Model;

namespace Bliztard.Master.Repository.Machine;

public class MachineRepository : IMachineRepository
{
    public ConcurrentDictionary<Guid, MachineInfo> Machines { get; } = new();

    public bool Add(MachineInfo machineInfo)
    {
        return Machines.TryAdd(machineInfo.Id, machineInfo);
    }

    public bool Remove(Guid machineId)
    {
        return Machines.Remove(machineId, out _);
    }

    public MachineInfo? Get(Guid machineId)
    {
        Machines.TryGetValue(machineId, out var machineInfo);
        
        return machineInfo;
    }

    public IEnumerable<MachineInfo> GetAll()
    {
        return Machines.Values;
    }
}

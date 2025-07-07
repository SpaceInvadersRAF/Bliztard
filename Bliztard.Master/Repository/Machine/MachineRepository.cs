using System.Collections.Concurrent;

using Bliztard.Application.Model;

namespace Bliztard.Master.Repository.Machine;

public class MachineRepository : IMachineRepository
{
    private readonly ConcurrentDictionary<Guid, MachineInfo> m_Machines = new();

    public bool Add(MachineInfo machineInfo)
    {
        return m_Machines.TryAdd(machineInfo.Id, machineInfo);
    }

    public bool Remove(Guid machineId)
    {
        return m_Machines.Remove(machineId, out _);
    }

    public MachineInfo? Get(Guid machineId)
    {
        m_Machines.TryGetValue(machineId, out var machineInfo);

        return machineInfo;
    }

    public List<MachineInfo> GetAll(List<Guid> machineIds)
    {
        return machineIds.Select(machineId => m_Machines[machineId])
                         .ToList();
    }

    public IEnumerable<MachineInfo> Machines()
    {
        return m_Machines.Values;
    }

    public IDictionary<Guid, MachineInfo> GetAll()
    {
        return m_Machines;
    }
}

using System.Collections.Concurrent;
using Bliztard.Application.Model;
using Bliztard.Application.utils;

namespace Bliztard.Application.Repository.Machine;

public class MachineRepository : IMachineRepository
{
    public ConcurrentDictionary<Guid, MachineInfo> Machines { get; } = new();
    public ConcurrentDictionary<string, IEnumerable<string>> FileParts { get; } = new();
    public ConcurrentDictionary<string, IEnumerable<Guid>> PartSlaves { get; } = new();

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

    public bool MapFileNameAndParts(string fileName, IEnumerable<string> partsName)
    {
        return FileParts.TryAdd(fileName, partsName);
    }

    public bool MapPartNameAndMachines(string partName, IEnumerable<Guid> machineIds)
    {
        return PartSlaves.TryAdd(partName, machineIds);
    }
}

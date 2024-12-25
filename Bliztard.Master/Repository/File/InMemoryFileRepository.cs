using System.Collections.Concurrent;
using Bliztard.Application.Model;
using Bliztard.Master.Repository.Machine;

namespace Bliztard.Master.Repository.File;

public class InMemoryFileRepository(IMachineRepository machineRepository) : IFileRepository
{
    public          ConcurrentDictionary<string, List<Guid>> ResourceDictionary { get; } = new();
    public readonly IMachineRepository                       MachineRepository = machineRepository;

    public bool SaveUpload(Guid machineId, string resource)
    {
        ResourceDictionary.AddOrUpdate(resource, [machineId], (_, machineIds) => machineIds.Concat([machineId]).ToList());
        
        return true;
    }

    public MachineInfo? RetrieveMachine(string resource)
    {
        return ResourceDictionary.TryGetValue(resource, out var machineIds) ? machineIds.Select(id => MachineRepository.Get(id)).First() : null;
    }
    
    public bool IsResourceOnMachine(Guid machineId, string resource)
    {
        return ResourceDictionary.TryGetValue(resource, out var machineIds) && machineIds.Contains(machineId);
    }
}

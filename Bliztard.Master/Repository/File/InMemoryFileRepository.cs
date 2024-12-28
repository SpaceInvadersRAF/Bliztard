using System.Collections.Concurrent;
using Bliztard.Application.Model;
using Bliztard.Master.Repository.Machine;

namespace Bliztard.Master.Repository.File;

public class InMemoryFileRepository(IMachineRepository machineRepository) : IFileRepository
{
    private readonly ConcurrentDictionary<string, List<Guid>> m_ResourceDictionary = new();
    private readonly IMachineRepository                       m_MachineRepository  = machineRepository;

    public bool SaveUpload(Guid machineId, string resource)
    {
        m_ResourceDictionary.AddOrUpdate(resource, [machineId], (_, machineIds) => machineIds.Concat([machineId]).ToList());
        
        return true;
    }

    public MachineInfo? RetrieveMachine(string resource)
    {
        return m_ResourceDictionary.TryGetValue(resource, out var machineIds) ? machineIds.Select(id => m_MachineRepository.Get(id)).First() : null;
    }
    
    public bool IsResourceOnMachine(Guid machineId, string resource)
    {
        return m_ResourceDictionary.TryGetValue(resource, out var machineIds) && machineIds.Contains(machineId);
    }
}

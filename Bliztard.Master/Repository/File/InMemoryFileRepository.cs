using System.Collections.Concurrent;
using Bliztard.Application.Model;
using Bliztard.Master.Repository.Machine;

namespace Bliztard.Master.Repository.File;

public class InMemoryFileRepository(IMachineRepository machineRepository) : IFileRepository
{
    public          ConcurrentDictionary<string, Guid> ResourceDictionary { get; } = new();
    public readonly IMachineRepository                 MachineRepository = machineRepository;

    public bool SaveUpload(Guid machineId, string resource)
    {
        ResourceDictionary[resource] = machineId; 
        
        return true;
    }

    public MachineInfo? RetrieveMachine(string resource)
    {
        return MachineRepository.Get(ResourceDictionary[resource]);
    }
}

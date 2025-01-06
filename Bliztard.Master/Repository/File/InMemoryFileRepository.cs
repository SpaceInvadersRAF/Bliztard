using System.Collections.Concurrent;
using Bliztard.Application.Extension;
using Bliztard.Application.Model;
using Bliztard.Master.Repository.Machine;
using Bliztard.Master.Service.Machine;

namespace Bliztard.Master.Repository.File;

public class InMemoryFileRepository(IMachineRepository machineRepository, ILogger<MachineService> logger) : IFileRepository
{
    private readonly ILogger<MachineService> m_Logger = logger; 
    
    private readonly IMachineRepository m_MachineRepository = machineRepository;

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, bool>> m_ResourceMachineDictionary   = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, bool>> m_MachineResourceDictionary   = new();
    private readonly ConcurrentDictionary<Tuple<string, Guid>, bool>                m_TransientResourceDictionary = new();
    
    public bool SaveUpload(Guid machineId, string resource)
    {
        m_ResourceMachineDictionary.AddOrUpdate(resource, new ConcurrentDictionary<Guid, bool> { [machineId] = true }, (_, machineIds) => machineIds.TryAddAndReturn(machineId, true));
        m_MachineResourceDictionary.AddOrUpdate(machineId, new ConcurrentDictionary<string, bool> { [resource] = true}, (_, resources) => resources.TryAddAndReturn(resource, true));
        
        return m_TransientResourceDictionary.TryRemove(Tuple.Create(resource, machineId), out _);
    }

    public MachineInfo? RetrieveMachine(string resource)
    {
        return m_ResourceMachineDictionary.TryGetValue(resource, out var machineIds) ? machineIds.Select(machineId => m_MachineRepository.Get(machineId.Key)).First() : null;
    }
    
    public bool IsResourceOnMachine(Guid machineId, string resource)
    {
        var isTransient = m_TransientResourceDictionary.TryGetValue(Tuple.Create(resource, machineId), out _);
        var isSaved     = m_ResourceMachineDictionary.TryGetValue(resource, out var machineIds) && machineIds.ContainsKey(machineId);
        
        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Resource: {Resource} | IsTransient: {IsTransient} | IsSaved: {IsSaved} | Is Resource On Machine", DateTime.Now, machineId, resource, isTransient, isSaved);
        
        return isTransient || isSaved;
    }

    public bool SaveTransient(Guid machineId, string resource)
    {
        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Resource: {Resource} | Save Transient", DateTime.Now, machineId, resource);
        return m_TransientResourceDictionary.TryAdd(Tuple.Create(resource, machineId), true);
    }
}

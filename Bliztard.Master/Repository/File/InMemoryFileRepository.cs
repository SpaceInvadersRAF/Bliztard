using System.Collections.Concurrent;
using Bliztard.Application.Extension;
using Bliztard.Application.Model;
using Bliztard.Master.Repository.Machine;
using Bliztard.Master.Service.Machine;
using FileInfo = Bliztard.Application.Model.FileInfo;

namespace Bliztard.Master.Repository.File;

public class InMemoryFileRepository(IMachineRepository machineRepository, ILogger<MachineService> logger) : IFileRepository
{
    private readonly ILogger<MachineService> m_Logger = logger; 
    
    private readonly IMachineRepository m_MachineRepository = machineRepository;

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, FileInfo>> m_ResourceMachineDictionary   = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, FileInfo>> m_MachineResourceDictionary   = new();
    private readonly ConcurrentDictionary<Tuple<string, Guid>, bool>                    m_TransientResourceDictionary = new();
    
    public bool SaveUpload(Guid machineId, FileInfo fileInfo)
    {
        m_ResourceMachineDictionary.AddOrUpdate(fileInfo.Resource, new ConcurrentDictionary<Guid, FileInfo> { [machineId] = fileInfo }, (_, machineIds) => machineIds.TryAddAndReturn(machineId, fileInfo));
        m_MachineResourceDictionary.AddOrUpdate(machineId, new ConcurrentDictionary<string, FileInfo> { [fileInfo.Resource] = fileInfo}, (_, resources) => resources.TryAddAndReturn(fileInfo.Resource, fileInfo));
        
        return m_TransientResourceDictionary.TryRemove(Tuple.Create(fileInfo.Resource, machineId), out _);
    }

    public MachineInfo? RetrieveMachine(string resource)
    {
        return m_ResourceMachineDictionary.TryGetValue(resource, out var machineIds) ? machineIds.Select(machineId => m_MachineRepository.Get(machineId.Key)).First() : null;
    }

    public int ReplicationCount(string resource)
    {
        return m_ResourceMachineDictionary.TryGetValue(resource, out var machineIds) ? machineIds.Count : -1;
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

    public bool RemoveMachine(Guid machineId, out ICollection<FileInfo> outFileInfos)
    {
        outFileInfos = [];

        if (!m_MachineResourceDictionary.Remove(machineId, out var resources))
            return false;

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | ResourceCount: {ResourceCount} | Remove Machine", DateTime.Now, machineId, resources.Count);

        outFileInfos = new List<FileInfo>(resources.Count);

        foreach (var resource in resources.Keys)
            if (m_ResourceMachineDictionary[resource].TryRemove(machineId, out var fileInfo))
                outFileInfos.Add(fileInfo);
        
        return true;
    }
}

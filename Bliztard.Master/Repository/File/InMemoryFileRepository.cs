using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

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
        m_ResourceMachineDictionary.AddOrUpdate(fileInfo.Resource, new ConcurrentDictionary<Guid, FileInfo> { [machineId] = fileInfo },
                                                (_, machineIds) => machineIds.TryAddAndReturn(machineId, fileInfo));

        m_MachineResourceDictionary.AddOrUpdate(machineId, new ConcurrentDictionary<string, FileInfo> { [fileInfo.Resource] = fileInfo },
                                                (_, resources) => resources.TryAddAndReturn(fileInfo.Resource, fileInfo));

        return m_TransientResourceDictionary.TryRemove(Tuple.Create(fileInfo.Resource, machineId), out _);
    }
    
    public bool SaveLoggedUpload(Guid machineId, FileInfo fileInfo)
    {
        m_ResourceMachineDictionary.AddOrUpdate(fileInfo.Resource, new ConcurrentDictionary<Guid, FileInfo> { [machineId] = fileInfo },
                                                (_, machineIds) => machineIds.TryAddAndReturn(machineId, fileInfo));

        m_MachineResourceDictionary.AddOrUpdate(machineId, new ConcurrentDictionary<string, FileInfo> { [fileInfo.Resource] = fileInfo },
                                                (_, resources) => resources.TryAddAndReturn(fileInfo.Resource, fileInfo));

        return true;
    }

    public bool TryRemove(string resource, out List<(MachineInfo MachineInfo, FileInfo FileInfo)> machineFileInfoList)
    {
        machineFileInfoList = [];

        if (!m_ResourceMachineDictionary.TryRemove(resource, out var machineDictionary))
        {
            m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Resource: {Resource} | Remove File Failed", DateTime.Now, resource);

            return false;
        }

        foreach (var machineEntry in machineDictionary)
        {
            var machineInfo = m_MachineRepository.Get(machineEntry.Key);

            if (machineInfo is null) // warning
                continue;

            machineFileInfoList.Add((machineInfo, machineEntry.Value));

            m_MachineResourceDictionary[machineInfo.Id]
            .Remove(resource, out _);
        }

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Resource: {Resource} | PathIds: {PathId} | Machines: {Machines} | Remove File Succeeded", DateTime.Now, resource,
                          string.Join(", ", machineFileInfoList.Select(machineFileInfo => machineFileInfo.FileInfo.PathId)),
                          string.Join(", ", machineFileInfoList.Select(machineFileInfo => machineFileInfo.MachineInfo.Id)));

        return true;
    }

    public MachineInfo? RetrieveMachine(string resource)
    {
        return m_ResourceMachineDictionary.TryGetValue(resource, out var machineIds)
               ? machineIds.Select(machineId => m_MachineRepository.Get(machineId.Key))
                           .First()
               : null;
    }

    //TODO d.z.(domaci) Nemanja proveri
    public List<MachineInfo> RetrieveAllMachines(string resource)
    {
        return m_ResourceMachineDictionary.TryGetValue(resource, out var machineIds)
               ? machineIds.Select(machineId => m_MachineRepository.Get(machineId.Key)!)
                           .ToList()
               : [];
    }

    public int ReplicationCount(string resource)
    {
        return m_ResourceMachineDictionary.TryGetValue(resource, out var machineIds) ? machineIds.Count : -1;
    }

    public bool IsResourceOnMachine(Guid machineId, string resource)
    {
        var isTransient = m_TransientResourceDictionary.TryGetValue(Tuple.Create(resource, machineId), out _);
        var isSaved     = m_ResourceMachineDictionary.TryGetValue(resource, out var machineIds) && machineIds.ContainsKey(machineId);

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Resource: {Resource} | IsTransient: {IsTransient} | IsSaved: {IsSaved} | Is Resource On Machine",
                          DateTime.Now, machineId, resource, isTransient, isSaved);

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
            if (m_ResourceMachineDictionary[resource]
                .TryRemove(machineId, out var fileInfo))
                outFileInfos.Add(fileInfo);

        return true;
    }

    public void Stats()
    {
        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Start Statistics", DateTime.Now);

        foreach (var machineResourceEntry in m_MachineResourceDictionary)
            foreach (var resourceEntry in machineResourceEntry.Value)
                m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Resource: {Resource} | PathId: {PathId} | Statistics", DateTime.Now, machineResourceEntry.Key,
                                  resourceEntry.Key, resourceEntry.Value.PathId);

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | End Statistics", DateTime.Now);
    }
}

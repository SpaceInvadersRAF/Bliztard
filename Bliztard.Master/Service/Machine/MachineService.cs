using System.Collections.Concurrent;
using System.Data;
using Bliztard.Application.Configurations;
using Bliztard.Application.Extension;
using Bliztard.Application.Mapper;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Master.Repository.File;
using Bliztard.Master.Repository.Machine;
using Bliztard.Master.Service.MachineFile;
using Bliztard.Master.Service.Network;
using Bliztard.Master.Utilities;
using FileInfo = Bliztard.Application.Model.FileInfo;

namespace Bliztard.Master.Service.Machine;

public class MachineService(IMachineRepository repository, IFileRepository fileRepository, ILogger<MachineService> logger, INetworkService networkService, IMachineFileService machineFileService) : IMachineService 
{
    private readonly ILogger<MachineService> m_Logger             = logger; 
    private readonly IMachineRepository      m_Repository         = repository;
    private readonly IFileRepository         m_FileRepository     = fileRepository;
    private readonly INetworkService         m_NetworkService     = networkService;
    private readonly IMachineFileService     m_MachineFileService = machineFileService;
    
    private readonly ConcurrentQueue<TwincateCancellationToken> m_TwincateTokenQueue = new();
    
    private readonly object m_Lock            = new();
    private readonly object m_LockNew         = new();
    private          Timer? m_MassMurderTimer;
    
    public void OnStart()
    {
        m_MassMurderTimer = new Timer(_ => MassMurderOnTheBeatSoItsNotNice(), this, TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(8));
    }

    public void OnStop()
    {
        m_MassMurderTimer?.Change(Timeout.Infinite, 0);
        m_MassMurderTimer?.Dispose();
    }
    
    private void MassMurderOnTheBeatSoItsNotNice()
    {
        foreach (var (machineId, machineInfo) in m_Repository.GetAll())
            if (!machineInfo.Alive ^ (machineInfo.Alive = false) && Unregister(machineId))
                m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | MachineId: {Resource} | Machine Is Murdered", DateTime.Now, machineId);
    }

    public bool Register(MachineInfoRequest machineInfo)
    {
        if (!m_Repository.Add(machineInfo.ToModel()))
            return false;
        
        if (m_TwincateTokenQueue.TryDequeue(out var twincateToken))
            twincateToken.Cancel(machineInfo.ToModel());

        return true;
    } 

    public bool Unregister(Guid machineId)
    {
        if (!m_Repository.Remove(machineId))
            return false;
        
        if (!m_FileRepository.RemoveMachine(machineId, out var resources) || resources.Count == 0)
            return true;
        
        var twincateCancellationToken = new TwincateCancellationToken(out var token);

        var timer = new Timer(_ => Task.Run(() => TwincateExistingMachines(resources)), null, Configuration.Interval.TwincateNewMachineTimeout, Timeout.InfiniteTimeSpan);
        
        token.Register(() =>
                       {
                           timer.Cancel();
                           Task.Run(() => TwincateNewMachine(resources, twincateCancellationToken.MachineInfo));
                       });
        
        m_TwincateTokenQueue.Enqueue(twincateCancellationToken);
        
        return true;
    }
    
    private void TwincateExistingMachines(ICollection<FileInfo> fileInfos)
    {
        var index = 0;
        var tasks = new Task[fileInfos.Count];

        foreach (var fileInfo in fileInfos)
            tasks[index++] = m_NetworkService.TwincateFileToMachine(fileInfo, 
                                                                    m_FileRepository.RetrieveMachine(fileInfo.Resource)!,
                                                                    m_MachineFileService.GetUploadLocations(fileInfo.Resource, fileInfo.Size).First());

        Task.WaitAll(tasks);
    }

    private void TwincateNewMachine(ICollection<FileInfo> fileInfos, MachineInfo? uploadMachine)
    {
        if (uploadMachine is null)
            throw new NoNullAllowedException("smt");
        
        var index = 0;
        var tasks = new Task[fileInfos.Count];
        
        foreach (var fileInfo in fileInfos)
            tasks[index++] = m_NetworkService.TwincateFileToMachine(fileInfo, 
                                                                    m_FileRepository.RetrieveMachine(fileInfo.Resource)!,
                                                                    uploadMachine);
        
        Task.WaitAll(tasks);
    }
    

    public MachineInfo? Retrieve(Guid machineId)
    {
        return m_Repository.Get(machineId);
    }

    public IEnumerable<MachineInfo> GetUploadLocations(UploadLocationsRequest request)
    {
        return m_MachineFileService.GetUploadLocations(request.Resource, request.Size);
    }

    public bool Uroshbeat(Guid machineId)
    {
        return SetAlive(machineId);
    }

    private bool SetAlive(Guid machineId, bool value = true)
    {
        var machineInfo = m_Repository.Get(machineId);

        if (machineInfo == null)
            return false;

        machineInfo.Alive = value;

        return true;
    }
}

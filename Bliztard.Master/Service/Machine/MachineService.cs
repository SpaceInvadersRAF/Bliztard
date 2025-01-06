using Bliztard.Application.Mapper;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Master.Repository.File;
using Bliztard.Master.Repository.Machine;

namespace Bliztard.Master.Service.Machine;

public class MachineService(IMachineRepository repository, IFileRepository fileRepository, ILogger<MachineService> logger) : IMachineService 
{
    private readonly ILogger<MachineService> m_Logger         = logger; 
    private readonly IMachineRepository      m_Repository     = repository;
    private readonly IFileRepository         m_FileRepository = fileRepository;
    
    private readonly object m_Lock            = new();
    private readonly object m_LockNew         = new();
    private          int    m_CurrentIndex    = 0;
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
        return m_Repository.Add(machineInfo.ToModel());
    }

    public bool Unregister(Guid machineId) 
    {
        return m_Repository.Remove(machineId);
    }

    public MachineInfo? Retrieve(Guid machineId)
    {
        return m_Repository.Get(machineId);
    }
    
    public IEnumerable<MachineInfo> AllSlavesWillingToAcceptFile(UploadLocationsRequest request)
    {
        var machines   = m_Repository.Machines().ToList();
        int startIndex;

        lock (m_Lock)
            startIndex = m_CurrentIndex = (m_CurrentIndex + 1) % machines.Count;
        
        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | Resource: {Resource} | StartIndex {StartIndex} | Find Machine", DateTime.Now, request.Resource, startIndex);

        bool firstRound = true;
        
        for (int currentIndex = startIndex; firstRound || currentIndex < startIndex; ++currentIndex)
        {
            
            var currentMachine = machines[currentIndex];
            
            m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | Resource: {Resource} | CurrentIndex {CurrentIndex} | MachineId: {MachineId} | Current Machine", DateTime.Now,  request.Resource, currentIndex, currentMachine.Id);
            
            lock (m_LockNew)
            {
                if (!m_FileRepository.IsResourceOnMachine(currentMachine.Id, request.Resource))
                {
                    m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | Resource: {Resource} | CurrentIndex {CurrentIndex} | MachineId: {MachineId} | Machine is Found", DateTime.Now,  request.Resource, currentIndex, currentMachine.Id);
                    
                    if (!m_FileRepository.SaveTransient(currentMachine.Id, request.Resource))
                        continue;
                    
                    return [currentMachine];
                }
            }

            if (currentIndex == machines.Count - 1 && !(firstRound = false))
                currentIndex = -1;
        }

        m_Logger.LogDebug("Timestamp : {Timestamp:HH:mm:ss.ffffff} | Master | Resource: {Resource} | StartIndex {StartIndex} | Machine Not Found", DateTime.Now, request.Resource, startIndex);

        return [];
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

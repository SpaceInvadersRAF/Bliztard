using Bliztard.Application.Model;
using Bliztard.Master.Repository.File;
using Bliztard.Master.Repository.Machine;

namespace Bliztard.Master.Service.MachineFile;

public class MachineFileService(ILogger<MachineFileService> logger, IMachineRepository repository, IFileRepository fileRepository) : IMachineFileService
{
    private readonly ILogger<MachineFileService> m_Logger         = logger;
    private readonly IMachineRepository          m_Repository     = repository;
    private readonly IFileRepository             m_FileRepository = fileRepository;
    private readonly object                      m_LockIndex      = new();
    private readonly object                      m_LockResource   = new();
    private          int                         m_CurrentIndex   = -1;
    
    public IEnumerable<MachineInfo> GetUploadLocations(string resource, long size)  // old name: AllSlavesWillingToAcceptFile
    {
        var machines   = m_Repository.Machines().ToList();
        int startIndex;

        lock (m_LockIndex)
            startIndex = m_CurrentIndex = (m_CurrentIndex + 1) % machines.Count;
        
        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | Resource: {Resource} | StartIndex {StartIndex} | Find Machine", DateTime.Now, resource, startIndex);

        bool firstRound = true;
        
        for (int currentIndex = startIndex; firstRound || currentIndex < startIndex; ++currentIndex)
        {
            var currentMachine = machines[currentIndex];
            
            m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | Resource: {Resource} | CurrentIndex {CurrentIndex} | MachineId: {MachineId} | Current Machine", DateTime.Now,  resource, currentIndex, currentMachine.Id);
            
            lock (m_LockResource)
            {
                if (!m_FileRepository.IsResourceOnMachine(currentMachine.Id, resource))
                {
                    m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | Resource: {Resource} | CurrentIndex {CurrentIndex} | MachineId: {MachineId} | Machine is Found", DateTime.Now,  resource, currentIndex, currentMachine.Id);
                    
                    if (!m_FileRepository.SaveTransient(currentMachine.Id, resource))
                        continue;
                    
                    return [currentMachine];
                }
            }

            if (currentIndex == machines.Count - 1 && !(firstRound = false))
                currentIndex = -1;
        }

        m_Logger.LogDebug("Timestamp : {Timestamp:HH:mm:ss.ffffff} | Master | Resource: {Resource} | StartIndex {StartIndex} | Machine Not Found", DateTime.Now, resource, startIndex);

        return [];
    }
}

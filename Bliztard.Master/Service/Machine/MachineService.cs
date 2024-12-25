using Bliztard.Application.Mapper;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Master.Repository.File;
using Bliztard.Master.Repository.Machine;

namespace Bliztard.Master.Service.Machine;

public class MachineService(IMachineRepository repository, MachineInfo machineInfo, IFileRepository fileRepository) : IMachineService 
{
    public  IMachineRepository Repository     { get; } = repository;
    public  IFileRepository    FileRepository { get; } = fileRepository;
    public  MachineInfo        MachineInfo    { get; } = machineInfo; 
    private int                m_CurrentIndex          = 0; 
    
    public bool Register(MachineInfoRequest machineInfo) 
    {
        return Repository.Add(machineInfo.ToModel());
    }

    public bool Unregister(Guid machineId) 
    {
        return Repository.Remove(machineId);
    }

    public MachineInfo? Retrieve(Guid machineId)
    {
        return Repository.Get(machineId);
    }
    
    public IEnumerable<MachineInfo> AllSlavesWillingToAcceptFile(UploadLocationsRequest request)
    {
        var  machines   = Repository.Machines.Values.ToList();
        int  startIndex = Interlocked.Increment(ref m_CurrentIndex) % machines.Count;
        bool firstRound = true;
        
        for (int currentIndex = startIndex; firstRound || currentIndex < startIndex; ++currentIndex)
        {
            if (!FileRepository.IsResourceOnMachine(machines[currentIndex].Id, request.Resource))
                return [machines[currentIndex]];
            
            if (currentIndex == machines.Count - 1 && !(firstRound = false))
                currentIndex = -1;
        }

        return [];
    }

    public bool Uroshbeat(Guid machineId)
    {
        return SetAlive(machineId);
    }

    private bool SetAlive(Guid machineId, bool value = true)
    {
        var machineInfo = Repository.Get(machineId);

        if (machineInfo == null)
            return false;

        machineInfo.Alive = value;

        return true;
    }
}

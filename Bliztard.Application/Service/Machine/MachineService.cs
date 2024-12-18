using Bliztard.Application.Model;
using Bliztard.Application.Repository.Machine;

namespace Bliztard.Application.Service.Machine;

public class MachineService(IMachineRepository repository, MachineInfo machineInfo) : IMachineService 
{
    public  IMachineRepository Repository  { get; } = repository;
    public  MachineInfo        MachineInfo { get; } = machineInfo;
    private int                m_CurrentIndex       = 0;

    public bool Register(MachineInfo machineInfo) 
    {
        return Repository.Add(machineInfo);
    }

    public bool Unregister(Guid machineId) 
    {
        return Repository.Remove(machineId);
    }

    public MachineInfo? Retrieve(Guid machineId)
    {
        return Repository.Get(machineId);
    }
    
    public IEnumerable<MachineInfo> AllSlavesWillingToAcceptFile()
    {
        return [Repository.Machines.Values.ToList()[m_CurrentIndex++ % Repository.Machines.Count]];
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

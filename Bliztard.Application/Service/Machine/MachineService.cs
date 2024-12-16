using Bliztard.Application.Model;
using Bliztard.Application.Repository.Machine;

namespace Bliztard.Application.Service.Machine;

public class MachineService(IMachineRepository repository, MachineInfo machineInfo) : IMachineService
{
    public IMachineRepository Repository  { get; } = repository;
    public MachineInfo        MachineInfo { get; } = machineInfo;

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

    public bool Uroshbeat(Guid machineId) 
    {
        return SetAlive(machineId);
    }

    private bool SetAlive(Guid machineId, bool value = true)
    {
        var machineInfo = Repository.Get(machineId);

        if (machineInfo == null)
            return false;

        machineInfo.Alive = true;

        return true;
    }
}

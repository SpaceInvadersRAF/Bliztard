using Bliztard.Application.Model;
using Bliztard.Application.Repository.Machine;
using Bliztard.Application.utils;

namespace Bliztard.Application.Service.Machine;

public class MachineService(IMachineRepository repository, MachineInfo machineInfo) : IMachineService
{
    public IMachineRepository Repository  { get; } = repository;
    public MachineInfo        MachineInfo { get; } = machineInfo;

    private const int c_ChunkSize = 128 * 1024 * 1024;

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

    public IEnumerable<MachineInfo> GetAll()
    {
        return Repository.GetAll();
    }

    //TODO
    public bool UploadFile(Stream openReadStream, string fileName, string extension)
    {
        List<byte[]> partsData = new ();
        List<string> partsName = new ();

        byte[] buffer = new byte[c_ChunkSize];
        int bytesRead, index = 0;
        while ((bytesRead = openReadStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            partsName.Add($"{fileName}<{index++}>");

            if (bytesRead < c_ChunkSize)
            {
                byte[] lastPart = new byte[bytesRead];
                Array.Copy(buffer, 0, lastPart, 0, bytesRead);
                partsData.Add(lastPart);
                continue;
            }

            partsData.Add((byte[])buffer.Clone());
        }

        //TODO
        //Call Slave REST API to save partsData and await to confirm it saved
        List<MachineInfo> machines = GetChosenMachines(partsName);

        return true;
    }

    private bool SetAlive(Guid machineId, bool value = true)
    {
        var machineInfo = Repository.Get(machineId);

        if (machineInfo == null)
            return false;

        machineInfo.Alive = true;

        return true;
    }

    private List<MachineInfo> GetChosenMachines(List<string> partsName)
    {
        int size = Repository.Machines.Count;
        List<MachineInfo> machines = Repository.Machines.Values.ToList();

        return partsName.Select(part => machines[FileUtils.ValueOfString(part, size)]).ToList();
    }
}

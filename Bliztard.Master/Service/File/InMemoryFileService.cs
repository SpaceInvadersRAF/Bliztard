using Bliztard.Application.Configurations;
using Bliztard.Application.Mapper;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Master.Repository.File;
using Bliztard.Master.Repository.Machine;
using Bliztard.Master.Service.MachineFile;
using Bliztard.Master.Service.Network;

namespace Bliztard.Master.Service.File;

public class InMemoryFileService(IFileRepository fileRepository, IMachineFileService machineFileService, INetworkService networkService, IMachineRepository machineRepository) : IFileService
{
    private readonly IFileRepository     m_FileRepository     = fileRepository;
    private readonly IMachineRepository  m_MachineRepository  = machineRepository;
    private readonly IMachineFileService m_MachineFileService = machineFileService;
    private readonly INetworkService     m_NetworkService     = networkService;

    public bool RegisterFile(NotifySaveRequest notifySave)
    {
        if (!m_FileRepository.SaveUpload(notifySave.MachineInfo.Id, notifySave.ToFileInfo()))
            return false;

        if (m_FileRepository.ReplicationCount(notifySave.Resource) >= Configuration.Core.ReplicationFactor)
            return true;

        var machineInfos = m_MachineFileService.GetUploadLocations(notifySave.Resource, notifySave.ToFileInfo()
                                                                                                  .Size);

        m_NetworkService.TwincateFileToMachine(notifySave.ToFileInfo(), notifySave.MachineInfo.ToModel(), machineInfos.First()); //todo make it async

        return true;
    }

    public bool RegisterLog(NotifyLogContentRequest request)
    {
        var result = true;
        
        foreach (var saveFileInfo in request.SaveFileRequest)
            if (!m_FileRepository.SaveLoggedUpload(saveFileInfo.MachineInfo.Id, saveFileInfo.ToFileInfo()))
                result = false;

        return result;
    }

    public bool DegenerateFile(string resource)
    {
        if (!m_FileRepository.TryRemove(resource, out var machineFileInfoList))
            return false;

        foreach (var machineFileInfo in machineFileInfoList)
            _ = Task.Run(() => m_NetworkService.NotifyDelete(machineFileInfo.MachineInfo, new NotifyDeleteRequest(machineFileInfo.FileInfo.PathId, resource)));

        return true;
    }

    public void Stats()
    {
        m_FileRepository.Stats();

        foreach (var machineInfo in m_MachineRepository.GetAll().Values)
            _ = Task.Run(() => m_NetworkService.Stats(machineInfo));
    }

    public MachineInfo? LocateFile(string resource)
    {
        return m_FileRepository.RetrieveMachine(resource);
    }
}

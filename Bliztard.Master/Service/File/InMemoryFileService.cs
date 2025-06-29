using Bliztard.Application.Configurations;
using Bliztard.Application.Mapper;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Master.Repository.File;
using Bliztard.Master.Service.MachineFile;
using Bliztard.Master.Service.Network;

namespace Bliztard.Master.Service.File;

public class InMemoryFileService(IFileRepository fileRepository, IMachineFileService machineFileService, INetworkService networkService) : IFileService
{
    private readonly IFileRepository     m_FileRepository     = fileRepository;
    private readonly IMachineFileService m_MachineFileService = machineFileService;
    private readonly INetworkService     m_NetworkService     = networkService;

    public bool RegisterFile(NotifySaveRequest notifySave)
    {
        if (!m_FileRepository.SaveUpload(notifySave.MachineInfo.Id, notifySave.ToFileInfo())) 
            return false;

        if (m_FileRepository.ReplicationCount(notifySave.Resource) >= Configuration.Core.ReplicationFactor)
            return true;
        
        var machineInfos = m_MachineFileService.GetUploadLocations(notifySave.Resource, notifySave.ToFileInfo().Size);

        m_NetworkService.TwincateFileToMachine(notifySave.ToFileInfo(), notifySave.MachineInfo.ToModel(), machineInfos.First()); //todo make it async
        
        return true;
    } 

    public MachineInfo? LocateFile(string resource)
    {
        return m_FileRepository.RetrieveMachine(resource);
    }
}

using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Master.Repository.File;

namespace Bliztard.Master.Service.File;

public class InMemoryFileService(IFileRepository repository) : IFileService
{
    private readonly IFileRepository m_Repository = repository;

    public bool RegisterFile(NotifySaveRequest notifySave)
    {
        return m_Repository.SaveUpload(notifySave.MachineInfo.Id, notifySave.Resource);
    }

    public MachineInfo? LocateFile(string resource)
    {
        return m_Repository.RetrieveMachine(resource);
    }
}

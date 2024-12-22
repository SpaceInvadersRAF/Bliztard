using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Master.Repository.File;

namespace Bliztard.Master.Service.File;

public class InMemoryFileService(IFileRepository repository) : IFileService
{
    public IFileRepository Repository { get; } = repository;

    public bool RegisterFile(NotifySaveRequest notifySave)
    {
        return Repository.SaveUpload(notifySave.MachineInfo.Id, notifySave.Resource);
    }

    public MachineInfo? LocateFile(string resource)
    {
        return Repository.RetrieveMachine(resource);
    }
}

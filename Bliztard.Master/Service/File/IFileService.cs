using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Master.Repository.File;

namespace Bliztard.Master.Service.File;

public interface IFileService
{
    public IFileRepository Repository { get; }
    
    public bool RegisterFile(NotifySaveRequest notifySave);

    public MachineInfo? LocateFile(string resource);
}

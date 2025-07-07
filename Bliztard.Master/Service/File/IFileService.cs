using Bliztard.Application.Core;
using Bliztard.Application.Model;
using Bliztard.Contract.Request;

namespace Bliztard.Master.Service.File;

public interface IFileService : ILifecycle
{
    public bool RegisterFile(NotifySaveRequest notifySave);

    public bool DegenerateFile(string resource);

    public MachineInfo? LocateFile(string resource);
}

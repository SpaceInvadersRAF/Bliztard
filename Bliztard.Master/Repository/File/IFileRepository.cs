using Bliztard.Application.Core;
using Bliztard.Application.Model;

namespace Bliztard.Master.Repository.File;

public interface IFileRepository : ILifecycle
{
    public bool SaveUpload(Guid machineId, string resource);

    public MachineInfo? RetrieveMachine(string resource);

    public bool IsResourceOnMachine(Guid machineId, string resource);
}

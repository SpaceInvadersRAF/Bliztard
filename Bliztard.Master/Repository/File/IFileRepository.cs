using Bliztard.Application.Model;

namespace Bliztard.Master.Repository.File;

public interface IFileRepository
{
    public bool SaveUpload(Guid machineId, string resource);

    public MachineInfo? RetrieveMachine(string resource);
}

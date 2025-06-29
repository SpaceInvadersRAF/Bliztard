using Bliztard.Application.Core;
using Bliztard.Application.Model;
using FileInfo = Bliztard.Application.Model.FileInfo;

namespace Bliztard.Master.Repository.File;

public interface IFileRepository : ILifecycle
{
    public bool SaveUpload(Guid machineId, FileInfo fileInfo);

    public MachineInfo? RetrieveMachine(string resource);

    public int ReplicationCount(string resource);

    public bool IsResourceOnMachine(Guid machineId, string resource);

    public bool SaveTransient(Guid machineId, string resource);
    
    public bool RemoveMachine(Guid machineId, out ICollection<FileInfo> outResources);
}

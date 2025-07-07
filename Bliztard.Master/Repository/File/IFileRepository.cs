using System.Diagnostics.CodeAnalysis;

using Bliztard.Application.Core;
using Bliztard.Application.Model;

using FileInfo = Bliztard.Application.Model.FileInfo;

namespace Bliztard.Master.Repository.File;

public interface IFileRepository : ILifecycle
{
    public bool SaveUpload(Guid machineId, FileInfo fileInfo); 
    
    public bool TryRemove(string resource, out List<(MachineInfo MachineInfo, FileInfo FileInfo)> machineFileInfoList);
    
    public MachineInfo? RetrieveMachine(string resource);

    public List<MachineInfo> RetrieveAllMachines(string resource);

    public int ReplicationCount(string resource);

    public bool IsResourceOnMachine(Guid machineId, string resource);

    public bool SaveTransient(Guid machineId, string resource);

    public bool RemoveMachine(Guid machineId, out ICollection<FileInfo> outResources);
}

using Bliztard.Application.Core;
using Bliztard.Application.Model;

namespace Bliztard.Master.Service.MachineFile;

public interface IMachineFileService : ILifecycle
{
    public IEnumerable<MachineInfo> GetUploadLocations(string resource, long size);
}

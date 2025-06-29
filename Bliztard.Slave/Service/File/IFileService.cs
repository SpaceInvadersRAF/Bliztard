using Bliztard.Application.Core;
using Bliztard.Application.Model;

namespace Bliztard.Slave.Service.File;

public interface IFileService : ILifecycle
{
    public Stream CreateStream(out Guid pathId);

    public Task<bool> Save(SaveFileInfo saveFileInfo);

    public Task<bool> Update(SaveFileInfo saveFileInfo);

    public Task<bool> Remove(string resource, Guid guid);

    public Stream? Read(string resource);
}

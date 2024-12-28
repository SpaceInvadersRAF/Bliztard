using Bliztard.Application.Core;
using Bliztard.Application.Model;

namespace Bliztard.Slave.Service.File;

public interface IFileService : ILifecycle
{
    public Stream CreateStream(out Guid pathId);

    public bool Save(SaveFileInfo saveFileInfo);

    public Stream? Read(string resource);
}

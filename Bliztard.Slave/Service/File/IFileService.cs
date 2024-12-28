using Bliztard.Application.Core;
using Bliztard.Application.Model;
using Bliztard.Slave.Application;

namespace Bliztard.Slave.Service.File;

public interface IFileService : ILifecycle
{
    public Stream CreateStream(out Guid pathId);

    public bool Save(SaveFileInfo saveFileInfo);

    public Stream? Read(string resource);
}

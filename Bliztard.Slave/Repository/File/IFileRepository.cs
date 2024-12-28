using Bliztard.Application.Core;

namespace Bliztard.Slave.Repository.File;

public interface IFileRepository : ILifecycle
{
    public Stream CreateStream(string path);

    public bool Save(string resource, Guid pathId);
    
    public Stream? Load(string resource);
}

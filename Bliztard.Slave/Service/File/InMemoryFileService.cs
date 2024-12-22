using Bliztard.Application.Model;
using Bliztard.Slave.Repository.File;

namespace Bliztard.Slave.Service.File;

public class InMemoryFileService(IFileRepository repository) : IFileService
{
    public IFileRepository Repository { get; } = repository;

    public Stream CreateStream(out Guid pathId)
    {
        pathId = Guid.NewGuid();
        
        return Repository.CreateStream(pathId.ToString());
    }

    public bool Save(SaveFileInfo saveFileInfo)
    {
        return Repository.Save(saveFileInfo.Resource, saveFileInfo.PathId);
    }

    public Stream? Read(string resource)
    {
        return Repository.Load(resource);
    }
}

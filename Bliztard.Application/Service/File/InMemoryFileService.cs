using Bliztard.Application.Repository.File;

namespace Bliztard.Application.Service.File;

public class InMemoryFileService(IFileRepository repository) : IFileService
{
    public IFileRepository Repository { get; } = repository;

    public Stream CreateStream(out Guid pathId)
    {
        pathId = Guid.NewGuid();
        
        return Repository.CreateStream(pathId.ToString());
    }

    public bool Save(IDictionary<string, string> data, Guid pathId)
    {
        return Repository.Save($"{data["username"]}/{data["path"]}", pathId);
    }

    public Stream? Read(string resource)
    {
        return Repository.Load(resource);
    }
}

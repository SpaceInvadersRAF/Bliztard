using Bliztard.Slave.Repository.File;

namespace Bliztard.Slave.Service.File;

public interface IFileService
{
    public IFileRepository Repository { get; }

    public Stream CreateStream(out Guid pathId);

    public bool Save(IDictionary<string, string> data, Guid pathId);

    public Stream? Read(string resource);
}

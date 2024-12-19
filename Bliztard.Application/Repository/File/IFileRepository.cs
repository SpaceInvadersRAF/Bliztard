namespace Bliztard.Application.Repository.File;

public interface IFileRepository
{
    public Stream CreateStream(string path);

    public bool Save(string resource, Guid pathId);
    
    public Stream? Load(string resource);
}

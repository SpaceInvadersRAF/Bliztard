using System.Collections.Concurrent;

namespace Bliztard.Application.Repository.File;

public class InMemoryFileRepository : IFileRepository
{
    public ConcurrentDictionary<string, MemoryStream> Files              { get; } = new();
    public ConcurrentDictionary<string, string>       ResourceDictionary { get; } = new();

    public Stream CreateStream(string path)
    {
        return Files[path] = new MemoryStream();
    }

    public bool Save(string resource, Guid pathId)
    {
        var streamOld  = Files[pathId.ToString()];
        var streamCopy = new MemoryStream();
        
        streamOld.Position = 0;
        streamOld.CopyTo(streamCopy);

        Files[pathId.ToString()]     = streamCopy;
        ResourceDictionary[resource] = pathId.ToString();

        return true;
    }

    public Stream? Load(string resource)
    {
        var stream = Files[ResourceDictionary[resource]];
        stream.Position = 0;
        
        return stream;
    }
}

using System.Collections.Concurrent;

namespace Bliztard.Slave.Repository.File;

public class InMemoryFileRepository : IFileRepository
{
    private readonly ConcurrentDictionary<string, MemoryStream> m_Files              = new();
    private readonly ConcurrentDictionary<string, string>       m_ResourceDictionary = new();

    public Stream CreateStream(string path)
    {
        return m_Files[path] = new MemoryStream();
    }

    public bool Save(string resource, Guid pathId)
    {
        var streamCopy = CreateStreamCopy(m_Files[pathId.ToString()]);
        
        m_Files[pathId.ToString()]     = streamCopy;
        m_ResourceDictionary[resource] = pathId.ToString();
        
        return true;
    }

    public Stream? Load(string resource)
    {
        if (!m_ResourceDictionary.TryGetValue(resource, out var pathId))
            return null;

        if (!m_Files.TryGetValue(pathId, out var stream))
            return null;

        var newStream = CreateStreamCopy(stream);
        newStream.Position = 0;
        
        return newStream;
    }

    private MemoryStream CreateStreamCopy(MemoryStream stream)
    {
        var streamCopy = new MemoryStream();

        stream.Position = 0;
        stream.CopyTo(streamCopy);
        
        return streamCopy;
    }
}

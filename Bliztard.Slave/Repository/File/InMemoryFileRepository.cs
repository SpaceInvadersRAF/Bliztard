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
        var streamOld  = m_Files[pathId.ToString()];
        var streamCopy = new MemoryStream();
        
        streamOld.Position = 0;
        streamOld.CopyTo(streamCopy);

        m_Files[pathId.ToString()]     = streamCopy;
        m_ResourceDictionary[resource] = pathId.ToString();

        return true;
    }

    public Stream? Load(string resource)
    {
        var stream = m_Files[m_ResourceDictionary[resource]];
        stream.Position = 0;
        
        return stream;
    }
}

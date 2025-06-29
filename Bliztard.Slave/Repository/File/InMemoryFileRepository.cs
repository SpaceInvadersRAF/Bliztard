using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Bliztard.Slave.Repository.File;

public class InMemoryFileRepository // : IFileRepository note: legacy
{
    // private readonly ConcurrentDictionary<string, MemoryStream> m_Files              = new();
    // private readonly ConcurrentDictionary<string, MemoryStream> m_SessionContent     = new();
    // private readonly ConcurrentDictionary<string, string>       m_ResourceDictionary = new();
    //
    // public Stream CreateStream(string path)
    // {
    //     return m_Files[path] = new MemoryStream();
    //
    //     return m_SessionContent[path] = new MemoryStream();
    // }
    //
    // public bool Save(Guid pathId, string resource, string content)
    // {
    //     var streamCopy = CreateStreamCopy(m_Files[pathId.ToString()]);
    //     
    //     m_Files[pathId.ToString()]     = streamCopy;
    //     m_ResourceDictionary[resource] = pathId.ToString();
    //     
    //     return true;
    // }
    //
    // public bool Delete(string resource, Guid pathId)
    // {
    //     // throw new NotImplementedException();
    //     return false;
    // }
    //
    // public bool Update(Guid pathId, string content)
    // {
    //      // throw new NotImplementedException();
    //      return false;
    // }
    //
    // public bool TryRemoveSessionContent(Guid pathId, [MaybeNullWhen(false)] out string content)
    // {
    //     content = "";
    //     // throw new NotImplementedException();
    //     return false;
    // }
    //
    // public bool Rename(string oldResource, string newResource)
    // {
    //     // throw new NotImplementedException();
    //     return false;
    // }
    //
    // public Stream? Load(string resource)
    // {
    //     if (!m_ResourceDictionary.TryGetValue(resource, out var pathId))
    //         return null;
    //
    //     if (!m_Files.TryGetValue(pathId, out var stream))
    //         return null;
    //
    //     var newStream = CreateStreamCopy(stream);
    //     newStream.Position = 0;
    //     
    //     return newStream;
    // }
    //
    // private MemoryStream CreateStreamCopy(MemoryStream stream)
    // {
    //     var streamCopy = new MemoryStream();
    //
    //     stream.Position = 0;
    //     stream.CopyTo(streamCopy);
    //     
    //     return streamCopy;
    // }
}

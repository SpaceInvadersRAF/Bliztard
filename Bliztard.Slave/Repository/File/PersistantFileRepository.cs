using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Bliztard.Application.Extension;
using Bliztard.Persistence.Table;

namespace Bliztard.Slave.Repository.File;

public class PersistantFileRepository : IFileRepository
{
    private readonly WiwiwiTable                                m_Table          = new();
    private readonly ConcurrentDictionary<string, MemoryStream> m_SessionContent = new();

    public Stream CreateStream(string path)
    {
        return m_SessionContent[path] = new MemoryStream();
    }

    public bool Save(Guid pathId, string resource, Stream content)
    {
        return m_Table.Add(pathId, "primary_index", resource, content.ReadContent(Encoding.UTF8));
    }

    public bool Update(Guid pathId, Stream content)
    {
        return m_Table.Update(pathId, content.ReadContent(Encoding.UTF8));
    }

    public bool TryRemoveSessionContent(Guid pathId, [MaybeNullWhen(false)] out Stream content)
    {
        content = null;
        
        if (!m_SessionContent.TryRemove(pathId.ToString(), out var streamContent))
            return false;
        
        content = streamContent;

        return true;
    }

    public bool Rename(string oldResource, string newResource)
    {
        return m_Table.Rename("primary_index", oldResource, newResource);
    }

    public bool Delete(string resource, Guid pathId)
    {
        return m_Table.Remove(pathId, "primary_index", resource);
    }

    public Stream? Load(string resource)
    {
        if (!m_Table.TryFind("primary_index", resource, out var data))
            return null;
        
        return new MemoryStream(Encoding.UTF8.GetBytes(data));
    }
}

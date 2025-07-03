using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using Bliztard.Application.Extension;
using Bliztard.Slave.BackgroundService;

namespace Bliztard.Slave.Repository.File;

public class PersistantFileRepository(WiwiwiBackgroundService wiwiwiBackgroundService) : IFileRepository
{
    // ReSharper disable once ConvertToConstant.Local
    private readonly string m_DefaultIndex = "primary_index";

    private readonly ConcurrentDictionary<string, MemoryStream> m_SessionContent          = new();
    private readonly WiwiwiBackgroundService                    m_WiwiwiBackgroundService = wiwiwiBackgroundService;

    public Stream CreateStream(string path)
    {
        return m_SessionContent[path] = new MemoryStream();
    }

    public async Task<bool> Save(Guid pathId, string resource, Stream content)
    {
        var contentString = content.ReadContent(Encoding.UTF8);

        return await m_WiwiwiBackgroundService.LogTable.LogCreateAction(pathId, resource, contentString) && m_WiwiwiBackgroundService.WiwiwiTable.Add(pathId, m_DefaultIndex, resource, contentString);
    }

    public async Task<bool> Update(Guid pathId, string resource, Stream content)
    {
        var contentString = content.ReadContent(Encoding.UTF8);

        return await m_WiwiwiBackgroundService.LogTable.LogUpdateAction(pathId, resource, contentString) && m_WiwiwiBackgroundService.WiwiwiTable.Add(pathId, m_DefaultIndex, resource, contentString);
    }

    public async Task<bool> Rename(string oldResource, string newResource)
    {
        return await m_WiwiwiBackgroundService.LogTable.LogRenameAction(Guid.Empty, m_DefaultIndex) && m_WiwiwiBackgroundService.WiwiwiTable.Rename(m_DefaultIndex, oldResource, newResource);
    }

    public async Task<bool> Delete(string resource, Guid pathId)
    {
        return await m_WiwiwiBackgroundService.LogTable.LogDeleteAction(pathId, resource) && m_WiwiwiBackgroundService.WiwiwiTable.Remove(pathId, m_DefaultIndex, resource);
    }

    public bool TryRemoveSessionContent(Guid pathId, [MaybeNullWhen(false)] out Stream content)
    {
        content = null;

        if (!m_SessionContent.TryRemove(pathId.ToString(), out var streamContent))
            return false;

        content = streamContent;

        return true;
    }

    public Stream? Load(string resource)
    {
        if (!m_WiwiwiBackgroundService.WiwiwiTable.TryFind(m_DefaultIndex, resource, out var data))
            return null;

        return new MemoryStream(Encoding.UTF8.GetBytes(data));
    }
}

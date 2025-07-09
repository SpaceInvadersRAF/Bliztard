using System.Diagnostics.CodeAnalysis;

using Bliztard.Application.Model;
using Bliztard.Slave.Repository.File;

namespace Bliztard.Slave.Service.File;

public class PersistantFileService(ILogger<PersistantFileService> logger, IFileRepository repository) : IFileService
{
    private readonly ILogger<PersistantFileService> m_Logger          = logger;
    private readonly IFileRepository                m_Repository      = repository;

    public Stream CreateStream(out Guid pathId)
    {
        pathId = Guid.NewGuid();

        return m_Repository.CreateStream(pathId.ToString());
    }

    public async Task<bool> Save(SaveFileInfo saveFileInfo)
    {
        if (!m_Repository.TryRemoveSessionContent(saveFileInfo.PathId, out var content))
            return false;

        var result = await m_Repository.Save(saveFileInfo.PathId, saveFileInfo.Resource, content);

        if (!m_Repository.IsBlockFull())
            return result;

        _ = Task.Run(() => m_Repository.PersistWiwiwiTable());

        return result;
    }

    public Stream? Read(string resource)
    {
        return m_Repository.Load(resource);
    }

    public void Stats()
    {
        m_Repository.Stats();
    }

    public async Task<bool> Update(SaveFileInfo saveFileInfo)
    {
        if (!m_Repository.TryRemoveSessionContent(saveFileInfo.PathId, out var content))
            return false;

        return await m_Repository.Update(saveFileInfo.PathId, saveFileInfo.Resource, content);
    }

    public async Task<bool> Remove(string resource, Guid guid)
    {
        return await m_Repository.Delete(resource, guid);
    }

    //todo: mby one day or day one in the future, finals week or my final week :(
    public async Task<bool> Rename(string oldResource, string newResource) => false;
}

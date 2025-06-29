using System.Text;
using Bliztard.Application.Extension;
using Bliztard.Application.Model;
using Bliztard.Persistence.Log;
using Bliztard.Slave.Repository.File;

namespace Bliztard.Slave.Service.File;

public class PersistantFileService(LogTable logTable, IFileRepository repository, ILogger<InMemoryFileService> logger) : IFileService
{
    private readonly ILogger<InMemoryFileService> m_Logger     = logger;
    private readonly IFileRepository              m_Repository = repository;
    private readonly LogTable                     m_LogTable   = logTable;

    public Stream CreateStream(out Guid pathId)
    {
        pathId = Guid.NewGuid();
        
        return m_Repository.CreateStream(pathId.ToString());
    }

    public async Task<bool> Save(SaveFileInfo saveFileInfo)
    {
        if (!m_Repository.TryRemoveSessionContent(saveFileInfo.PathId, out var content))
            return false;

        var createTask = m_LogTable.LogCreateAction(saveFileInfo.PathId, saveFileInfo.Resource, content.ReadContent(Encoding.UTF8));
        
        return m_Repository.Save(saveFileInfo.PathId, saveFileInfo.Resource, content) && await createTask;
    }

    public Stream? Read(string resource)
    {
        return m_Repository.Load(resource);
    }

    public async Task<bool> Update(SaveFileInfo saveFileInfo)
    {
        if (!m_Repository.TryRemoveSessionContent(saveFileInfo.PathId, out var content))
            return false;
        
        var updateTask = m_LogTable.LogUpdateAction(saveFileInfo.PathId, saveFileInfo.Resource, content.ReadContent(Encoding.UTF8));
        
        return m_Repository.Update(saveFileInfo.PathId, content) && await updateTask;
    }

    public async Task<bool> Rename(string oldResource, string newResource)
    {
        //todo: mby one day or day one in the future, finals week or my final week
        return false;
    }

    public async Task<bool> Remove(string resource, Guid guid)
    {
        var deleteTask = m_LogTable.LogDeleteAction(guid, resource);
        
        return m_Repository.Delete(resource, guid) && await deleteTask;
    }
}

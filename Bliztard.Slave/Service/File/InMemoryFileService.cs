using Bliztard.Application.Model;
using Bliztard.Slave.Repository.File;

namespace Bliztard.Slave.Service.File;

public class InMemoryFileService(IFileRepository repository, ILogger<InMemoryFileService> logger) //  : IFileService note: legacy
{
    // private readonly ILogger<InMemoryFileService> m_Logger     = logger;
    // private readonly IFileRepository              m_Repository = repository;
    //
    // public Stream CreateStream(out Guid pathId)
    // {
    //     pathId = Guid.NewGuid();
    //     
    //     return m_Repository.CreateStream(pathId.ToString());
    // }
    //
    // public Task<bool> Save(SaveFileInfo saveFileInfo)
    // {
    //     return Task.FromResult(m_Repository.Save(saveFileInfo.Resource, saveFileInfo.PathId));
    // }
    //
    // public Stream? Read(string resource)
    // {
    //     return m_Repository.Load(resource);
    // }
}

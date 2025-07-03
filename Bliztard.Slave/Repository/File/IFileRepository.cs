using System.Diagnostics.CodeAnalysis;

using Bliztard.Application.Core;

namespace Bliztard.Slave.Repository.File;

public interface IFileRepository : ILifecycle
{
    public Stream CreateStream(string path);

    public Task<bool> Save(Guid pathId, string resource, Stream content);

    public Task<bool> Update(Guid pathId, string resource, Stream content);

    public bool TryRemoveSessionContent(Guid pathId, [MaybeNullWhen(false)] out Stream content);

    public Task<bool> Rename(string oldResource, string newResource);

    public Task<bool> Delete(string resource, Guid pathId);

    public Stream? Load(string resource);
}

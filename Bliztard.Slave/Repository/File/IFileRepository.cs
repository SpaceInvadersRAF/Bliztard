using System.Diagnostics.CodeAnalysis;
using Bliztard.Application.Core;

namespace Bliztard.Slave.Repository.File;

public interface IFileRepository : ILifecycle
{
    public Stream CreateStream(string path);

    public bool Save(Guid pathId, string resource, Stream content);

    public bool Update(Guid pathId, Stream content);

    public bool TryRemoveSessionContent(Guid pathId, [MaybeNullWhen(false)] out Stream content);

    public bool Rename(string oldResource, string newResource);

    public bool Delete(string resource, Guid pathId);

    public Stream? Load(string resource);
}

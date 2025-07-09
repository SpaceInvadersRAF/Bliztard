using System.Diagnostics.CodeAnalysis;

using Bliztard.Application.Configurations;
using Bliztard.Persistence.Table.Types;

namespace Bliztard.Persistence.Table;

public class WiwiwiTable
{
    public RecordTable RecordTable { get; } = new();
    public IndexTable  IndexTable  { get; } = new();

    public bool Add(Guid guid, string indexName, string resource, string content)
    {
        return RecordTable.AddEntry(guid, content) && IndexTable.AddEntry(indexName, resource, guid);
    }

    public bool Update(Guid guid, string content)
    {
        return RecordTable.UpdateEntry(guid, content);
    }

    public bool Rename(string indexName, string oldResource, string newResource)
    {
        return IndexTable.UpdateEntry(indexName, oldResource, newResource);
    }

    public bool Remove(Guid guid, string indexName, string resource)
    {
        return RecordTable.RemoveEntry(guid) && IndexTable.RemoveEntry(indexName, resource);
    }

    public List<(PersistentGuid Id, PersistentUtf8String Data)> FindAllResources()
    {
        var idSet = RecordTable.KeySegment.GetEntries()
                               .Where(entry => entry.RecordOffset == -1)
                               .Select(entry => entry.RecordGuid.value)
                               .ToHashSet();

        return IndexTable.DataSegment.GetEntries("primary_index")
                         .Select(entry => (entry.IndexValue, entry.IndexKey))
                         .Where(entry => !idSet.Contains(entry.IndexValue.value))
                         .ToList();
    }

    public PersistentUtf8String? Find(string indexName, string resource)
    {
        if (!IndexTable.TryFindEntry(indexName, resource, out var resourceGuid))
            return null;

        if (!RecordTable.TryFindEntry(resourceGuid, out var data))
            return null;

        return data;
    }

    public bool TryFind(string indexName, string resource, [MaybeNullWhen(false)] out PersistentUtf8String data)
    {
        data = Find(indexName, resource);

        return data is not null;
    }

    public bool Persist()
    {
        var fileName       = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var recordFilePath = Path.Combine(Configuration.File.RecordDirectory, $"{fileName}.{RecordTable.Extension}");
        var indexFilePath  = Path.Combine(Configuration.File.IndexDirectory,  $"{fileName}.{IndexTable.Extension}");

        using var recordStream = new FileStream(recordFilePath, FileMode.Create, FileAccess.Write);
        using var recordWriter = new BinaryWriter(recordStream);

        RecordTable.Serialize(recordWriter);

        using var indexStream = new FileStream(indexFilePath, FileMode.Create, FileAccess.Write);
        using var indexWriter = new BinaryWriter(indexStream);

        IndexTable.Serialize(indexWriter);

        return true;
    }
}

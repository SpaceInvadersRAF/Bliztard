using System.Diagnostics.CodeAnalysis;

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

    // public List<PersistentUtf8String> FindAllResources()
    // {
    //     var primaryIndexEntities = IndexTable.DataSegment.GetEntries("primary_index");
    //
    //     return primaryIndexEntities.Select(entry => entry.IndexKey)
    //                                .ToList();
    // }

    public List<(PersistentGuid Id, PersistentUtf8String Data)> FindAllResources()
    {
        return IndexTable.DataSegment.GetEntries("primary_index").Select(entry => (entry.IndexValue, entry.IndexKey))
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
}

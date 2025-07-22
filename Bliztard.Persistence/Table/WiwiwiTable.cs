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

    public List<(PersistentGuid Id, PersistentUtf8String Data)> FindAllPersistedResources()
    {
        var fileNames = Directory.GetFiles(Configuration.File.RecordDirectory)
                                 .Where(filename => Path.GetExtension(filename)[1..]
                                                        .Equals(RecordTable.FileExtension, StringComparison.InvariantCultureIgnoreCase))
                                 .Select(Path.GetFileNameWithoutExtension)
                                 .Order()
                                 .ToList();

        var table = new WiwiwiTable();

        var idSet = new HashSet<Guid>();

        foreach (var fileName in fileNames)
        {
            using var recordStream = new FileStream(Path.Combine(Configuration.File.RecordDirectory, $"{fileName}.{RecordTable.FileExtension}"), FileMode.Open, FileAccess.Read);
            using var recordReader = new BinaryReader(recordStream);

            table.RecordTable.KeySegment.Deserialize(recordReader);

            foreach (var entry in RecordTable.KeySegment.GetEntries())
                if (entry.RecordOffset == -1)
                    idSet.Remove(entry.RecordGuid.value);
                else
                    idSet.Add(entry.RecordGuid.value);

            table.RecordTable.Clear();
        }

        List<(PersistentGuid Id, PersistentUtf8String Data)> resources = [];

        foreach (var fileName in fileNames)
        {
            using var indexStream = new FileStream(Path.Combine(Configuration.File.IndexDirectory, $"{fileName}.{IndexTable.FileExtension}"), FileMode.Open, FileAccess.Read);
            using var indexReader = new BinaryReader(indexStream);

            table.IndexTable.Deserialize(indexReader, "primary_index");

            resources.AddRange(table.IndexTable.DataSegment.GetEntries("primary_index")
                                    .Select(entry => (entry.IndexValue, entry.IndexKey))
                                    .Where(entry => !idSet.Contains(entry.IndexValue.value))
                                    .ToList());

            table.IndexTable.Clear();
        }

        return resources;
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

    public static bool TryLocateResource(string resource, [MaybeNullWhen(false)] out PersistentUtf8String content) //out Stream contentStream
    {
        content = null;

        var fileNames = Directory.GetFiles(Configuration.File.RecordDirectory)
                                 .Where(filename => Path.GetExtension(filename)
                                                        .Equals(RecordTable.FileExtension, StringComparison.InvariantCultureIgnoreCase))
                                 .Select(Path.GetFileNameWithoutExtension)
                                 .OrderDescending()
                                 .ToList();

        var table = new WiwiwiTable();

        foreach (var fileName in fileNames)
        {
            using var indexStream = new FileStream(Path.Combine(Configuration.File.IndexDirectory, $"{fileName}.{IndexTable.FileExtension}"), FileMode.Open, FileAccess.Read);
            using var indexReader = new BinaryReader(indexStream);

            table.IndexTable.Deserialize(indexReader, "primary_index");

            if (!table.IndexTable.TryFindEntry("primary_index", resource, out var resourceId))
            {
                table.IndexTable.Clear();
                
                continue;
            }

            using var recordStream = new FileStream(Path.Combine(Configuration.File.RecordDirectory, $"{fileName}.{RecordTable.FileExtension}"), FileMode.Open, FileAccess.Read);
            using var recordReader = new BinaryReader(recordStream);

            table.RecordTable.Deserialize(recordReader, resourceId); // todo: check

            if (!table.RecordTable.TryFindEntry(resourceId, out var resourceContent)) //should always be !true
                continue;

            content = resourceContent;

            return true;
        }

        return false;
    }
}

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Bliztard.Application.Extension;
using Bliztard.Persistence.Extension;
using Bliztard.Persistence.Marshaling;
using Bliztard.Persistence.Table.Types;

using Towel.DataStructures;

namespace Bliztard.Persistence.Table;

#region Index Table | Structure

public class IndexTable : IMarshal
{
    internal IndexHeaderSegment HeaderSegment { get; }
    public   IndexKeySegment    KeySegment    { get; }
    public   IndexDataSegment   DataSegment   { get; }

    public IndexTable()
    {
        HeaderSegment = new IndexHeaderSegment(this);
        KeySegment    = new IndexKeySegment(this);
        DataSegment   = new IndexDataSegment(this);
    }

    public void Serialize(BinaryWriter writer)
    {
        HeaderSegment.Serialize(writer);
        KeySegment.Serialize(writer);
        DataSegment.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader)
    {
        HeaderSegment.Deserialize(reader);
        KeySegment.Deserialize(reader);
        DataSegment.Deserialize(reader);
    }

    public void Deserialize(BinaryReader reader, string indexName)
    {
        HeaderSegment.Deserialize(reader);
        KeySegment.Deserialize(reader);
        DataSegment.Deserialize(reader, indexName);
    }

    public bool AddEntry(string indexName, string resource, Guid recordGuid)
    {
        KeySegment.AddEntry(indexName);

        return DataSegment.AddEntry(indexName, resource, recordGuid);
    }

    public bool UpdateEntry(string indexName, string oldValue, string newValue)
    {
        return KeySegment.Contains(indexName) && DataSegment.UpdateEntry(indexName, oldValue, newValue);
    }

    public PersistentGuid FindEntry(string indexName, string resource)
    {
        return DataSegment.GetEntry(indexName, resource)
                          ?.IndexValue ?? default;
    }

    public bool TryFindEntry(string indexName, string resource, out PersistentGuid indexGuid)
    {
        indexGuid = FindEntry(indexName, resource);

        return indexGuid != default;
    }

    public bool RemoveEntry(string indexName, string resource)
    {
        return DataSegment.RemoveEntry(indexName, resource);
    }

    public long Size()
    {
        return HeaderSegment.Size() + KeySegment.Size() + DataSegment.Size();
    }
}

internal class IndexHeaderSegment(IndexTable indexTable) : IMarshal
{
    public readonly PersistentConstAsciiString Signature = "BLIZTARDSSINDEX";

    public   PersistentInt8  Version           { set;         get; }
    internal PersistentInt64 KeySegmentOffset  { private set; get; }
    internal PersistentInt64 DataSegmentOffset { private set; get; }

    private readonly IndexTable m_IndexTable = indexTable;

    public void Serialize(BinaryWriter writer)
    {
        Calculate();

        Signature.Serialize(writer);
        Version.Serialize(writer);
        KeySegmentOffset.Serialize(writer);
        DataSegmentOffset.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader)
    {
        Signature.Deserialize(reader);
        Version.Deserialize(reader);
        KeySegmentOffset.Deserialize(reader);
        DataSegmentOffset.Deserialize(reader);
    }

    public void Calculate()
    {
        KeySegmentOffset  = m_IndexTable.HeaderSegment.Size();
        DataSegmentOffset = KeySegmentOffset + m_IndexTable.KeySegment.Size();
    }

    public long Size()
    {
        // TODO: maybe lock
        return Signature.Size() + Version.Size() + KeySegmentOffset.Size() + DataSegmentOffset.Size();
    }
}

public class IndexKeySegment(IndexTable indexTable) : IMarshal
{
    private readonly IndexTable                 m_IndexTable = indexTable;
    private readonly HashSet<string>            m_IndexSet   = [];
    private readonly List<IndexKeySegmentEntry> m_Entries    = [];
    private readonly ReaderWriterLockSlim       m_Lock       = new(LockRecursionPolicy.SupportsRecursion);

    public void Serialize(BinaryWriter writer)
    {
        Calculate();

        foreach (var entry in m_Entries)
            entry.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader)
    {
        while (reader.BaseStream.Position < m_IndexTable.HeaderSegment.DataSegmentOffset)
        {
            var entry = new IndexKeySegmentEntry(m_IndexTable);

            entry.Deserialize(reader);

            AddEntry(entry);
        }
    }

    public bool AddEntry(string indexName)
    {
        return !Contains(indexName) && AddEntry(new IndexKeySegmentEntry(m_IndexTable, indexName));
    }

    private bool AddEntry(IndexKeySegmentEntry entry)
    {
        m_Lock.WriteBlock(() =>
                          {
                              m_Entries.Add(entry);
                              m_IndexSet.Add(entry.IndexName);
                          });

        return true;
    }

    public List<IndexKeySegmentEntry> GetEntries()
    {
        return m_Lock.ReadBlock(() => m_Entries.ToList());
    }

    public void Calculate()
    {
        m_Lock.ReadBlock(() => m_Entries.ForEach(entry => entry.Calculate()));
    }

    public long Size()
    {
        return m_Lock.ReadBlock(() => m_Entries.Aggregate(0L, (current, entry) => current + entry.Size()));
    }

    internal IndexKeySegmentEntry? GetEntry(string indexName)
    {
        return m_Lock.ReadBlock(() => m_Entries.FirstOrDefault(entry => entry.IndexName == indexName));
    }

    internal IndexKeySegmentEntry? GetPreviousEntry(string indexName)
    {
        //NOTE: dont lock, only one is allowed
        if (m_Entries.Count < 2 || m_Entries.First()
                                            .IndexName == indexName)
            return null;

        for (int index = 1; index < m_Entries.Count; ++index)
            if (m_Entries[index].IndexName == indexName)
                return m_Entries[index - 1];

        return null;
    }

    internal IndexKeySegmentEntry? GetNextEntry(string indexName)
    {
        //NOTE: dont lock, only one is allowed
        if (m_Entries.Count < 2 || m_Entries.Last()
                                            .IndexName == indexName)
            return null;

        for (int index = 0; index < m_Entries.Count - 1; ++index)
            if (m_Entries[index].IndexName == indexName)
                return m_Entries[index + 1];

        return null;
    }

    public bool Contains(string indexName) // Note: Complete
    {
        return m_IndexSet.Contains(indexName);
    }
}

public class IndexKeySegmentEntry(IndexTable indexTable, string indexName = "") : IMarshal
{
    private readonly IndexTable m_IndexTable = indexTable;

    internal PersistentAsciiString IndexName   { set; get; } = indexName;
    internal PersistentInt64       IndexOffset { set; get; }

    public void Serialize(BinaryWriter writer)
    {
        Calculate();

        IndexName.Serialize(writer);
        IndexOffset.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader)
    {
        IndexName.Deserialize(reader);
        IndexOffset.Deserialize(reader);
    }

    public void Calculate()
    {
        var previousEntry = m_IndexTable.KeySegment.GetPreviousEntry(IndexName);

        IndexOffset = previousEntry == null
                      ? 0
                      : previousEntry.IndexOffset + m_IndexTable.DataSegment.GetEntries(previousEntry.IndexName)
                                                                .Aggregate(0L, (current, entry) => current + entry.Size());
    }

    public long Size()
    {
        return IndexName.Size() + IndexOffset.Size();
    }
}

public class IndexDataSegment(IndexTable indexTable) : IMarshal
{
    private readonly IndexTable                                                         m_IndexTable = indexTable;
    private readonly ConcurrentDictionary<string, IRedBlackTree<IndexDataSegmentEntry>> m_Entries    = new();
    private readonly ReaderWriterLockSlim                                               m_Lock       = new(LockRecursionPolicy.SupportsRecursion);

    public void Serialize(BinaryWriter writer)
    {
        //NOTE: dont lock, only one is allowed

        m_IndexTable.KeySegment.GetEntries()
                    .Select(keyEntry => m_Entries.GetValueOrDefault(keyEntry.IndexName, RedBlackTreeLinked.New<IndexDataSegmentEntry>()))
                    .SelectMany(dataEntries => dataEntries)
                    .ToList()
                    .ForEach(dataEntry => dataEntry.Serialize(writer));
    }

    public void Deserialize(BinaryReader reader)
    {
        m_IndexTable.KeySegment.GetEntries()
                    .ForEach(keyEntry => Deserialize(reader, keyEntry.IndexName));
    }

    public void Deserialize(BinaryReader reader, string indexKey)
    {
        //NOTE: dont lock, only one is allowed

        var keyEntry     = m_IndexTable.KeySegment.GetEntry(indexKey);
        var nextKeyEntry = m_IndexTable.KeySegment.GetNextEntry(indexKey);

        if (keyEntry is null)
            return;

        long nextOffset = nextKeyEntry is null ? reader.BaseStream.Length : m_IndexTable.HeaderSegment.DataSegmentOffset + nextKeyEntry.IndexOffset;

        reader.BaseStream.Position = m_IndexTable.HeaderSegment.DataSegmentOffset + keyEntry.IndexOffset;

        while (reader.BaseStream.Position < nextOffset)
        {
            var entry = new IndexDataSegmentEntry(m_IndexTable);

            entry.Deserialize(reader);

            AddEntry(keyEntry.IndexName, entry);
        }
    }

    public bool AddEntry(string indexName, string indexKey, Guid indexValue)
    {
        return AddEntry(indexName, new IndexDataSegmentEntry(m_IndexTable, indexKey) { IndexValue = indexValue });
    }

    private bool AddEntry(string indexName, IndexDataSegmentEntry entry)
    {
        return m_Lock.WriteBlock(() =>
                                 {
                                     m_Entries.GetOrAdd(indexName, RedBlackTreeLinked.New<IndexDataSegmentEntry>())
                                              .Add(entry);

                                     return true;
                                 });
    }

    public IRedBlackTree<IndexDataSegmentEntry> GetEntries(string indexName)
    {
        return m_Lock.ReadBlock(() => m_Entries.GetValueOrDefault(indexName, RedBlackTreeLinked.New<IndexDataSegmentEntry>()));
    }

    public void Calculate() { }

    public long Size()
    {
        return m_Lock.ReadBlock(() => m_Entries.SelectMany(dataEntries => dataEntries.Value)
                                               .Aggregate(0L, (current, entry) => current + entry.Size()));
    }

    public bool UpdateEntry(string indexName, string oldIndexKey, string newIndexKey)
    {
        if (!m_Entries.ContainsKey(indexName))
            return false;

        var entry = GetEntry(indexName, oldIndexKey);

        return entry is not null && RemoveEntry(indexName, oldIndexKey) && AddEntry(indexName, newIndexKey, entry.IndexValue);
    }

    public bool RemoveEntry(string indexName, string resource)
    {
        return m_Entries.TryGetValue(indexName, out var tree) && tree.TryRemove(new IndexDataSegmentEntry(m_IndexTable, resource)).Success;
    }

    internal IndexDataSegmentEntry? GetEntry(string indexName, string indexKey)
    {
        return m_Entries.TryGetValue(indexName, out var tree)
               ? tree.TryGet(other => new IndexDataSegmentEntry(m_IndexTable, indexKey).Medic(other))
                     .Value
               : null;
    }
}

public class IndexDataSegmentEntry(IndexTable indexTable, string indexKey = "", Guid indexValue = default) : IMarshal, IComparable<IndexDataSegmentEntry>
{
    private readonly IndexTable m_IndexTable = indexTable;

    public PersistentUtf8String IndexKey   { internal set; get; } = indexKey;
    public PersistentGuid       IndexValue { internal set; get; } = indexValue;

    public void Serialize(BinaryWriter writer)
    {
        IndexKey.Serialize(writer);
        IndexValue.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader)
    {
        IndexKey.Deserialize(reader);
        IndexValue.Deserialize(reader);
    }

    public void Calculate() { }

    public long Size()
    {
        return IndexKey.Size() + IndexValue.Size();
    }

    public int CompareTo(IndexDataSegmentEntry? other) => IndexKey.CompareTo(other!.IndexKey);
}

#endregion

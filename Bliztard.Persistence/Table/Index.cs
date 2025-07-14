using System.Collections.Concurrent;

using Bliztard.Application.Extension;
using Bliztard.Persistence.Extension;
using Bliztard.Persistence.Marshaling;
using Bliztard.Persistence.Table.Types;

using Towel.DataStructures;

namespace Bliztard.Persistence.Table;

#region Index Table | Structure

public class IndexTable : IMarshal
{
    public const string FileExtension = "index";

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

    public bool Clear()
    {
        return KeySegment.Clear() && DataSegment.Clear();
    }

    public string Extension => FileExtension;
}

internal class IndexHeaderSegment(IndexTable indexTable) : IMarshal
{
    public PersistentConstAsciiString Signature         => m_Signature;
    public PersistentInt8             Version           => m_Version;
    public PersistentInt64            KeySegmentOffset  => m_KeySegmentOffset;
    public PersistentInt64            DataSegmentOffset => m_DataSegmentOffset;

    private readonly PersistentConstAsciiString m_Signature = "BLIZTARDSSINDEX";
    private          PersistentInt8             m_Version;
    private          PersistentInt64            m_KeySegmentOffset;
    private          PersistentInt64            m_DataSegmentOffset;

    private readonly IndexTable m_IndexTable = indexTable;

    public void Serialize(BinaryWriter writer)
    {
        Calculate();

        m_Signature.Serialize(writer);
        m_Version.Serialize(writer);
        m_KeySegmentOffset.Serialize(writer);
        m_DataSegmentOffset.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader)
    {
        m_Signature.Deserialize(reader);
        m_Version.Deserialize(reader);
        m_KeySegmentOffset.Deserialize(reader);
        m_DataSegmentOffset.Deserialize(reader);
    }

    public void Calculate()
    {
        m_KeySegmentOffset  = m_IndexTable.HeaderSegment.Size();
        m_DataSegmentOffset = m_KeySegmentOffset + m_IndexTable.KeySegment.Size();
    }

    public long Size()
    {
        // TODO: maybe lock
        return m_Signature.Size() + m_Version.Size() + m_KeySegmentOffset.Size() + m_DataSegmentOffset.Size();
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

    public bool Clear()
    {
        m_Entries.Clear();
        m_IndexSet.Clear();

        return true;
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
        return AddEntry(indexName, new IndexDataSegmentEntry(m_IndexTable, indexKey, indexValue));
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
        if (!m_Entries.TryGetValue(indexName, out var tree))
            return false;

        // if removed - record's key segment offset should be represented as -1
        // tree.TryRemove(new IndexDataSegmentEntry(m_IndexTable, resource)).Success;

        return true;
    }

    internal IndexDataSegmentEntry? GetEntry(string indexName, string indexKey)
    {
        return m_Entries.TryGetValue(indexName, out var tree)
               ? tree.TryGet(other => new IndexDataSegmentEntry(m_IndexTable, indexKey).Medic(other))
                     .Value
               : null;
    }

    public bool Clear()
    {
        m_Entries.Clear();

        return true;
    }
}

public class IndexDataSegmentEntry(IndexTable indexTable, string indexKey = "", Guid indexValue = default) : IMarshal, IComparable<IndexDataSegmentEntry>
{
    private readonly IndexTable m_IndexTable = indexTable;

    private PersistentUtf8String m_IndexKey   = indexKey;
    private PersistentGuid       m_IndexValue = indexValue;

    public PersistentUtf8String IndexKey   => m_IndexKey;
    public PersistentGuid       IndexValue => m_IndexValue;

    public void Serialize(BinaryWriter writer)
    {
        m_IndexKey.Serialize(writer);
        m_IndexValue.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader)
    {
        m_IndexKey.Deserialize(reader);
        m_IndexValue.Deserialize(reader);
    }

    public void Calculate() { }

    public long Size()
    {
        return m_IndexKey.Size() + m_IndexValue.Size();
    }

    public int CompareTo(IndexDataSegmentEntry? other) => m_IndexKey.CompareTo(other!.m_IndexKey);
}

#endregion

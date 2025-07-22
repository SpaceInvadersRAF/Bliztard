using System.Diagnostics.CodeAnalysis;

using Bliztard.Application.Extension;
using Bliztard.Persistence.Marshaling;
using Bliztard.Persistence.Table.Types;

namespace Bliztard.Persistence.Table;

#region Record Table | Structure

public class RecordTable : IMarshal
{
    public const string FileExtension = "record";

    internal RecordHeaderSegment HeaderSegment { get; }
    public   RecordKeySegment    KeySegment    { get; }
    public   RecordDataSegment   DataSegment   { get; }

    public RecordTable()
    {
        HeaderSegment = new RecordHeaderSegment(this);
        KeySegment    = new RecordKeySegment(this);
        DataSegment   = new RecordDataSegment(this);
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

    public void Deserialize(BinaryReader reader, Guid recordName)
    {
        HeaderSegment.Deserialize(reader);
        KeySegment.Deserialize(reader);
        DataSegment.Deserialize(reader, recordName);
    }

    private bool AddEntry(Guid recordGuid, string content, bool @checked)
    {
        var entry = KeySegment.GetEntry(recordGuid);

        if (!@checked && entry is not null)
            return false;

        KeySegment.AddEntry(recordGuid);
        DataSegment.AddEntry(recordGuid, content);

        return true;
    }

    private bool UpdateEntry(Guid recordGuid, string content, RecordKeySegmentEntry? entry)
    {
        return entry != null && DataSegment.UpdateEntry(recordGuid, content);
    }

    public bool AddEntry(Guid recordGuid, string content)
    {
        if (!AddEntry(recordGuid, content, false))
            throw new InvalidDataException($"Add Entry | guid: {recordGuid} | context: {content}");

        return true;
    }

    public bool UpdateEntry(Guid recordGuid, string content)
    {
        var entry = KeySegment.GetEntry(recordGuid);

        if (!UpdateEntry(recordGuid, content, entry))
            throw new InvalidDataException($"Update Entry | guid: {recordGuid}");

        return true;
    }

    public bool AddOrUpdateEntry(Guid recordGuid, string content)
    {
        var entry = KeySegment.GetEntry(recordGuid);

        if (UpdateEntry(recordGuid, content, entry))
            return true;

        if (AddEntry(recordGuid, content, true))
            return true;

        // something else maybe
        return false;
    }

    public PersistentUtf8String? FindEntry(PersistentGuid resourceGuid)
    {
        var keyEntry = KeySegment.GetEntry(resourceGuid);

        if (keyEntry is null || keyEntry.RecordOffset == -1)
            return null;

        return DataSegment.GetEntry(resourceGuid)
                          ?.RecordData;
    }

    public bool TryFindEntry(PersistentGuid resourceGuid, [MaybeNullWhen(false)] out PersistentUtf8String data)
    {
        data = FindEntry(resourceGuid);

        return data is not null;
    }

    public bool RemoveEntry(Guid recordGuid)
    {
        if (!KeySegment.RemoveEntry(recordGuid))
            throw new InvalidDataException();

        return DataSegment.RemoveEntry(recordGuid);
    }

    public long Size()
    {
        return HeaderSegment.Size() + KeySegment.Size() + DataSegment.Size();
    }

    public string Extension => FileExtension;

    public bool Clear()
    {
        return KeySegment.Clear() && DataSegment.Clear();
    }
}

internal class RecordHeaderSegment(RecordTable recordTable) : IMarshal
{
    public PersistentConstAsciiString Signature         => m_Signature;
    public PersistentInt8             Version           => m_Version;
    public PersistentInt64            KeySegmentOffset  => m_KeySegmentOffset;
    public PersistentInt64            DataSegmentOffset => m_DataSegmentOffset;

    private readonly PersistentConstAsciiString m_Signature = "BLIZTARDRECORD";
    private          PersistentInt8             m_Version;
    private          PersistentInt64            m_KeySegmentOffset;
    private          PersistentInt64            m_DataSegmentOffset;

    private readonly RecordTable m_RecordTable = recordTable;

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
        Console.WriteLine($"{nameof(Signature)}: {m_Signature}");
        m_Version.Deserialize(reader);
        Console.WriteLine($"{nameof(Version)}: {m_Version}");
        m_KeySegmentOffset.Deserialize(reader);
        Console.WriteLine($"{nameof(KeySegmentOffset)}: {m_KeySegmentOffset}");
        m_DataSegmentOffset.Deserialize(reader);
        Console.WriteLine($"{nameof(DataSegmentOffset)}: {m_DataSegmentOffset}");
    }

    public void Calculate()
    {
        m_KeySegmentOffset  = m_RecordTable.HeaderSegment.Size();
        m_DataSegmentOffset = m_KeySegmentOffset + m_RecordTable.KeySegment.Size();
    }

    public long Size()
    {
        return m_Signature.Size() + m_Version.Size() + m_KeySegmentOffset.Size() + m_DataSegmentOffset.Size();
    }
}

public class RecordKeySegment(RecordTable recordTable) : IMarshal
{
    private readonly RecordTable                                   m_RecordTable = recordTable;
    private readonly SortedDictionary<Guid, RecordKeySegmentEntry> m_Entries     = new();
    private readonly ReaderWriterLockSlim                          m_Lock        = new(LockRecursionPolicy.SupportsRecursion);

    public void Serialize(BinaryWriter writer)
    {
        Calculate();

        foreach (var entry in m_Entries)
            entry.Value.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader)
    {
        while (reader.BaseStream.Position < m_RecordTable.HeaderSegment.DataSegmentOffset)
        {
            var entry = new RecordKeySegmentEntry(m_RecordTable);

            entry.Deserialize(reader);

            AddEntry(entry);
        }
    }

    public void AddEntry(Guid recordGuid)
    {
        AddEntry(new RecordKeySegmentEntry(m_RecordTable, recordGuid));
    }

    private void AddEntry(RecordKeySegmentEntry entry)
    {
        m_Lock.WriteBlock(() => m_Entries.Add(entry.RecordGuid, entry));
    }

    public bool RemoveEntry(Guid recordGuid)
    {
        if (!TryGetEntry(recordGuid, out var entry))
            return false;

        m_Lock.WriteBlock(() => entry.m_RecordOffset = -1);

        return true;
    }

    internal RecordKeySegmentEntry? GetEntry(Guid recordGuid)
    {
        return m_Lock.ReadBlock(() => m_Entries.GetValueOrDefault(recordGuid));
    }

    internal bool TryGetEntry(Guid recordGuid, [MaybeNullWhen(false)] out RecordKeySegmentEntry entry)
    {
        entry = GetEntry(recordGuid);

        return entry is not null;
    }

    public IList<RecordKeySegmentEntry> GetEntries()
    {
        return m_Lock.ReadBlock(() => m_Entries.Values.ToList());
    }

    public void Calculate()
    {
        var listEntries = m_Entries.ToArray();

        var currentEntry = listEntries[0];

        for (var index = 0; index < listEntries.Length; index++)
        {
            var previousEntry = currentEntry;

            currentEntry = listEntries[index];

            if (currentEntry.Value.RecordOffset == -1)
                continue;

            if (!m_RecordTable.DataSegment.TryGetEntry(previousEntry.Key, out var entry))
                throw new KeyNotFoundException($"Record with key {previousEntry.Key} does not exist.");

            currentEntry.Value.m_RecordOffset = index is 0 ? 0 : previousEntry.Value.RecordOffset + entry.Size();
        }
    }

    public long Size()
    {
        return m_Entries.Aggregate(0L, (current, entry) => current + entry.Value.Size());
    }

    public bool Clear()
    {
        m_Entries.Clear();

        return true;
    }
}

public class RecordKeySegmentEntry(RecordTable recordTable, Guid recordId = default) : IMarshal
{
    private readonly RecordTable m_RecordTable = recordTable;

    public PersistentGuid  RecordGuid   => m_RecordGuid;
    public PersistentInt64 RecordOffset => m_RecordOffset;

    private  PersistentGuid  m_RecordGuid = recordId;
    internal PersistentInt64 m_RecordOffset;

    public void Serialize(BinaryWriter writer)
    {
        m_RecordGuid.Serialize(writer);
        m_RecordOffset.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader)
    {
        m_RecordGuid.Deserialize(reader);
        Console.WriteLine($"Key | {nameof(RecordGuid)}: {m_RecordGuid}");
        m_RecordOffset.Deserialize(reader);
        Console.WriteLine($"Key | {nameof(RecordOffset)}: {m_RecordOffset}");
    }

    public long Size()
    {
        return m_RecordGuid.Size() + m_RecordOffset.Size();
    }
}

public class RecordDataSegment(RecordTable recordTable) : IMarshal
{
    private readonly RecordTable                              m_RecordTable = recordTable;
    private readonly Dictionary<Guid, RecordDataSegmentEntry> m_Entries     = new();
    private readonly ReaderWriterLockSlim                     m_Lock        = new(LockRecursionPolicy.SupportsRecursion);

    public void Serialize(BinaryWriter writer)
    {
        foreach (var keyEntry in m_RecordTable.KeySegment.GetEntries())
            if (keyEntry.RecordOffset != -1 && m_Entries.TryGetValue(keyEntry.RecordGuid, out var dataEntry))
                dataEntry.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader)
    {
        foreach (var keyEntry in m_RecordTable.KeySegment.GetEntries())
        {
            if (keyEntry.RecordOffset == -1)
                continue;

            var dataEntry = new RecordDataSegmentEntry(m_RecordTable);

            dataEntry.Deserialize(reader);

            AddEntry(keyEntry.RecordGuid, dataEntry);
        }
    }

    public void Deserialize(BinaryReader reader, Guid recordGuid)
    {
        var keyEntry = m_RecordTable.KeySegment.GetEntry(recordGuid);

        if (keyEntry is null || keyEntry.RecordOffset == -1)
            return;

        reader.BaseStream.Position = m_RecordTable.HeaderSegment.DataSegmentOffset + keyEntry.RecordOffset;

        var entry = new RecordDataSegmentEntry(m_RecordTable);

        entry.Deserialize(reader);

        AddEntry(recordGuid, entry);
    }

    public bool AddEntry(Guid recordGuid, string recordData)
    {
        return AddEntry(recordGuid, new RecordDataSegmentEntry(m_RecordTable, recordData));
    }

    private bool AddEntry(Guid recordGuid, RecordDataSegmentEntry entry)
    {
        return m_Lock.WriteBlock(() => m_Entries.TryAdd(recordGuid, entry));
    }

    public bool UpdateEntry(Guid recordGuid, string recordData)
    {
        var entry = GetEntry(recordGuid);

        if (entry == null)
            return false;

        entry.m_RecordData = recordData;

        return true;
    }

    public bool RemoveEntry(Guid recordGuid)
    {
        return m_Lock.WriteBlock(() => m_Entries.Remove(recordGuid));
    }

    public RecordDataSegmentEntry? GetEntry(Guid recordGuid)
    {
        return m_Lock.ReadBlock(() => m_Entries.GetValueOrDefault(recordGuid));
    }

    public bool TryGetEntry(Guid recordGuid, [MaybeNullWhen(false)] out RecordDataSegmentEntry dataEntry)
    {
        dataEntry = m_Lock.ReadBlock(() => m_Entries.GetValueOrDefault(recordGuid));

        return dataEntry is not null;
    }

    public IList<RecordDataSegmentEntry> GetEntries() // unused - yes it is
    {
        return m_Lock.ReadBlock(() => m_Entries.Values.ToList());
    }

    public long Size()
    {
        return m_Entries.Aggregate(0L, (current, entry) => current + entry.Value.Size());
    }

    public bool Clear()
    {
        m_Entries.Clear();

        return true;
    }
}

public class RecordDataSegmentEntry(RecordTable recordTable, string recordData = "") : IMarshal
{
    private readonly RecordTable m_RecordTable = recordTable;

    public PersistentUtf8String RecordData => m_RecordData;

    internal PersistentUtf8String m_RecordData = recordData;

    public void Serialize(BinaryWriter writer)
    {
        m_RecordData.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader)
    {
        m_RecordData.Deserialize(reader);
        Console.WriteLine($"Data | {nameof(RecordData)}: {m_RecordData.Size()}");
    }

    public long Size()
    {
        return m_RecordData.Size();
    }
}

#endregion

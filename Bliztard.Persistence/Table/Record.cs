using System.Diagnostics.CodeAnalysis;
using System.Drawing;

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
        {
            Console.WriteLine($"Entry | guid: {entry.RecordGuid} | ");
            return false;
        }
        
        KeySegment.AddEntry(recordGuid);
        DataSegment.AddEntry(recordGuid, content);

        return true;
    }

    private bool UpdateEntry(Guid recordGuid, string content, RecordKeySegmentEntry? entry)
    {
        return entry != null && DataSegment.UpdateEntry(recordGuid, content);;
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
        
        return DataSegment.GetEntry(resourceGuid)?.RecordData;
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
}

internal class RecordHeaderSegment(RecordTable recordTable) : IMarshal
{
    public readonly PersistentConstAsciiString Signature = "BLIZTARDRECORD";

    public   PersistentInt8  Version           {         set; get; }
    internal PersistentInt64 KeySegmentOffset  { private set; get; }
    internal PersistentInt64 DataSegmentOffset { private set; get; }
    
    private readonly RecordTable m_RecordTable = recordTable;

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
        KeySegmentOffset  = m_RecordTable.HeaderSegment.Size();
        DataSegmentOffset = KeySegmentOffset + m_RecordTable.KeySegment.Size();
    }

    public long Size()
    {
        return Signature.Size() + Version.Size() + KeySegmentOffset.Size() + DataSegmentOffset.Size();
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
        AddEntry(new RecordKeySegmentEntry(m_RecordTable) { RecordGuid = recordGuid });
    }

    private void AddEntry(RecordKeySegmentEntry entry)
    {
        m_Lock.WriteBlock(() => m_Entries.Add(entry.RecordGuid, entry));
    }

    public bool RemoveEntry(Guid recordGuid)
    {
        if (!TryGetEntry(recordGuid, out var entry))
            return false;
        
        m_Lock.WriteBlock(() => entry.RecordOffset = -1);
        
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

            currentEntry.Value.RecordOffset = index is 0 ? 0
                                                         : previousEntry.Value.RecordOffset + previousEntry.Value.Size();
        }
    }

    public long Size()
    {
        return m_Entries.Aggregate(0L, (current, entry) => current + entry.Value.Size());
    }
}

public class RecordKeySegmentEntry(RecordTable recordTable) : IMarshal
{
    private readonly RecordTable m_RecordTable = recordTable;
    
    public PersistentGuid  RecordGuid   { internal set; get; }
    public PersistentInt64 RecordOffset { internal set; get; }
    
    public void Serialize(BinaryWriter writer)
    {
        RecordGuid.Serialize(writer);
        RecordOffset.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader)
    {
        RecordGuid.Deserialize(reader);
        RecordOffset.Deserialize(reader);
    }
    
    public long Size()
    {
        return RecordGuid.Size() + RecordOffset.Size() + sizeof(long);
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
        return AddEntry(recordGuid, new RecordDataSegmentEntry(m_RecordTable) { RecordData = recordData});
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

        entry.RecordData = recordData;
        
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
    
    public IList<RecordDataSegmentEntry> GetEntries() // unused - yes it is
    {
        return m_Lock.ReadBlock(() => m_Entries.Values.ToList());
    } 
    
    public long Size()
    {
        return m_Entries.Aggregate(0L, (current, entry) => current + entry.Value.Size());
    }
}


public class RecordDataSegmentEntry(RecordTable recordTable) : IMarshal
{
    private readonly RecordTable m_RecordTable  = recordTable;  

    internal PersistentUtf8String RecordData { set; get; }

    public void Serialize(BinaryWriter writer)
    {
        RecordData.Serialize(writer);
    }
    
    public void Deserialize(BinaryReader reader)
    {
        RecordData.Deserialize(reader);
    }

    public long Size()
    {
        return RecordData.Size();
    }
}

#endregion

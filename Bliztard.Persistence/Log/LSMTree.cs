using System.Collections.Concurrent;

using Bliztard.Persistence.Configurations;
using Bliztard.Persistence.Marshaling;
using Bliztard.Persistence.Table;
using Bliztard.Persistence.Table.Types;

using Microsoft.Extensions.Logging;

namespace Bliztard.Persistence.Log;

public readonly struct LogAction : IMarshal
{
    public static readonly LogAction Create = new(1 << 0);
    public static readonly LogAction Update = new(1 << 1);
    public static readonly LogAction Delete = new(1 << 2);

    public readonly PersistentUInt8 Value;

    private LogAction(byte value) => Value = value;

    public void Serialize(BinaryWriter writer)
    {
        Value.Serialize(writer);
    }

    public void Deserialize(BinaryReader reader) { }

    public long Size()
    {
        return Value.Size();
    }
}

internal abstract class AbstractLogAction : IMarshal
{
    internal static AbstractLogAction FindLogAction(BinaryReader reader)
    {
        byte logActionByte = reader.ReadByte();

        return logActionByte switch
               {
                   _ when logActionByte == LogAction.Create.Value => new CreateLogAction(),
                   _ when logActionByte == LogAction.Update.Value => new UpdateLogAction(),
                   _ when logActionByte == LogAction.Delete.Value => new DeleteLogAction(),
                   _                                              => throw new ArgumentOutOfRangeException(nameof(logActionByte), logActionByte, "uwuexception while doing wild things")
               };
    }

    public abstract Guid Id();
    
    public abstract void Populate(WiwiwiTable table);

    public abstract void Serialize(BinaryWriter writer);

    public abstract void Deserialize(BinaryReader reader);

    public abstract long Size();
}

file class CreateLogAction(Guid guid = default, string resource = "", string content = "") : AbstractLogAction
{
    public static readonly LogAction Action = LogAction.Create;

    internal          PersistentGuid          Guid     = guid;
    internal readonly PersistentAsciiString   Resource = resource;
    internal readonly PersistentUtf8String Content  = content;

    public override Guid Id() => Guid;

    public override void Populate(WiwiwiTable table)
    {
        table.Add(Guid, "primary_index", Resource, Content);
    }

    public override void Serialize(BinaryWriter writer)
    {
        Action.Serialize(writer);
        Guid.Serialize(writer);
        Resource.Serialize(writer);
        Content.Serialize(writer);
    }

    public override void Deserialize(BinaryReader reader)
    {
        Action.Deserialize(reader);
        Guid.Deserialize(reader);
        Resource.Deserialize(reader);
        Content.Deserialize(reader);
    }

    public override long Size()
    {
        return Action.Size() + Resource.Size() + Content.Size();
    }

    public override string ToString()
    {
        return $"{nameof(LogAction.Create)} | Guid: `{Guid}` | Resource: `{Resource}` | Content: `{Content}`";
    }
}

file class RenameLogAction() : AbstractLogAction //todo: one day in the future
{
    public override Guid Id() => Guid.Empty;
  
    public override void Populate(WiwiwiTable table)
    {
        throw new NotImplementedException();
    }

    public override void Serialize(BinaryWriter writer)
    {
        throw new NotImplementedException();
    }

    public override void Deserialize(BinaryReader reader)
    {
        throw new NotImplementedException();
    }

    public override long Size()
    {
        throw new NotImplementedException();
    }
}

file class UpdateLogAction(Guid guid = default, string resource = "", string content = "") : AbstractLogAction
{
    public static readonly LogAction Action = LogAction.Update;

    internal          PersistentGuid          Guid     = guid;
    internal readonly PersistentAsciiString   Resource = resource;
    internal readonly PersistentUtf8String Content  = content;

    public override Guid Id() => Guid;
    
    public override void Populate(WiwiwiTable table)
    {
        table.Update(Guid, Content);
    }

    public override void Serialize(BinaryWriter writer)
    {
        Action.Serialize(writer);
        Guid.Serialize(writer);
        Resource.Serialize(writer);
        Content.Serialize(writer);
    }

    public override void Deserialize(BinaryReader reader)
    {
        Action.Deserialize(reader);
        Guid.Deserialize(reader);
        Resource.Deserialize(reader);
        Content.Deserialize(reader);
    }

    public override long Size()
    {
        return Action.Size() + Resource.Size() + Content.Size();
    }

    public override string ToString()
    {
        return $"{nameof(LogAction.Update)} | Guid: `{Guid}` | Resource: `{Resource}` | Content: `{Content}`";
    }
}

file class DeleteLogAction(Guid guid = default, string resource = "") : AbstractLogAction
{
    public static readonly LogAction Action = LogAction.Delete;

    internal          PersistentGuid        Guid     = guid;
    internal readonly PersistentAsciiString Resource = resource;

    public override Guid Id() => Guid;
    
    public override void Populate(WiwiwiTable table)
    {
        table.Remove(Guid, "primary_index", Resource);
    }

    public override void Serialize(BinaryWriter writer)
    {
        Action.Serialize(writer);
        Guid.Serialize(writer);
        Resource.Serialize(writer);
    }

    public override void Deserialize(BinaryReader reader)
    {
        Action.Deserialize(reader);
        Guid.Deserialize(reader);
        Resource.Deserialize(reader);
    }

    public override long Size()
    {
        return Action.Size() + Resource.Size();
    }

    public override string ToString()
    {
        return $"{nameof(LogAction.Delete)} | Guid: `{Guid}` | Resource: `{Resource}`";
    }
}

internal class LogEntry(AbstractLogAction logAction)
{
    public readonly AbstractLogAction          LogAction = logAction;
    public readonly TaskCompletionSource<bool> Source    = new();
}

public class LogTable(ILogger logger)
{
    private readonly ConcurrentQueue<LogEntry> m_LogQueue  = new();
    private readonly ManualResetEventSlim      m_LogSignal = new(false);
    public readonly  ILogger                   m_Logger    = logger;
    

    private bool m_IsActive = true;

    public static WiwiwiTable RaspyTable() //NOTE recovery - reproduce
    {
        var table = new WiwiwiTable();

        if (!File.Exists(Configuration.File.LogTablePath))
            return table;
        
        using var fileStream = new FileStream(Configuration.File.LogTablePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader     = new BinaryReader(fileStream);
        
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var logAction = AbstractLogAction.FindLogAction(reader);

            logAction.Deserialize(reader);
            logAction.Populate(table);
        }

        return table;
    }

    public void Start(CancellationToken cancellationToken)
    {
        using var fileStream = new FileStream(Configuration.File.LogTablePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var writer     = new BinaryWriter(fileStream);

        while (m_IsActive || !m_LogQueue.IsEmpty)
        {
            if (m_LogQueue.IsEmpty)
            {
                m_LogSignal.Wait(cancellationToken);
                m_LogSignal.Reset();
            }

            while (m_LogQueue.TryDequeue(out var entry))
            {
                try
                {
                    entry.LogAction.Serialize(writer);

                    writer.Flush();
                    
                    m_Logger.LogDebug("Log Table | Guid: {Guid} | Succeeded", entry.LogAction.Id());
                    
                    entry.Source.SetResult(true);
                }
                catch
                {
                    m_Logger.LogDebug("Log Table | Guid: {Guid} | Failed", entry.LogAction.Id());
                    
                    entry.Source.SetResult(false);
                }
            }
        }
    }

    public Task<bool> LogCreateAction(Guid guid, string resource, string content)
    {
        m_Logger.LogDebug("Log Table | Create | Guid: {Guid} | Resource: {Resource}", guid, resource);
        
        return AddEntry(new LogEntry(new CreateLogAction(guid, resource, content)));
    }

    public Task<bool> LogUpdateAction(Guid guid, string resource, string content)
    {
        m_Logger.LogDebug("Log Table | Update | Guid: {Guid} | Resource: {Resource}", guid, resource);
        
        return AddEntry(new LogEntry(new UpdateLogAction(guid, resource, content)));
    }

    //todo finish RenameLogAction implementation
    public Task<bool> LogRenameAction(Guid guid, string indexName)
    {
        return AddEntry(new LogEntry(new RenameLogAction()));
    }

    public Task<bool> LogDeleteAction(Guid guid, string resource)
    {
        m_Logger.LogDebug("Log Table | Delete | Guid: {Guid} | Resource: {Resource}", guid, resource);
        
        return AddEntry(new LogEntry(new DeleteLogAction(guid, resource)));
    }

    private Task<bool> AddEntry(LogEntry entry)
    {
        m_LogQueue.Enqueue(entry);
        m_LogSignal.Set();

        return entry.Source.Task;
    }

    public void Shutdown()
    {
        m_IsActive = false;
        m_LogSignal.Set();
    }
}

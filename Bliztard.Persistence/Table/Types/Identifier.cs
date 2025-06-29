using Bliztard.Persistence.Marshaling;

namespace Bliztard.Persistence.Table.Types;

public record struct PersistentGuid(Guid value = default) : IMarshal
{
    public readonly void Serialize(BinaryWriter writer) => writer.Write(value.ToByteArray());

    public void Deserialize(BinaryReader reader) => this = new Guid(reader.ReadBytes((int)Size()));

    public readonly long Size() => 16;
    
    public static implicit operator PersistentGuid(Guid value) => new(value);

    public static implicit operator Guid(PersistentGuid persistentGuid) => persistentGuid.value;
}

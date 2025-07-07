using System.Text;
using Bliztard.Persistence.Marshaling;

namespace Bliztard.Persistence.Table.Types;

#region Integer Types

public record struct PersistentInt8(sbyte value) : IMarshal, IComparable<PersistentInt8>
{
    public readonly void Serialize(BinaryWriter writer) => writer.Write(value);

    public void Deserialize(BinaryReader reader) => this = reader.ReadSByte();

    public readonly long Size() => sizeof(sbyte);

    public static implicit operator PersistentInt8(sbyte value) => new(value);

    public static implicit operator sbyte(PersistentInt8 persistentInt) => persistentInt.value;
    
    public override string ToString() => value.ToString();

    public int CompareTo(PersistentInt8 other) => value.CompareTo(other.value);
}

public record struct PersistentInt16(short value) : IMarshal, IComparable<PersistentInt16>
{
    public readonly void Serialize(BinaryWriter writer) => writer.Write(value);

    public void Deserialize(BinaryReader reader) => this = reader.ReadInt16();

    public readonly long Size() => sizeof(short);

    public static implicit operator PersistentInt16(short value) => new(value);
 
    public static implicit operator short(PersistentInt16 persistentInt) => persistentInt.value;
    
    public override string ToString() => value.ToString();

    public int CompareTo(PersistentInt16 other) => value.CompareTo(other.value);
}

public record struct PersistentInt32(int value) : IMarshal, IComparable<PersistentInt32>
{
    public readonly void Serialize(BinaryWriter writer) => writer.Write(value);

    public void Deserialize(BinaryReader reader) => this = reader.ReadInt32();

    public readonly long Size() => sizeof(int);

    public static implicit operator PersistentInt32(int value) => new(value);
 
    public static implicit operator int(PersistentInt32 persistentInt) => persistentInt.value;
    
    public override string ToString() => value.ToString();

    public int CompareTo(PersistentInt32 other) => value.CompareTo(other.value);
}

public record struct PersistentInt64(long value) : IMarshal, IComparable<PersistentInt64>
{
    public readonly void Serialize(BinaryWriter writer) => writer.Write(value);

    public void Deserialize(BinaryReader reader) => this = reader.ReadInt64();

    public readonly long Size() => sizeof(long);

    public static implicit operator PersistentInt64(long value) => new(value);
 
    public static implicit operator long(PersistentInt64 persistentInt) => persistentInt.value;
    
    public override string ToString() => value.ToString();

    public int CompareTo(PersistentInt64 other) => value.CompareTo(other.value);
}

public record struct PersistentUInt8(byte value) : IMarshal, IComparable<PersistentUInt8>
{
    public readonly void Serialize(BinaryWriter writer) => writer.Write(value);

    public void Deserialize(BinaryReader reader) => this = reader.ReadByte();

    public readonly long Size() => sizeof(byte);

    public static implicit operator PersistentUInt8(byte value) => new(value);
 
    public static implicit operator byte(PersistentUInt8 persistentInt) => persistentInt.value;
    
    public override string ToString() => value.ToString();

    public int CompareTo(PersistentUInt8 other) => value.CompareTo(other.value);
}

public record struct PersistentUInt16(ushort value) : IMarshal, IComparable<PersistentUInt16>
{
    public readonly void Serialize(BinaryWriter writer) => writer.Write(value);

    public void Deserialize(BinaryReader reader) => this = reader.ReadUInt16();

    public readonly long Size() => sizeof(ushort);

    public static implicit operator PersistentUInt16(ushort value) => new(value);
 
    public static implicit operator ushort(PersistentUInt16 persistentInt) => persistentInt.value;
    
    public override string ToString() => value.ToString();

    public int CompareTo(PersistentUInt16 other) => value.CompareTo(other.value);
}

public record struct PersistentUInt32(uint value) : IMarshal, IComparable<PersistentUInt32>
{
    public readonly void Serialize(BinaryWriter writer) => writer.Write(value);

    public void Deserialize(BinaryReader reader) => this = reader.ReadUInt32();

    public readonly long Size() => sizeof(uint);

    public static implicit operator PersistentUInt32(uint value) => new(value);
 
    public static implicit operator uint(PersistentUInt32 persistentInt) => persistentInt.value;
    
    public override string ToString() => value.ToString();

    public int CompareTo(PersistentUInt32 other) => value.CompareTo(other.value);
}

public record struct PersistentUInt64(ulong value) : IMarshal, IComparable<PersistentUInt64>
{
    public readonly void Serialize(BinaryWriter writer) => writer.Write(value);

    public void Deserialize(BinaryReader reader) => this = reader.ReadUInt64();

    public readonly long Size() => sizeof(ulong);

    public static implicit operator PersistentUInt64(ulong value) => new(value);
 
    public static implicit operator ulong(PersistentUInt64 persistentInt) => persistentInt.value;
    
    public override string ToString() => value.ToString();

    public int CompareTo(PersistentUInt64 other) => value.CompareTo(other.value);
}

#endregion Integer Types

#region Floating-Point Types

public record struct PersistentFloat(float value) : IMarshal, IComparable<PersistentFloat>
{
    public readonly void Serialize(BinaryWriter writer) => writer.Write(value);

    public void Deserialize(BinaryReader reader) => value = reader.ReadSingle();

    public readonly long Size() => sizeof(float);

    public static implicit operator PersistentFloat(float value) => new(value);
 
    public static implicit operator float(PersistentFloat persistentInt) => persistentInt.value;

    public int CompareTo(PersistentFloat other) => value.CompareTo(other.value);
}

public record struct PersistentDouble(double value) : IMarshal, IComparable<PersistentDouble>
{
    public readonly void Serialize(BinaryWriter writer) => writer.Write(value);

    public void Deserialize(BinaryReader reader) => value = reader.ReadDouble();

    public readonly long Size() => sizeof(double);

    public static implicit operator PersistentDouble(double value) => new(value);
 
    public static implicit operator double(PersistentDouble persistentInt) => persistentInt.value;

    public int CompareTo(PersistentDouble other) => value.CompareTo(other.value);
}

public record struct PersistentDecimal(decimal value) : IMarshal, IComparable<PersistentDecimal>
{
    public readonly void Serialize(BinaryWriter writer) => writer.Write(value);
    
    public void Deserialize(BinaryReader reader) => value = reader.ReadDecimal();

    public readonly long Size() => sizeof(decimal);

    public static implicit operator PersistentDecimal(decimal value) => new(value);
    
    public static implicit operator decimal(PersistentDecimal persistentInt) => persistentInt.value;
    
    public int CompareTo(PersistentDecimal other) => value.CompareTo(other.value);
}

#endregion

#region Boolean Types

public record struct PersistentBoolean(bool value) : IMarshal, IComparable<PersistentBoolean>
{
    public readonly void Serialize(BinaryWriter writer) => writer.Write(value);

    public void Deserialize(BinaryReader reader) => value = reader.ReadBoolean();

    public readonly long Size() => sizeof(decimal);

    public static implicit operator PersistentBoolean(bool value) => new(value);
 
    public static implicit operator bool(PersistentBoolean persistentInt) => persistentInt.value;
    
    public int CompareTo(PersistentBoolean other) => value.CompareTo(other.value);
}

#endregion

#region String Types

public class PersistentAsciiString(string value = "") : IMarshal, IComparable<PersistentAsciiString>
{
    private string m_Value = value;
    
    public void Serialize(BinaryWriter writer)
    {
        writer.Write(StringSize());
        writer.Write(Encoding.ASCII.GetBytes(m_Value));
    }

    public void Deserialize(BinaryReader reader)
    {
        var size = reader.ReadInt32();
        
        m_Value = Encoding.ASCII.GetString(reader.ReadBytes(size));
    }

    private int StringSize() => m_Value.Length;
    
    public long Size() => sizeof(int) + StringSize();

    public static implicit operator PersistentAsciiString(string value) => new(value);
 
    public static implicit operator string(PersistentAsciiString persistentInt) => persistentInt.m_Value;

    public int CompareTo(PersistentAsciiString? other) => string.Compare(m_Value, other!.m_Value, StringComparison.Ordinal);

    public override string ToString() => m_Value;
}

public class PersistentUtf8String(string value = "") : IMarshal, IComparable<PersistentUtf8String>
{
    private readonly Encoding m_Encoding = Encoding.UTF8;
    
    private string m_Value = value;

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(StringSize());
        writer.Write(m_Encoding.GetBytes(m_Value));
    }

    public void Deserialize(BinaryReader reader)
    {
        var size = reader.ReadInt32();
        
        m_Value = m_Encoding.GetString(reader.ReadBytes(size));
    }

    private int StringSize() => 2 * m_Value.Length;
    
    public long Size() => sizeof(int) + StringSize();

    public static implicit operator PersistentUtf8String(string value) => new(value);
 
    public static implicit operator string(PersistentUtf8String persistentInt) => persistentInt.m_Value;

    public override string ToString() => m_Value;

    public int CompareTo(PersistentUtf8String? other) => string.Compare(m_Value, other!.m_Value, StringComparison.Ordinal);
}

public class PersistentConstAsciiString(string value) : IMarshal, IComparable<PersistentConstAsciiString>
{
    private readonly Encoding m_Encoding = Encoding.ASCII;
    
    private readonly string m_Value = value;

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(m_Encoding.GetBytes(m_Value));
    }

    public void Deserialize(BinaryReader reader)
    {
        var serialized = m_Encoding.GetString(reader.ReadBytes((int)Size()));

        if (serialized != m_Value)
            throw new InvalidDataException("smt");
    }

    public long Size() => m_Value.Length;
 
    public static implicit operator PersistentConstAsciiString(string value) => new(value);
    
    public static implicit operator string(PersistentConstAsciiString persistentInt) => persistentInt.m_Value;
    
    public override string ToString() => m_Value;

    public int CompareTo(PersistentConstAsciiString? other) => string.Compare(m_Value, other!.m_Value, StringComparison.Ordinal);
}

public class PersistentConstUnicodeString(string value) : IMarshal, IComparable<PersistentConstUnicodeString>
{
    private readonly Encoding m_Encoding = Encoding.Unicode;
    
    private readonly string m_Value = value;

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(m_Encoding.GetBytes(m_Value));
    }

    public void Deserialize(BinaryReader reader)
    {
        var serialized = m_Encoding.GetString(reader.ReadBytes((int)Size()));
        
        if (serialized != m_Value)
            throw new InvalidDataException("smt");
    }

    public long Size() => 2 * m_Value.Length;

    public static implicit operator PersistentConstUnicodeString(string value) => new(value);
 
    public static implicit operator string(PersistentConstUnicodeString persistentInt) => persistentInt.m_Value;
    
    public override string ToString() => m_Value;

    public int CompareTo(PersistentConstUnicodeString? other) => string.Compare(m_Value, other!.m_Value, StringComparison.Ordinal);
}

#endregion

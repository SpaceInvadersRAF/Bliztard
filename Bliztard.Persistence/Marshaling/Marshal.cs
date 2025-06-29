namespace Bliztard.Persistence.Marshaling;

public interface ISerialize
{
    void Serialize(BinaryWriter writer);
}

public interface IDeserialize
{
    void Deserialize(BinaryReader reader);
}

public interface IMarshal : ISerialize, IDeserialize
{
    long Size();
}

public interface ICalculable
{
    void Calculate();
}

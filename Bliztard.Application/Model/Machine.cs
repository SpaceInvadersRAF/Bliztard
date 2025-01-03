namespace Bliztard.Application.Model;

public class MachineInfo
{
    public Guid            Id       { init; get; } = Guid.NewGuid();
    public MachineType     Type     { init; get; }
    public MachineResource Resource { init; get; } = new();
    public bool            Alive    { set;  get; } = true;
}

public readonly struct MachineType(int value)
{
    public static readonly MachineType Master = new(0);
    public static readonly MachineType Slave  = new(1);
    
    public int Value { get; } = value;

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator MachineType(int value)
    {
        if(!s_MachineDictionary.TryGetValue(value, out var machineType))
            throw new InvalidCastException($"Cannot cast value: '{value}' into MachineType.");
        
        return machineType;
    }

    public static implicit operator int(MachineType machineType)
    {
        return machineType.Value;
    }
    
    private static readonly Dictionary<int, MachineType> s_MachineDictionary = new();

    static MachineType()
    {
        AddMachineType(Master);
        AddMachineType(Slave);
    }

    private static void AddMachineType(MachineType machineType)
    {
        if (!s_MachineDictionary.TryAdd(machineType.Value, machineType))
            throw new InvalidDataException("Duplicate MachineType value.");
    }
}

public class MachineResource
{
    public string BaseUrl { set; get; } = "";
}

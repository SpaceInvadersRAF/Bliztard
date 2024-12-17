using System.Collections.Concurrent;

namespace Bliztard.Application.Model;

public class MachineInfo
{
    public Guid            Id       { init; get; } 
    public MachineType     Type     { init; get; }
    public MachineResource Resource { init; get; } = new();
    public bool            Alive    { set;  get; }
}

public enum MachineType
{
    Master,
    Slave
}

public class MachineResource
{
    public string BaseUrl { set; get; } = "";
}

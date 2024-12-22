namespace Bliztard.Contract.Request;

public readonly struct MachineInfoRequest
{
    public Guid   Id      { init; get; } 
    public int    Type    { init; get; }
    public string BaseUrl { init; get; }
    public bool   Alive   { init; get; }
}

namespace Bliztard.Contract.Request;

public struct MachineInfoRequest
{
    public Guid   Id      { init; get; } 
    public int    Type    { init; get; }
    public string BaseUrl { init; get; }
    public bool   Alive   { init; get; }
}

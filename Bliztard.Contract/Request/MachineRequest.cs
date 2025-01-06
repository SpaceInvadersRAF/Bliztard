namespace Bliztard.Contract.Request;

public readonly struct MachineInfoRequest
{
    public Guid   Id      { init; get; } 
    public int    Type    { init; get; }
    public string BaseUrl { init; get; }
    public bool   Alive   { init; get; }
}

public readonly struct UploadLocationsRequest
{
    public string             FilePath    { init; get; }
    public string             Username    { init; get; } 
    public long               Length      { init; get; }
    public string             Resource    => $"{Username}/{FilePath}";
}

namespace Bliztard.Contract.Response;

public readonly struct MachineInfoResponse
{
    public Guid   Id      { init; get; } 
    public int    Type    { init; get; }
    public string BaseUrl { init; get; }
    public bool   Alive   { init; get; }
}

public readonly struct UploadLocationsResponse
{
    public IEnumerable<MachineInfoResponse> MachineInfos { init; get; }
}

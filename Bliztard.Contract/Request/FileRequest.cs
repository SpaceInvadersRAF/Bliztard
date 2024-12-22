namespace Bliztard.Contract.Request;

public readonly struct NotifySaveRequest 
{
    public MachineInfoRequest MachineInfo { init; get; }
    public Guid               PathId      { init; get; }
    public string             FilePath    { init; get; }
    public string             Username    { init; get; }
    public int                Replication { init; get; }
    public string             Resource    => $"{Username}/{FilePath}";
    public string             Location    => $"{MachineInfo.BaseUrl}/{Resource}";
}


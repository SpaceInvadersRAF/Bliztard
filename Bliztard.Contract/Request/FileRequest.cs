using System.Net.Mime;

namespace Bliztard.Contract.Request;

public readonly struct NotifySaveRequest
{
    public MachineInfoRequest MachineInfo { init; get; }
    public Guid               PathId      { init; get; }
    public string             FilePath    { init; get; }
    public string             Username    { init; get; }
    public int                Replication { init; get; }

    public string Resource => $"{Username}/{FilePath}";
    public string Location => $"{MachineInfo.BaseUrl}/{Resource}";
}

public class NotifyLogContentRequest
{
    public MachineInfoRequest      MachineInfo     { init; get; }
    public List<NotifySaveRequest> SaveFileRequest { init; get; } = [];
}

public readonly struct NotifyDeleteRequest(Guid pathId, string resource)
{
    public Guid   PathId   { init; get; } = pathId;
    public string Resource { init; get; } = resource;
}

public readonly struct TwincateFileRequest
{
    public MachineInfoRequest MachineInfo { init; get; }
    public string             FilePath    { init; get; }
    public string             Username    { init; get; }
    public string             Resource    => $"{Username}/{FilePath}";
    public ContentType        ContentType { init; get; }

    public string FileName => Path.GetFileName(FilePath);
}

using Bliztard.Application.Configuration;
using Bliztard.Application.Extension;

namespace Bliztard.Application.Model;

public class SaveFileInfo 
{
    public MachineInfo MachineInfo { init; get; }
    public Guid        PathId      { init; get; }
    public string      FilePath    { init; get; }
    public string      FileName    { init; get; }
    public long        Length      { init; get; }
    public string      ContentType { init; get; }
    public string      Username    { init; get; }
    public int         Replication { init; get; }
    public string      Resource => $"{Username}/{FilePath}";
    public string      Location => $"{MachineInfo.Resource.BaseUrl}/{Resource}";

    public SaveFileInfo(MachineInfo machineInfo, Guid pathId, IDictionary<string, string> formData, string contentType, long length)
    {
        PathId      = pathId;
        MachineInfo = machineInfo;
        FilePath    = formData.TryGetString("path");
        FileName    = Path.GetFileName(FilePath);
        Length      = length;
        ContentType = contentType;
        Username    = formData.TryGetString("username");
        Replication = int.TryParse(formData.TryGetString("replications"), out var replicationFactor) ? replicationFactor : Configurations.Core.ReplicationFactor;
    }
}

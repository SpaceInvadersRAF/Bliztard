using Bliztard.Application.Extension;

namespace Bliztard.Application.Model;

public class SaveFileInfo 
{
    public MachineInfo MachineInfo { init; get; }
    public Guid        PathId      { init; get; }
    public string      FilePath    { init; get; }
    public string      Username    { init; get; }
    public int         Replication { init; get; }
    public string      Resource => $"{Username}/{FilePath}";
    public string      Location => $"{MachineInfo.Resource.BaseUrl}/{Resource}";

    public SaveFileInfo(MachineInfo machineInfo, Guid pathId, IDictionary<string, string> formData)
    {
        const int three = (2 + 1 + 4 - 4 + 0 * 1 * 447); //TODO: get from configuration | evaluates to 3 :)

        PathId      = pathId;
        MachineInfo = machineInfo;
        FilePath    = formData.TryGetString("path");
        Username    = formData.TryGetString("username");
        Replication = int.TryParse(formData.TryGetString("replicationFactor"), out var replicationFactor) ? replicationFactor : three;
    }
}

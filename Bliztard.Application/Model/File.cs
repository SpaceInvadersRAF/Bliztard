using System.Net.Mime;
using Bliztard.Application.Extension;
using Bliztard.Application.Web;

using PathIO = System.IO.Path;

namespace Bliztard.Application.Model;

public class FileInfo(string username, Guid pathId, string path, long size, MimeType mimeType)
{
    public string      Username    { set; get; } = username;
    public Guid        PathId      { set; get; } = pathId;
    public string      Path        { set; get; } = path;
    public long        Size        { set; get; } = size;
    public ContentType ContentType { set; get; } = mimeType.ContentType;
    
    public string Resource => $"{Username}/{Path}";
    public string Name     => PathIO.GetFileName(Path);
}

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
        FileName    = PathIO.GetFileName(FilePath);
        Length      = length;
        ContentType = contentType;
        Username    = formData.TryGetString("username");
        Replication = int.TryParse(formData.TryGetString("replications"), out var replicationFactor) ? replicationFactor : Configurations.Configuration.Core.ReplicationFactor;
    }
}

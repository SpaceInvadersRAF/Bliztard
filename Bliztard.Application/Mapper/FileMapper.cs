using Bliztard.Application.Model;
using Bliztard.Application.Web;
using Bliztard.Contract.Request;
using FileInfo = Bliztard.Application.Model.FileInfo;

namespace Bliztard.Application.Mapper;

public static class SaveFileInfoMapper
{
    public static NotifySaveRequest ToRequest(this SaveFileInfo saveFileInfo)
    {
        return new NotifySaveRequest
               {
                   MachineInfo = saveFileInfo.MachineInfo.ToRequest(),
                   PathId      = saveFileInfo.PathId,
                   FilePath    = saveFileInfo.FilePath,
                   Username    = saveFileInfo.Username,
                   Replication = saveFileInfo.Replication
               };
    }
}

public static class FileInfoMapper
{
    public static UploadLocationsRequest ToUploadLocationsRequest(this FileInfo fileInfo)
    {
        return new UploadLocationsRequest
               {
                   FilePath = fileInfo.Path,
                   Username = fileInfo.Username,
                   Size   = fileInfo.Size,
               };
    }

    public static TwincateFileRequest ToTwincateFileRequest(this FileInfo fileInfo, MachineInfo machineInfo)
    {
        return new TwincateFileRequest
               {
                   FilePath = fileInfo.Path,
                   Username = fileInfo.Username,
                   MachineInfo = machineInfo.ToRequest(),
                   ContentType = fileInfo.ContentType
               };
    }
}

public static class NotifySaveRequestMapper
{
    public static FileInfo ToFileInfo(this NotifySaveRequest notifySaveRequest) // todo: MimeType
    {
        return new FileInfo(notifySaveRequest.Username,
                            notifySaveRequest.PathId,
                            notifySaveRequest.FilePath, 
                            0,
                            MimeType.Any
                            );
    }
}

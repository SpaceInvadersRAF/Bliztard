using Bliztard.Application.Model;
using Bliztard.Contract.Request;

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

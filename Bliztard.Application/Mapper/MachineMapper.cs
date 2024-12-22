using Bliztard.Application.Model;
using Bliztard.Contract.Request;

namespace Bliztard.Application.Mapper;

public static class MachineInfoMapper
{
    public static MachineInfoRequest ToRequest(this MachineInfo machineInfo)
    {
        return new MachineInfoRequest
               {
                   Id      = machineInfo.Id,
                   Type    = machineInfo.Type,
                   BaseUrl = machineInfo.Resource.BaseUrl,
                   Alive   = machineInfo.Alive
               };
    }
}

public static class MachineInfoRequestMapper
{
    public static MachineInfo ToModel(this MachineInfoRequest machineInfoRequest)
    {
        return new MachineInfo
               {
                   Id       = machineInfoRequest.Id,
                   Type     = machineInfoRequest.Type,
                   Resource = new MachineResource { BaseUrl = machineInfoRequest.BaseUrl },
                   Alive    = machineInfoRequest.Alive
               };
    }
}

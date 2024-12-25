using Bliztard.Application.Model;
using Bliztard.Contract.Request;
using Bliztard.Contract.Response;

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
    
    public static MachineInfoResponse ToResponse(this MachineInfo machineInfo)
    {
        return new MachineInfoResponse
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
                   Resource = { BaseUrl = machineInfoRequest.BaseUrl },
                   Alive    = machineInfoRequest.Alive
               };
    }
}

public static class EnumerableMapper
{
    public static IEnumerable<MachineInfoResponse> ToResponse(this IEnumerable<MachineInfo> enumerable)
    {
        return enumerable.Select(machineInfo => machineInfo.ToResponse());
    }
}

using Bliztard.Application.Model;
using Bliztard.Application.Repository.Machine;
using Bliztard.Application.Service.Machine;
using Microsoft.Extensions.DependencyInjection;

namespace Bliztard.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBliztardApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMachineRepository, MachineRepository>();
        services.AddSingleton<IMachineService, MachineService>();

        return services;
    }

    public static IServiceCollection AddMachine(this IServiceCollection services, MachineType type)
    {
        services.AddSingleton<MachineInfo>(_ => new MachineInfo()
                                                {
                                                    Id    = Guid.NewGuid(),
                                                    Type  = type,
                                                    Alive = true,
                                                });
        
        return services;
    }
}

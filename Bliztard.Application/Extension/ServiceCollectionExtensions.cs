using Bliztard.Application.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Bliztard.Application.Extension;

public static class ServiceCollectionExtensions
{
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

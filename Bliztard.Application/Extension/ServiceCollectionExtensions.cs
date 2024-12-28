﻿using Bliztard.Application.Core;
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
    
    public static IServiceCollection AddSingletonWithLifecycle<TService, TImplementation>(this IServiceCollection services) where TService : class, ILifecycle 
                                                                                                                            where TImplementation : class, TService
    {
        services.AddSingleton<TService, TImplementation>();
        services.AddSingleton<ILifecycle>(provider => provider.GetRequiredService<TService>());
        
        return services;
    }
}

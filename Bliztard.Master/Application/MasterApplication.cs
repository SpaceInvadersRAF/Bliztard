using Bliztard.Application.Extension;
using Bliztard.Application.Model;
using Bliztard.Master.Repository.File;
using Bliztard.Master.Repository.Machine;
using Bliztard.Master.Service;
using Bliztard.Master.Service.File;
using Bliztard.Master.Service.Machine;

namespace Bliztard.Master.Application;

public class MasterApplication
{
    public static void Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
                                         {
                                             options.IncludeScopes   = false;
                                             options.SingleLine      = true;
                                             options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
                                         });
        
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddBliztardApplication();
        builder.Services.AddControllers();

        var app = builder.Build();

        app.UseAuthorization();
        
        app.MapControllers();

        app.Start();

        app.WaitForShutdown();
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBliztardApplication(this IServiceCollection services)
    {
        services.AddMachine(MachineType.Master, out _);

        services.AddHostedService<ApplicationHostedService>();
        
        services.AddSingleton<ApplicationServiceLifecycle>();

        services.AddSingletonWithLifecycle<IFileRepository,    InMemoryFileRepository>();
        services.AddSingletonWithLifecycle<IFileService,       InMemoryFileService>();
        services.AddSingletonWithLifecycle<IMachineRepository, MachineRepository>();
        services.AddSingletonWithLifecycle<IMachineService,    MachineService>();
        
        return services;
    }
}

using Bliztard.Application.Configurations;
using Bliztard.Application.Extension;
using Bliztard.Application.Model;
using Bliztard.Master.Repository.File;
using Bliztard.Master.Repository.Machine;
using Bliztard.Master.Service;
using Bliztard.Master.Service.File;
using Bliztard.Master.Service.Machine;
using Bliztard.Master.Service.MachineFile;
using Bliztard.Master.Service.Network;
using Serilog;
using Serilog.Events;

namespace Bliztard.Master.Application;

public class MasterApplication
{
    public static void Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Console.WriteLine(Configuration.Core.MasterBaseUrl);
        Console.WriteLine(Configuration.Core.MachinePublicUrl);
        
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
                                         {
                                             options.IncludeScopes   = false;
                                             options.SingleLine      = true;
                                             options.TimestampFormat = $"{Configuration.Log.TimestampFormat} ";
                                         });
        builder.Logging.AddSerilog(new LoggerConfiguration().MinimumLevel.Warning()
                                                            .MinimumLevel.Override(typeof(Program).Namespace!, LogEventLevel.Debug)
                                                            .WriteTo.File(path: Configuration.Log.FilePath,
                                                                          rollingInterval: RollingInterval.Day,
                                                                          outputTemplate: Configuration.Log.Serilog.OutputTemplate,
                                                                          shared: Configuration.Log.Shared)
                                                            .CreateLogger());
        
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

        services.AddSingletonWithLifecycle<IFileRepository,     InMemoryFileRepository>();
        services.AddSingletonWithLifecycle<IFileService,        InMemoryFileService>();
        services.AddSingletonWithLifecycle<IMachineRepository,  MachineRepository>();
        services.AddSingletonWithLifecycle<IMachineService,     MachineService>();
        services.AddSingletonWithLifecycle<IMachineFileService, MachineFileService>();
        services.AddSingletonWithLifecycle<INetworkService,     NetworkService>();
        
        return services;
    }
}

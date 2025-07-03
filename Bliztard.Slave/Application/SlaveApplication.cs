using Bliztard.Application.Configurations;
using Bliztard.Application.Extension;
using Bliztard.Application.Model;
using Bliztard.Persistence.Log;
using Bliztard.Slave.BackgroundService;
using Bliztard.Slave.Repository.File;
using Bliztard.Slave.Service;
using Bliztard.Slave.Service.File;
using Bliztard.Slave.Service.Machine;
using Bliztard.Slave.Service.Network;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using Serilog.Events;

namespace Bliztard.Slave.Application;

public class SlaveApplication
{
    public static void Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        Console.WriteLine(Configuration.Core.MasterBaseUrl);
        
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
        
        builder.WebHost.ConfigureKestrel((_, options) =>
                                         {
                                             options.Limits.MaxRequestBodySize = 20_000_000_000;
                                         });

        builder.Services.Configure<FormOptions>(options =>
                                                {
                                                    options.MultipartBodyLengthLimit     = 20_000_000_000;
                                                    options.MultipartBoundaryLengthLimit = 2_000_000_000; 
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
        services.AddMachine(MachineType.Slave, out var machineInfo);

        services.AddHostedService<ApplicationHostedService>();

        services.AddSingletonWithLifecycle<IFileRepository, PersistantFileRepository>();
        services.AddSingletonWithLifecycle<IFileService,    PersistantFileService>();
        services.AddSingletonWithLifecycle<IMachineService, MachineService>();
        services.AddSingletonWithLifecycle<INetworkService, NetworkService>();

        services.AddSingleton<ApplicationServiceLifecycle>();
        services.AddSingleton<WiwiwiBackgroundService>();

        services.AddHttpClient(Configuration.HttpClient.FileNotifyUpload,
                               client =>
                               {
                                   client.DefaultRequestHeaders.UserAgent.ParseAdd($"Bliztard/{machineInfo.Type} ({machineInfo.Id})");
                                   client.BaseAddress = new Uri(Configuration.Core.MasterBaseUrl);
                               });
        services.AddHttpClient(Configuration.HttpClient.FileTwincateData,
                               client =>
                               {
                                   client.DefaultRequestHeaders.UserAgent.ParseAdd($"Bliztard/{machineInfo.Type} ({machineInfo.Id})");
                                   client.BaseAddress = new Uri(Configuration.Core.MasterBaseUrl);
                               });
        services.AddHttpClient(Configuration.HttpClient.MachineNotifyMaster,
                               client =>
                               {
                                   client.DefaultRequestHeaders.UserAgent.ParseAdd($"Bliztard/{machineInfo.Type} ({machineInfo.Id})");
                                   client.BaseAddress = new Uri(Configuration.Core.MasterBaseUrl);
                               });
        services.AddHttpClient(Configuration.HttpClient.MachineSendUroshbeat,
                               client =>
                               {
                                   client.DefaultRequestHeaders.UserAgent.ParseAdd($"Bliztard/{machineInfo.Type} ({machineInfo.Id})");
                                   client.BaseAddress = new Uri(Configuration.Core.MasterBaseUrl);
                               });
        
        return services;
    }
}

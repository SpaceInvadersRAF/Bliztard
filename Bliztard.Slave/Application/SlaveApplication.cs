using Bliztard.Application.Configurations;
using Bliztard.Application.Extension;
using Bliztard.Application.Model;
using Bliztard.Slave.Repository.File;
using Bliztard.Slave.Service;
using Bliztard.Slave.Service.File;
using Bliztard.Slave.Service.Machine;
using Microsoft.AspNetCore.Http.Features;

namespace Bliztard.Slave.Application;

public class SlaveApplication
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
        services.AddMachine(MachineType.Slave);

        services.AddHostedService<ApplicationHostedService>();

        services.AddSingletonWithLifecycle<IFileRepository, InMemoryFileRepository>();
        services.AddSingletonWithLifecycle<IFileService,    InMemoryFileService>();
        services.AddSingletonWithLifecycle<IMachineService, MachineService>();

        services.AddSingleton<ApplicationServiceLifecycle>();

        services.AddHttpClient(Configuration.HttpClient.FileNotifyUpload,
                               client =>
                               {
                                   client.BaseAddress = new Uri(Configuration.Core.MasterBaseUrl);
                               });
        services.AddHttpClient(Configuration.HttpClient.FileTwincateData,
                               client =>
                               {
                                   client.BaseAddress = new Uri(Configuration.Core.MasterBaseUrl);
                               });
        services.AddHttpClient(Configuration.HttpClient.MachineNotifyMaster,
                               client =>
                               {
                                   client.BaseAddress = new Uri(Configuration.Core.MasterBaseUrl);
                               });
        services.AddHttpClient(Configuration.HttpClient.MachineSendUroshbeat,
                               client =>
                               {
                                   client.BaseAddress = new Uri(Configuration.Core.MasterBaseUrl);
                               });
        
        return services;
    }
}

using Bliztard.Application.Extension;
using Bliztard.Application.Model;
using Bliztard.Master.Repository.File;
using Bliztard.Master.Repository.Machine;
using Bliztard.Master.Service;
using Bliztard.Master.Service.File;
using Bliztard.Master.Service.Machine;

namespace Bliztard.Master;

public static class Program
{
    public static void Main(string[] args)
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
        builder.Services.AddMachine(MachineType.Master);

        builder.Services.AddSingleton<InitializationService>();
        
        builder.Services.AddControllers();

        var app = builder.Build();

        app.UseAuthorization();
        
        app.MapControllers();

        app.Start();
        
        app.Services.GetRequiredService<MachineInfo>().Resource.BaseUrl = app.Urls.First();
        app.Services.GetRequiredService<InitializationService>().StartAsync();

        app.WaitForShutdown();
    }

    public static IServiceCollection AddBliztardApplication(this IServiceCollection services)
    {
        services.AddSingleton<IFileRepository,    InMemoryFileRepository>();
        services.AddSingleton<IFileService,       InMemoryFileService>();
        services.AddSingleton<IMachineRepository, MachineRepository>();
        services.AddSingleton<IMachineService,    MachineService>();

        return services;
    }
}

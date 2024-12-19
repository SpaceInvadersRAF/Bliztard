using Bliztard.Application;
using Bliztard.Application.Model;
using Bliztard.Slave.Service;
using Microsoft.AspNetCore.Http.Features;

namespace Bliztard.Slave;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
                                         {
                                             options.IncludeScopes   = true;
                                             options.SingleLine      = true;
                                             options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
                                         });
        
        builder.WebHost.ConfigureKestrel((_, options) =>
                                         {
                                             options.Limits.MaxRequestBodySize   = 20_000_000_000;
                                         });

        builder.Services.Configure<FormOptions>(options =>
                                                {
                                                    options.MultipartBodyLengthLimit     = 20_000_000_000;
                                                    options.MultipartBoundaryLengthLimit = 2_000_000_000; 
                                                });
        
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddBliztardApplication();
        builder.Services.AddMachine(MachineType.Slave);

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
}

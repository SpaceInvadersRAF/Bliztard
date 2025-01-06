using Bliztard.Application.Core;
using Bliztard.Application.Model;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

namespace Bliztard.Master.Service;

public class ApplicationServiceLifecycle(ILogger<ApplicationServiceLifecycle> logger, IServiceProvider serviceProvider, MachineInfo machineInfo)
{
    private readonly ILogger<ApplicationServiceLifecycle> m_Logger          = logger;
    private readonly IServiceProvider                     m_ServiceProvider = serviceProvider;
    private readonly MachineInfo                          m_MachineInfo     = machineInfo;

    private IEnumerable<ILifecycle> ServiceLifecycles => m_ServiceProvider.GetServices<ILifecycle>();
    private IFeatureCollection      ServerFeatures    => m_ServiceProvider.GetRequiredService<IServer>().Features;
    private ICollection<string>     Urls              => ServerFeatures.GetRequiredFeature<IServerAddressesFeature>().Addresses;

    public void OnApplicationStarted()
    {
        m_Logger.LogInformation("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | Application Started", DateTime.Now);

        m_MachineInfo.Resource.BaseUrl = Urls.First();
        
        foreach (var service in ServiceLifecycles)
            service.OnStart();
    }

    public void OnApplicationStopped()
    {
        m_Logger.LogInformation("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Master | Application Stopped", DateTime.Now);

        foreach (var service in ServiceLifecycles)
            service.OnStop();
    }
}

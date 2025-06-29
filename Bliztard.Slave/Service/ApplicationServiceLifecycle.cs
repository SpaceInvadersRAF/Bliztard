using Bliztard.Application.Core;
using Bliztard.Application.Model;
using Bliztard.Persistence.Log;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

namespace Bliztard.Slave.Service;

public class ApplicationServiceLifecycle(ILogger<ApplicationServiceLifecycle> logger, IServiceProvider serviceProvider, LogTable logTable, MachineInfo machineInfo)
{
    private readonly ILogger<ApplicationServiceLifecycle> m_Logger          = logger;
    private readonly IServiceProvider                     m_ServiceProvider = serviceProvider;
    private readonly MachineInfo                          m_MachineInfo     = machineInfo;
    private readonly LogTable                             m_LogTable        = logTable;

    private IEnumerable<ILifecycle> ServiceLifecycles => m_ServiceProvider.GetServices<ILifecycle>();
    private IFeatureCollection      ServerFeatures    => m_ServiceProvider.GetRequiredService<IServer>().Features;
    private ICollection<string>     Urls              => ServerFeatures.GetRequiredFeature<IServerAddressesFeature>().Addresses;

    public void OnApplicationStarted()
    {
        m_Logger.LogInformation("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Application Started", DateTime.Now, m_MachineInfo.Id);

        m_MachineInfo.Resource.BaseUrl = Urls.First();

        var uri = new Uri(Urls.First());
        
        m_Logger.LogInformation("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Port: {Port} | Port Number", DateTime.Now, m_MachineInfo.Id, uri.Port);
        
        m_LogTable.SetName($"Slave.{uri.Port}");
        m_LogTable.StartBackgroundThread();
        
        foreach (var service in ServiceLifecycles)
            service.OnStart();
    }

    public void OnApplicationStopped()
    {
        m_Logger.LogInformation("Timestamp: {Timestamp:HH:mm:ss.ffffff} | MachineId: {MachineId} | Application Stopped", DateTime.Now, m_MachineInfo.Id);
        
        foreach (var service in ServiceLifecycles)
            service.OnStop();
        
        m_LogTable.Shutdown();
    }
}

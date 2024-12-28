namespace Bliztard.Slave.Service;

public class ApplicationHostedService(IHostApplicationLifetime applicationLifetime, ApplicationServiceLifecycle applicationServiceLifecycle) : IHostedService
{
    private readonly IHostApplicationLifetime    m_ApplicationLifetime         = applicationLifetime;
    private readonly ApplicationServiceLifecycle m_ApplicationServiceLifecycle = applicationServiceLifecycle;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        m_ApplicationLifetime.ApplicationStarted.Register(m_ApplicationServiceLifecycle.OnApplicationStarted);
        m_ApplicationLifetime.ApplicationStopped.Register(m_ApplicationServiceLifecycle.OnApplicationStopped);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

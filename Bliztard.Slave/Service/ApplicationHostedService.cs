using Bliztard.Slave.BackgroundService;

namespace Bliztard.Slave.Service;

public class ApplicationHostedService(IHostApplicationLifetime applicationLifetime, ApplicationServiceLifecycle applicationServiceLifecycle, WiwiwiBackgroundService wiwiwiBackgroundService)
: IHostedService
{
    private readonly IHostApplicationLifetime    m_ApplicationLifetime         = applicationLifetime;
    private readonly ApplicationServiceLifecycle m_ApplicationServiceLifecycle = applicationServiceLifecycle;
    private readonly WiwiwiBackgroundService     m_WiwiwiBackgroundService     = wiwiwiBackgroundService;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await m_WiwiwiBackgroundService.StartAsync(cancellationToken);

        m_ApplicationLifetime.ApplicationStarted.Register(m_ApplicationServiceLifecycle.OnApplicationStarted);
        m_ApplicationLifetime.ApplicationStopped.Register(m_ApplicationServiceLifecycle.OnApplicationStopped);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await m_WiwiwiBackgroundService.StopAsync(cancellationToken);
    }
}

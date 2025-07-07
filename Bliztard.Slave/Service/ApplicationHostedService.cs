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

        m_ApplicationServiceLifecycle.OnApplicationStarted();

        await m_WiwiwiBackgroundService.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        m_ApplicationServiceLifecycle.OnApplicationStopped();

        await m_WiwiwiBackgroundService.StopAsync(cancellationToken);
    }
}

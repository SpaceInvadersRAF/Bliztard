using Bliztard.Persistence.Log;
using Bliztard.Persistence.Table;

namespace Bliztard.Slave.BackgroundService;

public class WiwiwiBackgroundService(ILogger<WiwiwiBackgroundService> logger)
{
    public WiwiwiTable WiwiwiTable { private set; get; } = null!;
    public LogTable    LogTable    { private set; get; } = null!;

    private readonly ILogger<WiwiwiBackgroundService> m_Logger       = logger;
    private          Task                             m_LogTableTask = null!;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        LogTable    = new LogTable(m_Logger);
        WiwiwiTable = LogTable.RaspyTable();

        m_LogTableTask = Task.Run(() => LogTable.Start(cancellationToken), cancellationToken);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        LogTable.Shutdown();

        await m_LogTableTask;
    }
}

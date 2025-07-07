using Bliztard.Application.Model;
using Bliztard.Application.Web;
using Bliztard.Persistence.Log;
using Bliztard.Persistence.Table;

namespace Bliztard.Slave.BackgroundService;

public class WiwiwiBackgroundService(ILogger<WiwiwiBackgroundService> logger, MachineInfo machineInfo)
{
    public           WiwiwiTable WiwiwiTable { private set; get; } = null!;
    public           LogTable    LogTable    { private set; get; } = null!;
    private readonly MachineInfo m_MachineInfo = machineInfo;

    private readonly ILogger<WiwiwiBackgroundService> m_Logger       = logger;
    private          Task                             m_LogTableTask = null!;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        LogTable = new LogTable(m_Logger);

        WiwiwiTable = LogTable.RaspyTable();

        // todo: send to master (notify)
        var fileInfoList = WiwiwiTable.FindAllResources()
                                      .Select(resource => new SaveFileInfo(m_MachineInfo, resource.Id, resource.Data, MimeType.FromExtension(Path.GetExtension(resource.Data))
                                                                                                                              .ContentType.MediaType, 0, 0));

        
        
        foreach (var saveFileInfo in fileInfoList)
        {
            Console.WriteLine(saveFileInfo.Resource); 
        }

        m_LogTableTask = Task.Run(() => LogTable.Start(cancellationToken), cancellationToken);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        LogTable.Shutdown();

        await m_LogTableTask;
    }
}

using Bliztard.Application.Mapper;
using Bliztard.Application.Model;
using Bliztard.Application.Web;
using Bliztard.Contract.Request;
using Bliztard.Persistence.Log;
using Bliztard.Persistence.Table;
using Bliztard.Slave.Service.Network;

namespace Bliztard.Slave.BackgroundService;

public class WiwiwiBackgroundService(ILogger<WiwiwiBackgroundService> logger, MachineInfo machineInfo, INetworkService networkService)
{
    public           WiwiwiTable     WiwiwiTable { private set; get; } = null!;
    public           LogTable        LogTable    { private set; get; } = null!;
    private readonly INetworkService m_NetworkService = networkService;
    private readonly MachineInfo     m_MachineInfo    = machineInfo;

    private readonly ILogger<WiwiwiBackgroundService> m_Logger       = logger;
    private          Task                             m_LogTableTask = null!;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LogTable = new LogTable(m_Logger);

        WiwiwiTable = LogTable.RaspyTable();

        var fileInfoList = WiwiwiTable.FindAllResources()
                                      .Select(resource => new SaveFileInfo(m_MachineInfo, resource.Id, resource.Data, MimeType.FromExtension(Path.GetExtension(resource.Data))
                                                                                                                              .ContentType.MediaType, 0, 0))
                                      .Select(saveFileInfo => saveFileInfo.ToRequest())
                                      .ToList();

        await m_NetworkService.NotifyLogContent(m_MachineInfo, new NotifyLogContentRequest { SaveFileRequest = fileInfoList, MachineInfo = m_MachineInfo.ToRequest() }, cancellationToken);

        m_LogTableTask = Task.Run(() => LogTable.Start(cancellationToken), cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        LogTable.Shutdown();

        await m_LogTableTask;
    }
}

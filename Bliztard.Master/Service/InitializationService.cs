using Bliztard.Application.Service.Machine;

namespace Bliztard.Master.Service;

public class InitializationService(IMachineService machineService, ILogger<InitializationService> logger)
{
    private readonly IMachineService                m_MachineService = machineService;
    private readonly ILogger<InitializationService> m_Logger         = logger;

    public async Task StartAsync()
    {
        await Task.Run(() => MassMurderOnTheBeatSoItsNotNice(CancellationToken.None));
    }
    
    private Task MassMurderOnTheBeatSoItsNotNice(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            foreach (var (machineId, machineInfo) in m_MachineService.Repository.Machines)
                if (!machineInfo.Alive ^ (machineInfo.Alive = false) && m_MachineService.Unregister(machineId))
                    m_Logger.LogDebug("Machine with id: {machine} has been successfully murdered!", machineId);
    
            m_Logger.LogDebug("Mass Murder happened to {count} machines!", m_MachineService.Repository.Machines.Count);
    
            Task.Delay(8000, token).Wait(token);
        }
        
        return Task.FromCanceled(token);
    }
}

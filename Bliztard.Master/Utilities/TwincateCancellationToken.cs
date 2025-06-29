using Bliztard.Application.Model;

namespace Bliztard.Master.Utilities;

public class TwincateCancellationToken
{
    public CancellationTokenSource Source                  { get; }
    public MachineInfo?            MachineInfo             { private set; get; }
    public CancellationToken       Token                   => Source.Token;
    public bool                    IsCancellationRequested => Token.IsCancellationRequested;

    public TwincateCancellationToken(out CancellationToken token)
    {  
        Source = new CancellationTokenSource();
        token  = Source.Token;
    }
    
    public void Cancel(MachineInfo machineInfo)
    {
        MachineInfo = machineInfo;
        Source.Cancel();
    }
}

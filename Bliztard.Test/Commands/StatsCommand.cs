using Bliztard.Test.Configurations;

using AppConfiguration = Bliztard.Application.Configurations.Configuration;

namespace Bliztard.Test.Commands;

public class StatsCommand(IHttpClientFactory clientFactory) : Command(key: Configuration.Command.Stats, description: "Notifies machines to display their content.")
{
    private readonly IHttpClientFactory m_ClientFactory = clientFactory;
    
    public override Command Execute()
    {
        Task.Run(Statistics);
        
        return DefaultCommand;
    }

    public override void SetDefaults() { }

    public override bool ParseArguments(params string[] arguments) => true;

    private async Task<bool> Statistics()
    {
        var httpClient = m_ClientFactory.CreateClient(Configuration.HttpClient.BliztardMaster);

        var response = await httpClient.PostAsync(AppConfiguration.Endpoint.Files.Stats, null);
    
        Console.WriteLine(response.IsSuccessStatusCode ? "Statistics command has succeeded." : "Statistics command has failed.");
    
        return response.IsSuccessStatusCode;
    }
}

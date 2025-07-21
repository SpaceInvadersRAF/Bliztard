using Bliztard.Test.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AppConfiguration = Bliztard.Application.Configurations.Configuration;
using TestConfiguration = Bliztard.Test.Configurations.Configuration;


namespace Bliztard.Test;

public class Application
{
    private Command m_CurrentCommand = Command.StartUpCommand; 

    public void Run()
    {
        while (!m_CurrentCommand.Exit)
            m_CurrentCommand = m_CurrentCommand.Execute();
    }
}

public static class ServiceCollectionExtension
{

    public static IServiceCollection AddApplication(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddLogging(configure =>
                                     {
                                         configure.ClearProviders();
                                         configure.AddSimpleConsole(options =>
                                                                    {
                                                                        options.IncludeScopes   = false;
                                                                        options.SingleLine      = true;
                                                                        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
                                                                    });
                                     });

        serviceCollection.AddSingleton<Command, HelpCommand>();
        serviceCollection.AddSingleton<Command, InputCommand>();
        serviceCollection.AddSingleton<Command, ExitCommand>();
        serviceCollection.AddSingleton<Command, StatsCommand>();
        serviceCollection.AddSingleton<Command, DeleteCommand>();
        serviceCollection.AddSingleton<Command, UploadCommand>();
        serviceCollection.AddSingleton<Command, DownloadCommand>();

        serviceCollection.AddSingleton<Application>();

        serviceCollection.AddHttpClient(TestConfiguration.HttpClient.BliztardMaster, client => client.BaseAddress = new Uri(AppConfiguration.Core.MasterBaseUrl));
        serviceCollection.AddHttpClient(TestConfiguration.HttpClient.BliztardSlave);
        
        return serviceCollection;
    }

    public static Application Build(this IServiceCollection serviceCollection)
    {
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        foreach (var command in serviceProvider.GetServices<Command>())
            command.OnStart();
        
        return serviceProvider.GetRequiredService<Application>();
    }
    
}

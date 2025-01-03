using Microsoft.Extensions.DependencyInjection;

namespace Bliztard.Test;

class Program
{
    private static void Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddApplication();

        var application = serviceCollection.Build();
        
        application.Run();
    }
}


using Bliztard.Test.Configurations;

namespace Bliztard.Test.Commands;

public class ExitCommand() : Command(key: Configuration.Command.Exit,
                                     description: "Exits the application.",
                                     exit: true)
{
    public override Command Execute()
    {
        Console.WriteLine("Exiting application...");

        return DefaultCommand;
    }

    public override void SetDefaults() { }

    public override bool ParseArguments(params string[] arguments) => true;
}

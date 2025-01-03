using Bliztard.Test.Configurations;

namespace Bliztard.Test.Commands;

public class InputCommand() : Command(key: Configuration.Command.Input,
                                      description: "Input description - should not be displayed.",
                                      display: false)
{
    public override Command Execute(params string[] arguments)
    {
        string? input = Console.ReadLine();
        
        if (input is null)
            return DefaultCommand;

        if (Commands.TryGetValue(input.Split(" ")[0], out var command))
            return command == this ? DefaultCommand : command; 

        Console.WriteLine("Command does not exist.");
        
        return DefaultCommand;
    }
}

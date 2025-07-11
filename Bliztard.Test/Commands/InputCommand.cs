using Bliztard.Test.Configurations;

namespace Bliztard.Test.Commands;

public class InputCommand() : Command(key: Configuration.Command.Input, description: "Input description - should not be displayed.", display: false)
{
    public override Command Execute()
    {
        Console.Write("> ");
        string? input = Console.ReadLine();

        if (input is null)
            return DefaultCommand;

        var commandInput = input.Split(" ");

        if (!Commands.TryGetValue(commandInput.First(), out var command))
        {
            Console.WriteLine("Command does not exist.");

            return DefaultCommand;
        }

        command.SetDefaults();

        if (command == this)
            return DefaultCommand;

        if (commandInput.Length == 1 && command.MinimumArguments == 0)
            return command;

        return command.ParseArguments(commandInput.Skip(1)
                                                  .ToArray())
               ? command
               : DefaultCommand;
    }

    public override void SetDefaults() { }

    public override bool ParseArguments(params string[] arguments) => true;
}

using System.Text;
using Bliztard.Test.Configurations;
using Bliztard.Test.Extensions;

namespace Bliztard.Test.Commands;

public class HelpCommand() : Command(key: Configuration.Command.Help,
                                     description: "Displays help information about available commands.")
{
    public override Command Execute(params string[] arguments)
    {
        const int width = 128;
        const int commandWidth = 16;
        
        var stringBuilder = new StringBuilder();
        
        stringBuilder.AppendLine('='.Repeat(width))
                     .AppendLine("Bliztard Application".Center(width))
                     .AppendLine('-'.Repeat(width));

        foreach (var command in Commands.Values.Where(command => command.Display))
        {
            stringBuilder.Append(command.Key.PadLeft(commandWidth))
                         .Append(" - ")
                         .AppendLine(command.Description);
            
            if (command.Arguments is null)
                continue;

            stringBuilder.Append(' '.Repeat(commandWidth + 3)).Append("usage: ")
                         .Append(command.Key) 
                         .Append(' ')
                         .AppendLine(string.Join(' ', command.Arguments));
        }
        
        stringBuilder.AppendLine('-'.Repeat(width));

        Console.Write(stringBuilder);
        
        return Commands[Configuration.Command.Input];
    }
}

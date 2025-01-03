using Bliztard.Application.Core;

namespace Bliztard.Test.Commands;

public abstract class Command(string key, string description = "", string[]? arguments = null, bool exit = false, bool display = true) : ILifecycle
{
    public static Command DefaultCommand => Commands["help"];

    protected static readonly Dictionary<string, Command> Commands = new();
    
    public readonly   string    Key         = key;
    public readonly   bool      Exit        = exit;
    internal readonly string    Description = description;
    internal readonly string[]? Arguments   = arguments;
    internal readonly bool      Display     = display;

    public abstract Command Execute(params string[] arguments);

    public void OnStart()
    {
        Commands.TryAdd(Key, this);
    }
}

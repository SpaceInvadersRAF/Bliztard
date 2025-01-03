using Bliztard.Test.Configurations;

namespace Bliztard.Test.Commands;

public class DownloadFilesCommand() : Command(key: Configuration.Command.Download,
                                              description: "Downloads specified files from the server to your local machine. (not implemented)",
                                              arguments: ["<username>", "<file_path>", "[file_path2]", "[...]"])
{
    public override Command Execute(params string[] arguments)
    {
        Console.WriteLine("Download: Not implemented yet.");

        return DefaultCommand;
    }
}

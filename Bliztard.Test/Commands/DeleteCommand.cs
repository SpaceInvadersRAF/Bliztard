using Bliztard.Test.Configurations;

using AppConfiguration = Bliztard.Application.Configurations.Configuration;

namespace Bliztard.Test.Commands;

public class DeleteCommand(IHttpClientFactory clientFactory) : Command(key: Configuration.Command.Delete, description: "Deletes file", minimumArguments: 2)
{
    private readonly IHttpClientFactory m_ClientFactory = clientFactory;

    private          string          m_Username = string.Empty;
    private readonly HashSet<string> m_Files    = [];

    public override Command Execute()
    {
        var tasks = new Task[m_Files.Count];
        var index = 0;

        foreach (var fileName in m_Files)
            tasks[index++] = Task.Run(() => DeleteFile(m_Username, fileName));

        Task.WaitAll(tasks);

        return DefaultCommand;
    }

    private async Task<bool> DeleteFile(string username, string path)
    {
        var httpClient = m_ClientFactory.CreateClient(Configuration.HttpClient.BliztardMaster);

        var formData = new MultipartFormDataContent();

        formData.Add(new StringContent(username), "username");
        formData.Add(new StringContent(path),     "path");

        var response = await httpClient.PostAsync(AppConfiguration.Endpoint.Files.Delete, formData);

        Console.WriteLine($"---------------- {AppConfiguration.Endpoint.Files.Delete} --------------------");

        Console.WriteLine(response.IsSuccessStatusCode ? $"File {username}/{path} has been successfully deleted." : $"File {username}/{path} deletion has failed.");

        return response.IsSuccessStatusCode;
    }

    public override void SetDefaults()
    {
        m_Username = string.Empty;
        m_Files.Clear();
    }

    public override bool ParseArguments(params string[] arguments)
    {
        if (arguments.Length < 2)
        {
            Console.WriteLine("Username and at least one file must be provided.");

            return false;
        }

        m_Username = arguments.First();

        return arguments.Skip(1)
                        .All(fileName => m_Files.Add(fileName));
    }
}

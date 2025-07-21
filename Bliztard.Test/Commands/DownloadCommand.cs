using System.Net.Http.Json;

using Bliztard.Contract.Response;
using Bliztard.Test.Configurations;

using AppConfiguration = Bliztard.Application.Configurations.Configuration;

namespace Bliztard.Test.Commands;

public class DownloadCommand(IHttpClientFactory clientFactory) : Command(key: Configuration.Command.Download, description: "Downloads specified files from the server to your local machine.",
                                                                         arguments: ["<username>", "[<server_path>]", "[...]"], minimumArguments: 2)
{
    private readonly IHttpClientFactory m_ClientFactory = clientFactory;

    private          string       m_Username    = string.Empty;
    private readonly List<string> m_ServerPaths = [];

    public override Command Execute()
    {
        Task.Run(DownloadFiles)
            .Wait();

        return DefaultCommand;
    }

    private async Task<string> LocateFile()
    {
        var httpClient = m_ClientFactory.CreateClient(Configuration.HttpClient.BliztardMaster);

        var formData = new MultipartFormDataContent();

        formData.Add(new StringContent(m_Username),            "username");
        formData.Add(new StringContent(m_ServerPaths.First()), "path");

        var response    = await httpClient.PostAsync(AppConfiguration.Endpoint.Files.Locate, formData);
        var machineInfo = await response.Content.ReadFromJsonAsync<MachineInfoResponse>();

        return machineInfo.BaseUrl;
    }

    private async Task<bool> DownloadFiles()
    {
        var machineUrl = await LocateFile();

        var httpClient = m_ClientFactory.CreateClient(Configuration.HttpClient.BliztardSlave);

        var formData = new MultipartFormDataContent();

        formData.Add(new StringContent(m_Username),            "username");
        formData.Add(new StringContent(m_ServerPaths.First()), "path");

        var response = await httpClient.PostAsync($"{machineUrl}/{AppConfiguration.Endpoint.Files.Download}", formData);

        Console.WriteLine($"---------------- {AppConfiguration.Endpoint.Files.Download} --------------------");

        Console.WriteLine(response.IsSuccessStatusCode
                          ? $"File {m_Username}/{m_ServerPaths.First()} has been successfully downloaded."
                          : $"File {m_Username}/{m_ServerPaths.First()} download has failed.");

        if (!response.IsSuccessStatusCode)
            return false;

        await using var stream     = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream(Path.Combine(Configuration.Core.DownloadFilesDirectory, m_ServerPaths.First()), FileMode.Create, FileAccess.Write);

        await stream.CopyToAsync(fileStream);

        return true;
    }

    public override void SetDefaults()
    {
        m_Username = string.Empty;
        m_ServerPaths.Clear();
    }

    public override bool ParseArguments(params string[] arguments)
    {
        if (arguments.Length < MinimumArguments)
        {
            Console.WriteLine("Username and at least one file are required.");

            return false;
        }

        m_Username = arguments.First();
        m_ServerPaths.AddRange(arguments.Skip(1));

        return true;
    }
}

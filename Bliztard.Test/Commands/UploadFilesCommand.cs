using System.Net.Http.Headers;
using System.Net.Http.Json;

using Bliztard.Application.Web;
using Bliztard.Contract.Request;
using Bliztard.Contract.Response;
using Bliztard.Test.Configurations;
using Bliztard.Test.Utilities;

using AppConfiguration = Bliztard.Application.Configurations.Configuration;

namespace Bliztard.Test.Commands;

public class UploadFilesCommand(IHttpClientFactory clientFactory) : Command(key: Configuration.Command.Upload,
                                                                            description: "Uploads specified files from your local machine to the server. (no arguments accepted)",
                                                                            arguments: ["<username>", "[<file_path> <server_path>]", "[...]"], minimumArguments: 3)

{
    private readonly IHttpClientFactory m_ClientFactory = clientFactory;

    private          string       m_Username    = string.Empty;
    private readonly List<string> m_LocalPaths  = [];
    private readonly List<string> m_ServerPaths = [];

    public override Command Execute()
    {
        var tasks = new Task[m_LocalPaths.Count];

        for (int index = 0; index < m_LocalPaths.Count; index++)
        {
            var tmp = index;
            
            tasks[index] = Task.Run(() => UploadTask(m_LocalPaths[tmp], m_Username, m_ServerPaths[tmp]));
        }

        Task.WaitAll(tasks);

        return DefaultCommand;
    }

    private async Task UploadTask(string filePath, string username, string serverFilePath)
    {
        var fileInfo = new FileInfo(filePath);

        var successful = false;

        try
        {
            var machineUrl = await FindUploadLocation(fileInfo);

            successful = await UploadToMachine(machineUrl, fileInfo, username, serverFilePath);
        }
        catch
        {
            /*ignored*/
        }

        Console.WriteLine(successful ? $"File {fileInfo.Name} has been successfully uploaded." : $"File {fileInfo.Name} upload has failed.");
    }

    private async Task<string> FindUploadLocation(FileInfo fileInfo)
    {
        var httpClient = m_ClientFactory.CreateClient(Configuration.HttpClient.BliztardMaster);

        var request = new UploadLocationsRequest()
                      {
                          FilePath = fileInfo.Name,
                          Size     = fileInfo.Length,
                          Username = m_Username
                      };

        var response        = await httpClient.PostAsJsonAsync(AppConfiguration.Endpoint.Machine.UploadLocations, request);
        var uploadLocations = await response.Content.ReadFromJsonAsync<UploadLocationsResponse>();

        return uploadLocations.MachineInfos.First()
                              .BaseUrl;
    }

    private async Task<bool> UploadToMachine(string machineUrl, FileInfo fileInfo, string username, string serverPath)
    {
        var httpClient = m_ClientFactory.CreateClient(Configuration.HttpClient.BliztardSlave);

        var formData = new MultipartFormDataContent();

        formData.Add(new StringContent(username),   "username");
        formData.Add(new StringContent(serverPath), "path");

        var file = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);

        var fileStream = new StreamContent(file);

        fileStream.Headers.ContentType = new MediaTypeHeaderValue(MimeType.FromExtension(fileInfo.Extension)
                                                                          .ContentType.MediaType);

        formData.Add(fileStream, "file", fileInfo.Name);

        var response = await httpClient.PostAsync($"{machineUrl}/{AppConfiguration.Endpoint.Files.Upload}", formData);

        Console.WriteLine($"---------------- {machineUrl}/{AppConfiguration.Endpoint.Files.Upload} --------------------");
        return response.IsSuccessStatusCode;
    }

    public override void SetDefaults()
    {
        m_Username = string.Empty;
        m_LocalPaths.Clear();
        m_ServerPaths.Clear();
    }

    public override bool ParseArguments(params string[] arguments)
    {
        if (arguments.Length < MinimumArguments || arguments.Length % 2 != 1)
        {
            Console.WriteLine("Username and at least one file are required.");

            return false;
        }

        var filesInDirectory = FileUtilities.GetFiles()
                                            .ToHashSet();

        m_Username = arguments.First();

        for (var index = 1; index < arguments.Length; index += 2)
        {
            var fileName       = arguments[index];
            var serverFilePath = arguments[index + 1];

            var filePath = filesInDirectory.FirstOrDefault(file => Path.GetFileName(file) == fileName);

            if (filePath is null)
            {
                Console.WriteLine($"File `{fileName}` does not exist.");

                return false;
            }

            m_LocalPaths.Add(filePath);
            m_ServerPaths.Add(serverFilePath);
        }

        return true;
    }
}

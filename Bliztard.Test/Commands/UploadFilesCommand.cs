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
                                                                            arguments: ["<username>", "[<file_path> <server_path>]", "[...]"])

{
    private readonly IHttpClientFactory m_ClientFactory = clientFactory;

    public override Command Execute(params string[] arguments)
    {
        var files = FileUtilities.GetFiles();
        var tasks = new Task[files.Length];
        var index = 0;

        foreach (var filePath in files)
            tasks[index++] = Task.Run(() => UploadTask(filePath));

        Task.WaitAll(tasks);
        
        return DefaultCommand;
    }

    private async Task UploadTask(string filePath)
    {
        var fileInfo = new FileInfo(filePath);

        var successful = false;

        try
        {
            var machineUrl = await FindUploadLocation(fileInfo);

            successful = await UploadToMachine(fileInfo, machineUrl);
        }
        catch { /*ignored*/ }


        Console.WriteLine(successful ? $"File {fileInfo.Name} has been successfully uploaded." : $"File {fileInfo.Name} upload has failed.");
    }

    private async Task<string> FindUploadLocation(FileInfo fileInfo)
    {
        var httpClient = m_ClientFactory.CreateClient(Configuration.HttpClient.BliztardMaster);

        var request = new UploadLocationsRequest()
                      {
                          FilePath = fileInfo.Name,
                          Length   = fileInfo.Length,
                          Username = "Urosh<3"
                      };

        var response        = await httpClient.PostAsJsonAsync(AppConfiguration.Endpoint.Machine.UploadLocations, request);
        var uploadLocations = await response.Content.ReadFromJsonAsync<UploadLocationsResponse>();

        return uploadLocations.MachineInfos.First().BaseUrl;
    }

    private async Task<bool> UploadToMachine(FileInfo fileInfo, string machineUrl)
    {
        var httpClient = m_ClientFactory.CreateClient(Configuration.HttpClient.BliztardSlave);

        var formData = new MultipartFormDataContent();
        
        formData.Add(new StringContent("Urosh<3"), "username");
        formData.Add(new StringContent(fileInfo.Name), "path");

        var file = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);

        var fileStream = new StreamContent(file);

        fileStream.Headers.ContentType = new MediaTypeHeaderValue(MimeType.FromExtension(fileInfo.Extension).ContentType.MediaType);
        
        formData.Add(fileStream, "file", fileInfo.Name);

        var response = await httpClient.PostAsync($"{machineUrl}/{AppConfiguration.Endpoint.Files.Upload}", formData);

        return response.IsSuccessStatusCode;
    }
}

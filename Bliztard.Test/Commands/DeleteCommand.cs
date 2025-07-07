using Bliztard.Test.Configurations;

using AppConfiguration = Bliztard.Application.Configurations.Configuration;

namespace Bliztard.Test.Commands;

public class DeleteCommand(IHttpClientFactory clientFactory) : Command(key: Configuration.Command.Delete, description: "Delete's file")
{
    private readonly IHttpClientFactory m_ClientFactory = clientFactory;

    public override Command Execute(params string[] arguments)
    {
        Task.Run(() => DeleteFile("Urosh<3", "file01.csv"));    
        
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
}

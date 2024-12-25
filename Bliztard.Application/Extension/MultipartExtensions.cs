using System.Net.Http.Headers;
using Bliztard.Application.Model;

namespace Bliztard.Application.Extension;

public static class MultipartFormDataContentExtensions
{
    public static void AddTwincate(this MultipartFormDataContent content, SaveFileInfo saveFileInfo, Stream file)
    {
        content.Add(new StringContent(saveFileInfo.Username),                     "username");
        content.Add(new StringContent(saveFileInfo.FilePath),                     "path");
        content.Add(new StringContent((saveFileInfo.Replication - 1).ToString()), "replications");
        
        var fileStream = new StreamContent(file);
        fileStream.Headers.ContentType = new MediaTypeHeaderValue(saveFileInfo.ContentType);
        
        content.Add(fileStream, "file", saveFileInfo.FileName);
    }
}

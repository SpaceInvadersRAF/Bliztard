using System.Text;

namespace Bliztard.Application.Extension;

public static class StreamExtensions
{
    public static string ReadContent(this Stream stream, Encoding encoding)
    {
        var position = stream.Position;

        stream.Position = 0;

        var content = new StreamReader(stream, encoding).ReadToEnd();

        stream.Position = position;

        return content;
    }
}

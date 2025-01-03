using System.Net.Mime;

namespace Bliztard.Application.Web;

public class MimeType(string contentType, params string[] extensions)
{
    public static readonly MimeType Any               = new("*/*");
    public static readonly MimeType ApplicationBinary = new("application/x-msdownload", "dll", "exe");
    public static readonly MimeType ApplicationJson   = new("application/json", "json");
    public static readonly MimeType ApplicationXml    = new("application/xml", "xml");
    public static readonly MimeType MultipartFormData = new("multipart/form-data");
    public static readonly MimeType TextCsv           = new("text/csv", "csv");
    public static readonly MimeType TextPlain         = new("text/plain", "txt");
    public static readonly MimeType TextTsv           = new("text/tab-separated-values", "tsv");

    public readonly string[]    Extensions  = extensions;
    public readonly ContentType ContentType = new(contentType);

    private static readonly Dictionary<string, MimeType> s_ExtensionDictionary = new();

    public static MimeType FromExtension(string extension)
    {
        return s_ExtensionDictionary.GetValueOrDefault(extension, Any);
    }

    static MimeType()
    {
        Register(Any);
        Register(ApplicationBinary);
        Register(ApplicationJson);
        Register(ApplicationXml);
        Register(MultipartFormData);
        Register(TextCsv);
        Register(TextPlain);
        Register(TextTsv);
    }

    private static void Register(MimeType mimeType)
    {
        foreach (var extension in mimeType.Extensions)
            s_ExtensionDictionary.TryAdd(extension, mimeType);
    }
}

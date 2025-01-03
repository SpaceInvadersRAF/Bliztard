namespace Bliztard.Test.Configurations;

public partial struct Configuration
{
    public struct Core
    {
        public static readonly string? UploadFilesDirectory = Environment.GetEnvironmentVariable("BLIZTARD_UPLOAD_FILES_DIRECTORY");
    }

    public struct HttpClient
    {
        public const string BliztardMaster = nameof(BliztardMaster);
        public const string BliztardSlave = nameof(BliztardSlave);
    }
}

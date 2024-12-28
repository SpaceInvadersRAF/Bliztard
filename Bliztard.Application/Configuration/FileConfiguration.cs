namespace Bliztard.Application.Configuration;

public partial struct Configurations
{
    public partial struct Core
    {
        public static readonly int    ReplicationFactor = 3;
        public static readonly string MasterBaseUrl     = "http://localhost:3031";
    }
    
    public partial struct HttpClient
    {
        public static readonly string FileTwincateData = nameof(FileTwincateData);
        public static readonly string FileNotifyUpload = nameof(FileNotifyUpload);
    }
    
    public partial struct Interval { }
    
    public partial struct Endpoint
    {
        public struct Files
        {
            private const string Base         = "files";
            public  const string Upload       = $"{Base}/upload";
            public  const string Download     = $"{Base}/download";
            public  const string NotifyUpload = $"{Base}/notify-upload";
            public  const string Locate       = $"{Base}/locate";
        }
    }
}

using Bliztard.Application.Utilities;

namespace Bliztard.Application.Configurations;

public partial struct Configuration
{
    public partial struct Core
    {
        public static readonly int    ReplicationFactor = 3;
        public static readonly string MasterBaseUrl     = EnvironmentUtilities.GetStringVariable("BLIZTARD_MASTER_BASE_URL", "http://localhost:3031");
    }

    public partial struct HttpClient
    {
        public static readonly string FileTwincateData     = nameof(FileTwincateData);
        public static readonly string FileNotifyUpload     = nameof(FileNotifyUpload);
        public static readonly string FileNotifyLogContent = nameof(FileNotifyLogContent);
    }

    public partial struct Interval { }

    public partial struct Endpoint
    {
        public struct Files
        {
            private const string Base = "files";

            public const string Upload           = $"{Base}/upload";
            public const string Download         = $"{Base}/download";
            public const string Delete           = $"{Base}/delete";
            public const string Stats            = $"{Base}/stats";
            public const string NotifyUpload     = $"{Base}/notify-upload";
            public const string NotifyDelete     = $"{Base}/notify-delete";
            public const string Locate           = $"{Base}/locate";
            public const string Twincate         = $"{Base}/twincate";
            public const string NotifyLogContent = $"{Base}/notify-log";
        }
    }
}

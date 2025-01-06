namespace Bliztard.Application.Configurations;

public partial struct Configuration
{
    public struct Log
    {
        public static readonly string FilePath             = Environment.GetEnvironmentVariable("BLIZTARD_UPLOAD_FILES_DIRECTORY") ?? "logs/Bliztard.log";
        public static readonly string TimestampFormat      = "yyyy-MM-dd HH:mm:ss.fff";
        public static readonly bool   Shared               = true;

        public struct Serilog
        {
            public static readonly string OutputTemplate = $"{{Timestamp:{TimestampFormat}}} [{{Level:u3}}] {{SourceContext}} {{Message}}{{NewLine}}{{Exception}}";
        }
    }
}

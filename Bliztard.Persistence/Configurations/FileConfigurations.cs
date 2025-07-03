using Bliztard.Application.Utilities;

namespace Bliztard.Persistence.Configurations;

public static class Configuration
{
    public static class File
    {
        public static readonly string LogTableDirectory = EnvironmentUtilities.GetStringVariable("BLIZTARD_LOG_TABLE_DIRECTORY");
        public static readonly string LogTableName      = EnvironmentUtilities.GetStringVariable("BLIZTARD_LOG_TABLE_NAME", ".logtable");
        public static readonly string LogTablePath      = $"{LogTableDirectory}/{LogTableName}";
    }
}

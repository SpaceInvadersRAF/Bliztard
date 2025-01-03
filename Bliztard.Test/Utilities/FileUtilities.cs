using Bliztard.Test.Configurations;

namespace Bliztard.Test.Utilities;

public static class FileUtilities
{
    public static string[] GetFiles()
    {
        var directory = Configuration.Core.UploadFilesDirectory;

        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException("You need to set valid path as BLIZTARD_UPLOAD_FILES_DIRECTORY environment variable.");

        return Directory.GetFiles(directory);
    }
}

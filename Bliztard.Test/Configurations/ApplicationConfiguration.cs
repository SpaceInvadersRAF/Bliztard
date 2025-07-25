﻿namespace Bliztard.Test.Configurations;

public partial struct Configuration
{
    public struct Core
    {
        public static readonly string UploadFilesDirectory   = Environment.GetEnvironmentVariable("BLIZTARD_UPLOAD_FILES_DIRECTORY")   ?? "D:\\Data\\Bliztard\\Files";
        public static readonly string DownloadFilesDirectory = Environment.GetEnvironmentVariable("BLIZTARD_DOWNLOAD_FILES_DIRECTORY") ?? "D:\\Data\\Bliztard\\Download";
    }

    public struct HttpClient
    {
        public const string BliztardMaster = nameof(BliztardMaster);
        public const string BliztardSlave  = nameof(BliztardSlave);
    }
}

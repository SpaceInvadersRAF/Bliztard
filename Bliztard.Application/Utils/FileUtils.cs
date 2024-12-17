namespace Bliztard.Application.utils;

public class FileUtils
{
    public static int ValueOfString(string fileName, int maxSize)
    {
        return fileName.Sum(c => c) % maxSize;
    }
}
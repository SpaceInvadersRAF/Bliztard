using System.Text;

namespace Bliztard.Application.Extension;

public static class StringExtensions
{
    public static byte[] AsciiBytes(this string value)
    {
        return Encoding.ASCII.GetBytes(value);
    }

    public static int AsciiByteCount(this string value)
    {
        return Encoding.ASCII.GetByteCount(value);
    }
    
    public static string OrDefault(this string? value, string defaultValue = "")
    {
        return value ?? defaultValue;
    }

    public static int ParseIntOrDefault(this string? value, int defaultValue = 0)
    {
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    public static bool ParseBoolOrDefault(this string? value, bool defaultValue = false)
    {
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }
}

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
}

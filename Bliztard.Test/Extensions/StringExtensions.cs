namespace Bliztard.Test.Extensions;

public static class StringExtensions
{

    public static string Repeat(this char character, int count)
    {
        return new string(character, count);
    }

    public static string Center(this string text, int width)
    {
        return text.Length >= width ? text : text.PadLeft((width + text.Length) / 2)
                                                 .PadRight(width);
    }
    
}

namespace Bliztard.Application.Extension;

public static class DictionaryExtensions
{
    public static string TryGetString(this IDictionary<string, string> dictionary, string key)
    {
        return dictionary.TryGetValue(key, out var value) ? value : "";
    }
}

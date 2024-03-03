namespace Fetcher.Extensions;

internal static class MemoryStreamExtensions
{
    public static async Task<string> ConvertToString(this MemoryStream stream)
    {
        stream.Position = 0;

        using StreamReader reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}

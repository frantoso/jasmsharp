using System.Text;

namespace jasmsharp_debug_adapter;

using System.IO.Compression;
using System.Text.Json;

internal static class AdapterExtensions
{
    public static JasmCommandForString ToCommand(this string method, string payload) => new(method, payload);

    public static string Serialize<T>(this T value) => JsonSerializer.Serialize(value);

    public static JasmCommand? Deserialize(this string value) => JsonSerializer.Deserialize<JasmCommand>(value);

    public static byte[] ToBytes(this string text) => Encoding.UTF8.GetBytes(text);

    public static string ToUtf8(this byte[] buffer, int bytesRead) => Encoding.UTF8.GetString(buffer, 0, bytesRead);

    /// <summary>
    ///     Creates a list from a single element.
    /// </summary>
    /// <returns>Returns a new list with the given element as member.</returns>
    public static List<T> MakeList<T>(this T t) => [t];

    public static byte[] Compress(this string text)
    {
        var bytes = text.ToBytes();
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionMode.Compress))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }

        return output.ToArray();
    }

}
// -----------------------------------------------------------------------
// <copyright file="AdapterExtensions.cs">
//     Created by Frank Listing at 2025/12/21.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp_debug_adapter;

using System.IO.Compression;
using System.Text;
using System.Text.Json;
using model;

internal static class AdapterExtensions
{
    /// <summary>
    ///     Generates a key for a command handler.
    /// </summary>
    /// <param name="fsm">The FSM associated with the command.</param>
    /// <param name="command">The command.</param>
    /// <returns>Returns the key.</returns>
    public static string MakeKey(this string fsm, string command) => $"{fsm}::{command}";

    /// <summary>
    ///     Serializes the specified value to Json.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>Returns a Json string with the serialized object.</returns>
    public static string Serialize<T>(this T value) => JsonSerializer.Serialize(value);

    /// <summary>
    ///     Deserializes the specified Json string to a command object.
    /// </summary>
    /// <param name="json">The Json string to deserialize.</param>
    /// <returns>Returns the <see cref="JasmCommand" /> object or null in case of an error.</returns>
    // ReSharper disable once ConvertToExtensionBlock
    public static JasmCommand? Deserialize(this string json) => JsonSerializer.Deserialize<JasmCommand>(json);

    /// <summary>
    ///     Converts the specified string to a byte array (UTF8 format).
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>Returns a byte array containing the text.</returns>
    public static byte[] ToBytes(this string text) => Encoding.UTF8.GetBytes(text);

    /// <summary>
    ///     Converts the specified byte array to a string (UTF8 format).
    /// </summary>
    /// <param name="buffer">The byte array to convert.</param>
    /// <param name="count">The number of bytes in the array to use.</param>
    /// <returns>Returns a string containing the text.</returns>
    public static string ToString(this byte[] buffer, int count) => Encoding.UTF8.GetString(buffer, 0, count);

    /// <summary>
    ///     Creates a list from a single element.
    /// </summary>
    /// <returns>Returns a new list with the given element as member.</returns>
    public static List<T> MakeList<T>(this T t) => [t];

    /// <summary>
    ///     Compresses the specified text (gzip).
    /// </summary>
    /// <param name="text">The text to compress.</param>
    /// <returns>Returns a byte array containing the compressed text.</returns>
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

    /// <summary>
    ///     Converts a string to an <see cref="int" />. If conversion fails, the specified default value is returned.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>Returns the number.</returns>
    public static int ToInt32(this string? text, int defaultValue) =>
        int.TryParse(text, out var result) ? result : defaultValue;
}
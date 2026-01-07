// -----------------------------------------------------------------------
// <copyright file="AdapterExtensionsTest.cs">
//     Created by Frank Listing at 2025/12/21.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp_debug_adapter.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using jasmsharp_debug_adapter.model;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestSubject(typeof(AdapterExtensions))]
public class AdapterExtensionsTest
{
    public static IEnumerable<object[]> ToInt32TestData =>
    [
        ["", 23, 23],
        [null, 23, 23],
        ["abc", 23, 23],
        ["2.4", 23, 23],
        ["42", 23, 42]
    ];

    [TestMethod]
    public void MakeKeyTest()
    {
        var key = "MyFsm".MakeKey("do-it");

        Assert.AreEqual("MyFsm::do-it", key);
    }

    [TestMethod]
    public void SerializeDeserializeTest()
    {
        var originalCommand = new JasmCommand("MyFsm", "do-it", "payload-data");

        var json = originalCommand.Serialize();
        var deserializedCommand = json.Deserialize();

        Assert.IsNotNull(deserializedCommand);
        Assert.AreEqual(originalCommand.Fsm, deserializedCommand!.Fsm);
        Assert.AreEqual(originalCommand.Command, deserializedCommand.Command);
        Assert.AreEqual(originalCommand.Payload, deserializedCommand.Payload);
    }

    [TestMethod]
    public void SerializeNullObjectTest()
    {
        var json = ((object)null).Serialize();
        Assert.AreEqual("null", json);
    }

    [TestMethod]
    public void DeserializeInvalidJsonTest()
    {
        const string invalidJson = "{ invalid json }";
        Assert.Throws<JsonException>(() => invalidJson.Deserialize());
    }

    [TestMethod]
    public void DeserializeNullJsonTest()
    {
        const string nullJson = "null";
        var command = nullJson.Deserialize();
        Assert.IsNull(command);
    }

    [TestMethod]
    public void ToBytesTest()
    {
        const string text = "Hello, World!";
        var bytes = text.ToBytes();
        var convertedText = Encoding.UTF8.GetString(bytes);
        Assert.AreEqual(text, convertedText);
    }

    [TestMethod]
    public void ToBytesEmptyStringTest()
    {
        var bytes = "".ToBytes();
        Assert.AreEqual(0, bytes.Length);
    }

    [TestMethod]
    public void ToStringTest()
    {
        const string text = "Hello, World!";
        var bytes = Encoding.UTF8.GetBytes(text);
        var convertedText = bytes.ToString(bytes.Length);
        Assert.AreEqual(text, convertedText);
    }

    [TestMethod]
    public void ToStringPartialTest()
    {
        const string text = "Hello, World!";
        var bytes = Encoding.UTF8.GetBytes(text);
        var convertedText = bytes.ToString(5);
        Assert.AreEqual("Hello", convertedText);
    }

    [TestMethod]
    public void ToStringEmptyByteArrayTest()
    {
        var bytes = Array.Empty<byte>();
        var text = bytes.ToString(0);
        Assert.AreEqual("", text);
    }

    [TestMethod]
    public void MakeListTest()
    {
        var list = 42.MakeList();

        Assert.HasCount(1, list);
        Assert.AreEqual(42, list[0]);
    }

    [TestMethod]
    public void CompressTest()
    {
        const string text = "This is a test string for compression.";
        var compressedBytes = text.Compress();

        Assert.IsLessThan(compressedBytes.Length, text.Length);

        using var compressed = new MemoryStream(compressedBytes);
        using var gzip = new GZipStream(compressed, CompressionMode.Decompress);
        using var decompressed = new MemoryStream();
        gzip.CopyTo(decompressed);
        var decompressedBytes = decompressed.ToArray();
        var decompressedText = Encoding.UTF8.GetString(decompressedBytes);

        Assert.AreEqual(text, decompressedText);
    }

    [TestMethod]
    [DynamicData(nameof(ToInt32TestData))]
    public void ToInt32Test([CanBeNull] string input, int defaultValue, int expected)
    {
        var number = input.ToInt32(defaultValue);

        Assert.AreEqual(expected, number);
    }
}
// -----------------------------------------------------------------------
// <copyright file="FsmExceptionTest.cs">
//     Created by Frank Listing at 2025/09/30.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestSubject(typeof(FsmException))]
public class FsmExceptionTest
{
    private static readonly InvalidOperationException TestException = new();

    public record Expected(string? Message, string StateName, Exception? InnerException);

    private static Expected Expect(string? message, string stateName, Exception? innerException) =>
        new(message, stateName, innerException);

    public static IEnumerable<object[]> TestData =>
    [
        [1, new FsmException(), Expect(null, "", null)],
        [2, new FsmException(message: "test"), Expect("test", "", null)],
        [
            3,
            new FsmException(innerException: FsmExceptionTest.TestException),
            Expect(null, "", FsmExceptionTest.TestException)
        ],
        [
            4,
            new FsmException(message: "test", innerException: FsmExceptionTest.TestException),
            Expect("test", "", FsmExceptionTest.TestException)
        ],
        [
            5,
            new FsmException(message: "test", stateName: "doing", innerException: FsmExceptionTest.TestException),
            Expect("test", "doing", FsmExceptionTest.TestException)
        ]
    ];

    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void CreatesTheException(int index, FsmException input, Expected expected)
    {
        if (expected.Message is not null)
        {
            Assert.AreEqual(expected.Message, input.Message);
        }

        Assert.AreEqual(expected.StateName, input.StateName);
        Assert.AreEqual(expected.InnerException, input.InnerException);
    }
}
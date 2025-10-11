// -----------------------------------------------------------------------
// <copyright file="EventTest.cs">
//     Created by Frank Listing at 2025/09/30.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtils;


[TestClass]
[TestSubject(typeof(Event))]
public class EventTest
{
    public static IEnumerable<object[]> NameTestData =>
    [
        [new OtherTestEvent(), "OtherTestEvent"],
        [new TestEvent(), "TestEvent"]
    ];

    [TestMethod]
    [DynamicData(nameof(NameTestData))]
    public void ToStringShowsTheSimpleClassName(Event eventInstance, string expectedName)
    {
        Assert.AreEqual(expectedName, eventInstance.ToString());
    }

    private static readonly TestEvent StaticTestEvent = new();

    public static IEnumerable<object?[]> CompareTestData =>
    [
        [new TestEvent(), new TestEvent(), true, false],
        [new OtherTestEvent(), new OtherTestEvent(), true, false],
        [new TestEvent(), new OtherTestEvent(), false, false],
        [StaticTestEvent, StaticTestEvent, true, true],
        [new TestEvent(), null, false, false],
    ];

    [TestMethod]
    [DynamicData(nameof(CompareTestData))]
    public void ComparesEvents(Event event1, Event? event2, bool expectedEqual, bool expectedSame)
    {
        Assert.AreEqual(expectedEqual, event1.Equals(event2));
        Assert.AreEqual(expectedSame, object.ReferenceEquals(event1, event2));
    }

    public static IEnumerable<object[]> TypeTestData =>
    [
        [new TestEvent(), typeof(TestEvent)],
        [new OtherTestEvent(), typeof(OtherTestEvent)],
        [StaticTestEvent, typeof(TestEvent)]
    ];

    [TestMethod]
    [DynamicData(nameof(TypeTestData))]
    public void TypeReturnsTheRightValue(Event @event, Type expectedType)
    {
        Assert.AreEqual(expectedType, @event.Type);
    }

    public static IEnumerable<object[]> HashTestData =>
    [
        [new OtherTestEvent(), new OtherTestEvent(), true],
        [new TestEvent(), new TestEvent(), true],
        [StaticTestEvent, StaticTestEvent, true],
        [new TestEvent(), new OtherTestEvent(), false]
    ];

    [TestMethod]
    [DynamicData(nameof(EventTest.HashTestData))]
    public void TwoEventObjectsOfTheSameTypeHaveTheSameHashCode(Event event1, Event event2, bool expected)
    {
        Assert.AreEqual(expected, object.Equals(event1.GetHashCode(), event2.GetHashCode()));
    }
}
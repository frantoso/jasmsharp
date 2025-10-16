// -----------------------------------------------------------------------
// <copyright file="DataEventTest.cs">
//     Created by Frank Listing at 2025/10/03.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtils;

[TestClass]
[TestSubject(typeof(DataEvent<,>))]
public class DataEventTest
{
    [TestMethod]
    public void CreatesADataEvent()
    {
        var dataEvent = new DataEvent<TestEvent, int>(42);

        Assert.AreEqual(typeof(TestEvent), dataEvent.Type);
    }

    [TestMethod]
    public void CopiesTheData()
    {
        var dataEvent = new DataEvent<TestEvent, int>(42);
        var copiedEvent = dataEvent.FromData<OtherTestEvent>();

        Assert.AreEqual(typeof(TestEvent), dataEvent.Type);
        Assert.AreEqual(typeof(OtherTestEvent), copiedEvent.Type);
        Assert.AreEqual(dataEvent.Data, copiedEvent.Data);
    }

    [TestMethod]
    public void ToStringShowsTheSimpleClassNameOfTheEncapsulatedEvent()
    {
        var dataEvent = new DataEvent<TestEvent, int>(42);

        Assert.AreEqual(nameof(TestEvent), dataEvent.ToString());
    }
}
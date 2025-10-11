// -----------------------------------------------------------------------
// <copyright file="ActionTest.cs">
//     Created by Frank Listing at 2025/09/30.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestSubject(typeof(Action))]
public class ActionTest
{
    [TestMethod]
    public void ExecutesTheAction()
    {
        var counter = 0;
        var action = new Action(() => ++counter);

        action.Fire(new StartEvent());

        Assert.AreEqual(1, counter);
    }

    [TestMethod]
    public void ExecutesAnInvalidActionAndThrowsAnFsmException()
    {
        var counter = 0;
        var action = new Action(() => counter = 4 / counter);

        Assert.ThrowsExactly<FsmException>(() => action.Fire(new StartEvent()));
    }
}

[TestClass]
[TestSubject(typeof(Action<>))]
public class DataActionTest
{
    private class TestParams(int number)
    {
        public int Number { get; set; } = number;
    }

    private class DerivedTestParams(string name, int number) : TestParams(number)
    {
        public string Name { get; set; } = name;
    }

    [TestMethod]
    public void ExecutesTheAction()
    {
        var counter = 1;
        var action = new Action<int>(data => { counter += data; });

        action.Fire(new DataEvent<StartEvent, int>(3));

        Assert.AreEqual(4, counter);
    }

    [TestMethod]
    public void ExecutesTheActionWithNullAsParameter_NoDataEvent()
    {
        int? dataProvided = 2;
        var action = new Action<int?>(data => dataProvided = data);

        action.Fire(new StartEvent());

        Assert.IsNull(dataProvided);
    }

    [TestMethod]
    public void ExecutesTheActionWithDerivedClassAsParameter()
    {
        var dataProvided = new DerivedTestParams("Test", 5);
        var action = new Action<TestParams>(data => data!.Number = 42);

        action.Fire(new DataEvent<StartEvent, DerivedTestParams>(dataProvided));

        Assert.AreEqual(42, dataProvided.Number);
    }

    [TestMethod]
    public void ExecutesTheActionWithNullAsParameter_WrongDataType()
    {
        object dataProvided = 2;
        var action = new Action<int>(data => dataProvided = data);

        action.Fire(new DataEvent<StartEvent, double>(3.2));

        Assert.AreEqual(0, dataProvided);
    }

    [TestMethod]
    public void ExecutesAnInvalidActionAndThrowsAnFsmException()
    {
        var counter = 0;
        var action = new Action<int>(_ => { counter = 4 / counter; });

        Assert.ThrowsExactly<FsmException>(() => action.Fire(new StartEvent()));
    }
}
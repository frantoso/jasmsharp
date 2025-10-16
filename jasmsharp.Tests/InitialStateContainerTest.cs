// -----------------------------------------------------------------------
// <copyright file="InitialStateContainerTest.cs">
//     Created by Frank Listing at 2025/10/03.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestSubject(typeof(StateContainer))]
public class InitialStateContainerTest
{
    [TestMethod]
    public void TestInitialization()
    {
        var container = new InitialStateContainer( new List<ITransition>());

        Assert.AreEqual("Initial", container.Name);
        Assert.IsFalse(container.HasTransitions);
        Assert.IsFalse(container.HasChildren);
    }

    [TestMethod]
    public void AddsATransitionToState()
    {
        var container = InitialStateContainer.Transition(new State(InitialStateContainerTest.TestStateName));

        Assert.IsTrue(container.HasTransitions);
        Assert.IsFalse(container.HasChildren);
    }

    private const string TestStateName = "test-state-2";
}
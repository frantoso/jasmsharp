// -----------------------------------------------------------------------
// <copyright file="FinalStateContainerTest.cs">
//     Created by Frank Listing at 2025/10/03.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestSubject(typeof(StateContainer))]
public class FinalStateContainerTest
{
    [TestMethod]
    public void TestInitialization()
    {
        var container = new FinalStateContainer();

        Assert.AreEqual("Final", container.Name);
        Assert.IsFalse(container.HasTransitions);
        Assert.IsFalse(container.HasChildren);
    }
}
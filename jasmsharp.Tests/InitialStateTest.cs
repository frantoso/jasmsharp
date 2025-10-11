// -----------------------------------------------------------------------
// <copyright file="InitialStateTest.cs">
//     Created by Frank Listing at 2025/10/03.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class InitialStateTest
{
    [TestMethod]
    public void VerifyInitialState()
    {
        var state = new InitialState();

        Assert.AreEqual("Initial", state.Name);
    }
}
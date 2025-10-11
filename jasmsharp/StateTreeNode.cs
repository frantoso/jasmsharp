// -----------------------------------------------------------------------
// <copyright file="StateTreeNode.cs">
//     Created by Frank Listing at 2025/10/01.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

/// <summary> Helper class to store information when iterating through states.</summary>
public sealed record StateTreeNode(IState State, List<StateTreeNode> Children);

/// <summary>Helper class to store information when iterating through states.</summary>
public sealed record StateContainerTreeNode(
    IStateContainer<StateBase> Container,
    List<StateContainerTreeNode> Children);
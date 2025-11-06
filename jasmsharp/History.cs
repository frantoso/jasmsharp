// -----------------------------------------------------------------------
// <copyright file="History.cs">
//     Created by Frank Listing at 2025/11/06.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

/// <summary>
///     Helper class to have a well-defined type to support history.
/// </summary>
public abstract class History
{
    /// <summary>Represents no history.</summary>
    public static readonly History None = new NoHistory();

    /// <summary>Represents shallow history.</summary>
    public static readonly History H = new NormalHistory();

    /// <summary>Represents deep history.</summary>
    public static readonly History Hd = new DeepHistory();

    /// <summary>Gets a value indicating whether this instance represents history.</summary>
    public bool IsHistory => object.ReferenceEquals(this, History.H);

    /// <summary>Gets a value indicating whether this instance represents deep history.</summary>
    public bool IsDeepHistory => object.ReferenceEquals(this, History.Hd);

    private sealed class NoHistory : History;

    private sealed class NormalHistory : History;

    private sealed class DeepHistory : History;
}
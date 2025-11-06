// -----------------------------------------------------------------------
// <copyright file="Extensions.cs">
//     Created by Frank Listing at 2025/11/06.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

using System.Diagnostics;
using System.Reflection;

/// <summary>
///     Extension functions used inside the state macine classes.
/// </summary>
public static class Extensions
{
    /// <summary>
    ///     Functional helper: returns the result of invoking <paramref name="func" /> with <paramref name="self" />.
    ///     Useful for fluent transformations where you want to apply a function and continue with its result.
    /// </summary>
    /// <typeparam name="T">Type of the input value.</typeparam>
    /// <typeparam name="TResult">Type of the result value.</typeparam>
    /// <param name="self">The input value to pass to <paramref name="func" />.</param>
    /// <param name="func">Function to invoke.</param>
    /// <returns>The result of <paramref name="func" />(<paramref name="self" />).</returns>
    public static TResult Let<T, TResult>(this T self, Func<T, TResult> func) => func(self);

    /// <summary>
    ///     Functional helper: executes <paramref name="action" /> with <paramref name="self" /> and returns void.
    ///     Useful for performing side effects in a fluent chain.
    /// </summary>
    /// <typeparam name="T">Type of the input value.</typeparam>
    /// <param name="self">The input value to pass to <paramref name="action" />.</param>
    /// <param name="action">Action to execute.</param>
    public static void Run<T>(this T self, System.Action<T> action) => action(self);

    /// <summary>
    ///     Retrieves the closed generic <see cref="MethodInfo" /> for <see cref="Fsm.Start{T}(T)" /> on the concrete FSM
    ///     instance for the given <paramref name="dataType" />.
    ///     If <paramref name="dataType" /> is null the result is null.
    /// </summary>
    /// <param name="fsm">The FSM instance to inspect.</param>
    /// <param name="dataType">The data type to make the generic start method for.</param>
    /// <returns>
    ///     A <see cref="MethodInfo" /> representing the constructed generic <see cref="Fsm.Start{T}(T)" /> for
    ///     <paramref name="dataType" />,
    ///     or null if <paramref name="dataType" /> is null or the method cannot be found.
    /// </returns>
    public static MethodInfo? GetGenericStart(this Fsm fsm, Type? dataType) =>
        dataType?.Let(_ => fsm.GetType().GetMethods()
            .FirstOrDefault(m => m.Name == nameof(Fsm.Start) && m.GetParameters().Length == 1)
            ?.MakeGenericMethod(dataType));

    /// <summary>
    ///     Executes <paramref name="block" /> when the boolean is true and returns the boolean.
    ///     Enables fluent conditional execution like: <c>condition.If(() =&gt; ...).Else(() =&gt; ...)</c>.
    /// </summary>
    /// <param name="b">Condition to evaluate.</param>
    /// <param name="block">Action to execute if <paramref name="b" /> is true.</param>
    /// <returns>The original boolean <paramref name="b" />.</returns>
    public static bool If(this bool b, System.Action block)
    {
        if (b)
        {
            block();
        }

        return b;
    }

    /// <summary>
    ///     Executes <paramref name="block" /> and returns its result when the boolean is true; otherwise returns default.
    ///     Useful for short conditional value construction.
    /// </summary>
    /// <typeparam name="TResult">The return type of the block.</typeparam>
    /// <param name="b">Condition to evaluate.</param>
    /// <param name="block">Function to execute if <paramref name="b" /> is true.</param>
    /// <returns>
    ///     The result of <paramref name="block" /> when <paramref name="b" /> is true; otherwise default(
    ///     <typeparamref name="TResult" />).
    /// </returns>
    public static TResult? LetIf<TResult>(this bool b, Func<TResult> block) => b ? block() : default;

    /// <summary>
    ///     Executes <paramref name="block" /> when the boolean is false.
    ///     Used in combination with <see cref="If(bool, System.Action)" /> to provide an else branch in a fluent style.
    /// </summary>
    /// <param name="b">Condition to evaluate.</param>
    /// <param name="block">Action to execute if <paramref name="b" /> is false.</param>
    public static void Else(this bool b, System.Action block)
    {
        if (!b)
        {
            block();
        }
    }

    /// <summary>
    ///     Logical AND where the second operand is evaluated lazily via <paramref name="condition" />.
    /// </summary>
    /// <param name="b">Left-hand boolean operand.</param>
    /// <param name="condition">Function that provides the right-hand operand when invoked.</param>
    /// <returns>True if both operands are true; otherwise false.</returns>
    public static bool And(this bool b, Func<bool> condition) => b && condition();

    /// <summary>
    ///     Logical OR where the second operand is evaluated lazily via <paramref name="condition" />.
    /// </summary>
    /// <param name="b">Left-hand boolean operand.</param>
    /// <param name="condition">Function that provides the right-hand operand when invoked.</param>
    /// <returns>True if either operand is true; otherwise false.</returns>
    public static bool Or(this bool b, Func<bool> condition) => b || condition();

    /// <summary>
    ///     Iterates over <paramref name="source" /> and invokes <paramref name="action" /> for each element.
    ///     A small convenience to use instead of foreach loops in fluent chains.
    /// </summary>
    /// <typeparam name="T">Element type of <paramref name="source" />.</typeparam>
    /// <param name="source">Sequence to iterate.</param>
    /// <param name="action">Action to execute for each element.</param>
    public static void ForEach<T>(this IEnumerable<T> source, System.Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }
}

/// <summary>
///     Helper class for time measurement.
/// </summary>
public static class Measure
{
    /// <summary>
    ///     Measures the time required to execute <paramref name="block" /> and returns the elapsed milliseconds.
    /// </summary>
    /// <param name="block">The action to measure.</param>
    /// <returns>Elapsed time in milliseconds spent executing <paramref name="block" />.</returns>
    public static long TimeMillis(System.Action block)
    {
        var stopwatch = Stopwatch.StartNew();
        block();
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
}
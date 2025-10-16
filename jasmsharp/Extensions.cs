// -----------------------------------------------------------------------
// <copyright file="Extensions.cs">
//     Created by Frank Listing at 2025/10/03.
// </copyright>
// -----------------------------------------------------------------------

namespace jasmsharp;

using System.Diagnostics;
using System.Reflection;

public static class Extensions
{
    public static TResult Let<T, TResult>(this T self, Func<T, TResult> func) => func(self);

    public static void Run<T>(this T self, System.Action<T> action) => action(self);

    public static MethodInfo? GetGenericStart(this Fsm fsm, Type? dataType) =>
        dataType?.Let(_ => fsm.GetType().GetMethods()
            .FirstOrDefault(m => m.Name == nameof(Fsm.Start) && m.GetParameters().Length == 1)
            ?.MakeGenericMethod(dataType));

    public static bool If(this bool b, System.Action block)
    {
        if (b)
        {
            block();
        }

        return b;
    }

    public static TResult? LetIf<TResult>(this bool b, System.Func<TResult> block) => b ? block() : default;

    public static void Else(this bool b, System.Action block)
    {
        if (!b)
        {
            block();
        }
    }

    public static bool And(this bool b, Func<bool> condition) => b && condition();

    public static bool Or(this bool b, Func<bool> condition) => b || condition();

    public static void ForEach<T>(this IEnumerable<T> source, System.Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }
}


public static class Measure
{
    public static long TimeMillis(System.Action block)
    {
        var stopwatch = Stopwatch.StartNew();
        block();
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
}
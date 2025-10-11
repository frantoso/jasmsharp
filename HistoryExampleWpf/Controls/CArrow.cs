// -----------------------------------------------------------------------
// <copyright file="CArrow.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.Controls;

using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

/// <summary>
///     Draws a curved arrow using a cubic bezier curve.
/// </summary>
/// <seealso cref="HistoryExampleWpf.Controls.QArrow" />
public sealed class CArrow : QArrow
{
    /// <summary>
    ///     A dependency property to get or set the x-coordinate of the line end point.
    /// </summary>
    public static readonly DependencyProperty X4Property = DependencyProperty.Register(
        nameof(CArrow.X4),
        typeof(double),
        typeof(CArrow),
        new FrameworkPropertyMetadata(
            0.0,
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    ///     A dependency property to get or set the y-coordinate of the line end point.
    /// </summary>
    public static readonly DependencyProperty Y4Property = DependencyProperty.Register(
        nameof(CArrow.Y4),
        typeof(double),
        typeof(CArrow),
        new FrameworkPropertyMetadata(
            0.0,
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    ///     Gets or sets the x-coordinate of the line end point.
    /// </summary>
    [TypeConverter(typeof(LengthConverter))]
    public double X4
    {
        get => (double)this.GetValue(CArrow.X4Property);
        set => this.SetValue(CArrow.X4Property, value);
    }

    /// <summary>
    ///     Gets or sets the y-coordinate of the line end point.
    /// </summary>
    [TypeConverter(typeof(LengthConverter))]
    public double Y4
    {
        get => (double)this.GetValue(CArrow.Y4Property);
        set => this.SetValue(CArrow.Y4Property, value);
    }

    /// <summary>
    ///     Draws the arrow geometry.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    protected override void InternalDrawArrowGeometry(StreamGeometryContext context)
    {
        var point1 = new Point(this.X1, this.Y1);
        var point2 = new Point(this.X2, this.Y2);
        var point3 = new Point(this.X3, this.Y3);
        var point4 = new Point(this.X4, this.Y4);

        context.BeginFigure(point1, true, false);
        context.BezierTo(point2, point3, point4, true, true);

        this.DrawHead(context, point3, point4);
    }
}
// -----------------------------------------------------------------------
// <copyright file="Arrow.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.Controls;

using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

/// <summary>
///     Draws a straight arrow.
/// </summary>
/// <seealso cref="System.Windows.Shapes.Shape" />
public class Arrow : Shape
{
    /// <summary>
    ///     A dependency property to get or set the x-coordinate of the line start point.
    /// </summary>
    public static readonly DependencyProperty X1Property = DependencyProperty.Register(
        nameof(Arrow.X1),
        typeof(double),
        typeof(Arrow),
        new FrameworkPropertyMetadata(
            0.0,
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    ///     A dependency property to get or set the y-coordinate of the line start point.
    /// </summary>
    public static readonly DependencyProperty Y1Property = DependencyProperty.Register(
        nameof(Arrow.Y1),
        typeof(double),
        typeof(Arrow),
        new FrameworkPropertyMetadata(
            0.0,
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    ///     A dependency property to get or set the x-coordinate.
    /// Straight arrow: the x-coordinate of the line end point.
    /// Curved arrow: the x-coordinate of the first control point.
    /// </summary>
    public static readonly DependencyProperty X2Property = DependencyProperty.Register(
        nameof(Arrow.X2),
        typeof(double),
        typeof(Arrow),
        new FrameworkPropertyMetadata(
            0.0,
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    ///     A dependency property to get or set the y-coordinate.
    ///     Straight arrow: the y-coordinate of the line end point.
    ///     Curved Arrow: the y-coordinate of the first control point.
    /// </summary>
    public static readonly DependencyProperty Y2Property = DependencyProperty.Register(
        nameof(Arrow.Y2),
        typeof(double),
        typeof(Arrow),
        new FrameworkPropertyMetadata(
            0.0,
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    ///     A dependency property to get or set the width of the arrow head.
    /// </summary>
    public static readonly DependencyProperty HeadWidthProperty = DependencyProperty.Register(
        nameof(Arrow.HeadWidth),
        typeof(double),
        typeof(Arrow),
        new FrameworkPropertyMetadata(
            0.0,
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    ///     A dependency property to get or set the height of the arrow head.
    /// </summary>
    public static readonly DependencyProperty HeadHeightProperty = DependencyProperty.Register(
        nameof(Arrow.HeadHeight),
        typeof(double),
        typeof(Arrow),
        new FrameworkPropertyMetadata(
            0.0,
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    ///     Gets or sets the x-coordinate of the line start point.
    /// </summary>
    [TypeConverter(typeof(LengthConverter))]
    public double X1
    {
        get => (double)this.GetValue(Arrow.X1Property);
        set => this.SetValue(Arrow.X1Property, value);
    }

    /// <summary>
    ///     Gets or sets the y-coordinate of the line start point.
    /// </summary>
    [TypeConverter(typeof(LengthConverter))]
    public double Y1
    {
        get => (double)this.GetValue(Arrow.Y1Property);
        set => this.SetValue(Arrow.Y1Property, value);
    }

    /// <summary>
    ///     Get or sets the x-coordinate.
    /// Straight arrow: the x-coordinate of the line end point.
    /// Curved arrow: the x-coordinate of the first control point.
    /// </summary>
    [TypeConverter(typeof(LengthConverter))]
    public double X2
    {
        get => (double)this.GetValue(Arrow.X2Property);
        set => this.SetValue(Arrow.X2Property, value);
    }

    /// <summary>
    ///     Gets or sets the y-coordinate.
    ///     Straight arrow: the y-coordinate of the line end point.
    ///     Curved Arrow: the y-coordinate of the first control point.
    /// </summary>
    [TypeConverter(typeof(LengthConverter))]
    public double Y2
    {
        get => (double)this.GetValue(Arrow.Y2Property);
        set => this.SetValue(Arrow.Y2Property, value);
    }

    /// <summary>
    ///     Gets or sets the width of the arrow head.
    /// </summary>
    [TypeConverter(typeof(LengthConverter))]
    public double HeadWidth
    {
        get => (double)this.GetValue(Arrow.HeadWidthProperty);
        set => this.SetValue(Arrow.HeadWidthProperty, value);
    }

    /// <summary>
    ///     Gets or sets the height of the arrow head.
    /// </summary>
    [TypeConverter(typeof(LengthConverter))]
    public double HeadHeight
    {
        get => (double)this.GetValue(Arrow.HeadHeightProperty);
        set => this.SetValue(Arrow.HeadHeightProperty, value);
    }

    /// <summary>
    ///     Gets a value that represents the <see cref="T:System.Windows.Media.Geometry" /> of the
    ///     <see cref="T:System.Windows.Shapes.Shape" />.
    /// </summary>
    protected override Geometry DefiningGeometry
    {
        get
        {
            // Create a StreamGeometry to describe the shape
            var geometry = new StreamGeometry
            {
                FillRule = FillRule.EvenOdd
            };

            using (var context = geometry.Open())
            {
                this.InternalDrawArrowGeometry(context);
            }

            // Freeze the geometry for performance benefits
            geometry.Freeze();

            return geometry;
        }
    }

    /// <summary>
    ///     Draws the head of the arrow.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="startPoint">The start point.</param>
    /// <param name="endPoint">The end point.</param>
    protected void DrawHead(StreamGeometryContext context, Point startPoint, Point endPoint)
    {
        var theta = Math.Atan2(startPoint.Y - endPoint.Y, startPoint.X - endPoint.X);
        var sinT = Math.Sin(theta);
        var cosT = Math.Cos(theta);

        var pointHead1 = new Point(
            endPoint.X + (this.HeadWidth * cosT - this.HeadHeight * sinT),
            endPoint.Y + (this.HeadWidth * sinT + this.HeadHeight * cosT));

        var pointHead2 = new Point(
            endPoint.X + (this.HeadWidth * cosT + this.HeadHeight * sinT),
            endPoint.Y - (this.HeadHeight * cosT - this.HeadWidth * sinT));

        context.BeginFigure(pointHead1, true, false);
        context.LineTo(endPoint, true, false);
        context.LineTo(pointHead2, true, true);
    }

    /// <summary>
    ///     Draws the arrow geometry.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    protected virtual void InternalDrawArrowGeometry(StreamGeometryContext context)
    {
        var point1 = new Point(this.X1, this.Y1);
        var point2 = new Point(this.X2, this.Y2);

        context.BeginFigure(point1, true, false);
        context.LineTo(point2, true, true);

        this.DrawHead(context, point1, point2);
    }
}
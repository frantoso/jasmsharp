// -----------------------------------------------------------------------
// <copyright file="QArrow.cs">
//     Created by Frank Listing at 2025/10/07.
// </copyright>
// -----------------------------------------------------------------------

namespace HistoryExampleWpf.Controls;

    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    ///     Draws a curved arrow using a quadratic Bézier curve.
    /// </summary>
    /// <seealso cref="HistoryExampleWpf.Controls.Arrow" />
    public class QArrow : Arrow
    {
        /// <summary>
        ///     A dependency property to get or set the x-coordinate of the line end point (quadratic arrow) or the x-coordinate of
        ///     the second control point (Bézier curve).
        /// </summary>
        public static readonly DependencyProperty X3Property = DependencyProperty.Register(
            nameof(QArrow.X3),
            typeof(double),
            typeof(QArrow),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     A dependency property to get or set the y-coordinate of the line end point (quadratic arrow) or the y-coordinate of
        ///     the second control point (Bézier curve).
        /// </summary>
        public static readonly DependencyProperty Y3Property = DependencyProperty.Register(
            nameof(QArrow.Y3),
            typeof(double),
            typeof(QArrow),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the x-coordinate of the line end point (quadratic arrow) or the x-coordinate of the second control
        ///     point (Bézier curve).
        /// </summary>
        [TypeConverter(typeof(LengthConverter))]
        public double X3
        {
            get => (double)this.GetValue(QArrow.X3Property);
            set => this.SetValue(QArrow.X3Property, value);
        }

        /// <summary>
        ///     Gets or sets the y-coordinate of the line end point (quadratic arrow) or the y-coordinate of the second control
        ///     point (Bézier curve).
        /// </summary>
        [TypeConverter(typeof(LengthConverter))]
        public double Y3
        {
            get => (double)this.GetValue(QArrow.Y3Property);
            set => this.SetValue(QArrow.Y3Property, value);
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

            context.BeginFigure(point1, true, false);
            context.QuadraticBezierTo(point2, point3, true, true);

            this.DrawHead(context, point2, point3);
        }
    }
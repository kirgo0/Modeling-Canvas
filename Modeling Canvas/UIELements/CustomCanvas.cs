using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Modeling_Canvas.UIELements
{

    public class CustomCanvas : Canvas
    {
        public double UnitSize
        {
            get => (double)GetValue(UnitSizeProperty);
            set => SetValue(UnitSizeProperty, value);
        }

        public static readonly DependencyProperty UnitSizeProperty =
            DependencyProperty.Register(nameof(UnitSize), typeof(double), typeof(CustomCanvas),
                new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public int GridFrequency
        {
            get => (int)GetValue(GridFrequencyProperty);
            set => SetValue(GridFrequencyProperty, value);
        }

        public static readonly DependencyProperty GridFrequencyProperty =
            DependencyProperty.Register(nameof(GridFrequency), typeof(int), typeof(CustomCanvas),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender));

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            DrawCoordinateGrid(dc);
        }
        public bool GridSnapping { get; set; } = false; // Enable grid snapping by default
        // Calculate grid lines for snapping

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double centerX = arrangeSize.Width / 2;
            double centerY = arrangeSize.Height / 2;

            foreach (UIElement child in InternalChildren)
            {
                if (child is CustomElement element)
                {
                    var point = element.GetOriginPoint(arrangeSize);
                    //var point = new Point(0, 0);
                    // Arrange the element
                    element.Arrange(new Rect(point, element.DesiredSize));
                }
            }

            return arrangeSize;
        }

        protected void DrawCoordinateGrid(DrawingContext dc)
        {
            double width = ActualWidth;
            double height = ActualHeight;
            double halfWidth = width / 2;
            double halfHeight = height / 2;

            Pen gridPen = new Pen(Brushes.Black, 0.1);
            Pen axisPen = new Pen(Brushes.Black, 2);

            // Draw grid
            if (UnitSize > 0 && GridFrequency > 0)
            {
                // Vertical lines and coordinates
                for (double x = halfWidth; x < width; x += UnitSize * GridFrequency)
                {
                    dc.DrawLine(gridPen, new Point(x, 0), new Point(x, height));
                }
                for (double x = halfWidth; x > 0; x -= UnitSize * GridFrequency)
                {
                    dc.DrawLine(gridPen, new Point(x, 0), new Point(x, height));
                }

                // Horizontal lines and coordinates
                for (double y = halfHeight; y < height; y += UnitSize * GridFrequency)
                {
                    dc.DrawLine(gridPen, new Point(0, y), new Point(width, y));
                }
                for (double y = halfHeight; y > 0; y -= UnitSize * GridFrequency)
                {
                    dc.DrawLine(gridPen, new Point(0, y), new Point(width, y));
                }
            }

            // Draw axes
            dc.DrawLine(axisPen, new Point(0, halfHeight), new Point(width, halfHeight)); // X-axis
            dc.DrawLine(axisPen, new Point(halfWidth, 0), new Point(halfWidth, height)); // Y-axis

            // Draw labels
            FormattedText xLabel = new FormattedText(
                "X",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                14,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            FormattedText yLabel = new FormattedText(
                "Y",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                14,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            // Position X and Y labels at the ends of the axes
            dc.DrawText(xLabel, new Point(width - xLabel.Width - 5, halfHeight - xLabel.Height - 5)); // X-axis label
            dc.DrawText(yLabel, new Point(halfWidth + 5, 5)); // Y-axis label
        }

    }
}


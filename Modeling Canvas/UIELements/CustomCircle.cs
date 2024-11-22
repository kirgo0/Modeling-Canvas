using System.Windows.Media;
using System.Windows;

namespace Modeling_Canvas.UIELements
{

    public class CustomCircle : CustomElement
    {
        public double Radius { get; set; } = 5; // Default radius
        public int Precision { get; set; } = 100; // Number of points for the circle
        public double SegmentStartDegrees { get; set; } = 0; // Start angle in degrees
        public double SegmentEndDegrees { get; set; } = 360; // End angle in degrees

        public Point Center { get; set; } = new Point(1,1);

        public CustomCircle()
        {
            if(Center.X % 1 == 0 && Center.Y % 1 == 0)
            {
                LastGridSnappedPoint = Center;
            }
        }

        // Properties for the center dot
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // Get the center of the element
            // Create a geometry for the circle segment
            var geometry = CreateCircleSegmentGeometry(new Point(0, 0), Radius * UnitSize, SegmentStartDegrees, SegmentEndDegrees, Precision);

            // Draw the circle segment
            drawingContext.DrawGeometry(Fill, new Pen(Stroke, StrokeThickness), geometry);

            // Draw the center dot
            drawingContext.DrawGeometry(Brushes.Black, new Pen(Brushes.Black, 1), CreateCircleSegmentGeometry(new Point(0, 0), 4, 0, 360, Precision));
        }


        private Geometry CreateCircleSegmentGeometry(Point center, double radius, double startDegrees, double endDegrees, int precision)
        {
            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                // Convert degrees to radians
                double startRadians = Math.PI * startDegrees / 180.0;
                double endRadians = Math.PI * endDegrees / 180.0;

                // Calculate the step size for each segment
                double segmentStep = (endRadians - startRadians) / precision;

                // Calculate the start point
                var startPoint = new Point(
                    center.X + radius * Math.Cos(startRadians),
                    center.Y + radius * Math.Sin(startRadians));

                context.BeginFigure(startPoint, true, false); // Is filled, Is closed

                // Add points for the segment
                for (int i = 1; i <= precision; i++)
                {
                    double angle = startRadians + i * segmentStep;
                    var point = new Point(
                        center.X + radius * Math.Cos(angle),
                        center.Y + radius * Math.Sin(angle));

                    context.LineTo(point, true, false); // Line to the calculated point
                }
            }
            
            geometry.Freeze(); // Freeze for performance
            return geometry;
        }
        public override Point GetOriginPoint(Size arrangedSize)
        {
            return new Point(arrangedSize.Width / 2  + UnitSize * Center.X, arrangedSize.Height / 2 - UnitSize * Center.Y);
        }

        // Override MeasureOverride and ArrangeOverride to size the element
        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size((Radius * UnitSize + StrokeThickness) * 2 , (Radius * UnitSize + StrokeThickness) * 2);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }

        protected override void MoveElement(Vector offset)
        {
            if(Canvas.IsSpacePressed)
            {
                return;
            }
            Point newCenter = new Point(
                Center.X + offset.X / UnitSize,
                Center.Y - offset.Y / UnitSize
                );

            if(Canvas.GridSnapping)
            {
                // calculate last grid lines to snap
                Point oldCenterRounded = new Point(
                    Round(Center.X),
                    Round(Center.Y)
                );

                // check if new and old grid lines to snap are the same
                if (Round(newCenter.X) == oldCenterRounded.X && 
                    Round(newCenter.Y) == oldCenterRounded.Y)
                {
                    // don't snap
                    Center = new Point(Center.X + offset.X / UnitSize, Center.Y - offset.Y / UnitSize);
                } else
                {
                    // if not snap to new line
                    Center = new Point(Round(newCenter.X), Round(newCenter.Y));
                }
            }
            else
            {
                Center = newCenter;
            }
            
            // Update the circle's center position
            InvalidateCanvas();
        }

        public override string ToString()
        {
            return $"X: {Center.X} \nY: {Center.Y} \n| Radius: {Radius}";
        }

        public static double Round(double value)
        {
            // Define thresholds
            const double lowerThreshold = 0.1;
            const double upperThreshold = 0.9;
            const double toHalfLower = 0.45;
            const double toHalfUpper = 0.55;

            // Extract the integer and fractional parts
            double integerPart = Math.Floor(value);
            double fractionalPart = value - integerPart;

            if (fractionalPart <= lowerThreshold)
            {
                // Round down to the nearest integer
                return integerPart;
            }
            else if (fractionalPart >= upperThreshold)
            {
                // Round up to the nearest integer
                return integerPart + 1;
            }
            else if (fractionalPart >= toHalfLower && fractionalPart <= toHalfUpper)
            {
                // Round to .5
                return integerPart + 0.5;
            }
            else
            {
                // Keep the original value
                return value;
            }
        }


    }
}
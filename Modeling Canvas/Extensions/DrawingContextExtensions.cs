using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.Extensions
{
    public static class DrawingContextExtensions
    {
        #region lines
        public static void DrawLine(this DrawingContext dc, Pen pen, Point p1, Point p2, double transparentThickness)
        {
            //var pen = new Pen(Stroke, StrokeThickness);
            dc.DrawLine(pen, p1, p2);
            dc.DrawLine(new Pen(Brushes.Transparent, pen.Thickness + transparentThickness), p1, p2);
        }

        public static void DrawDashedLine(this DrawingContext dc, Pen pen, Point p1, Point p2, double transparentThickness)
        {
            dc.DrawLine(pen, p1, p2);
            dc.DrawLine(new Pen(Brushes.Transparent, pen.Thickness + transparentThickness), p1, p2);
        }
        #endregion

        #region points
        public static void DrawCircle(this DrawingContext dc, Brush fill, Pen strokePen, Point center, double radius, int precision)
        {
            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                // Calculate the step size for each segment in radians
                double segmentStep = (2 * Math.PI) / precision;

                // Calculate the start point
                var startPoint = new Point(
                    center.X + radius * Math.Cos(0),
                    center.Y + radius * Math.Sin(0)
                    );

                context.BeginFigure(startPoint, true, true); // Is filled, Is closed

                // Add points for the segments
                for (int i = 1; i <= precision; i++)
                {
                    double angle = i * segmentStep; // Angle in radians
                    var point = new Point(
                        center.X + radius * Math.Cos(angle),
                        center.Y + radius * Math.Sin(angle));

                    context.LineTo(point, true, false); // Line to the calculated point
                }
            }

            geometry.Freeze(); // Freeze for performance
            dc.DrawGeometry(fill, strokePen, geometry);
        }

        public static void DrawAnchorPoint(this DrawingContext dc, Brush fill, Pen strokePen, Point center, double radius, int precision, double lineLength)
        {
            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                // Calculate the step size for each segment in radians
                double segmentStep = (2 * Math.PI) / precision;

                // Calculate the start point of the circle
                var startPoint = new Point(
                    center.X + radius * Math.Cos(0),
                    center.Y + radius * Math.Sin(0));

                context.BeginFigure(startPoint, true, true); // Is filled, Is closed

                // Add points for the circle
                for (int i = 1; i <= precision; i++)
                {
                    double angle = i * segmentStep; // Angle in radians
                    var point = new Point(
                        center.X + radius * Math.Cos(angle),
                        center.Y + radius * Math.Sin(angle));

                    context.LineTo(point, true, false); // Line to the calculated point
                }

                // Draw the short lines at 0, 90, 180, and 270 degrees
                double[] angles = { 0, Math.PI / 2, Math.PI, 3 * Math.PI / 2 }; // Angles in radians
                foreach (var angle in angles)
                {
                    // Calculate the start and end points of the line
                    var lineStart = new Point(
                        center.X + radius * Math.Cos(angle),
                        center.Y + radius * Math.Sin(angle));

                    var lineEnd = new Point(
                        center.X + (radius + lineLength) * Math.Cos(angle),
                        center.Y + (radius + lineLength) * Math.Sin(angle));

                    // Move to the start of the line and draw it
                    context.BeginFigure(lineStart, false, false);
                    context.LineTo(lineEnd, true, false);
                }
            }

            geometry.Freeze(); // Freeze for performance
            dc.DrawGeometry(fill, strokePen, geometry);
        }

        public static void DrawSquare(this DrawingContext dc, Brush fill, Pen strokePen, Point center, double sideLength)
        {
            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                // Half the side length for easier calculations
                double halfSide = sideLength / 2;

                // Calculate the corners of the square
                var topLeft = new Point(center.X - halfSide, center.Y - halfSide);
                var topRight = new Point(center.X + halfSide, center.Y - halfSide);
                var bottomRight = new Point(center.X + halfSide, center.Y + halfSide);
                var bottomLeft = new Point(center.X - halfSide, center.Y + halfSide);

                // Begin the square path
                context.BeginFigure(topLeft, true, true); // Is filled, Is closed
                context.LineTo(topRight, true, false);
                context.LineTo(bottomRight, true, false);
                context.LineTo(bottomLeft, true, false);
                context.LineTo(topLeft, true, false); // Close the square

                // Draw the short lines at the middle of each side
                var midTop = new Point(center.X, center.Y - halfSide);
                var midRight = new Point(center.X + halfSide, center.Y);
                var midBottom = new Point(center.X, center.Y + halfSide);
                var midLeft = new Point(center.X - halfSide, center.Y);
            }

            geometry.Freeze(); // Freeze for performance
            dc.DrawGeometry(fill, strokePen, geometry);
        }

        #endregion

        #region custom circle 
        public static void DrawCircleWithArcs(
            this DrawingContext dc,
            Brush fill,
            Pen strokePen,
            Point center,
            double radius,
            double startDegrees,
            double endDegrees,
            int precision,
            double transparentThickness
            )
        {
            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                double normalizedStart = Helpers.NormalizeAngle(startDegrees);
                double normalizedEnd = Helpers.NormalizeAngle(endDegrees);

                if (normalizedEnd <= normalizedStart)
                {
                    DrawArcSegment(context, center, radius, normalizedStart, 360, precision);
                    DrawArcSegment(context, center, radius, 0, normalizedEnd, precision);
                }
                else
                {
                    DrawArcSegment(context, center, radius, normalizedStart, normalizedEnd, precision);
                }
            }

            geometry.Freeze();
            dc.DrawGeometry(fill, strokePen, geometry);
            dc.DrawGeometry(null, new Pen(Brushes.Transparent,transparentThickness), geometry);
        }

        private static void DrawArcSegment(StreamGeometryContext context, Point center, double radius, double startDegrees, double endDegrees, int precision)
        {
            double startRadians = Helpers.DegToRad(startDegrees);
            double endRadians = Helpers.DegToRad(endDegrees);

            double segmentStep = (endRadians - startRadians) / precision;

            var startPoint = new Point(
                center.X + radius * Math.Cos(startRadians),
                center.Y + radius * Math.Sin(startRadians));
            context.BeginFigure(startPoint, true, false);

            for (int i = 1; i <= precision; i++)
            {
                double angle = startRadians + i * segmentStep;
                var point = new Point(
                    center.X + radius * Math.Cos(angle),
                    center.Y + radius * Math.Sin(angle));
                context.LineTo(point, true, false);
            }
        }

        #endregion

    }
}

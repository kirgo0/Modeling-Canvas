using Modeling_Canvas.UIElements;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.Extensions
{
    public static class DrawingContextExtensions
    {
        public static void DrawLine(this DrawingContext dc, CustomCanvas canvas, Pen pen, Point p1, Point p2, double transparentThickness = 0)
        {
            p1 = canvas.TransformPoint(p1);
            p2 = canvas.TransformPoint(p2);
            dc.DrawLine(pen, p1, p2);
            dc.DrawLine(new Pen(Brushes.Transparent, pen.Thickness + transparentThickness), p1, p2);
        }

        public static StreamGeometry GetCircleGeometry(this CustomCanvas canvas, Point center, double radius, int precision)
        {
            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                // Calculate the step size for each segment in radians
                double segmentStep = (2 * Math.PI) / precision;

                //if (!applyShapeTransform)
                //    center = canvas.TransformPoint(center);

                // Calculate the start point
                var startPoint = new Point(
                    center.X + radius * Math.Cos(0),
                    center.Y + radius * Math.Sin(0)
                    );

                //if (applyShapeTransform)
                //    startPoint = canvas.TransformPoint(startPoint);

                context.BeginFigure(startPoint, true, true); // Is filled, Is closed

                // Add points for the segments
                for (int i = 1; i <= precision; i++)
                {
                    double angle = i * segmentStep; // Angle in radians
                    var point = new Point(
                        center.X + radius * Math.Cos(angle),
                        center.Y + radius * Math.Sin(angle));

                    //if (applyShapeTransform)
                    //    point = canvas.TransformPoint(point);

                    context.LineTo(point, true, false); // Line to the calculated point
                }
            }

            return geometry;
        }

        public static StreamGeometry GetAnchorGeometry(this CustomCanvas canvas, Point center, double radius, int precision, double lineLength)
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

            return geometry;
        }

        public static StreamGeometry GetSquareGeometry(this CustomCanvas canvas, Point center, double sideLength, bool applyTransform = true)
        {
            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                double halfSide = sideLength / 2;

                var topLeft = new Point(center.X - halfSide, center.Y - halfSide);
                var topRight = new Point(center.X + halfSide, center.Y - halfSide);
                var bottomRight = new Point(center.X + halfSide, center.Y + halfSide);
                var bottomLeft = new Point(center.X - halfSide, center.Y + halfSide);

                context.BeginFigure(topLeft, true, true);
                context.LineTo(topRight, true, false);
                context.LineTo(bottomRight, true, false);
                context.LineTo(bottomLeft, true, false);
                context.LineTo(topLeft, true, false);

            }

            return geometry;
        }

        public static StreamGeometry DrawCircleWithArcs(
            this CustomCanvas canvas,
            Point center,
            double radius,
            double startDegrees,
            double endDegrees,
            int precision
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

            return geometry;
        }

        private static void DrawArcSegment(StreamGeometryContext context, Point center, double radius, double startDegrees, double endDegrees, int precision)
        {
            double startRadians = Helpers.DegToRad(startDegrees);
            double endRadians = Helpers.DegToRad(endDegrees);

            double segmentStep = (endRadians - startRadians) / precision;

            var startPoint = new Point(
                center.X + radius * Math.Cos(startRadians),
                center.Y + radius * Math.Sin(startRadians));

            //startPoint = canvas.TransformPoint(startPoint);

            context.BeginFigure(startPoint, true, false);

            for (int i = 1; i <= precision; i++)
            {
                double angle = startRadians + i * segmentStep;
                var point = new Point(
                    center.X + radius * Math.Cos(angle),
                    center.Y + radius * Math.Sin(angle));

                //point = canvas.TransformPoint(point);

                context.LineTo(point, true, false);
            }
        }

        public static void DrawBezierCurve(this DrawingContext dc, CustomCanvas canvas, Pen pen, Point p0, Point c1, Point c2, Point p3, int steps = 100, double transparentThickness = 0)
        {
            Point prevPoint = p0;

            for (int i = 1; i <= steps; i++)
            {
                double t = i / (double)steps;
                Point currentPoint = CalculateBezierPoint(t, p0, c1, c2, p3);
                dc.DrawLine(canvas, pen, prevPoint, currentPoint, transparentThickness);
                prevPoint = currentPoint;
            }
        }

        private static Point CalculateBezierPoint(double t, Point p0, Point c1, Point c2, Point p3)
        {
            double u = 1 - t;
            double tt = t * t;
            double uu = u * u;
            double uuu = uu * u;
            double ttt = tt * t;

            Point result = new Point
            {
                X = uuu * p0.X + 3 * uu * t * c1.X + 3 * u * tt * c2.X + ttt * p3.X,
                Y = uuu * p0.Y + 3 * uu * t * c1.Y + 3 * u * tt * c2.Y + ttt * p3.Y
            };

            return result;
        }

    }
}

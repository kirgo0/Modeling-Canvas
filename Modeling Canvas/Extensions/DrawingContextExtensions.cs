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

        public static Point[][] GetCircleGeometry(this CustomCanvas canvas, Point center, double radius, int precision)
        {
            var geometryData = new Point[1][]; 

            var circlePoints = new Point[precision + 1]; 

            double segmentStep = (2 * Math.PI) / precision;

            for (int i = 0; i <= precision; i++)
            {
                double angle = i * segmentStep; 
                circlePoints[i] = new Point(
                    center.X + radius * Math.Cos(angle),
                    center.Y + radius * Math.Sin(angle));
            }

            geometryData[0] = circlePoints;

            return geometryData;
        }

        public static Point[][] GetAnchorGeometry(this CustomCanvas canvas, Point center, double radius, int precision, double lineLength)
        {
            var geometryData = new Point[5][];
            geometryData[0] = canvas.GetCircleGeometry(center, radius, precision)[0];

            double[] angles = { 0, Math.PI / 2, Math.PI, 3 * Math.PI / 2 };

            for (int i = 0; i < angles.Length; i++)
            {
                var angle = angles[i];
                var lineStart = new Point(
                    center.X + radius * Math.Cos(angle),
                    center.Y + radius * Math.Sin(angle));

                var lineEnd = new Point(
                    center.X + (radius + lineLength) * Math.Cos(angle),
                    center.Y + (radius + lineLength) * Math.Sin(angle));
                geometryData[i + 1] = new[] { lineStart, lineEnd };
            } 

            return geometryData;
        }

        public static Point[][] GetSquarePointGeometry(this CustomCanvas canvas, Point center, double sideLength, bool applyTransform = true)
        {
            var geometryData = new Point[1][];

            // Обчислення координат вершин квадрату
            double halfSide = sideLength / 2;

            var topLeft = new Point(center.X - halfSide, center.Y - halfSide);
            var topRight = new Point(center.X + halfSide, center.Y - halfSide);
            var bottomRight = new Point(center.X + halfSide, center.Y + halfSide);
            var bottomLeft = new Point(center.X - halfSide, center.Y + halfSide);

            geometryData[0] = new[] { topLeft, topRight, bottomRight, bottomLeft, topLeft}; 

            return geometryData;
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

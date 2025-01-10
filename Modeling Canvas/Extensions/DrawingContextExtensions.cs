using Modeling_Canvas.Enums;
using Modeling_Canvas.UIElements;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.Extensions
{
    public static class DrawingContextExtensions
    {
        public static Point[][] GetCircleGeometry(this CustomCanvas canvas, Point center, double radius, double startDegrees = 0, double endDegrees = 360, int precision = 100)
        {
            double startRadians = Helpers.DegToRad(startDegrees);
            double endRadians = Helpers.DegToRad(endDegrees);

            var geometryData = new Point[1][];

            var circlePoints = new Point[precision + 1];

            double segmentStep = (endRadians - startRadians) / precision;

            for (int i = 0; i <= precision; i++)
            {
                double angle = startRadians + i * segmentStep;
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
            geometryData[0] = canvas.GetCircleGeometry(center, radius, precision: precision)[0];

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

            double halfSide = sideLength / 2;

            var topLeft = new Point(center.X - halfSide, center.Y - halfSide);
            var topRight = new Point(center.X + halfSide, center.Y - halfSide);
            var bottomRight = new Point(center.X + halfSide, center.Y + halfSide);
            var bottomLeft = new Point(center.X - halfSide, center.Y + halfSide);

            geometryData[0] = new[] { topLeft, topRight, bottomRight, bottomLeft, topLeft };

            return geometryData;
        }

        public static Point[][] GetCircleSegmentGeometry(
            this CustomCanvas canvas,
            Point center,
            double radius,
            double startDegrees,
            double endDegrees,
            int precision
            )
        {
            var circleList = new List<Point>();
            double normalizedStart = Helpers.NormalizeAngle(startDegrees);
            double normalizedEnd = Helpers.NormalizeAngle(endDegrees);

            if (normalizedEnd <= normalizedStart)
            {
                circleList.AddRange(canvas.GetCircleGeometry(center, radius, normalizedStart, 360, precision)[0]);
                circleList.AddRange(canvas.GetCircleGeometry(center, radius, 0, normalizedEnd, precision)[0]);
                return [circleList.ToArray()];
            }
            else
            {
                return canvas.GetCircleGeometry(center, radius, normalizedStart, normalizedEnd, precision);
            }
        }
        
        public static Point[][] GetBezierCurveGeometry(this CustomCanvas canvas, List<BezierPoint> points, bool isClosed = false, int steps = 100)
        {
            var lineList = new List<Point>();
            for(var i = 0; i < points.Count - 1; i++)
            {
                lineList.AddRange(CalculateBezierCurve(points[i], points[i+1], steps));
            }
            if(isClosed)
            {
                lineList.AddRange(CalculateBezierCurve(points.LastOrDefault(),points.FirstOrDefault(),steps));
            }
            return new Point[][] { lineList.ToArray() };
        }

        private static Point[] CalculateBezierCurve(BezierPoint start, BezierPoint end, int steps)
        {
            if (start is null || end is null) return new Point[0];
            var curvePoints = new Point[steps];
            for (int j = 1; j <= steps; j++)
            {
                double t = j / (double)steps;
                Point currentPoint = CalculateBezierPoint(t, start.Position, start.ControlNextPoint.Position, end.ControlPrevPoint.Position, end.Position);
                curvePoints[j - 1] = currentPoint;
            }
            return curvePoints;
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

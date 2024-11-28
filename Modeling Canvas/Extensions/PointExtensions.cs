using Modeling_Canvas.UIELements;
using System.Windows;

namespace Modeling_Canvas.Extensions
{
    public static class PointExtensions
    {
        public static CustomCanvas Canvas { get; set; }
        public static Point RotatePoint(this Point point1, Point point2, double degrees)
        {
            // Calculate rotation
            double dx = point1.X - point2.X;
            double dy = point1.Y - point2.Y;
            double radians = Helpers.DegToRad(degrees);

            double rotatedX = Math.Cos(radians) * dx - Math.Sin(radians) * dy + point2.X;
            double rotatedY = Math.Sin(radians) * dx + Math.Cos(radians) * dy + point2.Y;

            // Update point position
            return new Point(Math.Round(rotatedX, Canvas.RotationPrecision), Math.Round(rotatedY, Canvas.RotationPrecision));
        }

        public static Point OffsetPoint(this Point point, Vector offset)
        {
            return new Point(
                point.X + offset.X / Canvas.UnitSize,
                point.Y - offset.Y / Canvas.UnitSize
                );
        }

        public static Point OffsetAndSpanPoint(this Point point, Vector offset)
        {
            return new Point(
                Helpers.SnapValue(point.X + offset.X / Canvas.UnitSize),
                Helpers.SnapValue(point.Y - offset.Y / Canvas.UnitSize)
            );
        }

        public static Point ScalePoint(this Point point, Point anchor, Vector scaleVector)
        {
            // Apply scaling transformation
            double newX = anchor.X + scaleVector.X * (point.X - anchor.X);
            double newY = anchor.Y + scaleVector.Y * (point.Y - anchor.Y);

            // Update the point in the list
            return new Point(newX, newY);
        }
    }
}

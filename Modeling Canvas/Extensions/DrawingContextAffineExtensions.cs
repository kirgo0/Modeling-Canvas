using Modeling_Canvas.Models;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.Extensions
{
    public static class DrawingContextAffineExtensions
    {
        #region lines
        public static void DrawAffineLine(this DrawingContext dc, Pen pen, Point p1, Point p2, AffineModel affine, double transparentThickness)
        {
            // Apply the affine transformation to the points
            Point transformedP1 = p1.ApplyAffineTransformation(affine);
            Point transformedP2 = p2.ApplyAffineTransformation(affine);

            // Draw the transformed line
            dc.DrawLine(pen, transformedP1, transformedP2);
            dc.DrawLine(new Pen(Brushes.Transparent, pen.Thickness + transparentThickness), transformedP1, transformedP2);
        }

        public static void DrawAffineDashedLine(this DrawingContext dc, Pen pen, Point p1, Point p2, AffineModel affine, double transparentThickness)
        {
            // Apply the affine transformation to the points
            Point transformedP1 = p1.ApplyAffineTransformation(affine);
            Point transformedP2 = p2.ApplyAffineTransformation(affine);

            // Draw the transformed dashed line
            dc.DrawLine(pen, transformedP1, transformedP2);
            dc.DrawLine(new Pen(Brushes.Transparent, pen.Thickness + transparentThickness), transformedP1, transformedP2);
        }
        #endregion

        #region points
        public static void DrawAffineCircle(this DrawingContext dc, Brush fill, Pen strokePen, Point center, double radius, int precision, AffineModel affine)
        {
            // Transform the center point
            Point transformedCenter = center.ApplyAffineTransformation(affine);

            // Draw the transformed circle
            DrawingContextExtensions.DrawCircle(dc, fill, strokePen, transformedCenter, radius, precision);
        }

        public static void DrawAffineAnchorPoint(this DrawingContext dc, Brush fill, Pen strokePen, Point center, double radius, int precision, double lineLength, AffineModel affine)
        {
            // Transform the center point
            Point transformedCenter = center.ApplyAffineTransformation(affine);

            // Draw the transformed anchor point
            DrawingContextExtensions.DrawAnchorPoint(dc, fill, strokePen, transformedCenter, radius, precision, lineLength);
        }

        public static void DrawAffineSquare(this DrawingContext dc, Brush fill, Pen strokePen, Point center, double sideLength, AffineModel affine)
        {
            var transformedCenter = center.ApplyAffineTransformation(affine);
            // Draw the transformed square
            DrawingContextExtensions.DrawSquare(dc, fill, strokePen, transformedCenter, sideLength);
        }
        #endregion

        #region custom circle 
        public static void DrawAffineCircleWithArcs(
            this DrawingContext dc,
            Brush fill,
            Pen strokePen,
            Point center,
            double radius,
            double startDegrees,
            double endDegrees,
            int precision,
            AffineModel affine,
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
                    DrawArcSegment(context, center, radius, normalizedStart, 360, precision, affine);
                    DrawArcSegment(context, center, radius, 0, normalizedEnd, precision, affine);
                }
                else
                {
                    DrawArcSegment(context, center, radius, normalizedStart, normalizedEnd, precision, affine);
                }
            }

            geometry.Freeze();
            dc.DrawGeometry(fill, strokePen, geometry);
            dc.DrawGeometry(null, new Pen(Brushes.Transparent, transparentThickness), geometry);
        }

        private static void DrawArcSegment(StreamGeometryContext context, Point center, double radius, double startDegrees, double endDegrees, int precision, AffineModel affine)
        {
            double startRadians = Helpers.DegToRad(startDegrees);
            double endRadians = Helpers.DegToRad(endDegrees);

            double segmentStep = (endRadians - startRadians) / precision;

            // Calculate the start point and apply the affine transformation
            var startPoint = new Point(
                center.X + radius * Math.Cos(startRadians),
                center.Y + radius * Math.Sin(startRadians)).ApplyAffineTransformation(affine);

            context.BeginFigure(startPoint, true, false); // Is not filled, Is open

            // Recalculate each point in the arc with affine transformation
            for (int i = 1; i <= precision; i++)
            {
                double angle = startRadians + i * segmentStep;
                var point = new Point(
                    center.X + radius * Math.Cos(angle),
                    center.Y + radius * Math.Sin(angle)).ApplyAffineTransformation(affine);

                context.LineTo(point, true, false); // Line to the calculated point
            }
        }
        #endregion
    }
}

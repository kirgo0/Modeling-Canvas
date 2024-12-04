using Modeling_Canvas.Models;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.Extensions
{
    public static class DrawingContextProjectiveExtensions
    {
        #region lines
        public static void DrawProjectiveLine(this DrawingContext dc, Pen pen, Point p1, Point p2, ProjectiveModel projective, double transparentThickness)
        {
            // Apply the projective transformation to the points
            Point transformedP1 = p1.ApplyProjectiveTransformation(projective);
            Point transformedP2 = p2.ApplyProjectiveTransformation(projective);

            // Draw the transformed line
            dc.DrawLine(pen, transformedP1, transformedP2);
            dc.DrawLine(new Pen(Brushes.Transparent, pen.Thickness + transparentThickness), transformedP1, transformedP2);
        }

        public static void DrawProjectiveDashedLine(this DrawingContext dc, Pen pen, Point p1, Point p2, ProjectiveModel projective, double transparentThickness)
        {
            // Apply the projective transformation to the points
            Point transformedP1 = p1.ApplyProjectiveTransformation(projective);
            Point transformedP2 = p2.ApplyProjectiveTransformation(projective);

            // Draw the transformed dashed line
            dc.DrawLine(pen, transformedP1, transformedP2);
            dc.DrawLine(new Pen(Brushes.Transparent, pen.Thickness + transparentThickness), transformedP1, transformedP2);
        }
        #endregion

        #region points
        public static void DrawProjectiveCircle(this DrawingContext dc, Brush fill, Pen strokePen, Point center, double radius, int precision, ProjectiveModel projective, double transparentThickness = 0)
        {
            // Transform the center point
            Point transformedCenter = center.ApplyProjectiveTransformation(projective);

            // Draw the transformed circle
            DrawingContextExtensions.DrawCircle(dc, fill, strokePen, transformedCenter, radius, precision);
        }

        public static void DrawProjectiveAnchorPoint(this DrawingContext dc, Brush fill, Pen strokePen, Point center, double radius, int precision, double lineLength, ProjectiveModel projective)
        {
            // Transform the center point
            Point transformedCenter = center.ApplyProjectiveTransformation(projective);

            // Draw the transformed anchor point
            DrawingContextExtensions.DrawAnchorPoint(dc, fill, strokePen, transformedCenter, radius, precision, lineLength);
        }

        public static void DrawProjectiveSquare(this DrawingContext dc, Brush fill, Pen strokePen, Point center, double sideLength, ProjectiveModel projective)
        {
            // Transform the center point
            Point transformedCenter = center.ApplyProjectiveTransformation(projective);

            // Draw the transformed square
            DrawingContextExtensions.DrawSquare(dc, fill, strokePen, transformedCenter, sideLength);
        }
        #endregion

        #region custom circle 
        public static void DrawProjectiveCircleWithArcs(
            this DrawingContext dc,
            Brush fill,
            Pen strokePen,
            Point center,
            double radius,
            double startDegrees,
            double endDegrees,
            int precision,
            ProjectiveModel projective,
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
                    DrawArcSegment(context, center, radius, normalizedStart, 360, precision, projective);
                    DrawArcSegment(context, center, radius, 0, normalizedEnd, precision, projective);
                }
                else
                {
                    DrawArcSegment(context, center, radius, normalizedStart, normalizedEnd, precision, projective);
                }
            }

            geometry.Freeze();
            dc.DrawGeometry(fill, strokePen, geometry);
            dc.DrawGeometry(null, new Pen(Brushes.Transparent, transparentThickness), geometry);
        }

        private static void DrawArcSegment(StreamGeometryContext context, Point center, double radius, double startDegrees, double endDegrees, int precision, ProjectiveModel projective)
        {
            double startRadians = Helpers.DegToRad(startDegrees);
            double endRadians = Helpers.DegToRad(endDegrees);

            double segmentStep = (endRadians - startRadians) / precision;

            // Calculate the start point and apply the affine transformation
            var startPoint = new Point(
                center.X + radius * Math.Cos(startRadians),
                center.Y + radius * Math.Sin(startRadians)).ApplyProjectiveTransformation(projective);

            context.BeginFigure(startPoint, true, false); // Is not filled, Is open

            // Recalculate each point in the arc with affine transformation
            for (int i = 1; i <= precision; i++)
            {
                double angle = startRadians + i * segmentStep;
                var point = new Point(
                    center.X + radius * Math.Cos(angle),
                    center.Y + radius * Math.Sin(angle)).ApplyProjectiveTransformation(projective);

                context.LineTo(point, true, false); // Line to the calculated point
            }
        }
        #endregion
    }
}

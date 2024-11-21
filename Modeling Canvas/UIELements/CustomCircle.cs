using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        // Properties for the center dot
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // Get the center of the element
            // Create a geometry for the circle segment
            var geometry = CreateCircleSegmentGeometry(Center, Radius * UnitSize, SegmentStartDegrees, SegmentEndDegrees, Precision);

            // Draw the circle segment
            drawingContext.DrawGeometry(Fill, new Pen(Stroke, StrokeThickness), geometry);

            // Draw the center dot
            drawingContext.DrawGeometry(Brushes.Black, new Pen(Brushes.Black, 1), CreateCircleSegmentGeometry(Center, 4, 0, 360, Precision));
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
            return new Point(arrangedSize.Width / 2  + (UnitSize-1) * Center.X, arrangedSize.Height / 2 - (UnitSize+1) * Center.Y);
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
    }
}
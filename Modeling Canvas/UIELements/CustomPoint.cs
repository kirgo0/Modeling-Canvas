using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;


namespace Modeling_Canvas.UIELements
{
    public class CustomPoint : CustomElement
    {
        public double Radius { get; set; } = 10;
        public PointShape Shape { get; set; } = PointShape.Circle;

        private Point _position;
        public Point Position
        {
            get => _position;
            set
            {
                _position = value;
                InvalidateVisual();
            }
        }

        public CustomPoint(CustomCanvas canvas) : base(canvas)
        {
            Fill = Brushes.Black;
            StrokeThickness = 0;
            IsSelectable = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var semiTransparentFill = Fill.Clone();
            semiTransparentFill.Opacity = Opacity;

            switch(Shape)
            {
                case PointShape.Circle:
                    drawingContext.DrawGeometry(semiTransparentFill, new Pen(Stroke, StrokeThickness),
                        CreateCircleGeometry(new Point(0, 0), Radius, 100));
                    break;
                case PointShape.Square:
                    drawingContext.DrawGeometry(semiTransparentFill, new Pen(Stroke, StrokeThickness),
                        CreateSquareWithLinesGeometry(new Point(0, 0), Radius*2, 2));
                    break;
                case PointShape.Anchor:
                    drawingContext.DrawGeometry(semiTransparentFill, new Pen(Stroke, StrokeThickness),
                        CreateCircleWithLinesGeometry(new Point(0, 0), Radius, 100, 5));
                    break;
            }
        }

        protected Geometry CreateCircleGeometry(Point center, double radius, int precision)
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
            return geometry;
        }

        public override Point GetTopLeftPosition()
        {
            return new Point(Position.X + Radius / UnitSize, Position.Y + Radius / UnitSize);
        }
        public override Point GetBottomRightPosition()
        {
            return new Point(Position.X - Radius / UnitSize, Position.Y - Radius / UnitSize);
        }
        protected Geometry CreateCircleWithLinesGeometry(Point center, double radius, int precision, double lineLength)
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
            return geometry;
        }

        protected Geometry CreateSquareWithLinesGeometry(Point center, double sideLength, double lineLength)
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
            return geometry;
        }

        // Helper method to draw a line
        private void DrawLine(StreamGeometryContext context, Point start, Point end)
        {
            context.BeginFigure(start, false, false);
            context.LineTo(end, true, false);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(Radius, Radius);
        }

        public override Point GetOriginPoint(Size arrangedSize)
        {
            return new Point(arrangedSize.Width / 2 + UnitSize * Position.X, arrangedSize.Height / 2 - UnitSize * Position.Y);
        }

        public override void MoveElement(Vector offset)
        {
        }
        public override void RotateElement(Point anchorPoint, double degrees)
        {
        }

        public override string ToString()
        {
            return $"Point\nX: {Position.X}\nY: {Position.Y}";
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
        }
    }
}

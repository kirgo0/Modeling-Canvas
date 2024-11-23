using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;


namespace Modeling_Canvas.UIELements
{
    public class CustomPoint : CustomElement
    {
        public double Radius { get; set; } = 10;

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
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var semiTransparentFill = Fill.Clone();
            semiTransparentFill.Opacity = Opacity;

            drawingContext.DrawGeometry(semiTransparentFill, new Pen(Stroke, StrokeThickness), 
                CreateCircleSegmentGeometry(new Point(0, 0), Radius, 20));
        }

        private Geometry CreateCircleSegmentGeometry(Point center, double radius, int precision)
        {
            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {

                // Calculate the step size for each segment
                double segmentStep = 360 / precision;

                // Calculate the start point
                var startPoint = new Point(
                    center.X + radius * Math.Cos(0),
                    center.Y + radius * Math.Sin(0));


                context.BeginFigure(startPoint, true, true); // Is filled, Is closed

                // Add points for the segment
                for (int i = 1; i <= precision + 1; i++)
                {
                    double angle = 0 + i * segmentStep;
                    var point = new Point(
                        center.X + radius * Math.Cos(angle),
                        center.Y + radius * Math.Sin(angle));

                    context.LineTo(point, true, true); // Line to the calculated point
                }
            }

            geometry.Freeze(); // Freeze for performance
            return geometry;
        }

        private double DegToRad(double deg)
        {
            return Math.PI * deg / 180.0;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(Radius, Radius);
        }

        public override Point GetOriginPoint(Size arrangedSize)
        {
            return new Point(arrangedSize.Width / 2 + UnitSize * Position.X, arrangedSize.Height / 2 - UnitSize * Position.Y);
        }
        public override string ToString()
        {
            return $"Point\nX: {Position.X}\nY: {Position.Y}";
        }

        public Action<Vector> MoveAction;
        protected override void MoveElement(Vector offset)
        {
            MoveAction?.Invoke(offset);
        }
    }
}

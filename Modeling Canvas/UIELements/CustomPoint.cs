using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Media;


namespace Modeling_Canvas.UIELements
{
    public class CustomPoint : CustomElement
    {
        public double Radius { get; set; } = 10;
        public PointShape Shape { get; set; } = PointShape.Circle;

        public int PositionPrecision { get; set; } = 3;

        private Point _position;
        public Point Position
        {
            get => _position;
            set
            {
                _position = new Point(Math.Round(value.X, PositionPrecision), Math.Round(value.Y, PositionPrecision));
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

            switch (Shape)
            {
                case PointShape.Circle:
                    drawingContext.DrawCircle(semiTransparentFill, StrokePen, new Point(0, 0), Radius, 100);
                    break;
                case PointShape.Square:
                    drawingContext.DrawSquare(semiTransparentFill, StrokePen, new Point(0, 0), Radius * 2);
                    break;
                case PointShape.Anchor:
                    drawingContext.DrawAnchorPoint(semiTransparentFill, StrokePen, new Point(0, 0), Radius, 100, 5);
                    break;
            }
        }

        public override Point GetTopLeftPosition()
        {
            return new Point(Position.X + Radius / UnitSize, Position.Y + Radius / UnitSize);
        }
        public override Point GetBottomRightPosition()
        {
            return new Point(Position.X - Radius / UnitSize, Position.Y - Radius / UnitSize);
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

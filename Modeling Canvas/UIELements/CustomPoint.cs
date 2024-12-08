using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using Modeling_Canvas.UIElements.Interfaces;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class CustomPoint : CustomElement, IPoint
    {
        public double Radius { get; set; } = 10;
        public PointShape Shape { get; set; } = PointShape.Circle;
        public int PositionPrecision { get; set; } = 3;
        public double X { get => Position.X; }
        public double Y { get => Position.Y; }
        protected Point _position;
        public Point Position
        {
            get => _position;
            set
            {
                _position = new Point(Math.Round(value.X, PositionPrecision), Math.Round(value.Y, PositionPrecision));
                OnPropertyChanged();
            }
        }

        public Point PixelPosition
        {
            get => new Point(Canvas.ActualWidth / 2 + Position.X * UnitSize, Canvas.ActualHeight / 2 - Position.Y * UnitSize).AddCanvasOffsets();
        }

        public CustomPoint(CustomCanvas canvas, bool hasAnchorPoint = false) : base(canvas, hasAnchorPoint)
        {
            Fill = Brushes.Black;
            StrokeThickness = 0;
            IsSelectable = false;
            LabelText = "Point";
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            var semiTransparentFill = Fill.Clone();
            semiTransparentFill.Opacity = Opacity;

            switch (Shape)
            {
                case PointShape.Circle:
                    dc.DrawCircle(Canvas, semiTransparentFill, StrokePen, PixelPosition, Radius, 100, 0, false);
                    break;
                case PointShape.Square:
                    dc.DrawSquare(Canvas, semiTransparentFill, StrokePen, PixelPosition, Radius * 2, false);
                    break;
                case PointShape.Anchor:
                    dc.DrawAnchorPoint(Canvas, semiTransparentFill, StrokePen, PixelPosition, Radius, 100, 5, false);
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

        public override void RotateElement(Point anchorPoint, double degrees)
        {
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
        }

        public override string ToString()
        {
            return $"Point\nX: {Position.X}\nY: {Position.Y}";
        }

    }
}

using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using Modeling_Canvas.UIElements.Abstract;
using Modeling_Canvas.UIElements.Interfaces;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class CustomPoint : GroupableElement, IPoint
    {
        private StreamGeometry _cachedGeometry;

        private bool _isGeometryDirty = true;

        private double _radius = 0.1;

        private PointShape _shape = PointShape.Circle;

        protected Point _position;

        public int PositionPrecision { get; set; } = 3;

        public Action<MouseButtonEventArgs> MouseLeftButtonDownAction { get; set; }

        public double Radius
        {
            get => _radius;
            set
            {
                if (_radius != value)
                {
                    _radius = value;
                    _isGeometryDirty = true;
                }
            }
        }

        public PointShape Shape
        {
            get => _shape;
            set
            {
                if (_shape != value)
                {
                    _shape = value;
                    _isGeometryDirty = true;
                }
            }
        }

        public Point Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = new Point(Math.Round(value.X, PositionPrecision), Math.Round(value.Y, PositionPrecision));
                    _isGeometryDirty = true;
                    OnPropertyChanged();
                }
            }
        }

        public Point PixelPosition
        {
            get => new Point(Position.X, Position.Y);
        }

        public double X { get => Position.X; }

        public double Y { get => Position.Y; }


        public CustomPoint(CustomCanvas canvas, bool hasAnchorPoint = false) : base(canvas, hasAnchorPoint)
        {
            Fill = Brushes.Black;
            StrokeThickness = 0;
            IsSelectable = false;
            LabelText = "Point";
            Canvas.CanvasGridChanged += (s, e) =>
            {
                _isGeometryDirty = true;
            };
        }

        protected override StreamGeometry GetElementGeometry()
        {
            if (_isGeometryDirty)
            {
                switch (Shape)
                {
                    case PointShape.Circle:
                        _cachedGeometry = Canvas.GetCircleGeometry(PixelPosition, Radius, 100);
                        break;
                    case PointShape.Square:
                        _cachedGeometry = Canvas.GetSquareGeometry(PixelPosition, Radius * 2, false);
                        break;
                    case PointShape.Anchor:
                        _cachedGeometry = Canvas.GetAnchorGeometry(PixelPosition, Radius, 100, 0.1);
                        break;
                }
                _isGeometryDirty = false;
            }
            return _cachedGeometry;
        }

        public override Point GetTopLeftPosition() => new Point(Position.X + Radius / UnitSize, Position.Y + Radius / UnitSize);

        public override Point GetBottomRightPosition() => new Point(Position.X - Radius / UnitSize, Position.Y - Radius / UnitSize);

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            MouseLeftButtonDownAction?.Invoke(e);
            e.Handled = true;
            base.OnMouseLeftButtonDown(e);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(Radius, Radius);
        }

        public override string ToString()
        {
            return $"Point\nX: {Position.X}\nY: {Position.Y}";
        }

    }
}

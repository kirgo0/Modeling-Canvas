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
        private Point[][] _cachedGeometry;

        private bool _isGeometryDirty = true;

        private double _pixelRadius = 5;

        private PointShape _shape = PointShape.Circle;

        protected Point _position;

        public int PositionPrecision { get; set; } = 3;

        public Action<MouseButtonEventArgs> MouseLeftButtonDownAction { get; set; }

        public double PixelRadius
        {
            get => _pixelRadius;
            set
            {
                if (_pixelRadius != value)
                {
                    _pixelRadius = value;
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
            _transformGeometry = false;
        }

        protected override Point[][] GetElementGeometry()
        {
            if (_isGeometryDirty)
            {
                var transformedPosition = TransformPoint(Position);
                switch (Shape)
                {
                    case PointShape.Circle:
                        _cachedGeometry = Canvas.GetCircleGeometry(transformedPosition, PixelRadius, precision: 100);
                        break;
                    case PointShape.Square:
                        _cachedGeometry = Canvas.GetSquarePointGeometry(transformedPosition, PixelRadius * 2, false);
                        break;
                    case PointShape.Anchor:
                        _cachedGeometry = Canvas.GetAnchorGeometry(transformedPosition, PixelRadius, 100, 5);
                        break;
                }
                _isGeometryDirty = false;
            }
            return _cachedGeometry;
        }

        public override Point GetTopLeftPosition() => new Point(Position.X + PixelRadius / UnitSize, Position.Y + PixelRadius / UnitSize);

        public override Point GetBottomRightPosition() => new Point(Position.X - PixelRadius / UnitSize, Position.Y - PixelRadius / UnitSize);

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            MouseLeftButtonDownAction?.Invoke(e);
            e.Handled = true;
            base.OnMouseLeftButtonDown(e);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(PixelRadius, PixelRadius);
        }

        public override string ToString()
        {
            var p = TransformPoint(Position, false);
            return $"Point\nX: {Position.X}\nY: {Position.Y}\n{p.X} | {p.Y}";
        }

    }
}

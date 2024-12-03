using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;


namespace Modeling_Canvas.UIELements
{
    public class CustomPoint : CustomElement
    {
        public double Radius { get; set; } = 10;
        public PointShape Shape { get; set; } = PointShape.Circle;

        public double X { get => Position.X; }
        public double Y { get => Position.Y; }
        public int PositionPrecision { get; set; } = 3;

        protected Point _position;
        public virtual Point Position
        {
            get => _position;
            set
            {
                _position = new Point(Math.Round(value.X, PositionPrecision), Math.Round(value.Y, PositionPrecision));
                InvalidateVisual();
            }
        }

        public Point PixelPosition { 
            get => new Point(Canvas.ActualWidth / 2 + Position.X * UnitSize, Canvas.ActualHeight/2 - Position.Y * UnitSize).AddCanvasOffsets(); }
            
        public CustomPoint(CustomCanvas canvas) : base(canvas)
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
                    dc.DrawCircle(semiTransparentFill, StrokePen, PixelPosition, Radius, 100);
                    break;
                case PointShape.Square:
                    dc.DrawSquare(semiTransparentFill, StrokePen, PixelPosition, Radius * 2);
                    break;
                case PointShape.Anchor:
                    dc.DrawAnchorPoint(semiTransparentFill, StrokePen, PixelPosition, Radius, 100, 5);
                    break;
            }
        }

        protected override void AffineRender(DrawingContext dc)
        {
            var semiTransparentFill = Fill.Clone();
            semiTransparentFill.Opacity = Opacity;

            switch (Shape)
            {
                case PointShape.Circle:
                    dc.DrawAffineCircle(semiTransparentFill, StrokePen, PixelPosition, Radius, 100, Canvas.AffineParams);
                    break;
                case PointShape.Square:
                    dc.DrawAffineSquare(semiTransparentFill, StrokePen, PixelPosition, Radius * 2, Canvas.AffineParams);
                    break;
                case PointShape.Anchor:
                    dc.DrawAffineAnchorPoint(semiTransparentFill, StrokePen, PixelPosition, Radius, 100, 5, Canvas.AffineParams);
                    break;
            }
        }
        protected override void ProjectiveRender(DrawingContext dc)
        {
            var semiTransparentFill = Fill.Clone();
            semiTransparentFill.Opacity = Opacity;

            switch (Shape)
            {
                case PointShape.Circle:
                    dc.DrawProjectiveCircle(semiTransparentFill, StrokePen, PixelPosition, Radius, 100, Canvas.ProjectiveParams);
                    break;
                case PointShape.Square:
                    dc.DrawProjectiveSquare(semiTransparentFill, StrokePen, PixelPosition, Radius * 2, Canvas.ProjectiveParams);
                    break;
                case PointShape.Anchor:
                    dc.DrawProjectiveAnchorPoint(semiTransparentFill, StrokePen, PixelPosition, Radius, 100, 5, Canvas.ProjectiveParams);
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

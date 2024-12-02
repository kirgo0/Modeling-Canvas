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

        public CustomPoint(CustomCanvas canvas) : base(canvas)
        {
            Fill = Brushes.Black;
            StrokeThickness = 0;
            IsSelectable = false;
            LabelText = "Point";
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            base.DefaultRender(dc);

            var semiTransparentFill = Fill.Clone();
            semiTransparentFill.Opacity = Opacity;

            switch (Shape)
            {
                case PointShape.Circle:
                    dc.DrawCircle(semiTransparentFill, StrokePen, new Point(0, 0), Radius, 100);
                    break;
                case PointShape.Square:
                    dc.DrawSquare(semiTransparentFill, StrokePen, new Point(0, 0), Radius * 2);
                    break;
                case PointShape.Anchor:
                    dc.DrawAnchorPoint(semiTransparentFill, StrokePen, new Point(0, 0), Radius, 100, 5);
                    break;
            }
        }

        protected override void AffineRender(DrawingContext dc)
        {
            base.AffineRender(dc);

            var semiTransparentFill = Fill.Clone();
            semiTransparentFill.Opacity = Opacity;

            switch (Shape)
            {
                case PointShape.Circle:
                    dc.DrawAffineCircle(semiTransparentFill, StrokePen, new Point(0, 0), Radius, 100, Canvas.AffineParams);
                    break;
                case PointShape.Square:
                    dc.DrawAffineSquare(semiTransparentFill, StrokePen, new Point(0, 0), Radius * 2, Canvas.AffineParams);
                    break;
                case PointShape.Anchor:
                    dc.DrawAffineAnchorPoint(semiTransparentFill, StrokePen, new Point(0, 0), Radius, 100, 5, Canvas.AffineParams);
                    break;
            }
        }
        protected override void ProjectiveRender(DrawingContext dc)
        {
            base.ProjectiveRender(dc);

            var semiTransparentFill = Fill.Clone();
            semiTransparentFill.Opacity = Opacity;

            switch (Shape)
            {
                case PointShape.Circle:
                    dc.DrawProjectiveCircle(semiTransparentFill, StrokePen, new Point(0, 0), Radius, 100, Canvas.ProjectiveParams);
                    break;
                case PointShape.Square:
                    dc.DrawProjectiveSquare(semiTransparentFill, StrokePen, new Point(0, 0), Radius * 2, Canvas.ProjectiveParams);
                    break;
                case PointShape.Anchor:
                    dc.DrawProjectiveAnchorPoint(semiTransparentFill, StrokePen, new Point(0, 0), Radius, 100, 5, Canvas.ProjectiveParams);
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

        public Action? OnRenderControlPanel { get; set; }
        public bool OverrideRenderControlPanelAction { get; set; } = false;
        protected override void RenderControlPanel()
        {
            if (!OverrideRenderControlPanelAction)
            {
                OnRenderControlPanel?.Invoke();
                RenderControlPanelLabel();
                AddPointControls();
            } else
            {
                OnRenderControlPanel?.Invoke();
            }
        }

        protected virtual void AddPointControls()
        {
            AddDefaultPointControls(
                "Point",
                this,
                "Position.X",
                "Position.Y",
                (x) =>
                {
                    Position = new Point(x, Position.Y);
                    InvalidateCanvas();
                },
                (y) =>
                {
                   Position = new Point(Position.X, y);
                    InvalidateCanvas();
                }
            );
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

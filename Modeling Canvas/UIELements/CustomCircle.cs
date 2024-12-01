using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIELements
{
    public class CustomCircle : CustomElement
    {
        public CustomCircle(CustomCanvas canvas) : base(canvas)
        {
        }
        public int Precision { get; set; } = 100; // Number of points for the circle
        private double _radius = 5;
        public double Radius
        {
            get => _radius;
            set
            {
                if (value <= 0.5) _radius = 0.5;
                else _radius = value;
            }
        }
        public double StartDegrees { get; set; } = 0;
        public double StartRadians { get => Helpers.DegToRad(StartDegrees); }
        public double EndDegrees { get; set; } = 360;
        public double EndRadians { get => Helpers.DegToRad(EndDegrees); }
        public Point Center { get; set; } = new Point(0, 0);
        public DraggablePoint CenterPoint { get; set; }
        public DraggablePoint RadiusPoint { get; set; }
        public DraggablePoint StartDegreesPoint { get; set; }
        public DraggablePoint EndDegreesPoint { get; set; }

        public override Point GetOriginPoint(Size arrangedSize)
        {
            return new Point(arrangedSize.Width / 2 + UnitSize * Center.X, arrangedSize.Height / 2 - UnitSize * Center.Y);
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size((Radius * UnitSize + StrokeThickness) * 2, (Radius * UnitSize + StrokeThickness) * 2);
        }
        protected override void InitControls()
        {
            RadiusPoint = new DraggablePoint(Canvas)
            {
                Radius = 8,
                Opacity = 0.7,
                OverrideMoveAction = RadiusPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                HasAnchorPoint = false,
            };
            Canvas.Children.Add(RadiusPoint);
            Panel.SetZIndex(RadiusPoint, Canvas.Children.Count + 1);

            StartDegreesPoint = new DraggablePoint(Canvas)
            {
                Opacity = 0.7,
                OverrideMoveAction = StartDegreesPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                Fill = Brushes.Red,
                HasAnchorPoint = false
            };
            Canvas.Children.Add(StartDegreesPoint);
            Panel.SetZIndex(StartDegreesPoint, Canvas.Children.Count + 1);

            EndDegreesPoint = new DraggablePoint(Canvas)
            {
                Opacity = 0.7,
                OverrideMoveAction = EndDegreesPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                Fill = Brushes.Blue,
                HasAnchorPoint = false
            };
            Canvas.Children.Add(EndDegreesPoint);
            Panel.SetZIndex(EndDegreesPoint, Canvas.Children.Count + 1);

            CenterPoint = new DraggablePoint(Canvas)
            {
                Radius = 3,
                OverrideMoveAction = CenterPointMoveAction,
                HasAnchorPoint = false
            };
            Canvas.Children.Add(CenterPoint);
            Panel.SetZIndex(CenterPoint, Canvas.Children.Count + 1);
            base.InitControls();
        }

        protected override void OnRender(DrawingContext dc)
        {
            UpdateUIControls();
            base.OnRender(dc);
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            dc.DrawCircleWithArcs(Fill, StrokePen, new Point(0, 0), Radius * UnitSize, StartDegrees, EndDegrees, Precision, 10);
            base.DefaultRender(dc);
        }

        protected override void AffineRender(DrawingContext dc)
        {
            dc.DrawAffineCircleWithArcs(Fill, StrokePen, new Point(0, 0), Radius * UnitSize, StartDegrees, EndDegrees, Precision, Canvas.AffineParams, 10);
            base.AffineRender(dc);
        }

        protected override void ProjectiveRender(DrawingContext dc)
        {
            dc.DrawProjectiveCircleWithArcs(Fill, StrokePen, new Point(0, 0), Radius * UnitSize, StartDegrees, EndDegrees, Precision, Canvas.ProjectiveParams, 10);
            base.ProjectiveRender(dc);
        }

        protected override void RenderControlPanel()
        {
            base.RenderControlPanel();
            AddFillColorControls();
            AddStrokeColorControls();
            AddStrokeThicknessControls();
        }

        protected override Point GetAnchorDefaultPosition()
        {
            return Center;
        }

        public override Point GetTopLeftPosition()
        {
            return new Point(Center.X - Radius - StrokeThickness / UnitSize, Center.Y + Radius + StrokeThickness / UnitSize);
        }

        public override Point GetBottomRightPosition()
        {
            return new Point(Center.X + Radius + StrokeThickness / UnitSize, Center.Y - Radius - StrokeThickness / UnitSize);
        }

        public virtual void RadiusPointMoveAction(DraggablePoint point, Vector offset)
        {
            if (SnappingEnabled)
            {
                Radius = Helpers.SnapValue(Radius + offset.X / UnitSize);
            }
            else
            {
                Radius += offset.X / UnitSize;
            }
        }
        public virtual void StartDegreesPointMoveAction(DraggablePoint point, Vector offset)
        {
            if (InputManager.ShiftPressed && Math.Abs(StartDegrees - EndDegrees) < 5)
            {
                StartDegrees = EndDegrees;
            }
            else
            {
                StartDegrees = Canvas.GetDegreesBetweenMouseAndPoint(Center);
            }
        }
        public virtual void EndDegreesPointMoveAction(DraggablePoint point, Vector offset)
        {
            if (InputManager.ShiftPressed && Math.Abs(StartDegrees - EndDegrees) < 5)
            {
                EndDegrees = StartDegrees;
            }
            else
            {
                EndDegrees = Canvas.GetDegreesBetweenMouseAndPoint(Center);
            }
        }
        public virtual void CenterPointMoveAction(DraggablePoint point, Vector offset)
        {
            MoveElement(offset);
        }

        public override void MoveElement(Vector offset)
        {
            if (InputManager.AnyKeyButShiftPressed) return;

            Center = SnappingEnabled ? Center.OffsetAndSpanPoint(offset) : Center.OffsetPoint(offset);

            base.MoveElement(offset);
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
            Center = Center.RotatePoint(anchorPoint, degrees);
            EndDegrees -= degrees;
            StartDegrees -= degrees;
            StartDegrees = Helpers.NormalizeAngle(StartDegrees);
            EndDegrees = Helpers.NormalizeAngle(EndDegrees);
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
            Center = Center.ScalePoint(anchorPoint, scaleVector);
            Radius *= ScaleFactor;
        }

        private void UpdateUIControls()
        {
            AnchorVisibility = ShowControls;
            CenterPoint.Visibility = ShowControls;
            RadiusPoint.Visibility = ShowControls;
            StartDegreesPoint.Visibility = ShowControls;
            EndDegreesPoint.Visibility = ShowControls;

            RadiusPoint.Position = new Point(Center.X + (Radius + 1) * Math.Cos(Helpers.DegToRad(0)), Center.Y - Radius * Math.Sin(0));
            StartDegreesPoint.Position = new Point(Center.X + Radius * Math.Cos(StartRadians), Center.Y - Radius * Math.Sin(StartRadians));
            EndDegreesPoint.Position = new Point(Center.X + Radius * Math.Cos(EndRadians), Center.Y - Radius * Math.Sin(EndRadians));
            CenterPoint.Position = Center;
        }
        public override string ToString()
        {
            return $"X: {Center.X} \nY: {Center.Y} \nRadius: {Radius}\nStart: {StartDegrees}\nEnd: {EndDegrees}\nTL: {GetTopLeftPosition()}\nBR: {GetBottomRightPosition()}";
        }

    }
}

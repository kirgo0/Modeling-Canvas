using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public partial class SegmentedCircle : CustomCircle
    {
        public double MaxDegrees { get; set; } = 360;
        public double MinDegrees { get; set; } = 0;

        private double _startDegrees = 0;
        public double StartDegrees
        {
            get => _startDegrees;
            set
            {
                _startDegrees = Math.Round(value, 1);
                OnPropertyChanged();
                InvalidateCanvas();
            }
        }
        public double StartRadians { get => Helpers.DegToRad(StartDegrees); }

        private double _endDegrees = 360;
        public double EndDegrees
        {
            get => _endDegrees;
            set
            {
                _endDegrees = Math.Round(value, 1);
                OnPropertyChanged();
                InvalidateCanvas();
            }
        }
        public double EndRadians { get => Helpers.DegToRad(EndDegrees); }
        public DraggablePoint StartDegreesPoint { get; set; }
        public DraggablePoint EndDegreesPoint { get; set; }
        public SegmentedCircle(CustomCanvas canvas) : base(canvas)
        {
        }
        protected override void OnRender(DrawingContext dc)
        {
            StartDegreesPoint.Visibility = ControlsVisibility;
            EndDegreesPoint.Visibility = ControlsVisibility;
            StartDegreesPoint.Position = new Point(Center.X + Radius * Math.Cos(StartRadians), Center.Y - Radius * Math.Sin(StartRadians));
            EndDegreesPoint.Position = new Point(Center.X + Radius * Math.Cos(EndRadians), Center.Y - Radius * Math.Sin(EndRadians));
            base.OnRender(dc);
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            dc.DrawCircleWithArcs(Canvas, Fill, StrokePen, CenterPoint.PixelPosition, Radius * UnitSize, StartDegrees, EndDegrees, Precision, 10);
        }

        protected override void InitControlPanel()
        {
            base.InitControlPanel();
            AddDegreesControls();
        }

        protected override void InitChildren()
        {
            StartDegreesPoint = new DraggablePoint(Canvas, false)
            {
                Opacity = 0.7,
                OverrideMoveAction = StartDegreesPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                Fill = Brushes.Red,
                OverrideRenderControlPanelAction = true
            };
            Canvas.Children.Add(StartDegreesPoint);
            Panel.SetZIndex(StartDegreesPoint, Canvas.Children.Count + 1);

            EndDegreesPoint = new DraggablePoint(Canvas, false)
            {
                Opacity = 0.7,
                OverrideMoveAction = EndDegreesPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                Fill = Brushes.Blue,
                OverrideRenderControlPanelAction = true
            };
            Canvas.Children.Add(EndDegreesPoint);
            Panel.SetZIndex(EndDegreesPoint, Canvas.Children.Count + 1);

            base.InitChildren();
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
        public override void RotateElement(Point anchorPoint, double degrees)
        {
            base.RotateElement(anchorPoint, degrees);
            EndDegrees -= degrees;
            StartDegrees -= degrees;
            StartDegrees = Helpers.NormalizeAngle(StartDegrees);
            EndDegrees = Helpers.NormalizeAngle(EndDegrees);
        }

        public override string ToString()
        {
            return $"X: {Center.X} \nY: {Center.Y} \nRadius: {Radius}\nStart: {StartDegrees}\nEnd: {EndDegrees}\nTL: {GetTopLeftPosition()}\nBR: {GetBottomRightPosition()}";
        }
    }
}

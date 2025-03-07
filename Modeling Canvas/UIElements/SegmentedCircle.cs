using Modeling_Canvas.Extensions;
using Modeling_Canvas.Models;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public partial class SegmentedCircle : CustomCircle
    {
        private double _endDegrees = 360;

        private double _startDegrees = 0;

        public double MaxDegrees { get; set; } = 360;

        public double MinDegrees { get; set; } = 0;

        public double StartRadians { get => Helpers.DegToRad(StartDegrees); }

        public double EndRadians { get => Helpers.DegToRad(EndDegrees); }

        public DraggablePoint StartDegreesPoint { get; set; }

        public DraggablePoint EndDegreesPoint { get; set; }

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

        public SegmentedCircle(CustomCanvas canvas) : base(canvas)
        {
        }

        protected override void OnRender(DrawingContext dc)
        {
            StartDegreesPoint.Visibility = ControlsVisibility;
            EndDegreesPoint.Visibility = ControlsVisibility;
            StartDegreesPoint.Position = new Point(Center.X + Radius * Math.Cos(StartRadians), Center.Y + Radius * Math.Sin(StartRadians));
            EndDegreesPoint.Position = new Point(Center.X + Radius * Math.Cos(EndRadians), Center.Y + Radius * Math.Sin(EndRadians));
            base.OnRender(dc);
        }

        protected override List<(FigureStyle, Point[])> GetElementGeometry()
        {
            return new() { ( Style, Canvas.GetCircleSegmentGeometry(Center, Radius, StartDegrees, EndDegrees, Precision)) };
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
                Style = new()
                {
                    FillColor = Brushes.Red,
                },
                IsSelectable = false
            };
            AddChildren(StartDegreesPoint);

            EndDegreesPoint = new DraggablePoint(Canvas, false)
            {
                Opacity = 0.7,
                OverrideMoveAction = EndDegreesPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                Style = new()
                {
                    FillColor = Brushes.Blue,
                },
                IsSelectable = false
            };
            AddChildren(EndDegreesPoint);

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
            EndDegrees += degrees;
            StartDegrees += degrees;
            StartDegrees = Helpers.NormalizeAngle(StartDegrees);
            EndDegrees = Helpers.NormalizeAngle(EndDegrees);
        }

        public override string ToString()
        {
            return $"X: {Center.X} \nY: {Center.Y} \nRadius: {Radius}\nStart: {StartDegrees}\nEnd: {EndDegrees}\nTL: {GetTopLeftPosition()}\nBR: {GetBottomRightPosition()}";
        }
    }
}

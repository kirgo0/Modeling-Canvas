using Modeling_Canvas.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class SegmentedCircle : CustomCircle
    {
        public DraggablePoint StartDegreesPoint { get; set; }
        public DraggablePoint EndDegreesPoint { get; set; }

        private double _startDegrees = 0;
        public double StartDegrees
        {
            get => _startDegrees;
            set
            {
                _startDegrees = Math.Round(value, 1);
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
            }
        }
        public double EndRadians { get => Helpers.DegToRad(EndDegrees); }
        public SegmentedCircle(CustomCanvas canvas) : base(canvas)
        {
        }
        protected override void DefaultRender(DrawingContext dc)
        {
            dc.DrawCircleWithArcs(Fill, StrokePen, CenterPoint.PixelPosition, Radius * UnitSize, StartDegrees, EndDegrees, Precision, 10);
        }

        protected override void AffineRender(DrawingContext dc)
        {
            dc.DrawAffineCircleWithArcs(Fill, StrokePen, CenterPoint.PixelPosition, Radius * UnitSize, StartDegrees, EndDegrees, Precision, Canvas.AffineParams, 10);
        }

        protected override void ProjectiveRender(DrawingContext dc)
        {
            dc.DrawProjectiveCircleWithArcs(Fill, StrokePen, CenterPoint.PixelPosition, Radius * UnitSize, StartDegrees, EndDegrees, Precision, Canvas.ProjectiveParams, 10);
        }

        protected override void RenderControlPanel()
        {
            base.RenderControlPanel();
            AddDegreesControls();
        }

        protected override void InitControls()
        {
            StartDegreesPoint = new DraggablePoint(Canvas)
            {
                Opacity = 0.7,
                OverrideMoveAction = StartDegreesPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                Fill = Brushes.Red,
                HasAnchorPoint = false,
                OverrideRenderControlPanelAction = true
            };
            Canvas.Children.Add(StartDegreesPoint);
            Panel.SetZIndex(StartDegreesPoint, Canvas.Children.Count + 1);

            EndDegreesPoint = new DraggablePoint(Canvas)
            {
                Opacity = 0.7,
                OverrideMoveAction = EndDegreesPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                Fill = Brushes.Blue,
                HasAnchorPoint = false,
                OverrideRenderControlPanelAction = true
            };
            Canvas.Children.Add(EndDegreesPoint);
            Panel.SetZIndex(EndDegreesPoint, Canvas.Children.Count + 1);

            base.InitControls();
        }

        protected virtual void AddDegreesControls()
        {
            var startDegPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };
            var startDegLabel = new TextBlock { Text = $"StartDegrees: {StartDegrees}", TextAlignment = TextAlignment.Center };
            var startDegSlider = new Slider
            {
                Minimum = 0,
                Maximum = 360,
                Value = StartDegrees,
                TickFrequency = 1,
                IsSnapToTickEnabled = true,
                Width = 200
            };

            startDegSlider.ValueChanged += (s, e) =>
            {
                StartDegrees = Math.Round(e.NewValue); // Update Radius
                startDegLabel.Text = $"StartDegrees: {StartDegrees}";
                InvalidateCanvas();
            };
            startDegPanel.Children.Add(startDegLabel);
            startDegPanel.Children.Add(startDegSlider);

            AddElementToControlPanel(startDegPanel);
            var endDegPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };
            var endDegLabel = new TextBlock { Text = $"EndDegrees: {EndDegrees}", TextAlignment = TextAlignment.Center };
            var endDegSlider = new Slider
            {
                Minimum = 0,
                Maximum = 360,
                Value = EndDegrees,
                TickFrequency = 0.5,
                IsSnapToTickEnabled = true,
                Width = 200
            };

            endDegSlider.ValueChanged += (s, e) =>
            {
                EndDegrees = Math.Round(e.NewValue); // Update Radius
                endDegLabel.Text = $"EndDegrees: {EndDegrees}";
                InvalidateCanvas();
            };
            endDegPanel.Children.Add(endDegLabel);
            endDegPanel.Children.Add(endDegSlider);
            AddElementToControlPanel(endDegPanel);
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

        protected override void UpdateUIControls()
        {
            StartDegreesPoint.Visibility = ControlsVisibility;
            EndDegreesPoint.Visibility = ControlsVisibility;
            StartDegreesPoint.Position = new Point(Center.X + Radius * Math.Cos(StartRadians), Center.Y - Radius * Math.Sin(StartRadians));
            EndDegreesPoint.Position = new Point(Center.X + Radius * Math.Cos(EndRadians), Center.Y - Radius * Math.Sin(EndRadians));
            base.UpdateUIControls();
        }
        public override string ToString()
        {
            return $"X: {Center.X} \nY: {Center.Y} \nRadius: {Radius}\nStart: {StartDegrees}\nEnd: {EndDegrees}\nTL: {GetTopLeftPosition()}\nBR: {GetBottomRightPosition()}";
        }
    }
}

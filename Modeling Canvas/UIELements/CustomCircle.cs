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
            canvas.MouseWheel += (o, e) =>
            {
                if(Canvas.SelectedElements.Contains(this))
                    RenderControlPanel();
            };
        }
        public int Precision { get; set; } = 100; // Number of points for the circle
        private double _radius = 5;
        public double Radius
        {
            get => _radius;
            set
            {
                if (value <= 0.5) _radius = 0.5;
                else _radius = Math.Round(value,3);
            }
        }

        private double _startDegrees = 0;
        public double StartDegrees { 
            get => _startDegrees; 
            set
            {
                _startDegrees = Math.Round(value,1);
            }
        }
        public double StartRadians { get => Helpers.DegToRad(StartDegrees); }

        private double _endDegrees = 360;
        public double EndDegrees {
            get => _endDegrees;
            set
            {
                _endDegrees = Math.Round(value,1);
            }
        }
        public double EndRadians { get => Helpers.DegToRad(EndDegrees); }
        public Point Center
        {
            get => CenterPoint.Position;
            set => CenterPoint.Position = value;
        }
        public DraggablePoint CenterPoint { get; set; }
        public DraggablePoint RadiusPoint { get; set; }
        public DraggablePoint StartDegreesPoint { get; set; }
        public DraggablePoint EndDegreesPoint { get; set; }

        public override Point GetOriginPoint(Size arrangedSize)
        {
            if(Canvas.RenderMode is Enums.RenderMode.Projective)
            {
                return new Point(-Canvas.XOffset, Canvas.YOffset);
            }
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
                OverrideRenderControlPanelAction = true
            };
            Canvas.Children.Add(RadiusPoint);
            Panel.SetZIndex(RadiusPoint, Canvas.Children.Count + 1);

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

            CenterPoint = new DraggablePoint(Canvas)
            {
                Radius = 3,
                OverrideMoveAction = CenterPointMoveAction,
                HasAnchorPoint = false,
                OverrideRenderControlPanelAction = true,
                Position = new Point(0, 0)
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
            var center = new Point(Canvas.ActualWidth / 2 + Center.X * UnitSize, Canvas.ActualHeight / 2 - Center.Y * UnitSize).AddCanvasOffsets();
            dc.DrawProjectiveCircleWithArcs(Fill, StrokePen, center, Radius * UnitSize, StartDegrees, EndDegrees, Precision, Canvas.ProjectiveParams, 10);
            base.ProjectiveRender(dc);
        }

        protected override void RenderControlPanel()
        {
            base.RenderControlPanel();
            AddCenterControls();
            AddFillColorControls();
            AddStrokeColorControls();
            AddStrokeThicknessControls();
            AddRadiusControls();
            AddDegreesControls();
        }

        protected virtual void AddRadiusControls()
        {
            var panel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };
            var radiusLabel = new TextBlock { Text = $"Radius: {Radius}", TextAlignment = TextAlignment.Center };
            var radiusSlider = new Slider
            {
                Minimum = 1,
                Maximum = Math.Max(Canvas.ActualWidth / 2 / UnitSize, Radius),
                Value = Radius,
                TickFrequency = 0.1,
                IsSnapToTickEnabled = true,
                Width = 200
            };
            radiusSlider.ValueChanged += (s, e) =>
            {
                Radius = Math.Round(e.NewValue,2); // Update Radius
                radiusLabel.Text = $"Radius: {Radius}";
                InvalidateCanvas();
            };
            panel.Children.Add(radiusLabel);
            panel.Children.Add(radiusSlider);
            AddElementToControlPanel(panel);
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
        protected virtual void AddCenterControls()
        {
            AddDefaultPointControls(
                "Center",
                this,
                "Center.X",
                "Center.Y",
                (x) =>
                {
                    OverrideAnchorPoint = true;
                    var difference = Center.X - x;
                    Center = new Point(x, Center.Y);
                    AnchorPoint.Position = new Point(AnchorPoint.Position.X - difference, AnchorPoint.Position.Y);
                    InvalidateCanvas();
                },
                (y) =>
                {
                    OverrideAnchorPoint = true;
                    var difference = Center.Y - y;
                    Center = new Point(Center.X, y);
                    AnchorPoint.Position = new Point(AnchorPoint.Position.X, AnchorPoint.Position.Y - difference);
                    InvalidateCanvas();
                }
                );
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

        //protected override void RenderControlPanelLabel()
        //{
        //    var label = new TextBlock { Text = "| CIRCLE |" };
        //    AddElementToControlPanel(label);
        //}

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

        public override void InvalidateCanvas()
        {
            UpdateUIControls();
            base.InvalidateCanvas();
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

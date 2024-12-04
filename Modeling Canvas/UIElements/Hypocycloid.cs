using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class Hypocycloid : CustomElement
    {
        protected double _distance = 1;
        public double Distance
        {
            get => _distance;
            set => _distance = Math.Round(value, 3);
        }
        protected double _angle = 720;
        public double Angle
        {
            get => _angle;
            set => _angle = Math.Round(value, 1);
        }
        public double _rotationAngle { get; set; } = 0;
        public int PointsCount { get; set; } = 1000;
        public CustomCircle LargeCircle { get; set; }
        public CustomCircle SmallCircle { get; set; }
        public DraggablePoint CenterPoint { get; set; }


        public double LargeRadius { get => LargeCircle.Radius; }
        public double SmallRadius { get => SmallCircle.Radius; }

        public Point Center
        {
            get => CenterPoint.Position;
            set => CenterPoint.Position = value;
        }
        public Hypocycloid(CustomCanvas canvas, double largeRadius, double smallRadius, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            StrokeThickness = 3;
            LargeCircle = new CustomCircle(canvas)
            {
                Radius = largeRadius
            };
            SmallCircle = new CustomCircle(canvas)
            {
                Radius = smallRadius
            };
            canvas.MouseWheel += (o, e) =>
            {
                if (Canvas.SelectedElements.Contains(this))
                    RenderControlPanel();
            };
        }

        protected override void OnRender(DrawingContext dc)
        {
            UpdateUIControls();
            base.OnRender(dc);
        }

        protected override Point GetAnchorDefaultPosition()
        {
            return Center;
        }
        protected override void DefaultRender(DrawingContext dc)
        {
            double R = LargeRadius * UnitSize; // Великий радіус в одиницях Canvas
            double r = SmallRadius * UnitSize; // Малий радіус в одиницях Canvas
            double d = Distance * UnitSize;    // Відстань до точки в одиницях Canvas

            PointCollection points = new PointCollection();

            var center = CenterPoint.PixelPosition;

            for (int i = 0; i < PointsCount; i++)
            {
                double t = Helpers.DegToRad(Angle) * i / PointsCount;
                double x = (R - r) * Math.Cos(t) + d * Math.Cos((R - r) / r * t);
                double y = (R - r) * Math.Sin(t) - d * Math.Sin((R - r) / r * t);

                var rotatedPoint = new Point(x, y).RotatePoint(AnchorPoint.Position, _rotationAngle);

                points.Add(new Point(center.X + rotatedPoint.X, center.Y + rotatedPoint.Y));
            }

            for (int i = 0; i < points.Count - 1; i++)
            {
                dc.DrawLine(StrokePen, points[i], points[i + 1], 10);
            }
        }

        public Point GetSmallCircleCenter(double angleDeg)
        {
            var angle = Helpers.DegToRad(angleDeg);
            double x, y;

            x = Center.X + (LargeRadius - SmallRadius) * Math.Cos(angle);
            y = Center.Y + (LargeRadius - SmallRadius) * Math.Sin(angle);

            return new Point(x, -y);
        }

        protected override void InitControls()
        {
            CenterPoint = new DraggablePoint(Canvas)
            {
                Radius = 3,
                OverrideMoveAction = CenterPointMoveAction,
                HasAnchorPoint = false,
                OverrideRenderControlPanelAction = true,
                Position = new Point(0, 0)
            };
            LargeCircle.IsSelectable = false;
            SmallCircle.IsSelectable = false;

            LargeCircle.OverrideMoveAction = true;
            SmallCircle.OverrideMoveAction = true;

            LargeCircle.OverrideRotateAction = true;
            SmallCircle.OverrideRotateAction = true;

            LargeCircle.OverrideScaleAction = true;
            SmallCircle.OverrideScaleAction = true;

            LargeCircle.IsInteractable = false;
            SmallCircle.IsInteractable = false;

            LargeCircle.RadiusControlDistance = 1.5;
            SmallCircle.RadiusControlDistance = 0.5;

            LargeCircle.HasAnchorPoint = false;
            SmallCircle.OnRadiusChange += (s, e) =>
            {
                if (e.PreviousRadius == Distance || SmallRadius < Distance)
                {
                    Distance = e.NewRadius;
                }
            };

            Canvas.Children.Add(CenterPoint);
            Canvas.Children.Add(SmallCircle);
            Canvas.Children.Add(LargeCircle);
            base.InitControls();
        }


        protected override void AffineRender(DrawingContext dc)
        {
        }

        protected override void ProjectiveRender(DrawingContext dc)
        {
        }

        protected override void RenderControlPanel()
        {
            ClearControlPanel();
            AddDistanceControls();
            AddAngleControls();
            AddRadiusControls();
        }

        protected virtual void AddDistanceControls()
        {
            var panel = GetDefaultVerticalPanel();
            var distanceLabel = new TextBlock { Text = $"Distance: {Distance}", TextAlignment = TextAlignment.Center };
            var distanceSlider = new Slider
            {
                Minimum = 0.1,
                Maximum = SmallCircle.Radius,
                Value = Distance,
                TickFrequency = 0.1,
                IsSnapToTickEnabled = true,
                Width = 200
            };
            distanceSlider.ValueChanged += (s, e) =>
            {
                Distance = e.NewValue;
                distanceLabel.Text = $"Distance: {Distance}";
                InvalidateVisual();
            };
            panel.Children.Add(distanceLabel);
            panel.Children.Add(distanceSlider);
            AddElementToControlPanel(panel);
        }

        protected void AddRadiusControls()
        {
            // Large Radius Controls
            var largePanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };
            var largeRadiusLabel = new TextBlock { Text = $"Large Radius: {LargeRadius}", TextAlignment = TextAlignment.Center };

            var largeRadiusSlider = new Slider
            {
                Minimum = SmallRadius,
                Maximum = Math.Max(Canvas.ActualWidth / 2 / UnitSize, LargeRadius),
                Value = LargeRadius,
                TickFrequency = 0.1,
                IsSnapToTickEnabled = true,
                Width = 200
            };

            // Small Radius Controls
            var smallPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };
            var smallRadiusLabel = new TextBlock { Text = $"Small Radius: {SmallRadius}", TextAlignment = TextAlignment.Center };

            var smallRadiusSlider = new Slider
            {
                Minimum = 1,
                Maximum = LargeRadius, // Initially set to LargeRadius.
                Value = SmallRadius,
                TickFrequency = 0.1,
                IsSnapToTickEnabled = true,
                Width = 200
            };

            // Large Radius ValueChanged Event
            largeRadiusSlider.ValueChanged += (s, e) =>
            {
                var newLargeRadius = Math.Round(e.NewValue, 2);
                LargeCircle.Radius = newLargeRadius;

                largeRadiusLabel.Text = $"Large Radius: {LargeRadius}";
                smallRadiusSlider.Maximum = LargeRadius; // Update SmallRadius slider max value dynamically.

                InvalidateCanvas();
            };

            // Small Radius ValueChanged Event
            smallRadiusSlider.ValueChanged += (s, e) =>
            {
                var newSmallRadius = Math.Round(e.NewValue, 2);
                if (SmallRadius == Distance || SmallRadius < Distance)
                {
                    Distance = newSmallRadius;
                }
                SmallCircle.Radius = newSmallRadius;

                smallRadiusLabel.Text = $"Small Radius: {SmallRadius}";
                InvalidateCanvas();
            };

            // Add both panels to the control panel
            largePanel.Children.Add(largeRadiusLabel);
            largePanel.Children.Add(largeRadiusSlider);
            AddElementToControlPanel(largePanel);

            smallPanel.Children.Add(smallRadiusLabel);
            smallPanel.Children.Add(smallRadiusSlider);
            AddElementToControlPanel(smallPanel);
        }

        protected virtual void AddAngleControls()
        {
            var panel = GetDefaultVerticalPanel();
            var distanceLabel = new TextBlock { Text = $"Angle: {Angle}", TextAlignment = TextAlignment.Center };
            var distanceSlider = new Slider
            {
                Minimum = 0,
                Maximum = 1080,
                Value = Angle,
                TickFrequency = 1,
                IsSnapToTickEnabled = true,
                Width = 200
            };
            distanceSlider.ValueChanged += (s, e) =>
            {
                Angle = e.NewValue;
                distanceLabel.Text = $"Angle: {Angle}";
                UpdateUIControls();
                InvalidateCanvas();
            };
            panel.Children.Add(distanceLabel);
            panel.Children.Add(distanceSlider);
            AddElementToControlPanel(panel);
        }

        public override void MoveElement(Vector offset)
        {
            if (InputManager.AnyKeyButShiftPressed) return;
            Center = SnappingEnabled ? Center.OffsetAndSpanPoint(offset) : Center.OffsetPoint(offset);

            base.MoveElement(offset);
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
            _rotationAngle -= degrees;
        }
        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
        }
        public virtual void CenterPointMoveAction(DraggablePoint point, Vector offset)
        {
            MoveElement(offset);
        }

        public void UpdateUIControls()
        {
            LargeCircle.Center = Center;
            SmallCircle.Center = GetSmallCircleCenter(Angle + _rotationAngle);

            LargeCircle.Visibility = ControlsVisibility;
            SmallCircle.Visibility = ControlsVisibility;

            LargeCircle.ControlsVisible = ControlsVisibility is Visibility.Visible;
            SmallCircle.ControlsVisible = ControlsVisibility is Visibility.Visible;
        }

        public override string ToString()
        {
            return $"Hypercycloid\nLr: {LargeCircle.Radius}\nSr: {SmallCircle.Radius}\nDistance: {Distance}";
        }
    }
}

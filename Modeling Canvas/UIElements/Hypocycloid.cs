using Modeling_Canvas.Extensions;
using Modeling_Canvas.Models;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MathNet.Numerics;
using Modeling_Canvas.Enums;
using System.Linq;


namespace Modeling_Canvas.UIElements
{
    public partial class Hypocycloid : CustomElement
    {
        public int PointsCount { get => (int) (Model.Angle / 360 * 2000); }
        public CustomCircle LargeCircle { get; set; }
        public CustomCircle SmallCircle { get; set; }
        public CustomPoint EndPoint { get; set; }
        public HypocycloidModel Model { get; set; }
        public HypocycloidModel AnimationModel { get; set; }
        public HypocycloidCalculationsModel CalculatedValues { get; set; }
        public double MinAnimationDuration { get; } = 0.3;
        public double MaxAnimationDuration { get; } = 10;

        private double _animationDuration = 3;
        public double AnimationDuration {

            get => _animationDuration;
            set
            {
                if (_animationDuration != value)
                {
                    _animationDuration = value;
                    OnPropertyChanged(nameof(AnimationDuration));
                }
            }
        }

        private Dictionary<string, FrameworkElement> _animationControls = new();
        public List<Point> HypocycloidPoints { get; set; } = new();

        private bool _showAnimationControls = false;
        public bool ShowAnimationControls
        {
            get => _showAnimationControls;
            set
            {
                if (_showAnimationControls != value)
                {
                    _showAnimationControls = value;
                    OnPropertyChanged(nameof(ShowAnimationControls));
                    RenderControlPanel();
                }
            }
        }

        private bool _isNotAnimating = true;
        public bool IsNotAnimating
        {
            get => _isNotAnimating;
            set
            {
                if (_isNotAnimating != value)
                {
                    _isNotAnimating = value;
                    OnPropertyChanged(nameof(IsNotAnimating));
                }
            }
        }
        public Point Center
        {
            get => LargeCircle.Center;
            set => LargeCircle.Center = value;
        }
        public Hypocycloid(CustomCanvas canvas, double largeRadius, double smallRadius, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            Model = new();
            CalculatedValues = new();
            AnimationModel = new();
            Model.LargeRadius = largeRadius;
            Model.SmallRadius = smallRadius;
            StrokeThickness = 3;

            canvas.MouseWheel += (o, e) =>
            {
                Model.MaxLargeCircleRadius = Math.Max(Canvas.ActualHeight, Canvas.ActualWidth) / UnitSize / 2;
                AnimationModel.MaxLargeCircleRadius = Model.MaxLargeCircleRadius;
            };

            Model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Model.LargeRadius))
                {
                    LargeCircle.Radius = Model.LargeRadius;
                }
                else if (e.PropertyName == nameof(Model.SmallRadius))
                {
                    SmallCircle.Radius = Model.SmallRadius;
                }
                InvalidateCanvas();
            };

            CalculatedValues.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CalculatedValues.ShowInflectionPoints))
                {
                    InvalidateCanvas();
                }
            };

        }
        protected override Point GetAnchorDefaultPosition()
        {
            return Center;
        }
        protected override void OnRender(DrawingContext dc)
        {
            UpdateUIControls();
            if (InputManager.AltPressed)
            {
                var t = FindNearestPoint(Mouse.GetPosition(Canvas));
                if(t is not null) DrawTangentsAndNormals(dc, t.Value);
            }
            base.OnRender(dc);
        }

        protected override void InitChildren()
        {
            LargeCircle = new CustomCircle(Canvas)
            {
                Radius = Model.LargeRadius,
                HasAnchorPoint = false
            };

            SmallCircle = new CustomCircle(Canvas)
            {
                Radius = Model.SmallRadius,
                HasAnchorPoint = false
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

            LargeCircle.OnRadiusChange += (s, e) =>
            {
                Model.LargeRadius = e.NewRadius;
            };
            SmallCircle.OnRadiusChange += (s, e) =>
            {
                Model.SmallRadius = e.NewRadius;
            };

            EndPoint = new CustomPoint(Canvas)
            {
                StrokeThickness = 0,
                Fill = Brushes.Red,
                Radius = 5
            }; 

            Canvas.Children.Add(SmallCircle);
            Canvas.Children.Add(LargeCircle);
            Canvas.Children.Add(EndPoint);

            base.InitChildren();
        }

        protected override void InitControlPanel()
        {
            base.InitControlPanel();


            var showInflectionPointsCheckbox = WpfHelper.CreateLabeledCheckBox("Inflection points", CalculatedValues, nameof(HypocycloidCalculationsModel.ShowInflectionPoints));

            _controls.Add(nameof(HypocycloidCalculationsModel.ShowInflectionPoints), showInflectionPointsCheckbox);

            var animateMenuCheckbox = WpfHelper.CreateLabeledCheckBox("Animate", this, nameof(ShowAnimationControls));

            _controls.Add(nameof(ShowAnimationControls), animateMenuCheckbox);

            var hypoControls = CreateHypocycloidControls(Model);

            _controls = _controls.Concat(hypoControls).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            _animationControls = CreateHypocycloidControls(AnimationModel, "Animate to ");

            foreach (var ac in _animationControls.Values)
            {
                ac.AddVisibilityBinding(this, nameof(ShowAnimationControls));
            }

            var timeSlider = WpfHelper.CreateSliderControl(
                "Time",
                this,
                nameof(AnimationDuration),
                nameof(MinAnimationDuration),
                nameof(MaxAnimationDuration),
                0.1
                );
            timeSlider.AddVisibilityBinding(this, nameof(ShowAnimationControls));

            _controls.Add("TimeSlider", timeSlider);

            var startAnimationButton = WpfHelper.CreateButton(
                content: "Animate",
                clickAction: () => Animate(),
                visibilityBindingSource: this,
                visibilityBindingPath: nameof(ShowAnimationControls)
                );

            startAnimationButton.AddIsDisabledBinding(this, nameof(IsNotAnimating));

            _controls.Add("AnimateButton", startAnimationButton);

        }

        protected override void RenderControlPanel()
        {
            ClearControlPanel();
            foreach (var control in _controls)
            {
                AddElementToControlPanel(control.Value);

                if (_animationControls.TryGetValue(control.Key, out var animationControl))
                {
                    AddElementToControlPanel(animationControl);
                }
            }
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            HypocycloidPoints = CalculateHypocycloidPoints(dc, Model);
            EndPoint.Position = Canvas.GetUnitCoordinates(HypocycloidPoints.LastOrDefault());
            DrawPoints(dc, HypocycloidPoints);
            if (CalculatedValues.ShowInflectionPoints)
            {
                CalculatedValues.InflectionPoints = CalculateInflectionPoints(Model);
                foreach (var point in CalculatedValues.InflectionPoints)
                {
                    dc.DrawCircle(Brushes.DeepPink, null, point, 5, 15);
                }
            }
        }

        protected override void AffineRender(DrawingContext dc)
        {
            HypocycloidPoints = CalculateHypocycloidPoints(dc, Model);
            EndPoint.Position = Canvas.GetUnitCoordinates(HypocycloidPoints.LastOrDefault());
            //DrawPoints(dc, HypocycloidPoints);
            for (int i = 0; i < HypocycloidPoints.Count - 1; i++)
            {
                dc.DrawAffineLine(StrokePen, HypocycloidPoints[i], HypocycloidPoints[i + 1], Canvas.AffineParams, 10);
            }
            if (CalculatedValues.ShowInflectionPoints)
            {
                CalculatedValues.InflectionPoints = CalculateInflectionPoints(Model);
                foreach (var point in CalculatedValues.InflectionPoints)
                {
                    dc.DrawCircle(Brushes.DeepPink, null, point, 5, 15);
                }
            }
        }

        protected override void ProjectiveRender(DrawingContext dc)
        {
        }

        protected virtual void DrawPoints(DrawingContext dc, List<Point> points)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                dc.DrawLine(StrokePen, points[i], points[i + 1], 10);
            }
        }

        protected virtual List<Point> CalculateHypocycloidPoints(DrawingContext dc, HypocycloidModel model)
        {
            var points = new List<Point>();

            double R = model.LargeRadius * UnitSize; 
            double r = model.SmallRadius * UnitSize;
            double d = model.Distance * UnitSize;

            var center = LargeCircle.CenterPoint.PixelPosition;

            for (int i = 0; i < PointsCount; i++)
            {
                double t = Helpers.DegToRad(model.Angle) * i / PointsCount;
                double x = (R - r) * Math.Cos(t) + d * Math.Cos((R - r) / r * t);
                double y = (R - r) * Math.Sin(t) - d * Math.Sin((R - r) / r * t);

                var rotationAngle = Helpers.DegToRad(model.RotationAngle);
                double rotatedX = Math.Cos(rotationAngle) * (x) - Math.Sin(rotationAngle) * (y);
                double rotatedY = Math.Sin(rotationAngle) * (x) + Math.Cos(rotationAngle) * (y);
                points.Add((new Point(center.X + rotatedX, center.Y + rotatedY)));
            }

            return points;
        }

        protected virtual List<Point> CalculateInflectionPoints(HypocycloidModel model)
        {
            var inflectionPoints = new List<Point>();

            double R = model.LargeRadius * UnitSize;
            double r = model.SmallRadius * UnitSize;
            double d = model.Distance * UnitSize;

            var center = LargeCircle.CenterPoint.PixelPosition;

            // Попереднє значення радіус-вектора
            double prevRadius = 0;
            double prevDerivative = 0;

            for (int i = 0; i <= PointsCount; i++)
            {
                double t = Helpers.DegToRad(model.Angle) * i / PointsCount;

                // Координати точки
                double x = (R - r) * Math.Cos(t) + d * Math.Cos((R - r) / r * t);
                double y = (R - r) * Math.Sin(t) - d * Math.Sin((R - r) / r * t);

                // Радіус-вектор
                double radius = Math.Sqrt(x * x + y * y);

                // Похідна радіус-вектора
                double derivative = radius - prevRadius;

                // Перевірка зміни знаку похідної (точка перегину)
                if (prevDerivative * derivative < 0)
                {
                    var rotationAngle = Helpers.DegToRad(model.RotationAngle);
                    double rotatedX = Math.Cos(rotationAngle) * x - Math.Sin(rotationAngle) * y;
                    double rotatedY = Math.Sin(rotationAngle) * x + Math.Cos(rotationAngle) * y;

                    inflectionPoints.Add(new Point(center.X + rotatedX, center.Y + rotatedY));
                }

                // Оновлення попередніх значень
                prevRadius = radius;
                prevDerivative = derivative;
            }

            return inflectionPoints;
        }
        public Point GetSmallCircleCenter(double angleDeg)
        {
            angleDeg = Helpers.NormalizeAngle(angleDeg);
            var angle = Helpers.DegToRad(angleDeg);
            double x, y;

            x = Center.X + (Model.LargeRadius - Model.SmallRadius) * Math.Cos(angle);
            y = Center.Y - (Model.LargeRadius - Model.SmallRadius) * Math.Sin(angle);

            return new Point(x, y);
        }

        public void DrawTangentsAndNormals(DrawingContext dc, double tParameter)
        {
            double R = Model.LargeRadius * UnitSize;
            double r = Model.SmallRadius * UnitSize;
            double d = Model.Distance * UnitSize;

            var center = LargeCircle.CenterPoint.PixelPosition;

            double t = Helpers.DegToRad(Model.Angle) * tParameter / PointsCount;
            double x = (R - r) * Math.Cos(t) + d * Math.Cos((R - r) / r * t);
            double y = (R - r) * Math.Sin(t) - d * Math.Sin((R - r) / r * t);

            var rotationAngle = Helpers.DegToRad(Model.RotationAngle);
            double rotatedX = Math.Cos(rotationAngle) * x - Math.Sin(rotationAngle) * y;
            double rotatedY = Math.Sin(rotationAngle) * x + Math.Cos(rotationAngle) * y;

            Point pointOnCurve = new Point(center.X + rotatedX, center.Y + rotatedY);

            double dx = -(R - r) * Math.Sin(t) - d * ((R - r) / r) * Math.Sin((R - r) / r * t);
            double dy = (R - r) * Math.Cos(t) - d * ((R - r) / r) * Math.Cos((R - r) / r * t);

            double rotatedDx = Math.Cos(rotationAngle) * dx - Math.Sin(rotationAngle) * dy;
            double rotatedDy = Math.Sin(rotationAngle) * dx + Math.Cos(rotationAngle) * dy;

            Vector tangent = new Vector(rotatedDx, rotatedDy);
            tangent.Normalize();

            Vector normal = new Vector(-tangent.Y, tangent.X);

            Point tangentEnd = pointOnCurve + tangent * Canvas.ActualWidth;
            dc.DrawLine(new Pen(Brushes.Blue, 2), pointOnCurve, tangentEnd);

            Point normalEnd = pointOnCurve + normal * Canvas.ActualWidth;
            dc.DrawLine(new Pen(Brushes.Green, 2), pointOnCurve, normalEnd);
            
            dc.DrawEllipse(Brushes.Red, null, pointOnCurve, 3, 3);
        }

        public double? FindNearestPoint(Point targetPoint, double tolerance = 10)
        {
            for(var i = 0; i < HypocycloidPoints.Count; i++)
            {
                var point = HypocycloidPoints[i];
                double distance = Math.Sqrt(Math.Pow(point.X - targetPoint.X, 2) + Math.Pow(point.Y - targetPoint.Y, 2));
                if (distance <= tolerance)
                {
                    return i;
                }
            }
            return null;
        }


        private void Animate()
        {
            if (AnimationDuration <= 0)
            {
                throw new InvalidOperationException("AnimationDuration must be greater than zero.");
            }

            if (Model.AreValuesEqual(AnimationModel)) return;

            var startTime = DateTime.Now;
            var duration = TimeSpan.FromSeconds(AnimationDuration);

            // Capture starting values
            double initialLargeRadius = Model.LargeRadius;
            double targetLargeRadius = AnimationModel.LargeRadius;

            double initialSmallRadius = Model.SmallRadius;
            double targetSmallRadius = AnimationModel.SmallRadius;

            double initialDistance = Model.Distance;
            double targetDistance = AnimationModel.Distance;

            double initialAngle = Model.Angle;
            double targetAngle = AnimationModel.Angle;

            double initialRotationAngle = Model.RotationAngle;
            double targetRotationAngle = AnimationModel.RotationAngle;

            // Set up the timer
            var timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (s, e) =>
            {
                var elapsedTime = DateTime.Now - startTime;
                var progress = Math.Min(1.0, elapsedTime.TotalMilliseconds / duration.TotalMilliseconds);

                // Interpolate values
                Model.LargeRadius = Lerp(initialLargeRadius, targetLargeRadius, progress);
                Model.SmallRadius = Lerp(initialSmallRadius, targetSmallRadius, progress);
                Model.Distance = Lerp(initialDistance, targetDistance, progress);
                Model.Angle = Lerp(initialAngle, targetAngle, progress);
                Model.RotationAngle = Lerp(initialRotationAngle, targetRotationAngle, progress);

                // Stop the timer when the animation is complete
                if (progress >= 1.0)
                {
                    ((DispatcherTimer)s).Stop();
                    IsNotAnimating = true;
                }
            },
            Dispatcher.CurrentDispatcher
            );

            IsNotAnimating = false;
            timer.Start();
        }

        private static double Lerp(double start, double end, double t)
        {
            return start + (end - start) * t;
        }

        public override void MoveElement(Vector offset)
        {
            if (InputManager.AnyKeyButShiftPressed) return;
            Center = SnappingEnabled ? Center.OffsetAndSpanPoint(offset) : Center.OffsetPoint(offset);

            base.MoveElement(offset);
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
            Model.RotationAngle -= degrees;
            Model.RotationAngle = Helpers.NormalizeAngle(Model.RotationAngle);
            Center = Center.RotatePoint(anchorPoint, degrees);
        }
        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
            Center = Center.ScalePoint(anchorPoint, scaleVector);
            Model.LargeRadius *= ScaleFactor;
            Model.SmallRadius *= ScaleFactor;
        }

        public void UpdateUIControls()
        {
            LargeCircle.Center = Center;

            SmallCircle.Center = GetSmallCircleCenter(Model.Angle + Model.RotationAngle);

            LargeCircle.Visibility = ControlsVisibility;
            SmallCircle.Visibility = ControlsVisibility;
            EndPoint.Visibility = ControlsVisibility;

            LargeCircle.ControlsVisible = ControlsVisibility is Visibility.Visible;
            SmallCircle.ControlsVisible = ControlsVisibility is Visibility.Visible;
        }

        public override string ToString()
        {
            return $"Hypercycloid\nLr: {LargeCircle.Radius}\nSr: {SmallCircle.Radius}\nDistance: {Model.Distance}";
        }
    }
}

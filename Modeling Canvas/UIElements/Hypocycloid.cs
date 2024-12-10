using Modeling_Canvas.Extensions;
using Modeling_Canvas.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;


namespace Modeling_Canvas.UIElements
{
    public partial class Hypocycloid : Element
    {
        private int _maxPointCount = 1000;

        public int MaxPointCount { 
            get => _maxPointCount; 
            set {
                if (_maxPointCount != value)
                {
                    _maxPointCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Precision {
            get {
                var maxQualityPointsCount = Model.Angle / 360 * _maxPointCount;
                var calculatedCount = Canvas.UnitSize > 1 ? (maxQualityPointsCount / (20 / Canvas.UnitSize)) : _maxPointCount;
                return calculatedCount > maxQualityPointsCount ? (int)maxQualityPointsCount : (int) calculatedCount;
            }
        }
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

        protected override void InitChildren()
        {
            LargeCircle = new CustomCircle(Canvas, false)
            {
                Radius = Model.LargeRadius,
            };

            SmallCircle = new CustomCircle(Canvas, false)
            {
                Radius = Model.SmallRadius,
            };

            LargeCircle.IsSelectable = false;
            SmallCircle.IsSelectable = false;

            LargeCircle.IsInteractable = false;
            SmallCircle.IsInteractable = false;

            LargeCircle.RadiusControlDistance = 1.5;
            SmallCircle.RadiusControlDistance = 0.5;

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

            LargeCircle.Visibility = ControlsVisibility;
            SmallCircle.Visibility = ControlsVisibility;
            EndPoint.Visibility = ControlsVisibility;

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName.Equals(nameof(ControlsVisibility)))
                {
                    LargeCircle.Visibility = ControlsVisibility;
                    SmallCircle.Visibility = ControlsVisibility;
                    EndPoint.Visibility = ControlsVisibility;
                }
            };
            var index = Panel.GetZIndex(this) - 1;
            if (index < 0) index = 0;
            AddChildren(SmallCircle, index);
            AddChildren(LargeCircle, index);
            AddChildren(EndPoint, index);
            base.InitChildren();
        }

        protected override void InitControlPanel()
        {
            var precisionSlider =
                WpfHelper.CreateSliderControl<double>(
                    "Max points count",
                    this,
                    nameof(MaxPointCount),
                    5,
                    10000,
                    5
                );
            _uiControls.Add(nameof(MaxPointCount), precisionSlider);

            var arcLengthText = WpfHelper.CreateValueTextBlock(
                "Arc Length",
                CalculatedValues,
                nameof(HypocycloidCalculationsModel.ArcLength)
                );

            arcLengthText.AddVisibilityBinding(CalculatedValues,nameof(HypocycloidCalculationsModel.ShowArcLength));

            _uiControls.Add(nameof(HypocycloidCalculationsModel.ArcLength), arcLengthText);

            var areaText = WpfHelper.CreateValueTextBlock(
                "Area",
                CalculatedValues,
                nameof(HypocycloidCalculationsModel.HypocycloidArea)
                );

            areaText.AddVisibilityBinding(CalculatedValues, nameof(HypocycloidCalculationsModel.ShowArcLength));

            _uiControls.Add(nameof(HypocycloidCalculationsModel.ShowHypocycloidArea), areaText);

            var showInflectionPointsCheckbox = WpfHelper.CreateLabeledCheckBox("Inflection points", CalculatedValues, nameof(HypocycloidCalculationsModel.ShowInflectionPoints));

            _uiControls.Add(nameof(HypocycloidCalculationsModel.ShowInflectionPoints), showInflectionPointsCheckbox);

            var animateMenuCheckbox = WpfHelper.CreateLabeledCheckBox("Animate", this, nameof(ShowAnimationControls));

            _uiControls.Add(nameof(ShowAnimationControls), animateMenuCheckbox);

            var hypoControls = CreateHypocycloidControls(Model);

            _uiControls = _uiControls.Concat(hypoControls).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

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

            _uiControls.Add("TimeSlider", timeSlider);

            var startAnimationButton = WpfHelper.CreateButton(
                content: "Animate",
                clickAction: () => Animate()
                );

            startAnimationButton.AddVisibilityBinding(this, nameof(ShowAnimationControls));

            startAnimationButton.AddIsDisabledBinding(this, nameof(IsNotAnimating));

            _uiControls.Add("AnimateButton", startAnimationButton);

            base.InitControlPanel();

        }

        protected override void RenderControlPanel()
        {
            ClearControlPanel();
            foreach (var control in _uiControls)
            {
                AddElementToControlPanel(control.Value);

                if (_animationControls.TryGetValue(control.Key, out var animationControl))
                {
                    AddElementToControlPanel(animationControl);
                }
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            LargeCircle.Center = Center;
            SmallCircle.Center = GetSmallCircleCenter(Model.Angle + Model.RotationAngle);


            LargeCircle.ControlsVisible = ControlsVisibility is Visibility.Visible;
            SmallCircle.ControlsVisible = ControlsVisibility is Visibility.Visible;
            if (InputManager.AltPressed)
            {
                var t = FindNearestPoint(Mouse.GetPosition(Canvas));
                if (t is not null) DrawTangentsAndNormals(dc, t.Value);
            }
            base.OnRender(dc);
        }
        protected override void DefaultRender(DrawingContext dc)
        {
            HypocycloidPoints = CalculateHypocycloidPoints(dc, Model, HypocycloidPoints);
            EndPoint.Position = Canvas.GetUnitCoordinates(HypocycloidPoints.LastOrDefault());

            if(HypocycloidPoints.Count == 0)
            {
                dc.DrawCircle(Canvas, Brushes.DeepPink, null, LargeCircle.CenterPoint.PixelPosition, 5, 15, 0, false);
                return;
            }

            for (int i = 0; i < HypocycloidPoints.Count - 1; i++)
            {
                dc.DrawLine(Canvas, StrokePen, HypocycloidPoints[i], HypocycloidPoints[i + 1], 10);
            }
            if (CalculatedValues.ShowInflectionPoints && Model.Distance > 1e-2)
            {
                CalculatedValues.InflectionPoints = CalculateInflectionPoints(Model);
                foreach (var point in CalculatedValues.InflectionPoints)
                {
                    dc.DrawCircle(Canvas, Brushes.DeepPink, null, point, 5, 15, 0, false);
                }
            }
            if (CalculatedValues.ShowArcLength)
            {
                CalculatedValues.ArcLength = CalculateHypocycloidLength(Model);
            }
            if (CalculatedValues.ShowHypocycloidArea)
            {
                CalculatedValues.HypocycloidArea = CalculateHypocycloidArea(Model);
            }
        }

        protected virtual List<Point> CalculateHypocycloidPoints(DrawingContext dc, HypocycloidModel model, List<Point> points = null)
        {
            if(points is null) points = new List<Point>();
            else points.Clear();

            double R = model.LargeRadius * UnitSize;
            double r = model.SmallRadius * UnitSize;
            double d = model.Distance * UnitSize;

            var center = LargeCircle.CenterPoint.PixelPosition;

            double constant1 = R - r;
            double constant2 = constant1 / r;
            double rotationAngle = Helpers.DegToRad(model.RotationAngle);
            double cosRotation = Math.Cos(rotationAngle);
            double sinRotation = Math.Sin(rotationAngle);

            for (int i = 0; i < Precision; i++)
            {
                double t = Helpers.DegToRad(model.Angle) * i / Precision;
                double x = constant1 * Math.Cos(t) + d * Math.Cos(constant2 * t);
                double y = constant1 * Math.Sin(t) - d * Math.Sin(constant2 * t);

                double rotatedX = cosRotation * x - sinRotation * y;
                double rotatedY = sinRotation * x + cosRotation * y;

                points.Add(new Point(center.X + rotatedX, center.Y + rotatedY));
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

            double prevRadius = 0;
            double prevDerivative = 0;

            for (int i = 0; i <= Precision; i++)
            {
                double t = Helpers.DegToRad(model.Angle) * i / Precision;

                double x = (R - r) * Math.Cos(t) + d * Math.Cos((R - r) / r * t);
                double y = (R - r) * Math.Sin(t) - d * Math.Sin((R - r) / r * t);

                double radius = Math.Sqrt(x * x + y * y);

                double derivative = radius - prevRadius;

                if (prevDerivative * derivative < 0)
                {
                    var rotationAngle = Helpers.DegToRad(model.RotationAngle);
                    double rotatedX = Math.Cos(rotationAngle) * x - Math.Sin(rotationAngle) * y;
                    double rotatedY = Math.Sin(rotationAngle) * x + Math.Cos(rotationAngle) * y;

                    inflectionPoints.Add(new Point(center.X + rotatedX, center.Y + rotatedY));
                }

                prevRadius = radius;
                prevDerivative = derivative;
            }

            return inflectionPoints;
        }

        protected double CalculateHypocycloidLength(HypocycloidModel model)
        {
            double R = model.LargeRadius;
            double r = model.SmallRadius;
            double d = model.Distance;

            int segments = 1000;
            double length = 0;

            for (int i = 0; i < segments; i++)
            {
                double t1 = i * Helpers.DegToRad(Model.Angle) / segments;
                double t2 = (i + 1) * Helpers.DegToRad(Model.Angle) / segments;

                double x1 = (R - r) * Math.Cos(t1) + d * Math.Cos((R - r) / r * t1);
                double y1 = (R - r) * Math.Sin(t1) - d * Math.Sin((R - r) / r * t1);
                double x2 = (R - r) * Math.Cos(t2) + d * Math.Cos((R - r) / r * t2);
                double y2 = (R - r) * Math.Sin(t2) - d * Math.Sin((R - r) / r * t2);

                // calculate each segment length
                length += Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            }

            return length;
        }

        protected double CalculateHypocycloidArea(HypocycloidModel model)
        {
            double R = model.LargeRadius;
            double r = model.SmallRadius;
            double angleFraction = model.Angle / 360.0; // Fraction of the full rotation

            // Area is scaled based on the fraction of the angle
            return Math.PI * Math.Pow(R - r, 2) * angleFraction;
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

            double t = Helpers.DegToRad(Model.Angle) * tParameter / Precision;
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
            dc.DrawLine(Canvas, new Pen(Brushes.Blue, 2), pointOnCurve, tangentEnd);

            Point normalEnd = pointOnCurve + normal * Canvas.ActualWidth;
            dc.DrawLine(Canvas, new Pen(Brushes.Green, 2), pointOnCurve, normalEnd);
            
            dc.DrawCircle(Canvas, Brushes.Red, null, pointOnCurve, 7, 15, 0, false);
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

            //double initialRotationAngle = Model.RotationAngle;
            //double targetRotationAngle = AnimationModel.RotationAngle;

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
                //Model.RotationAngle = Lerp(initialRotationAngle, targetRotationAngle, progress);

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

        protected override Point GetAnchorDefaultPosition()
        {
            return Center;
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

        public override string ToString()
        {
            return $"Hypocycloid\nLr: {LargeCircle.Radius}, Sr: {SmallCircle.Radius}\nDistance: {Model.Distance}{(CalculatedValues.ShowArcLength ? $"\nArc Length: {Math.Round(CalculatedValues.ArcLength, 3)}" : "")}{(CalculatedValues.ShowHypocycloidArea ? $",Area: {Math.Round(CalculatedValues.HypocycloidArea, 3)}" : "")}";

        }
    }
}

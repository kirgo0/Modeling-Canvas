using Modeling_Canvas.Extensions;
using Modeling_Canvas.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public partial class Hypocycloid : CustomElement
    {
        public int PointsCount { get; set; } = 1000;
        public CustomCircle LargeCircle { get; set; }
        public CustomCircle SmallCircle { get; set; }
        public CustomPoint EndPoint { get; set; }
        public HypocycloidModel Model { get; set; }

        private Dictionary<string, UIElement> _controls = new();
        public Point Center
        {
            get => LargeCircle.Center;
            set => LargeCircle.Center = value;
        }
        public Hypocycloid(CustomCanvas canvas, double largeRadius, double smallRadius, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            StrokeThickness = 3;
            Model = new();

            canvas.MouseWheel += (o, e) =>
            {
                Model.MaxLargeCircleRadius = Math.Max(Canvas.ActualHeight, Canvas.ActualWidth) / UnitSize / 2;
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

        protected override void InitControls()
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
            CreateUIControls();
            base.InitControls();
        }

        public List<(Point, int)> HypocycloidPoints { get; set; } = new();

        protected override void DefaultRender(DrawingContext dc)
        {
            DrawHypocycliod(dc);
        }

        protected override void AffineRender(DrawingContext dc)
        {
        }

        protected override void ProjectiveRender(DrawingContext dc)
        {
        }

        protected virtual void DrawHypocycliod(DrawingContext dc)
        {
            HypocycloidPoints.Clear();
            double R = Model.LargeRadius * UnitSize; 
            double r = Model.SmallRadius * UnitSize;
            double d = Model.Distance * UnitSize;

            var center = LargeCircle.CenterPoint.PixelPosition;

            for (int i = 0; i < PointsCount; i++)
            {
                double t = Helpers.DegToRad(Model.Angle) * i / PointsCount;
                double x = (R - r) * Math.Cos(t) + d * Math.Cos((R - r) / r * t);
                double y = (R - r) * Math.Sin(t) - d * Math.Sin((R - r) / r * t);

                var rotationAngle = Helpers.DegToRad(Model.RotationAngle);
                double rotatedX = Math.Cos(rotationAngle) * (x) - Math.Sin(rotationAngle) * (y);
                double rotatedY = Math.Sin(rotationAngle) * (x) + Math.Cos(rotationAngle) * (y);
                HypocycloidPoints.Add((new Point(center.X + rotatedX, center.Y + rotatedY), i));
            }

            for (int i = 0; i < HypocycloidPoints.Count - 1; i++)
            {
                dc.DrawLine(StrokePen, HypocycloidPoints[i].Item1, HypocycloidPoints[i + 1].Item1, 10);
            }

            EndPoint.Position = Canvas.GetCanvasUnitCoordinates(HypocycloidPoints.Last().Item1);
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
            double R = Model.LargeRadius * UnitSize; // Великий радіус в одиницях Canvas
            double r = Model.SmallRadius * UnitSize; // Малий радіус в одиницях Canvas
            double d = Model.Distance * UnitSize;    // Відстань до точки в одиницях Canvas

            var center = LargeCircle.CenterPoint.PixelPosition;

            // Обчислюємо координати точки на гіпоциклоїді
            double t = Helpers.DegToRad(Model.Angle) * tParameter / PointsCount;
            double x = (R - r) * Math.Cos(t) + d * Math.Cos((R - r) / r * t);
            double y = (R - r) * Math.Sin(t) - d * Math.Sin((R - r) / r * t);

            var rotationAngle = Helpers.DegToRad(Model.RotationAngle);
            double rotatedX = Math.Cos(rotationAngle) * x - Math.Sin(rotationAngle) * y;
            double rotatedY = Math.Sin(rotationAngle) * x + Math.Cos(rotationAngle) * y;

            Point pointOnCurve = new Point(center.X + rotatedX, center.Y + rotatedY);

            // Обчислюємо похідні для дотичної
            double dx = -(R - r) * Math.Sin(t) - d * ((R - r) / r) * Math.Sin((R - r) / r * t);
            double dy = (R - r) * Math.Cos(t) - d * ((R - r) / r) * Math.Cos((R - r) / r * t);

            double rotatedDx = Math.Cos(rotationAngle) * dx - Math.Sin(rotationAngle) * dy;
            double rotatedDy = Math.Sin(rotationAngle) * dx + Math.Cos(rotationAngle) * dy;

            Vector tangent = new Vector(rotatedDx, rotatedDy);
            tangent.Normalize();

            // Нормальний вектор (перпендикуляр до дотичної)
            Vector normal = new Vector(-tangent.Y, tangent.X);

            // Малюємо дотичну
            Point tangentEnd = pointOnCurve + tangent * Canvas.ActualWidth; // Довжина вектора
            dc.DrawLine(new Pen(Brushes.Blue, 2), pointOnCurve, tangentEnd);

            // Малюємо нормаль
            Point normalEnd = pointOnCurve + normal * Canvas.ActualWidth; // Довжина вектора
            dc.DrawLine(new Pen(Brushes.Green, 2), pointOnCurve, normalEnd);

            // Малюємо точку на кривій
            dc.DrawEllipse(Brushes.Red, null, pointOnCurve, 3, 3);
        }

        public double? FindNearestPoint(Point targetPoint, double tolerance = 10)
        {
            foreach (var item in HypocycloidPoints)
            {
                var point = item.Item1;
                double distance = Math.Sqrt(Math.Pow(point.X - targetPoint.X, 2) + Math.Pow(point.Y - targetPoint.Y, 2));
                if (distance <= tolerance)
                {
                    return item.Item2;
                }
            }
            return null;
        }

        protected override void RenderControlPanel()
        {
            ClearControlPanel();
            //AddDistanceControls();
            //AddAngleControls();
            //AddRadiusControls();
            ClearControlPanel();
            foreach (var control in _controls.Values)
            {
                AddElementToControlPanel(control);
            }
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
        }
        public virtual void CenterPointMoveAction(DraggablePoint point, Vector offset)
        {
            MoveElement(offset);
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

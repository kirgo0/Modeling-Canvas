using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Modeling_Canvas.UIElements
{
    public partial class Hypocycloid : CustomElement
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
        private double _rotationAngle = 0;
        public int PointsCount { get; set; } = 1000;
        public CustomCircle LargeCircle { get; set; }
        public CustomCircle SmallCircle { get; set; }
        public DraggablePoint CenterPoint { get; set; }
        public CustomPoint EndPoint { get; set; }
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
                Radius = largeRadius,
                HasAnchorPoint = false
            };
            SmallCircle = new CustomCircle(canvas)
            {
                Radius = smallRadius,
                HasAnchorPoint = false
            };
            canvas.MouseWheel += (o, e) =>
            {
                if (Canvas.SelectedElements.Contains(this))
                    RenderControlPanel();
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

            EndPoint = new CustomPoint(Canvas)
            {
                StrokeThickness = 0,
                Fill = Brushes.Red,
                Radius = 5
            };

            Canvas.Children.Add(CenterPoint);
            Canvas.Children.Add(SmallCircle);
            Canvas.Children.Add(LargeCircle);
            Canvas.Children.Add(EndPoint);
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
            double R = LargeRadius * UnitSize; // Великий радіус в одиницях Canvas
            double r = SmallRadius * UnitSize; // Малий радіус в одиницях Canvas
            double d = Distance * UnitSize;    // Відстань до точки в одиницях Canvas

            var center = CenterPoint.PixelPosition;

            for (int i = 0; i < PointsCount; i++)
            {
                double t = Helpers.DegToRad(Angle) * i / PointsCount;
                double x = (R - r) * Math.Cos(t) + d * Math.Cos((R - r) / r * t);
                double y = (R - r) * Math.Sin(t) - d * Math.Sin((R - r) / r * t);

                var rotationAngle = Helpers.DegToRad(_rotationAngle);
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

            x = Center.X + (LargeRadius - SmallRadius) * Math.Cos(angle);
            y = Center.Y - (LargeRadius - SmallRadius) * Math.Sin(angle);

            return new Point(x, y);
        }

        public void DrawTangentsAndNormals(DrawingContext dc, double tParameter)
        {
            double R = LargeRadius * UnitSize; // Великий радіус в одиницях Canvas
            double r = SmallRadius * UnitSize; // Малий радіус в одиницях Canvas
            double d = Distance * UnitSize;    // Відстань до точки в одиницях Canvas

            var center = CenterPoint.PixelPosition;

            // Обчислюємо координати точки на гіпоциклоїді
            double t = Helpers.DegToRad(Angle) * tParameter / PointsCount;
            double x = (R - r) * Math.Cos(t) + d * Math.Cos((R - r) / r * t);
            double y = (R - r) * Math.Sin(t) - d * Math.Sin((R - r) / r * t);

            var rotationAngle = Helpers.DegToRad(_rotationAngle);
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
            AddDistanceControls();
            AddAngleControls();
            AddRadiusControls();
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
            _rotationAngle = Helpers.NormalizeAngle(_rotationAngle);
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

            SmallCircle.Center = GetSmallCircleCenter(Angle + _rotationAngle);

            LargeCircle.Visibility = ControlsVisibility;
            SmallCircle.Visibility = ControlsVisibility;
            EndPoint.Visibility = ControlsVisibility;

            LargeCircle.ControlsVisible = ControlsVisibility is Visibility.Visible;
            SmallCircle.ControlsVisible = ControlsVisibility is Visibility.Visible;
        }

        public override string ToString()
        {
            return $"Hypercycloid\nLr: {LargeCircle.Radius}\nSr: {SmallCircle.Radius}\nDistance: {Distance}";
        }
    }
}

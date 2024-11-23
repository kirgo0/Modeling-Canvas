using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Modeling_Canvas.UIELements
{
    public class CustomCircle : CustomElement 
    {
        public CustomCircle(CustomCanvas canvas) : base(canvas)
        {
        }

        public double Radius { get; set; } = 5; // Default radius
        public int Precision { get; set; } = 100; // Number of points for the circle
        public double StartDegrees { get; set; } = 0; // Start angle in degrees
        public double EndDegrees { get; set; } = 360; // End angle in degrees

        public Point Center { get; set; } = new Point(1, 1);

        public CustomPoint CenterPoint { get; set; }
        public DraggablePoint RadiusPoint { get; set; }
        public DraggablePoint StartDegreesPoint { get; set; }
        public DraggablePoint EndDegreesPoint { get; set; }

        public bool IsFirstRender { get; set; } = true;

        public bool ShowControls { get => Canvas.SelectedElements.Contains(this); }

        public override Point GetOriginPoint(Size arrangedSize)
        {
            return new Point(arrangedSize.Width / 2 + UnitSize * Center.X, arrangedSize.Height / 2 - UnitSize * Center.Y);
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Canvas != null && IsFirstRender)
            {
                AddUIControls();
                IsFirstRender = false;
            }

            base.OnRender(drawingContext);


            CenterPoint.Position = Center;

            RadiusPoint.Opacity = ShowControls ? 0.7 : 0;
            RadiusPoint.Position = new Point(Center.X + (Radius + 1) * Math.Cos(0), Center.Y - Radius * Math.Sin(0));

            StartDegreesPoint.Opacity = ShowControls ? 0.3 : 0;
            StartDegreesPoint.Position = new Point(Center.X + Radius * Math.Cos(DegToRad(StartDegrees)), Center.Y - Radius * Math.Sin(DegToRad(StartDegrees)));

            EndDegreesPoint.Opacity = ShowControls ? 0.3 : 0;
            EndDegreesPoint.Position = new Point(Center.X + Radius * Math.Cos(DegToRad(EndDegrees)), Center.Y - Radius * Math.Sin(DegToRad(EndDegrees)));

            // Нормалізуємо кути
            //double normalizedStart = NormalizeAngle(StartDegrees);
            //double normalizedEnd = NormalizeAngle(EndDegrees);

            // Створюємо геометрію сегмента кола
            var geometry = CreateCircleSegmentGeometry(new Point(0, 0), Radius * UnitSize, StartDegrees, EndDegrees, Precision);

            // Малюємо сегмент
            drawingContext.DrawGeometry(Fill, new Pen(Stroke, StrokeThickness), geometry);
        }

        private void AddUIControls()
        {
            RadiusPoint = new DraggablePoint(Canvas)
            {
                Radius = 8,
                MoveAction = RadiusPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
            };
            Canvas.Children.Add(RadiusPoint);

            StartDegreesPoint = new DraggablePoint(Canvas)
            {
                MoveAction = StartDegreesPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                Fill = Brushes.Red
            };
            Canvas.Children.Add(StartDegreesPoint);

            EndDegreesPoint = new DraggablePoint(Canvas)
            {
                MoveAction = EndDegreesPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                Fill = Brushes.Blue
            };
            Canvas.Children.Add(EndDegreesPoint);

            CenterPoint = new CustomPoint(Canvas) { Radius = 3, MoveAction = MoveElement };
            Canvas.Children.Add(CenterPoint);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if(!Canvas.IsSpacePressed)
            {
                Canvas.SelectedElements.Clear();
                Canvas.SelectedElements.Add(this);
                e.Handled = true;
            }
            base.OnMouseLeftButtonDown(e);
            InvalidateCanvas();
        }

        public virtual void RadiusPointMoveAction(Vector offset)
        {
            if (Canvas.GridSnapping)
            {
                Radius = Round(Radius + offset.X / UnitSize);
            }
            else
            {
                Radius += offset.X / UnitSize;
            }
            if (Radius <= 0.5) Radius = 0.5;
        }

        public virtual void StartDegreesPointMoveAction(Vector offset)
        {
            if (Canvas.GridSnapping && Math.Abs(StartDegrees - EndDegrees) < 5)
            {
                StartDegrees = EndDegrees;
            } else
            {
                StartDegrees = GetAngleBetweenMouseAndCenter();
            }
        }
        public virtual void EndDegreesPointMoveAction(Vector offset)
        {
            if (Canvas.GridSnapping && Math.Abs(StartDegrees - EndDegrees) < 5)
            {
                EndDegrees = StartDegrees;
            }
            else
            {
                EndDegrees = GetAngleBetweenMouseAndCenter();
            }
        }

        public double GetAngleBetweenMouseAndCenter()
        {
            var mouseOnCanvas = Mouse.GetPosition(Canvas);
            var mousePosition = Canvas.GetCanvasUnitCoordinates(mouseOnCanvas);
            var angleInDegrees = Math.Atan2(-(mousePosition.Y - Center.Y), mousePosition.X - Center.X) * (180 / Math.PI);
            return (angleInDegrees + 360) % 360;
        }

        public virtual void OnPointMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private Geometry CreateCircleSegmentGeometry(Point center, double radius, double startDegrees, double endDegrees, int precision)
        {
            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                double normalizedStart = NormalizeAngle(startDegrees);
                double normalizedEnd = NormalizeAngle(endDegrees);

                if (normalizedEnd <= normalizedStart)
                {
                    DrawArcSegment(context, center, radius, normalizedStart, 360, precision);
                    DrawArcSegment(context, center, radius, 0, normalizedEnd, precision);
                }
                else
                {
                    DrawArcSegment(context, center, radius, normalizedStart, normalizedEnd, precision);
                }
            }

            geometry.Freeze();
            return geometry;
        }

        private void DrawArcSegment(StreamGeometryContext context, Point center, double radius, double startDegrees, double endDegrees, int precision)
        {
            double startRadians = DegToRad(startDegrees);
            double endRadians = DegToRad(endDegrees);

            double segmentStep = (endRadians - startRadians) / precision;

            var startPoint = new Point(
                center.X + radius * Math.Cos(startRadians),
                center.Y + radius * Math.Sin(startRadians));
            context.BeginFigure(startPoint, true, false);

            for (int i = 1; i <= precision; i++)
            {
                double angle = startRadians + i * segmentStep;
                var point = new Point(
                    center.X + radius * Math.Cos(angle),
                    center.Y + radius * Math.Sin(angle));
                context.LineTo(point, true, false);
            }
        }

        private double DegToRad(double deg) => Math.PI * deg / 180.0;
        private double NormalizeAngle(double angle) => (angle % 360 + 360) % 360;

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size((Radius * UnitSize + StrokeThickness) * 2, (Radius * UnitSize + StrokeThickness) * 2);
        }

        protected override void MoveElement(Vector offset)
        {
            if (Canvas.IsCtrlPressed || Canvas.IsSpacePressed) return;

            Point newCenter = new Point(
                Center.X + offset.X / UnitSize,
                Center.Y - offset.Y / UnitSize);

            Center = Canvas.GridSnapping ? new Point(Round(newCenter.X), Round(newCenter.Y)) : newCenter;
            InvalidateCanvas();
        }

        public static double Round(double value)
        {
            const double lowerThreshold = 0.1;
            const double upperThreshold = 0.9;
            const double toHalfLower = 0.45;
            const double toHalfUpper = 0.55;

            double integerPart = Math.Floor(value);
            double fractionalPart = value - integerPart;

            if (fractionalPart <= lowerThreshold) return integerPart;
            if (fractionalPart >= upperThreshold) return integerPart + 1;
            if (fractionalPart >= toHalfLower && fractionalPart <= toHalfUpper) return integerPart + 0.5;
            return value;
        }

        public override string ToString()
        {
            return $"X: {Center.X} \nY: {Center.Y} \nRadius: {Radius}\nStart: {StartDegrees}\nEnd: {EndDegrees}";
        }
    }
}

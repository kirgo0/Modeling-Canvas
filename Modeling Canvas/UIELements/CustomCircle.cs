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
        public int Precision { get; set; } = 100; // Number of points for the circle
        private double _radius  = 5; 
        public double Radius { 
            get => _radius; 
            set {
                if (value <= 0.5) _radius = 0.5;
                else _radius = value; 
            }
        }
        public double StartDegrees { get; set; } = 0; // Start angle in degrees
        public double EndDegrees { get; set; } = 360; // End angle in degrees
        public Point Center { get; set; } = new Point(1, 1);
        public DraggablePoint CenterPoint { get; set; }
        public DraggablePoint RadiusPoint { get; set; }
        public DraggablePoint StartDegreesPoint { get; set; }
        public DraggablePoint EndDegreesPoint { get; set; }

        public override Point GetOriginPoint(Size arrangedSize)
        {
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
            };
            Canvas.Children.Add(RadiusPoint);

            StartDegreesPoint = new DraggablePoint(Canvas)
            {
                Opacity = 0.7,
                OverrideMoveAction = StartDegreesPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                Fill = Brushes.Red,
                HasAnchorPoint = false
            };
            Canvas.Children.Add(StartDegreesPoint);

            EndDegreesPoint = new DraggablePoint(Canvas)
            {
                Opacity = 0.7,
                OverrideMoveAction = EndDegreesPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                Fill = Brushes.Blue,
                HasAnchorPoint = false
            };
            Canvas.Children.Add(EndDegreesPoint);

            CenterPoint = new DraggablePoint(Canvas)
            {
                Radius = 3,
                OverrideMoveAction = CenterPointMoveAction,
                HasAnchorPoint = false
            };
            Canvas.Children.Add(CenterPoint);
            base.InitControls();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            UpdateUIControls();

            // Create and draw the circle segment geometry
            var geometry = CreateCircleSegmentGeometry(new Point(0, 0), Radius * UnitSize, StartDegrees, EndDegrees, Precision);
            drawingContext.DrawGeometry(Fill, new Pen(Stroke, StrokeThickness), geometry);
            base.OnRender(drawingContext);
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
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if(!InputManager.SpacePressed)
            {
                Canvas.SelectedElements.Clear();
                Canvas.SelectedElements.Add(this);
                e.Handled = true;
            }
            base.OnMouseLeftButtonDown(e);
            InvalidateCanvas();
        }

        public virtual void RadiusPointMoveAction(DraggablePoint point, Vector offset)
        {
            if (SnappingEnabled)
            {
                Radius = SnapValue(Radius + offset.X / UnitSize);
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
            } else
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

        public override void MoveElement(Vector offset)
        {
            if (InputManager.AnyKeyButShiftPressed) return;

            Point newCenter = new Point(
                Center.X + offset.X / UnitSize,
                Center.Y - offset.Y / UnitSize);

            Center = SnappingEnabled ? new Point(SnapValue(newCenter.X), SnapValue(newCenter.Y)) : newCenter;
            base.MoveElement(offset);
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
            Center = RotatePoint(Center, anchorPoint, degrees);
            EndDegrees -= degrees;
            StartDegrees -= degrees;
            StartDegrees = NormalizeAngle(StartDegrees);
            EndDegrees = NormalizeAngle(EndDegrees);
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
            Center = ScalePoint(Center, anchorPoint, scaleVector);
            Radius *= ScaleFactor;
        }

        private void UpdateUIControls()
        {
            AnchorVisibility = ShowControls;
            CenterPoint.Visibility = ShowControls;
            RadiusPoint.Visibility = ShowControls;
            StartDegreesPoint.Visibility = ShowControls;
            EndDegreesPoint.Visibility = ShowControls;

            RadiusPoint.Position = new Point(Center.X + (Radius + 1) * Math.Cos(DegToRad(0)), Center.Y - Radius * Math.Sin(0));
            StartDegreesPoint.Position = new Point(Center.X + Radius * Math.Cos(DegToRad(StartDegrees)), Center.Y - Radius * Math.Sin(DegToRad(StartDegrees)));
            EndDegreesPoint.Position = new Point(Center.X + Radius * Math.Cos(DegToRad(EndDegrees)), Center.Y - Radius * Math.Sin(DegToRad(EndDegrees)));
            CenterPoint.Position = Center;
        }
        public override string ToString()
        {
            return $"X: {Center.X} \nY: {Center.Y} \nRadius: {Radius}\nStart: {StartDegrees}\nEnd: {EndDegrees}\nTL: {GetTopLeftPosition()}\nBR: {GetBottomRightPosition()}";
        }

    }
}

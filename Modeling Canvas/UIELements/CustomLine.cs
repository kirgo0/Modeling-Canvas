using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIELements
{
    public class CustomLine : CustomElement
    {
        public int PointsRadius { get; set; } = 5;
        public PointShape PointsShape { get; set; } = PointShape.Circle;
        public List<DraggablePoint> Points { get; } = new();
        public CustomLine(CustomCanvas canvas) : base(canvas)
        {
            StrokeThickness = 3;
            Stroke = Brushes.Cyan;
        }
        public CustomLine(CustomCanvas canvas, Point firstPoint, Point secondPoint) : base(canvas)
        {
            StrokeThickness = 2;
            Stroke = Brushes.Cyan;
            AddPoint(firstPoint.X, firstPoint.Y);
            AddPoint(secondPoint.X, secondPoint.Y);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            AnchorVisibility = ShowControls;
            foreach(var point in Points)
            {
                point.Visibility = ShowControls;
            }

            for (int i = 0; i < Points.Count-1; i++) {
                drawingContext.DrawLine(new Pen(Stroke, StrokeThickness), Points[i].PixelPosition, Points[i + 1].PixelPosition);
                drawingContext.DrawLine(new Pen(Brushes.Transparent, StrokeThickness + 10), Points[i].PixelPosition, Points[i + 1].PixelPosition);
            }
            base.OnRender(drawingContext);
        }
        protected override Point GetAnchorDefaultPosition()
        {
            if (Points == null || !Points.Any()) return new Point(0,0);

            // Calculate the average of X and Y coordinates
            double centerX = Points.Average(p => p.Position.X);
            double centerY = Points.Average(p => p.Position.Y);

            return new Point(centerX, centerY);
        }

        public void AddPoint(double x, double y)
        {
            var point = new DraggablePoint(Canvas, new Point(x, y))
            {
                Shape = PointsShape,
                Radius = PointsRadius
            };
            Points.Add(point);
            Canvas.Children.Add(point);
            Panel.SetZIndex(point, Canvas.Children.Count+1);
        }

        public void AddPoint(Point point)
        {
            var draggablepoint = new DraggablePoint(Canvas, point);
            Points.Add(draggablepoint);
            Canvas.Children.Add(draggablepoint);
        }

        public override void MoveElement(Vector offset)
        {
            foreach (var point in Points)
            {
                point.MoveElement(offset);
            }
            base.MoveElement(offset);
        }
        protected override void RotateElement(double degrees)
        {
            foreach (var point in Points)
            {
                point.Position = RotatePoint(point.Position, AnchorPoint.Position, degrees);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!Canvas.IsSpacePressed)
            {
                Canvas.SelectedElements.Clear();
                Canvas.SelectedElements.Add(this);
                e.Handled = true;
            }
            base.OnMouseLeftButtonDown(e);
            InvalidateCanvas();
        }

    }
}

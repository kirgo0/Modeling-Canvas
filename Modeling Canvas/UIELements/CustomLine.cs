using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIELements
{
    public class CustomLine : CustomElement
    {
        public List<DraggablePoint> Points { get; set; } = new();
        public bool IsFirstRender { get; set; } = true;
        public CustomLine(CustomCanvas canvas) : base(canvas)
        {
            StrokeThickness = 3;
            Stroke = Brushes.Cyan;
        }
        protected override Point GetAnchorDefaultPosition()
        {
            if (Points == null || !Points.Any())
                throw new InvalidOperationException("The list of points is empty.");

            // Calculate the average of X and Y coordinates
            double centerX = Points.Average(p => p.Position.X);
            double centerY = Points.Average(p => p.Position.Y);

            return new Point(centerX, centerY);
        }

        public CustomLine(CustomCanvas canvas, Point firstPoint, Point secondPoint) : base(canvas)
        {
            StrokeThickness = 2;
            Stroke = Brushes.Cyan;
            Points.Add(new DraggablePoint(Canvas, firstPoint));
            Points.Add(new DraggablePoint(Canvas, secondPoint));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if(IsFirstRender)
            {
                foreach(var point in Points)
                {
                    point.Radius = 5;
                    Canvas.Children.Add(point);
                }
                IsFirstRender = false;
            }

            AnchorVisible = ShowControls;
            base.OnRender(drawingContext);
            foreach(var point in Points)
            {
                point.Visibility = ShowControls ? Visibility.Visible : Visibility.Hidden;
            }

            for (int i = 0; i < Points.Count-1; i++) {
                drawingContext.DrawLine(new Pen(Stroke, StrokeThickness), Points[i].PixelPosition, Points[i + 1].PixelPosition);
                drawingContext.DrawLine(new Pen(Brushes.Transparent, StrokeThickness + 10), Points[i].PixelPosition, Points[i + 1].PixelPosition);
            }
        }
        public void AddPoint(double x, double y)
        {
            Points.Add(new DraggablePoint(Canvas, new Point(x, y)));
        }

        public void AddPoint(Point point)
        {
            Points.Add(new DraggablePoint(Canvas, point));
        }

        public override void MoveElement(Vector offset)
        {
            foreach (var point in Points)
            {
                point.MoveElement(offset);
            }
            base.MoveElement(offset);
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

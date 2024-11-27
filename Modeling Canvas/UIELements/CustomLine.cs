using System.Net.NetworkInformation;
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

        public bool IsClosed { get; set; } = true;
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

            if (Points.Count < 2) return;

            foreach(var point in Points)
            {
                point.Visibility = ShowControls;
                point.Shape = PointShape.Circle;
            }

            Points.First().Shape = PointShape.Square;
            Points.Last().Shape = PointShape.Square;

            for (int i = 0; i < Points.Count-1; i++) {
                DrawLine(drawingContext, Points[i].PixelPosition, Points[i + 1].PixelPosition);
            }

            if (IsClosed)
            {
                DrawLine(drawingContext, Points.First().PixelPosition, Points.Last().PixelPosition);
            }

            base.OnRender(drawingContext);
        }

        protected void DrawLine(DrawingContext drawingContext, Point p1, Point p2)
        {
            drawingContext.DrawLine(new Pen(Stroke, StrokeThickness), p1, p2);
            drawingContext.DrawLine(new Pen(Brushes.Transparent, StrokeThickness + 10), p1, p2);
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
            AnchorPoint.AllowSnapping = false;
            foreach (var point in Points)
            {
                point.MoveElement(offset);
            }
            base.MoveElement(offset);
            AnchorPoint.AllowSnapping = true;
        }
        public override void RotateElement(Point anchorPoint, double degrees)
        {
            foreach (var point in Points)
            {
                point.Position = RotatePoint(point.Position, anchorPoint, degrees);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!InputManager.SpacePressed)
            {
                Canvas.SelectedElements.Clear();
                Canvas.SelectedElements.Add(this);
                e.Handled = true;
            }
            base.OnMouseLeftButtonDown(e);
            InvalidateCanvas();
        }

        protected override void RenderControlPanel()
        {
            base.RenderControlPanel();
            var addPointButton = new Button
            {
                Content = "Add Point",
                Width = 100,
                Height = 30,
                Margin = new Thickness(5)
            };

            addPointButton.Click += (s, e) =>
            {
                AddPoint(0, 0);
            };
            AddElementToControlPanel(addPointButton);
        }

        public override string ToString()
        {
            return $"Line\nPoints: {Points.Count}";
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
            foreach(var point in Points)
            {
                point.Position = ScalePoint(point.Position, anchorPoint, scaleVector);
            }
        }
    }
}

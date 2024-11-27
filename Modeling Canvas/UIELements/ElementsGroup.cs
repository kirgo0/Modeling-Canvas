using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.UIELements
{
    public class ElementsGroup : CustomElement
    {
        public double RectPadding { get; set; } = 0.5;
        public List<CustomElement> Children { get; set; } = new();

        public ElementsGroup(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            StrokeThickness = 2;
            AnchorVisibility = Visibility.Visible;
        }

        public override Point GetOriginPoint(Size arrangedSize)
        {
            List<Point> tlPoints = new List<Point>();
            //List<Point> brPoints = new List<Point>();
            foreach (var child in Children)
            {
                tlPoints.Add(child.GetTopLeftPosition());
                //brPoints.Add(child.GetBottomRightPosition());
            }
            var topLeftPoint = new Point(tlPoints.Min(x => x.X), tlPoints.Max(y => y.Y));
            //var bottomRight = brPoints.OrderByDescending(y => y.Y).ThenBy(x => x.X).FirstOrDefault();
            return new Point(arrangedSize.Width / 2 + (topLeftPoint.X * UnitSize - RectPadding * UnitSize), arrangedSize.Height / 2 - (topLeftPoint.Y * UnitSize + RectPadding * UnitSize));
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            List<Point> tlPoints = new List<Point>();  
            List<Point> brPoints = new List<Point>();  
            foreach (var child in Children)
            {
                tlPoints.Add(child.GetTopLeftPosition());
                brPoints.Add(child.GetBottomRightPosition());
            }
            var topLeftPoint = new Point(tlPoints.Min(x => x.X), tlPoints.Max(y => y.Y));
            var bottomRight = new Point(brPoints.Max(x => x.X), brPoints.Min(y => y.Y));

            var width = Math.Abs(bottomRight.X - topLeftPoint.X) * UnitSize + RectPadding * 2 * UnitSize;
            var height = Math.Abs(topLeftPoint.Y - bottomRight.Y) * UnitSize + RectPadding * 2 * UnitSize;
            //Rect rectangle = new Rect(0, 0, width, height); // Position (50, 50) and size (200x100)
            //drawingContext.DrawRectangle(null, new Pen(Stroke, StrokeThickness), rectangle);
            var x = 0;
            var y = 0;
            // Top
            DrawLine(drawingContext, new Point(x, y), new Point(x + width, y), 20);
            // Left
            DrawLine(drawingContext, new Point(x, y), new Point(x, y + height), 20);
            // Right
            DrawLine(drawingContext, new Point(x + width, y), new Point(x + width, y + height), 20);
            // Bottom
            DrawLine(drawingContext, new Point(x, y + height), new Point(x + width, y + height), 20);
            //drawingContext.DrawLine(new Pen(Stroke, StrokeThickness),);
            //drawingContext.DrawLine(new Pen(Stroke, StrokeThickness), );
            //drawingContext.DrawLine(new Pen(Stroke, StrokeThickness), new Point(x + width, y), new Point(x + width, y + height));
            //drawingContext.DrawLine(new Pen(Stroke, StrokeThickness), );
            base.OnRender(drawingContext);
        }

        public void AddChild(CustomElement child)
        {
            if (!Canvas.Children.Contains(child))
            {
                Canvas.Children.Add(child);
            }
            Children.Add(child);
        }

        public void RemoveChild(CustomElement child)
        {
            if (Children.Contains(child))
            {
                Children.Remove(child);
            }
        }

        public override void MoveElement(Vector offset)
        {
            base.MoveElement(offset);
            foreach(var child in Children) {
                child.MoveElement(offset);
            }
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
            foreach(var child in Children)
            {
                child.RotateElement(AnchorPoint.Position, degrees);
            }
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
            foreach (var child in Children)
            {
                child.ScaleElement(AnchorPoint.Position, scaleVector, ScaleFactor);
            }
        }
    }
}

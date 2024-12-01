using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.UIELements
{
    public class ElementsGroup : CustomLine
    {
        public static int Counter { get; set; } = 1;
        public string Name { get; set; } = string.Empty;
        public double RectPadding { get; set; } = 0.5;

        public Pen DashedPen { get =>
                new Pen(Stroke, StrokeThickness)
                {
                    DashCap = PenLineCap.Round,
                    DashStyle = new DashStyle(new double[] { 10 }, 10)
                };
        }

        public List<CustomElement> Children { get; set; } = new();

        private Point _topLeftPosition = new Point(0, 0);
        private Point _bottomRightPosition = new Point(0, 0);

        public ElementsGroup(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas)
        {
            StrokeThickness = 3;
            Stroke = Brushes.Gray;
            AnchorVisibility = Visibility.Visible;
            Name = $"Group {Counter}";
            StrokePen = DashedPen;
            AddPoint(new Point(0, 0));
            AddPoint(new Point(0, 1));
            AddPoint(new Point(1, 0));
            AddPoint(new Point(1, 1));
        }

        protected override void OnRender(DrawingContext dc)
        {
            StrokePen = DashedPen;
            Points[0].Position = new Point(_topLeftPosition.X, _topLeftPosition.Y);
            Points[1].Position = new Point(_bottomRightPosition.X, _topLeftPosition.Y);
            Points[2].Position = new Point(_bottomRightPosition.X, _bottomRightPosition.Y);
            Points[3].Position = new Point(_topLeftPosition.X, _bottomRightPosition.Y);
            CalculateRectPoints();
            base.OnRender(dc);
        }


        public override void AddPoint(double x, double y)
        {
            //base.AddPoint(x, y);
        }
        protected override void InitControls()
        {
            base.InitControls();
            AnchorPoint.Stroke = Brushes.BlueViolet;
        }

        protected virtual void CalculateRectPoints()
        {
            List<Point> tlPoints = new List<Point>();
            List<Point> brPoints = new List<Point>();
            foreach (var child in Children)
            {
                tlPoints.Add(child.GetTopLeftPosition());
                brPoints.Add(child.GetBottomRightPosition());
            }
            _topLeftPosition = new Point(tlPoints.Min(x => x.X), tlPoints.Max(y => y.Y));
            _bottomRightPosition = new Point(brPoints.Max(x => x.X), brPoints.Min(y => y.Y));
        }

        protected override void RenderControlPanel()
        {
            //base.RenderControlPanel();
            ClearControlPanel();
            AddStrokeColorControls();
            AddStrokeThicknessControls();
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
            foreach (var child in Children)
            {
                child.MoveElement(offset);
            }
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
            foreach (var child in Children)
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

        

        public override string ToString()
        {
            return Name;
        }
    }
}

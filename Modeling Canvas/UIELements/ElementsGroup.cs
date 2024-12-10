using Modeling_Canvas.UIElements.Abstract;
using Modeling_Canvas.UIElements.Interfaces;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class ElementsGroup : CustomLine
    {
        public static int Counter { get; set; } = 1;

        public string Name { get; set; } = string.Empty;

        public double RectPadding { get; set; } = 0.5;

        public bool AnyItemIsSelected { get => Canvas.SelectedElements.Intersect(Children).Any(); }

        private Point _topLeftPosition = new Point(0, 0);

        private Point _bottomRightPosition = new Point(0, 0);

        public override Visibility ControlsVisibility => AnyItemIsSelected ? Visibility.Visible : Visibility.Hidden;

        public Pen DashedPen
        {
            get =>
                new Pen(Stroke, StrokeThickness)
                {
                    DashCap = PenLineCap.Round,
                    DashStyle = new DashStyle(new double[] { 10 }, 10)
                };
        }

        public List<GroupableElement> Children { get; set; } = new();

        public ElementsGroup(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            StrokeThickness = 2;
            Stroke = Brushes.Gray;
            Name = $"Group {Counter}";
            StrokePen = DashedPen;
            AddPoint(new Point(0, 0));
            AddPoint(new Point(0, 0));
            AddPoint(new Point(0, 0));
            AddPoint(new Point(0, 0));

        }
        protected override void OnRender(DrawingContext dc)
        {
            CalculateRectPoints();
            StrokePen = DashedPen;
            base.OnRender(dc);
            Visibility = ControlsVisibility;
        }

        protected override void InitControlPanel()
        {
            AddAnchorControls();
            AddOffsetControls();
            AddRotateControls();
            AddScaleControls();
            AddStrokeColorControls();
            AddStrokeThicknessControls();
        }

        protected override void InitChildren()
        {
            base.InitChildren();
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

            Points[0].Position = new Point(_topLeftPosition.X, _topLeftPosition.Y);
            Points[1].Position = new Point(_bottomRightPosition.X, _topLeftPosition.Y);
            Points[2].Position = new Point(_bottomRightPosition.X, _bottomRightPosition.Y);
            Points[3].Position = new Point(_topLeftPosition.X, _bottomRightPosition.Y);
        }

        public void AddChild(GroupableElement child)
        {
            if (!Canvas.Children.Contains(child))
            {
                Canvas.Children.Add(child);
            }
            Children.Add(child);
        }

        public void RemoveChild(GroupableElement child)
        {
            if (Children.Contains(child))
            {
                Children.Remove(child);
            }
        }

        protected override void OnElementSelected(MouseButtonEventArgs e)
        {
            RenderControlPanel();
            e.Handled = true;
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

using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class CustomRectangle : CustomElement
    {
        public int PointsRadius { get; set; } = 5;
        public PointShape PointsShape { get; set; } = PointShape.Circle;
        public List<DraggablePoint> Points { get; } = new();
        public double RectHeight
        {
            get
            {
                var topPoint = Points.OrderBy(y => y.Position.Y).First().Position;
                var bottomPoint = Points.OrderByDescending(y => y.Position.Y).First().Position;
                return Math.Abs(topPoint.Y - bottomPoint.Y);
            }
        }
        public double RectWidth
        {
            get
            {
                var rightPoint = Points.OrderBy(y => y.Position.X).First().Position;
                var leftPoint = Points.OrderByDescending(y => y.Position.X).First().Position;
                return Math.Abs(rightPoint.X - leftPoint.X);
            }
        }
        public CustomRectangle(CustomCanvas canvas, Point p1, Point p3) : base(canvas)
        {
            var p2 = new Point(p1.X, p3.Y);
            var p4 = new Point(p3.X, p1.Y);
            InitPoints(p1, p2, p3, p4);
            Stroke = Brushes.Pink;
            StrokeThickness = 3;
        }
        public CustomRectangle(CustomCanvas canvas, Point p1, double width, double height) : base(canvas)
        {
            var p2 = new Point(p1.X, p1.Y + height);
            var p3 = new Point(p1.X + width, p1.Y + height);
            var p4 = new Point(p1.X + width, p1.Y);
            InitPoints(p1, p2, p3, p4);
            Stroke = Brushes.Pink;
            StrokeThickness = 3;
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(RectWidth * UnitSize + StrokeThickness, RectHeight * UnitSize + StrokeThickness);
        }

        private void InitPoints(Point p1, Point p2, Point p3, Point p4)
        {
            foreach (var p in new[] { p1, p2, p3, p4 })
            {
                var point = new DraggablePoint(Canvas, new Point(p.X, p.Y))
                {
                    Shape = PointsShape,
                    Radius = PointsRadius,
                    OverrideMoveAction = CornerPointMoveAction
                };
                Points.Add(point);
                Canvas.Children.Add(point);
                Panel.SetZIndex(point, Canvas.Children.Count + 1);
            }
        }

        protected override Point GetAnchorDefaultPosition()
        {
            return new Point(Points.Select(x => x.Position.X).Sum() / 4, Points.Select(y => y.Position.Y).Sum() / 4);
        }
        public override Point GetTopLeftPosition()
        {
            return new Point(Points.Min(x => x.Position.X), Points.Max(y => y.Position.Y));
        }
        public override Point GetBottomRightPosition()
        {
            return new Point(Points.Max(x => x.Position.X), Points.Min(y => y.Position.Y));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            foreach (var point in Points)
            {
                point.Visibility = ControlsVisibility;
            }
            base.OnRender(drawingContext);

        }

        protected override void DefaultRender(DrawingContext dc)
        {
            for (int i = 0; i < 3; i++)
            {
                dc.DrawLine(Canvas, StrokePen, Points[i].PixelPosition, Points[i + 1].PixelPosition, 10);
            }

            dc.DrawLine(Canvas, StrokePen, Points[0].PixelPosition, Points[3].PixelPosition, 10);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (InputManager.SpacePressed)
            {
                Canvas.SelectedElements.Clear();
                Canvas.SelectedElements.Add(this);
                e.Handled = true;
            }
            base.OnMouseLeftButtonDown(e);
        }
        protected void CornerPointMoveAction(DraggablePoint movedPoint, Vector offset)
        {
            var firstPoint = Points.First(x => x.Position.X == movedPoint.Position.X && !x.Equals(movedPoint));
            var thirdPoint = Points.First(y => y.Position.Y == movedPoint.Position.Y && !y.Equals(movedPoint));
            if (InputManager.CtrlPressed || InputManager.SpacePressed)
            {
                return;
            }

            if (SnappingEnabled)
            {
                movedPoint.Position = movedPoint.Position.OffsetAndSpanPoint(offset);
            }
            else
            {
                movedPoint.Position = movedPoint.Position.OffsetPoint(offset);
            }
            firstPoint.Position = new Point(movedPoint.Position.X, firstPoint.Position.Y);
            thirdPoint.Position = new Point(thirdPoint.Position.X, movedPoint.Position.Y);
        }

        protected override void RenderControlPanelLabel()
        {
        }

        public override void MoveElement(Vector offset)
        {
            foreach (var point in Points)
            {
                if (SnappingEnabled)
                {
                    point.Position = point.Position.OffsetAndSpanPoint(offset);
                }
                else
                {
                    point.Position = point.Position.OffsetPoint(offset);
                }
            }
            base.MoveElement(offset);
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
        }
        public override string ToString()
        {
            return $"Rectange\nWidth: {RectWidth}\nHeight: {RectHeight}";
        }

        //protected override void ProjectiveRender(DrawingContext dc)
        //{
        //}
    }
}

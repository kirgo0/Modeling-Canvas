using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
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
            StrokeThickness = 2;
            Stroke = Brushes.Cyan;
            LabelText = "Line";
        }
        public CustomLine(CustomCanvas canvas, Point firstPoint, Point secondPoint) : base(canvas)
        {
            StrokeThickness = 2;
            Stroke = Brushes.Cyan;
            LabelText = "Line";
            AddPoint(firstPoint);
            AddPoint(secondPoint);
        }

        protected override void OnRender(DrawingContext dc)
        {
            AnchorVisibility = ShowControls;

            if (Points.Count < 2) return;

            foreach (var point in Points)
            {
                point.Visibility = ShowControls;
                point.Shape = PointShape.Circle;
            }

            Points.First().Shape = PointShape.Square;
            Points.Last().Shape = PointShape.Square;
            base.OnRender(dc);
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            for (int i = 0; i < Points.Count - 1; i++)
            {
                var p1 = Points[i].CanvasPixelPosition;
                var p2 = Points[i + 1].CanvasPixelPosition;
                dc.DrawLine(StrokePen, p1, p2, 10);
            }

            if (IsClosed)
            {
                dc.DrawLine(StrokePen, Points.First().CanvasPixelPosition, Points.Last().CanvasPixelPosition, 10);
            }

            base.DefaultRender(dc);
        }

        protected override void AffineRender(DrawingContext dc)
        {
            for (int i = 0; i < Points.Count - 1; i++)
            {
                var p1 = Points[i].CanvasPixelPosition;
                var p2 = Points[i + 1].CanvasPixelPosition;
                dc.DrawAffineLine(StrokePen, p1, p2, Canvas.AffineParams, 10);
            }

            if (IsClosed)
            {
                dc.DrawAffineLine(StrokePen, Points.First().CanvasPixelPosition, Points.Last().CanvasPixelPosition, Canvas.AffineParams, 10);
            }
            base.AffineRender(dc);
        }

        protected override void ProjectiveRender(DrawingContext dc)
        {
            for (int i = 0; i < Points.Count - 1; i++)
            {
                var p1 = Points[i].CanvasPixelPosition.AddCanvasOffsets();
                var p2 = Points[i + 1].CanvasPixelPosition.AddCanvasOffsets();
                dc.DrawProjectiveLine(StrokePen, p1, p2, Canvas.ProjectiveParams, 10);
            }

            if (IsClosed)
            {
                dc.DrawProjectiveLine(StrokePen, 
                    Points.First().CanvasPixelPosition.AddCanvasOffsets(), 
                    Points.Last().CanvasPixelPosition.AddCanvasOffsets(), 
                    Canvas.ProjectiveParams, 10);
            }
            base.ProjectiveRender(dc);
        }

        public override Point GetOriginPoint(Size arrangedSize)
        {
            if(Canvas.RenderMode is RenderMode.Projective)
            {
                return new Point(-Canvas.XOffset, Canvas.YOffset);
            } else
            {
                return new Point(0,0);
            }
        }

        protected override Point GetAnchorDefaultPosition()
        {
            if (Points == null || !Points.Any()) return new Point(0, 0);

            // Calculate the average of X and Y coordinates
            double centerX = Points.Average(p => p.Position.X);
            double centerY = Points.Average(p => p.Position.Y);

            return new Point(centerX, centerY);
        }

        public override Point GetTopLeftPosition()
        {
            return new Point(Points.Min(x => x.Position.X) - PointsRadius / UnitSize, Points.Max(y => y.Position.Y) + PointsRadius / UnitSize);
        }
        public override Point GetBottomRightPosition()
        {
            return new Point(Points.Max(x => x.Position.X) + PointsRadius / UnitSize, Points.Min(y => y.Position.Y) - PointsRadius / UnitSize);
        }

        // helper add method
        public virtual void AddPoint(double x, double y)
        {
            AddPoint(new Point(x, y));
        }

        // main add method
        public void AddPoint(Point point)
        {
            var draggablepoint = new DraggablePoint(Canvas, point)
            {
                Shape = PointsShape,
                Radius = PointsRadius,
                HasAnchorPoint = false,
                IsSelectable = false,
                OnRenderControlPanel = RenderControlPanel
            };
            Points.Add(draggablepoint);
            Canvas.Children.Add(draggablepoint);
            Panel.SetZIndex(draggablepoint, Canvas.Children.Count + 1);
        }

        public override void MoveElement(Vector offset)
        {
            foreach (var point in Points)
            {
                point.MoveElement(offset);
            }
            base.MoveElement(offset);
        }
        public override void RotateElement(Point anchorPoint, double degrees)
        {
            foreach (var point in Points)
            {
                point.Position = point.Position.RotatePoint(anchorPoint, degrees);
            }
        }
        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
            foreach (var point in Points)
            {
                point.Position = point.Position.ScalePoint(anchorPoint, scaleVector);
            }
        }

        protected override void RenderControlPanel()
        {
            base.RenderControlPanel();
            AddAddPointButton();
            AddStrokeColorControls();
            AddStrokeThicknessControls();
            AddIsClosedControls();
        }

        protected virtual void AddAddPointButton()
        {
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

        protected virtual void AddIsClosedControls()
        {
            var panel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };
            var isClosedLabel = new TextBlock { Text = "Is Closed:" };

            var isClosedCheckBox = new CheckBox
            {
                IsChecked = IsClosed // Bind the initial value
            };

            isClosedCheckBox.Checked += (s, e) =>
            {
                IsClosed = true;
                InvalidateVisual(); 
            };
            isClosedCheckBox.Unchecked += (s, e) =>
            {
                IsClosed = false;
                InvalidateVisual();
            };

            panel.Children.Add(isClosedLabel);
            panel.Children.Add(isClosedCheckBox);
            AddElementToControlPanel(panel);
        }


        public override string ToString()
        {
            return $"Line\nPoints: {Points.Count}";
        }

    }
}

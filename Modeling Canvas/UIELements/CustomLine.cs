using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
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
            if (Points.Count < 2) return;

            foreach (var point in Points)
            {
                point.Visibility = ControlsVisibility;
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
                var p1 = Points[i].PixelPosition;
                var p2 = Points[i + 1].PixelPosition;
                dc.DrawLine(StrokePen, p1, p2, 10);
            }

            if (IsClosed)
            {
                dc.DrawLine(StrokePen, Points.First().PixelPosition, Points.Last().PixelPosition, 10);
            }

        }

        protected override void AffineRender(DrawingContext dc)
        {
            for (int i = 0; i < Points.Count - 1; i++)
            {
                var p1 = Points[i].PixelPosition;
                var p2 = Points[i + 1].PixelPosition;
                dc.DrawAffineLine(StrokePen, p1, p2, Canvas.AffineParams, 10);
            }

            if (IsClosed)
            {
                dc.DrawAffineLine(StrokePen, Points.First().PixelPosition, Points.Last().PixelPosition, Canvas.AffineParams, 10);
            }
        }

        protected override void ProjectiveRender(DrawingContext dc)
        {
            for (int i = 0; i < Points.Count - 1; i++)
            {
                var p1 = Points[i].PixelPosition;
                var p2 = Points[i + 1].PixelPosition;
                dc.DrawProjectiveLine(StrokePen, p1, p2, Canvas.ProjectiveParams, 10);
            }

            if (IsClosed)
            {
                dc.DrawProjectiveLine(StrokePen, 
                    Points.First().PixelPosition, 
                    Points.Last().PixelPosition, 
                    Canvas.ProjectiveParams, 10);
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
                OnRenderControlPanel = OnPointClickRenderControlPanel
            };
            Points.Add(draggablepoint);
            Canvas.Children.Add(draggablepoint);
            Panel.SetZIndex(draggablepoint, Canvas.Children.Count + 1);
        }

        public void InsertPointAt(int pointIndex)
        {
            var draggablepoint = new DraggablePoint(Canvas)
            {
                Shape = PointsShape,
                Radius = PointsRadius,
                HasAnchorPoint = false,
                IsSelectable = false,
                OnRenderControlPanel = OnPointClickRenderControlPanel
            };

            if(pointIndex >= Points.Count) pointIndex = Points.Count - 1;

            draggablepoint.Position = GetAvaragePoint(pointIndex);

            Points.Insert(pointIndex, draggablepoint);

            Canvas.Children.Add(draggablepoint);
            Panel.SetZIndex(draggablepoint, Canvas.Children.Count + 1);
        }

        public Point GetAvaragePoint(int index)
        {
            if (index <= 0)
            {
                var nextPoint = Points[0];
                if (Points.Count > 1)
                {
                    var prevPoint = Points[Points.Count - 1];
                    var avgX = (prevPoint.X + nextPoint.X) / 2.0;
                    var avgY = (prevPoint.Y + nextPoint.Y) / 2.0;
                    return new Point(avgX, avgY);
                }

                return new Point(nextPoint.X + 1, nextPoint.Y + 1);
            }
            else if (index >= Points.Count)
            {
                var prevPoint = Points[Points.Count - 1];
                return new Point(prevPoint.X, prevPoint.Y);
            }
            else
            {
                var prevPoint = Points[index - 1];
                var nextPoint = Points[index];
                var avgX = (prevPoint.X + nextPoint.X) / 2.0;
                var avgY = (prevPoint.Y + nextPoint.Y) / 2.0;
                return new Point(avgX, avgY);
            }
        }

        public void RemovePoint(DraggablePoint point)
        {
            if (Points.Contains(point))
            {
                Points.Remove(point);
                Canvas.Children.Remove(point);
            }
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

        protected void OnPointClickRenderControlPanel(DraggablePoint point)
        {
            AddRemovePointControls(point);
            AddAddPointButton(point);
            RenderControlPanelLabel();
            AddRotateControls();
            AddOffsetControls();
            AddAnchorControls();
            AddStrokeColorControls();
            AddStrokeThicknessControls();
            AddIsClosedControls();
        }

        protected virtual void AddAddPointButton(DraggablePoint point = null)
        {
            var panel = GetDefaultVerticalPanel();
            var addPointButton = new Button
            {
                Content = "Add Point",
                Width = 100,
                Height = 20,
                Margin = new Thickness(5),
                IsTabStop = false
            };

            addPointButton.Click += (s, e) =>
            {
                if (point is null)
                {
                   AddPoint(GetAnchorDefaultPosition());
                } else
                {
                    var pointIndex = Points.IndexOf(point);
                    InsertPointAt(pointIndex);
                }
            };
            panel.Children.Add(addPointButton);
            AddElementToControlPanel(panel);
        }
        
        protected virtual void AddRemovePointControls(DraggablePoint point)
        {
            var panel = GetDefaultVerticalPanel();
            var removePointButton = new Button
            {
                Content = "Remove Point",
                Width = 100,
                Height = 20,
                Margin = new Thickness(5),
                IsTabStop = false
            };

            removePointButton.Click += (s, e) =>
            {
                RemovePoint(point);
                RenderControlPanel();
            };
            
            if(Points.Count == 2)
            {
                removePointButton.IsEnabled = false;
            }

            panel.Children.Add(removePointButton);
            AddElementToControlPanel(panel);
        }

        protected virtual void AddIsClosedControls()
        {
            var panel = GetDefaultHorizontalPanel();
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

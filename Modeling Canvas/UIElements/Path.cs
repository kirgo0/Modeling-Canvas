﻿using Modeling_Canvas.Extensions;
using Modeling_Canvas.UIElements.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class Path<T> : CustomElement where T : CustomElement, IPoint
    {
        public int PointsRadius { get; set; } = 5;
        public List<T> Points { get; } = new();
        public bool IsClosed { get; set; } = true;

        public Path(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            LabelText = "Path";
            StrokeThickness = 2;
            Stroke = Brushes.Cyan;
        }

        public Path(CustomCanvas canvas, Point firstPoint, Point secondPoint, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            LabelText = "Path";
            StrokeThickness = 2;
            Stroke = Brushes.Cyan;
            AddPoint(firstPoint);
            AddPoint(secondPoint);
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (Points.Count < 2) return;
            base.OnRender(dc);
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            for (int i = 0; i < Points.Count - 1; i++)
            {
                var p1 = Points[i].PixelPosition;
                var p2 = Points[i + 1].PixelPosition;
                dc.DrawLine(Canvas, StrokePen, p1, p2, 10);
            }

            if (IsClosed)
            {
                dc.DrawLine(Canvas, StrokePen, Points.First().PixelPosition, Points.Last().PixelPosition, 10);
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

        protected virtual T OnPointInit(Point point)
        {
            T customPoint = (T)Activator.CreateInstance(typeof(T), Canvas);

            if (customPoint is null)
                throw new InvalidOperationException($"Failed to create an instance of type {typeof(T)}.");

            customPoint.Position = point;
            customPoint.HasAnchorPoint = false;

            return customPoint;
        }

        public void AddPoint(Point point)
        {
            var customPoint = OnPointInit(point);

            Points.Add(customPoint);
            Canvas.Children.Add(customPoint);
            Panel.SetZIndex(customPoint, Canvas.Children.Count + 1);
        }

        public void InsertPointAt(int pointIndex)
        {
            if (pointIndex >= Points.Count) pointIndex = Points.Count - 1;

            var point = GetAvaragePoint(pointIndex);

            var customPoint = OnPointInit(point);

            Points.Insert(pointIndex, customPoint);
            Canvas.Children.Add(customPoint);
            Panel.SetZIndex(customPoint, Canvas.Children.Count + 1);
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

        public void RemovePoint(T point)
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
        }

        public override string ToString()
        {
            return $"Path\nPoints: {Points.Count}";
        }

    }
}

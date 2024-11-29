﻿using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.UIELements
{
    public class ElementsGroup : CustomElement
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

        public ElementsGroup(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            StrokeThickness = 1;
            AnchorVisibility = Visibility.Visible;
            Name = $"Group {Counter}";
            Counter++;
        }

        protected override Point GetAnchorDefaultPosition()
        {
            return new Point((_topLeftPosition.X + _bottomRightPosition.X) / 2, (_topLeftPosition.Y + _bottomRightPosition.Y) / 2);
        }

        public override Point GetOriginPoint(Size arrangedSize)
        {
            List<Point> tlPoints = new List<Point>();
            foreach (var child in Children)
            {
                tlPoints.Add(child.GetTopLeftPosition());
            }
            var topLeftPoint = new Point(tlPoints.Min(x => x.X), tlPoints.Max(y => y.Y));
            return new Point(arrangedSize.Width / 2 + (topLeftPoint.X * UnitSize - RectPadding * UnitSize), arrangedSize.Height / 2 - (topLeftPoint.Y * UnitSize + RectPadding * UnitSize));
        }

        protected override void OnRender(DrawingContext dc)
        {
            CalculateRectPoints();
            base.OnRender(dc);
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            var width = Math.Abs(_bottomRightPosition.X - _topLeftPosition.X) * UnitSize + RectPadding * 2 * UnitSize;
            var height = Math.Abs(_topLeftPosition.Y - _bottomRightPosition.Y) * UnitSize + RectPadding * 2 * UnitSize;
            var x = 0;
            var y = 0;

            // Top
            dc.DrawDashedLine(DashedPen, new Point(x, y), new Point(x + width, y), 20);
            // Left
            dc.DrawDashedLine(DashedPen, new Point(x, y), new Point(x, y + height), 20);
            // Right
            dc.DrawDashedLine(DashedPen, new Point(x + width, y), new Point(x + width, y + height), 20);
            // Bottom
            dc.DrawDashedLine(DashedPen, new Point(x, y + height), new Point(x + width, y + height), 20);
            base.DefaultRender(dc);
        }

        protected override void AffineRender(DrawingContext dc)
        {
            var width = Math.Abs(_bottomRightPosition.X - _topLeftPosition.X) * UnitSize + RectPadding * 2 * UnitSize;
            var height = Math.Abs(_topLeftPosition.Y - _bottomRightPosition.Y) * UnitSize + RectPadding * 2 * UnitSize;
            var x = 0;
            var y = 0;

            // Top
            dc.DrawAffineDashedLine(DashedPen, new Point(x, y), new Point(x + width, y), Canvas.AffineParams, 20);
            // Left
            dc.DrawAffineDashedLine(DashedPen, new Point(x, y), new Point(x, y + height), Canvas.AffineParams, 20);
            // Right
            dc.DrawAffineDashedLine(DashedPen, new Point(x + width, y), new Point(x + width, y + height), Canvas.AffineParams, 20);
            // Bottom
            dc.DrawAffineDashedLine(DashedPen, new Point(x, y + height), new Point(x + width, y + height), Canvas.AffineParams, 20);
            base.AffineRender(dc);
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
            base.RenderControlPanel();
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

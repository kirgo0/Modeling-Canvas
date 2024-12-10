using Modeling_Canvas.Extensions;
using Modeling_Canvas.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class BezierPoint : DraggablePoint
    {
        public bool ShowPrevControl { get; set; } = true;

        public bool ShowNextControl { get; set; } = true;

        public Pen ControlPrevLinePen { get; set; } = new Pen(Brushes.Magenta, 1);

        public Pen ControlNextLinePen { get; set; } = new Pen(Brushes.Green, 1);

        public DraggablePoint ControlPrevPoint { get; set; }

        public DraggablePoint ControlNextPoint { get; set; }

        public BezierPoint(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            Fill = Brushes.White;
            StrokeThickness = 2;
            Stroke = Brushes.Gray;
            Radius = 5;
            IsSelectable = true;
            LabelText = "Point";
        }

        public BezierPoint(BezierPoint other) : base(other.Canvas, other.HasAnchorPoint)
        {
            Position = other.Position;
            ControlNextPoint = new DraggablePoint(other.ControlNextPoint);
            ControlPrevPoint = new DraggablePoint(other.ControlPrevPoint);
        }


        protected override void InitChildren()
        {
            ControlPrevPoint = new DraggablePoint(Canvas, false)
            {
                Radius = 8,
                Opacity = 0.5,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = Brushes.DarkMagenta,
                IsSelectable = false
            };
            AddChildren(ControlPrevPoint, 9999);
            ControlPrevPoint.Position = new Point(Position.X - 0.5, Position.Y + 1);

            ControlNextPoint = new DraggablePoint(Canvas, false)
            {
                Radius = 8,
                Opacity = 0.5,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = Brushes.DarkGreen,
                IsSelectable = false
            };
            AddChildren(ControlNextPoint, 9999);
            ControlNextPoint.Position = new Point(Position.X + 0.5, Position.Y + 1);

            base.InitChildren();

        }

        protected override void DefaultRender(DrawingContext dc)
        {
            ControlPrevPoint.Visibility = ShowPrevControl ? ControlsVisibility : Visibility.Hidden;
            ControlNextPoint.Visibility = ShowNextControl ? ControlsVisibility : Visibility.Hidden;
            if (ControlsVisibility is Visibility.Visible)
            {
                if (ShowPrevControl)
                    dc.DrawLine(Canvas, ControlPrevLinePen, PixelPosition, ControlPrevPoint.PixelPosition);
                if (ShowNextControl)
                    dc.DrawLine(Canvas, ControlNextLinePen, PixelPosition, ControlNextPoint.PixelPosition);
            }
            base.DefaultRender(dc);
        }

        public void LoadFramePosition(BezierPointFrameModel frame)
        {
            Position = frame.Position;
            ControlPrevPoint.Position = frame.ControlPrevPosition;
            ControlNextPoint.Position = frame.ControlNextPosition;
        }

        public BezierPointFrameModel GetFramePosition()
        {
            return new BezierPointFrameModel()
            {
                Position = Position,
                ControlPrevPosition = ControlPrevPoint.Position,
                ControlNextPosition = ControlNextPoint.Position
            };
        }

        public override void MoveElement(Vector offset)
        {
            ControlPrevPoint.MoveElement(offset);
            ControlNextPoint.MoveElement(offset);
            base.MoveElement(offset);
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
            base.RotateElement(anchorPoint, degrees);
            ControlPrevPoint.RotateElement(anchorPoint, degrees);
            ControlNextPoint.RotateElement(anchorPoint, degrees);
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
            base.ScaleElement(anchorPoint, scaleVector, ScaleFactor);
            ControlPrevPoint.ScaleElement(anchorPoint, scaleVector, ScaleFactor);
            ControlNextPoint.ScaleElement(anchorPoint, scaleVector, ScaleFactor);
        }

    }
}

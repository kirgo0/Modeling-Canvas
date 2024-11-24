using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace Modeling_Canvas.UIELements
{
    public class DraggablePoint : CustomPoint
    {
        public double DragRadius { get; set; } = 10;
        public DraggablePoint(CustomCanvas canvas) : base(canvas)
        {
        }
        public DraggablePoint(CustomCanvas canvas, Point position) : base(canvas)
        {
            Position = position;
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            // Draw point
            base.OnRender(drawingContext);

            // Create an invisible drag zone
            var transparentFill = Brushes.Transparent;

            drawingContext.DrawGeometry(transparentFill, new Pen(Stroke, 0),
                CreateCircleSegmentGeometry(new Point(0, 0), DragRadius, 100));
        }

        public Action<Vector> OverrideMoveAction;

        protected override void MoveElement(Vector offset)
        {
            if (OverrideMoveAction is not null)
            {
                OverrideMoveAction?.Invoke(offset);
                return;
            }
            if (Canvas.IsCtrlPressed || Canvas.IsSpacePressed)
            {
                return;
            }

            if (SnappingEnabled)
            {
                Position = new Point(
                    SnapValue(Position.X + offset.X / UnitSize),
                    SnapValue(Position.Y - offset.Y / UnitSize)
                    );
            } else
            {
                Position = new Point(
                    Position.X + offset.X / UnitSize,
                    Position.Y - offset.Y / UnitSize
                    );
            }

            // Update the circle's center position
            InvalidateCanvas();
        }

        public Point PixelPosition { get => new Point(Position.X * UnitSize, -Position.Y * UnitSize); }

        public Action<MouseButtonEventArgs> MouseLeftButtonDownAction { get; set; }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            MouseLeftButtonDownAction?.Invoke(e);
            base.OnMouseLeftButtonDown(e);
        }


    }

}
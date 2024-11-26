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
            Focusable = false;
        }
        public DraggablePoint(CustomCanvas canvas, Point position) : base(canvas)
        {
            Position = position;
            Focusable = false;
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            // Draw point
            base.OnRender(drawingContext);

            // Create an invisible drag zone
            var transparentFill = Brushes.Transparent;

            drawingContext.DrawGeometry(transparentFill, new Pen(Stroke, 0),
                CreateCircleGeometry(new Point(0, 0), DragRadius, 100));
        }

        public Action<DraggablePoint, Vector> OverrideMoveAction;
        public Action<DraggablePoint, Vector> MoveAction;

        public override void MoveElement(Vector offset)
        {
            if (OverrideMoveAction is not null)
            {
                OverrideMoveAction?.Invoke(this, offset);
                return;
            }

            base.MoveElement(offset);
            MoveAction?.Invoke(this, offset);

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
        }

        public Point PixelPosition { get => new Point(Position.X * UnitSize, -Position.Y * UnitSize); }

        public Action<MouseButtonEventArgs> MouseLeftButtonDownAction { get; set; }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            MouseLeftButtonDownAction?.Invoke(e);
            Keyboard.Focus(Canvas);
            e.Handled = true;
            base.OnMouseLeftButtonDown(e);
        }

        public Func<DraggablePoint, string>? OverrideToStringAction;
        public override string ToString()
        {
            if (OverrideToStringAction is not null) return OverrideToStringAction.Invoke(this);
            return base.ToString();
        }
    }

}
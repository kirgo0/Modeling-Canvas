using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace Modeling_Canvas.UIELements
{
    public class DraggablePoint : CustomPoint
    {
        public DraggablePoint(CustomCanvas canvas) : base(canvas)
        {
        }

        protected override void MoveElement(Vector offset)
        {
            base.MoveElement(offset);
            if (Canvas.IsCtrlPressed || Canvas.IsSpacePressed)
            {
                return;
            }
            Position = new Point(
                Position.X + offset.X / UnitSize,
                Position.Y - offset.Y / UnitSize
                );

            // Update the circle's center position
            InvalidateCanvas();
        }

        public Action<MouseButtonEventArgs> MouseLeftButtonDownAction { get; set; }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            MouseLeftButtonDownAction.Invoke(e);
            base.OnMouseLeftButtonDown(e);
        }


    }

}
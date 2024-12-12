using System.Windows;

namespace Modeling_Canvas.Models
{
    public struct BezierPointFrameModel
    {
        public Point Position { get; set; }
        public Point ControlPrevPosition { get; set; }
        public Point ControlNextPosition { get; set; }

        public BezierPointFrameModel(Point position, Point controlPrevPosition, Point controlNextPosition)
        {
            Position = position;
            ControlPrevPosition = controlPrevPosition;
            ControlNextPosition = controlNextPosition;
        }
    }
}

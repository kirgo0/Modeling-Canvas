using System.Windows;

namespace Modeling_Canvas.UIElements.Interfaces
{
    public interface IGroupable
    {
        public Point GetTopLeftPosition();
        public Point GetBottomRightPosition();
    }
}

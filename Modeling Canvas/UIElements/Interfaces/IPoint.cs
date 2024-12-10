using System.Windows;
using System.Windows.Input;

namespace Modeling_Canvas.UIElements.Interfaces
{
    public interface IPoint
    {
        public Point Position { get; set; }
        public double Radius { get; set; }
        public double X { get; }
        public double Y { get; }
        public Point PixelPosition { get; }
        public Action<MouseButtonEventArgs> MouseLeftButtonDownAction { get; set; }
    }
}

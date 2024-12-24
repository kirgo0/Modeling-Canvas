using System.Windows;
using System.Windows.Input;

namespace Modeling_Canvas.UIElements.Interfaces
{
    public interface IPoint
    {
        public Point Position { get; set; }
        public double PixelRadius { get; set; }
        public double X { get; }
        public double Y { get; }
        public Action<MouseButtonEventArgs> MouseLeftButtonDownAction { get; set; }
    }
}

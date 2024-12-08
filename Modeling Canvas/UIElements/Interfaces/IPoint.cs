using System.Windows;

namespace Modeling_Canvas.UIElements.Interfaces
{
    public interface IPoint
    {
        public Point Position { get; set; }
        public double Radius { get; set; }
        public double X { get; }
        public double Y { get; }
        public Point PixelPosition { get; }
    }
}

using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.UIELements
{
    public class ElementsGroup : CustomElement
    {
        public List<CustomElement> Childrens { get; set; } = new();

        protected CustomRectangle _boundsRect;

        public ElementsGroup(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            _boundsRect = new CustomRectangle(canvas, new Point(0, 0), new Point(0, 0))
            {
                StrokeThickness = 1,
                Stroke = Brushes.Gray
            };
        }

        public void AddChild(CustomElement child)
        {
            if (!Canvas.Children.Contains(child))
            {
                Canvas.Children.Add(child);
            }
            Childrens.Add(child);
        }

        public void RemoveChild(CustomElement child)
        {
            if (Childrens.Contains(child))
            {
                Childrens.Remove(child);
            }
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
            throw new NotImplementedException();
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
            throw new NotImplementedException();
        }
    }
}

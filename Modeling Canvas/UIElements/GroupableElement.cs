
using Modeling_Canvas.UIElements.Interfaces;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public abstract class GroupableElement : Element, IGroupable
    {
        public GroupableElement(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
        }

        public virtual Point GetTopLeftPosition() => new Point(0, 0);

        public virtual Point GetBottomRightPosition() => new Point(0, 0);

        protected override void DefaultRender(DrawingContext dc)
        {
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
        }
    }
}

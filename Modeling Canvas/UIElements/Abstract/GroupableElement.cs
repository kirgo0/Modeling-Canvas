using Modeling_Canvas.UIElements.Interfaces;
using System.Windows;

namespace Modeling_Canvas.UIElements.Abstract
{
    public abstract class GroupableElement : Element, IGroupable
    {
        public GroupableElement(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
        }

        public virtual Point GetTopLeftPosition() => new Point(0, 0);

        public virtual Point GetBottomRightPosition() => new Point(0, 0);

        public override void RotateElement(Point anchorPoint, double degrees)
        {
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
        }
    }
}

using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public partial class DraggablePoint : CustomPoint
    {
        public Func<DraggablePoint, string>? OverrideToStringAction;

        public Action<DraggablePoint, Vector> OverrideMoveAction;

        public DraggablePoint(CustomCanvas canvas, bool hasAnchorPoint = false) : base(canvas, hasAnchorPoint)
        {
            LabelText = "D.Point";
        }

        public DraggablePoint(DraggablePoint other) : base(other.Canvas, other.HasAnchorPoint)
        {
            LabelText = "D.Point";
            Position = other.Position;
        }

        protected override void InitControlPanel()
        {
            base.InitControlPanel();
            AddPointControls();
        }

        public override void MoveElement(Vector offset)
        {
            if (OverrideMoveAction is not null)
            {
                OverrideMoveAction?.Invoke(this, offset);
                return;
            }

            base.MoveElement(offset);

            if (InputManager.CtrlPressed || InputManager.SpacePressed)
            {
                return;
            }

            Position = SnappingEnabled ? Position.OffsetAndSpanPoint(offset) : Position.OffsetPoint(offset);
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
            Position = Position.RotatePoint(anchorPoint, degrees);
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
            Position = Position.ScalePoint(anchorPoint, scaleVector);
        }

        public override string ToString()
        {
            if (OverrideToStringAction is not null) return OverrideToStringAction.Invoke(this);
            return base.ToString();
        }
    }

}
using Modeling_Canvas.Extensions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public partial class DraggablePoint : CustomPoint
    {
        public double DragRadius { get; set; } = 10;

        public DraggablePoint(CustomCanvas canvas, bool hasAnchorPoint = false) : base(canvas, hasAnchorPoint)
        {
            LabelText = "D.Point";
        }
        
        public DraggablePoint(DraggablePoint other) : base(other.Canvas, other.HasAnchorPoint)
        {
            LabelText = "D.Point";
            Position = other.Position;
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            base.DefaultRender(dc);
            dc.DrawCircle(Canvas, Brushes.Transparent, new Pen(Stroke, 0), PixelPosition, DragRadius, 100);
        }

        protected override void InitControlPanel()
        {
            base.InitControlPanel();
            AddPointControls();
        }

        public Action<DraggablePoint, Vector> OverrideMoveAction;
        public Action<DraggablePoint, Vector> MoveAction;

        public override void MoveElement(Vector offset)
        {
            if (OverrideMoveAction is not null)
            {
                OverrideMoveAction?.Invoke(this, offset);
                return;
            }

            base.MoveElement(offset);
            MoveAction?.Invoke(this, offset);

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

        public Action<MouseButtonEventArgs> MouseLeftButtonDownAction { get; set; }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            MouseLeftButtonDownAction?.Invoke(e);
            e.Handled = true;
            base.OnMouseLeftButtonDown(e);
        }

        public Action<DraggablePoint>? OnRenderControlPanel { get; set; }

        public Func<DraggablePoint, string>? OverrideToStringAction;
        public override string ToString()
        {
            if (OverrideToStringAction is not null) return OverrideToStringAction.Invoke(this);
            return base.ToString();
        }
    }

}
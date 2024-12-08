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

        public Action<MouseButtonEventArgs> MouseLeftButtonDownAction { get; set; }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            MouseLeftButtonDownAction?.Invoke(e);
            e.Handled = true;
            base.OnMouseLeftButtonDown(e);
        }

        public Action<DraggablePoint>? OnRenderControlPanel { get; set; }
        public bool OverrideRenderControlPanelAction { get; set; } = false;

        //protected override void RenderControlPanel()
        //{
        //    if (!OverrideRenderControlPanelAction)
        //    {
        //        ClearControlPanel();
        //        RenderControlPanelLabel();
        //        AddPointControls();
        //        OnRenderControlPanel?.Invoke(this);
        //    }
        //    else
        //    {
        //        OnRenderControlPanel?.Invoke(this);
        //    }

        public Func<DraggablePoint, string>? OverrideToStringAction;
        public override string ToString()
        {
            if (OverrideToStringAction is not null) return OverrideToStringAction.Invoke(this);
            return base.ToString();
        }
    }

}
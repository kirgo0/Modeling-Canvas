using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class BezierPoint : DraggablePoint
    {
        public DraggablePoint ControlPoint { get; set; }
        public Pen ControlLinePen { get; set; } = new Pen(Brushes.Magenta, 1);

        public BezierPoint(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            Fill = Brushes.White;
            StrokeThickness = 2;
            Stroke = Brushes.Gray;
            Radius = 5;
            IsSelectable = true;
            LabelText = "Point";
        }

        protected override void InitChildren()
        {
            ControlPoint = new DraggablePoint(Canvas, false)
            {
                Radius = 8,
                Opacity = 0.7,
                OverrideRenderControlPanelAction = true,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = Brushes.DarkMagenta
            };
            Canvas.Children.Add(ControlPoint);
            Panel.SetZIndex(ControlPoint, Canvas.Children.Count + 1);

            base.InitChildren();

        }

        public override void MoveElement(Vector offset)
        {
            ControlPoint.MoveElement(offset);
            base.MoveElement(offset);
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            ControlPoint.Visibility = ControlsVisibility;
            if(ControlsVisibility is Visibility.Visible)
            {
                dc.DrawLine(Canvas, ControlLinePen, PixelPosition, ControlPoint.PixelPosition);
            }
            base.DefaultRender(dc);
        }
    }
}

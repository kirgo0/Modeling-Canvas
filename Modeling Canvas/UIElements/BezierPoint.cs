using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class BezierPoint : DraggablePoint
    {
        public bool ShowPrevControl { get; set; } = true;
        public bool ShowNextControl { get; set; } = true;
        public Pen ControlPrevLinePen { get; set; } = new Pen(Brushes.Magenta, 1);
        public Pen ControlNextLinePen { get; set; } = new Pen(Brushes.Green, 1);
        public DraggablePoint ControlPreviousPoint { get; set; }
        public DraggablePoint ControlNextPoint { get; set; }

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
            ControlPreviousPoint = new DraggablePoint(Canvas, false)
            {
                Radius = 8,
                OverrideRenderControlPanelAction = true,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = Brushes.DarkMagenta
            };
            AddChildren(ControlPreviousPoint);

            ControlNextPoint = new DraggablePoint(Canvas, false)
            {
                Radius = 8,
                OverrideRenderControlPanelAction = true,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = Brushes.DarkGreen
            };
            AddChildren(ControlNextPoint);

            base.InitChildren();

        }

        public override void MoveElement(Vector offset)
        {
            ControlPreviousPoint.MoveElement(offset);
            ControlNextPoint.MoveElement(offset);
            base.MoveElement(offset);
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            ControlPreviousPoint.Visibility = ShowPrevControl ? ControlsVisibility : Visibility.Hidden;
            ControlNextPoint.Visibility = ShowNextControl ? ControlsVisibility : Visibility.Hidden;
            if(ControlsVisibility is Visibility.Visible)
            {
                if(ShowPrevControl)
                    dc.DrawLine(Canvas, ControlPrevLinePen, PixelPosition, ControlPreviousPoint.PixelPosition);
                if(ShowNextControl)
                    dc.DrawLine(Canvas, ControlNextLinePen, PixelPosition, ControlNextPoint.PixelPosition);
            }
            base.DefaultRender(dc);
        }
    }
}

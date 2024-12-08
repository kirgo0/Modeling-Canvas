using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public partial class CustomLine : Path<DraggablePoint>
    {
        public PointShape PointsShape { get; set; } = PointShape.Circle;

        public CustomLine(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            LabelText = "Line";
        }

        public CustomLine(CustomCanvas canvas, Point firstPoint, Point secondPoint, bool hasAnchorPoint = true) : base(canvas, firstPoint, secondPoint, hasAnchorPoint)
        {
            LabelText = "Line";
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            foreach (var point in Points)
            {
                point.Visibility = ControlsVisibility;
                point.Shape = PointShape.Circle;
            }

            Points.First().Shape = PointShape.Square;
            Points.Last().Shape = PointShape.Square;
        }

        // main add method

        protected override DraggablePoint OnPointInit(Point point)
        {
            var customPoint = base.OnPointInit(point);

            customPoint.Shape = PointsShape;
            customPoint.Radius = PointsRadius;
            customPoint.IsSelectable = false;
            //customPoint.OnRenderControlPanel = OnPointClickRenderControlPanel;

            return customPoint;
        }
       
        //protected void OnPointClickRenderControlPanel(DraggablePoint point)
        //{
        //    SelectedPoint = point;
        //    //RenderControlPanelLabel();
        //    //AddRotateControls();
        //    //AddOffsetControls();
        //    //AddAnchorControls();
        //    //AddStrokeColorControls();
        //    //AddStrokeThicknessControls();
        //}

        public override string ToString()
        {
            return $"Line\nPoints: {Points.Count}";
        }

    }
}

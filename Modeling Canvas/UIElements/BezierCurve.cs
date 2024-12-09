using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class BezierCurve : Path<BezierPoint>
    {
        public PointShape PointsShape { get; set; } = PointShape.Circle;
        public BezierCurve(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            PropertyChanged += (o, e) =>
            {
                if (e.PropertyName.Equals(nameof(IsClosed)))
                {
                    var p1 = Points.First();
                    var p2 = Points.Last();
                    if(p1 != null && p2 != null)
                    {
                        p1.ShowPrevControl = IsClosed;
                        p2.ShowNextControl = IsClosed;
                    }
                }
            };
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            foreach (var point in Points)
            {
                point.Visibility = ControlsVisibility;
                point.ControlsVisible = ControlsVisibility == Visibility.Visible; 
                point.Shape = PointShape.Circle;
            }

            Points.First().Shape = PointShape.Square;
            Points.Last().Shape = PointShape.Square;
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            if (Points.Count < 2) return;

            for (int i = 0; i < Points.Count - 1; i++)
            {
                BezierPoint start = Points[i];
                BezierPoint end = Points[i + 1];

                Point p0 = start.PixelPosition;
                Point c1 = start.ControlNextPoint.PixelPosition;
                Point c2 = end.ControlPreviousPoint.PixelPosition; 
                Point p3 = end.PixelPosition;

                // Draw the segment
                dc.DrawBezierCurve(Canvas, StrokePen, p0, c1, c2, p3, 10);
            }

            // Handle closed curves
            if (IsClosed)
            {
                BezierPoint start = Points.Last();
                BezierPoint end = Points.First();

                Point p0 = start.PixelPosition;
                Point c1 = start.ControlNextPoint.PixelPosition;
                Point c2 = end.ControlPreviousPoint.PixelPosition;
                Point p3 = end.PixelPosition;

                // Draw the closing segment
                dc.DrawBezierCurve(Canvas, StrokePen, p0, c1, c2, p3, 10);
            }
        }


        protected override BezierPoint OnPointInit(Point point)
        {
            var customPoint = base.OnPointInit(point);

            customPoint.Shape = PointsShape;
            customPoint.Radius = PointsRadius;
            customPoint.OverrideRenderControlPanelAction = true;
            //customPoint.OnRenderControlPanel = OnPointClickRenderControlPanel;
            return customPoint;
        }

        //protected void OnPointClickRenderControlPanel(DraggablePoint point)
        //{
        //    SelectedPoint = point;
        //    RenderControlPanelLabel();
        //}

        public override void RotateElement(Point anchorPoint, double degrees)
        {
            base.RotateElement(anchorPoint, degrees);
            foreach (var point in Points)
            {
                point.ControlPreviousPoint.Position = point.ControlPreviousPoint.Position.RotatePoint(anchorPoint, degrees);
            }
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
            base.ScaleElement(anchorPoint, scaleVector, ScaleFactor);
            foreach (var point in Points)
            {
                point.ControlPreviousPoint.Position = point.ControlPreviousPoint.Position.ScalePoint(anchorPoint, scaleVector);
            }
        }

        public override string ToString()
        {
            return $"Line\nPoints: {Points.Count}";
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.UIELements
{
    public class CustomLine : CustomElement
    {
        public List<DraggablePoint> Points { get; set; } = new();
        public bool IsFirstRender { get; set; } = true;
        public CustomLine(CustomCanvas canvas) : base(canvas)
        {
            StrokeThickness = 3;
            Stroke = Brushes.Cyan;
        }
        
        public CustomLine(CustomCanvas canvas, Point firstPoint, Point secondPoint) : base(canvas)
        {
            StrokeThickness = 2;
            Stroke = Brushes.Cyan;
            Points.Add(new DraggablePoint(Canvas, firstPoint));
            Points.Add(new DraggablePoint(Canvas, secondPoint));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if(IsFirstRender)
            {
                foreach(var point in Points)
                {
                    point.Radius = 5;
                    Canvas.Children.Add(point);
                }
                IsFirstRender = false;
            }
            base.OnRender(drawingContext);

            for (int i = 0; i < Points.Count-1; i++) {
                drawingContext.DrawLine(new Pen(Stroke, StrokeThickness), Points[i].PixelPosition, Points[i + 1].PixelPosition);
            }
        }
        public void AddPoint(double x, double y)
        {
            Points.Add(new DraggablePoint(Canvas, new Point(x, y)));
        }

        public void AddPoint(Point point)
        {
            Points.Add(new DraggablePoint(Canvas, point));
        }

        protected override void MoveElement(Vector offset)
        {
            //throw new NotImplementedException();
        }
    }
}

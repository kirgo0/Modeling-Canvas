using System.Windows;

namespace Modeling_Canvas.UIElements
{
    public class CoordinateGrid : Element
    {
        public CoordinateGrid(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
            throw new NotImplementedException();
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
            throw new NotImplementedException();
        }

        protected override Point[][] GetElementGeometry()
        {
            int width = (int)(Canvas.ProjectiveParams.Xx / UnitSize);
            int height = (int)(Canvas.ProjectiveParams.Yy / UnitSize);
            var geometryData = new Point[width * 2 + height * 2 + 1][];

            var rowCounter = 0;
            for (int x = -width; x <= width; x++)
            {
                var line = new Point[2];
                Point start = new Point(x, -height);
                Point end = new Point(x, height);
                line[0] = start;
                line[1] = end;
                geometryData[rowCounter] = line;
                rowCounter++;
            }
            // Draw horizontal grid lines and numbers
            for (int y = -height; y <= height; y++)
            {
                var line = new Point[2];
                Point start = new Point(-width, y);
                Point end = new Point(width, y);
                if (y == 0) continue;
                line[0] = start;
                line[1] = end;
                geometryData[rowCounter] = line;
                rowCounter++;
            }

            return geometryData;
            // Draw vertical grid lines and numbers

            // Draw the X and Y axes
            //Pen axisPen = new Pen(Brushes.Black, 2);

            //// X-axis
            //Point xAxisStart = TransformPoint(new Point(-width * UnitSize, 0));
            //Point xAxisEnd = TransformPoint(new Point(width * UnitSize, 0));
            //drawingContext.DrawLine(axisPen, xAxisStart, xAxisEnd);

            //// Y-axis
            //Point yAxisStart = TransformPoint(new Point(0, -height * UnitSize));
            //Point yAxisEnd = TransformPoint(new Point(0, height * UnitSize));
            //drawingContext.DrawLine(axisPen, yAxisStart, yAxisEnd);
        }
    }
}

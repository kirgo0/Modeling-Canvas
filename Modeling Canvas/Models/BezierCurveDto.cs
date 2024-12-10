using System.Windows;

namespace Modeling_Canvas.Models
{
    public class BezierCurveDto
    {
        public List<Point> Points { get; set; }
        public List<Point> ControlPrevPoints { get; set; }
        public List<Point> ControlNextPoints { get; set; }
        public string Fill { get; set; }
        public string Stroke { get; set; }
        public double StrokeThickness { get; set; }
        public bool HasAnchorPoint { get; set; }
        public Point? AnchorPointPosition { get; set; }
        public Dictionary<double, List<BezierPointFrameModel>> AnimationFrames { get; set; }
    }


}

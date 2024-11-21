using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.UIELements
{
    public class CustomElement : FrameworkElement
    {
        public Brush Fill { get; set; } = Brushes.Transparent; // Default fill color
        public Brush Stroke { get; set; } = Brushes.Black; // Default stroke color
        public double StrokeThickness { get; set; } = 2; // Default stroke thickness
        public CustomCanvas Canvas { get; set; } = new CustomCanvas() { UnitSize = 1};
        public double UnitSize { get => Canvas.UnitSize; }
        public virtual Point GetOriginPoint(Size arrangedSize)
        {
            return new Point(arrangedSize.Width/2-StrokeThickness, arrangedSize.Height/2-StrokeThickness);  
        }

    }
}

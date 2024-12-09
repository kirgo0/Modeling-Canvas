using Modeling_Canvas.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modeling_Canvas.Models
{
    public class CanvasSelectionChangedEventArgs : EventArgs
    {
        public Element SelectedElement { get; set; }

        public CanvasSelectionChangedEventArgs(Element selectedElement)
        {
            SelectedElement = selectedElement;
        }
    }
}

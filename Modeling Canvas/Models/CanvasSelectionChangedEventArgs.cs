using Modeling_Canvas.UIElements;

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

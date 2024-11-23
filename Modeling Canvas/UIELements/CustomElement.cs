using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIELements
{
    public abstract class CustomElement : FrameworkElement
    {
        public Brush Fill { get; set; } = Brushes.Transparent; // Default fill color
        public Brush Stroke { get; set; } = Brushes.Black; // Default stroke color
        public double StrokeThickness { get; set; } = 1; // Default stroke thickness
        public CustomCanvas Canvas { get; set; }
        public double Opacity { get; set; } = 1;
        public double UnitSize { get => Canvas.UnitSize; }

        protected bool _isDragging = false;

        protected Point _lastMousePosition;

        protected CustomElement(CustomCanvas canvas)
        {
            Canvas = canvas;
        }

        public virtual Point GetOriginPoint(Size arrangedSize)
        {
            return new Point(
                arrangedSize.Width/2, 
                arrangedSize.Height/2
                );
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            var window = App.Current.MainWindow as MainWindow;
            if (window != null)
            {
                window.CurrentElementLabel.Content = ToString();
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            var window = App.Current.MainWindow as MainWindow;
            if (window != null)
            {
                window.CurrentElementLabel.Content = "";
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            _isDragging = true;
            _lastMousePosition = e.GetPosition(Canvas);
            CaptureMouse();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging)
            {
                Point currentMousePosition = e.GetPosition(Canvas);
                Vector offset = currentMousePosition - _lastMousePosition;

                // Update the position of the element
                MoveElement(offset);

                _lastMousePosition = currentMousePosition; 
                var window = App.Current.MainWindow as MainWindow;
                if (window != null)
                {
                    window.CurrentElementLabel.Content = ToString();
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
            }
        }

        // Adjust element position and request canvas to redraw
        protected abstract void MoveElement(Vector offset);

        // Helper method to invalidate the parent canvas
        protected void InvalidateCanvas()
        {
            // Request the canvas to re-render by invalidating it
            Canvas?.InvalidateVisual();
        }
    }
}

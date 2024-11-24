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
        public bool ShowControls { get => Canvas.SelectedElements.Contains(this); }
        public bool AnchorVisible { get; set; } = false;
        public bool HasAnchorPoint { get; set; } = true;

        private bool overrideAnchorPoint = false;
        public bool OverrideAnchorPoint
        {
            get => HasAnchorPoint ? overrideAnchorPoint : false;
            set => overrideAnchorPoint = value;
        }
        private DraggablePoint anchorPoint;
        public DraggablePoint AnchorPoint
        {
            get => HasAnchorPoint ? anchorPoint : null;
            set => anchorPoint = value;
        }

        public CustomCanvas Canvas { get; set; }
        public double UnitSize { get => Canvas.UnitSize; }

        protected bool _isDragging = false;
        public bool IsFirstRender { get; set; } = true;

        protected Point _lastMousePosition;
        public bool AllowSnapping { get; set; } = true;
        protected virtual bool SnappingEnabled { get => AllowSnapping ? Canvas.GridSnapping : false; }

        protected CustomElement(CustomCanvas canvas)
        {
            Canvas = canvas;
            Focusable = true;
            FocusVisualStyle = null;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (HasAnchorPoint && AnchorPoint is null)
            {
                AnchorPoint = new DraggablePoint(Canvas)
                {
                    Radius = 10,
                    HasAnchorPoint = false,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Green,
                    StrokeThickness = 2,
                    Shape = PointShape.Anchor,
                    MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                    MoveAction = OnAnchorPointMove
                };
                Canvas.Children.Add(AnchorPoint);
            }
            if (HasAnchorPoint)
            {
                if (!OverrideAnchorPoint)
                {
                    AnchorPoint.Position = GetAnchorDefaultPosition();
                }
                AnchorPoint.Visibility = AnchorVisible ? Visibility.Visible : Visibility.Hidden;
            }
            base.OnRender(drawingContext);

        }
        protected virtual Point GetAnchorDefaultPosition()
        {
            return new Point(0,0);
        }

        protected virtual void OnAnchorPointMove(Vector offset)
        {
            OverrideAnchorPoint = true;
        }

        protected virtual void OnPointMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        public virtual Point GetOriginPoint(Size arrangedSize)
        {
            return new Point(
                arrangedSize.Width/2, 
                arrangedSize.Height/2
                );
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if(e.Key == Key.Home)
            {
                OverrideAnchorPoint = false;
                InvalidateVisual();
                Canvas.InvalidateVisual();
            }
        }

        #region logic for dragging elements
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
            Keyboard.Focus(this);
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
                InvalidateCanvas();

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
        public virtual void MoveElement(Vector offset)
        {
            if (HasAnchorPoint)
            {
                AnchorPoint.MoveElement(offset);
            }
            InvalidateCanvas();
        }
        #endregion

        // Helper method to invalidate the parent canvas
        protected void InvalidateCanvas()
        {
            // Request the canvas to re-render by invalidating it
            Canvas?.InvalidateVisual();
        }

        protected double SnapValue(double value)
        {
            const double lowerThreshold = 0.1;
            const double upperThreshold = 0.9;
            const double toHalfLower = 0.45;
            const double toHalfUpper = 0.55;

            double integerPart = Math.Floor(value);
            double fractionalPart = value - integerPart;

            if (fractionalPart <= lowerThreshold) return integerPart;
            if (fractionalPart >= upperThreshold) return integerPart + 1;
            if (fractionalPart >= toHalfLower && fractionalPart <= toHalfUpper) return integerPart + 0.5;
            return value;
        }
    }
}

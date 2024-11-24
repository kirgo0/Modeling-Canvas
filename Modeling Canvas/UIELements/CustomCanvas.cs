using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;

namespace Modeling_Canvas.UIELements
{

    public class CustomCanvas : Canvas
    {
        public double XOffset { get; set; } = 0;
        public double YOffset { get; set; } = 0;

        public int RotationPrecision { get; set; } = 4;
        public double UnitSize
        {
            get => (double)GetValue(UnitSizeProperty);
            set => SetValue(UnitSizeProperty, value);
        }

        public static readonly DependencyProperty UnitSizeProperty =
            DependencyProperty.Register(nameof(UnitSize), typeof(double), typeof(CustomCanvas),
                new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double GridFrequency
        {
            get => (double)GetValue(GridFrequencyProperty);
            set => SetValue(GridFrequencyProperty, value);
        }

        public static readonly DependencyProperty GridFrequencyProperty =
            DependencyProperty.Register(nameof(GridFrequency), typeof(double), typeof(CustomCanvas),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public HashSet<CustomElement> SelectedElements { get; set; } = new();

        public Point GetCanvasUnitCoordinates(Point pixelCoordinates)
        {
            return new Point((pixelCoordinates.X - ActualWidth / 2 - XOffset) / UnitSize, (ActualHeight / 2 - pixelCoordinates.Y - YOffset) / UnitSize);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            DrawCoordinateGrid(dc);
        }
        public bool GridSnapping { get; set; } = false; // Enable grid snapping by default
        // Calculate grid lines for snapping

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double centerX = arrangeSize.Width / 2;
            double centerY = arrangeSize.Height / 2;

            for (var i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                if (child is CustomElement element)
                {
                    var point = element.GetOriginPoint(arrangeSize);
                    var pointWithOffset = new Point(point.X + XOffset, point.Y - YOffset);
                    // Arrange the element
                    element.Arrange(new Rect(pointWithOffset, element.DesiredSize));
                    element.InvalidateVisual();
                    element.InvalidateMeasure();
                    element.InvalidateArrange();
                }
            }

            return arrangeSize;
        }

        protected void DrawCoordinateGrid(DrawingContext dc)
        {
            double width = ActualWidth;
            double height = ActualHeight;
            double halfWidth = width / 2 + XOffset;
            double halfHeight = height / 2 - YOffset;

            Pen gridPen = new Pen(Brushes.Black, 0.1);
            Pen axisPen = new Pen(Brushes.Black, 2);

            // Draw grid
            if (UnitSize < 0 || GridFrequency < 0)
            {
                return;
            }

            var calculatedFrequncy = GridFrequency;
            if (UnitSize < 5) calculatedFrequncy = 25;

            // Vertical lines and coordinates
            for (double x = halfWidth; x < width; x += UnitSize * calculatedFrequncy)
            {
                dc.DrawLine(gridPen, new Point(x, 0), new Point(x, height));
            }
            for (double x = halfWidth; x > 0; x -= UnitSize * calculatedFrequncy)
            {
                dc.DrawLine(gridPen, new Point(x, 0), new Point(x, height));
            }

            // Horizontal lines and coordinates
            for (double y = halfHeight; y < height; y += UnitSize * calculatedFrequncy)
            {
                dc.DrawLine(gridPen, new Point(0, y), new Point(width, y));
            }
            for (double y = halfHeight; y > 0; y -= UnitSize * calculatedFrequncy)
            {
                dc.DrawLine(gridPen, new Point(0, y), new Point(width, y));
            }

            // Draw axes
            dc.DrawLine(axisPen, new Point(0, halfHeight), new Point(width, halfHeight)); // X-axis
            dc.DrawLine(axisPen, new Point(halfWidth, 0), new Point(halfWidth, height)); // Y-axis

            // Draw labels
            FormattedText xLabel = new FormattedText(
                "X",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                14,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            FormattedText yLabel = new FormattedText(
                "Y",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                14,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            // Position X and Y labels at the ends of the axes
            dc.DrawText(xLabel, new Point(width - xLabel.Width - 5, halfHeight - xLabel.Height - 5)); // X-axis label
            dc.DrawText(yLabel, new Point(halfWidth + 5, 5)); // Y-axis label
        }
        public bool IsCtrlPressed { get; set; } = false;
        public bool IsAltPressed { get; set; } = false;
        public bool IsSpacePressed { get; set; } = false;
        public bool IsLeftMouseButtonPressed { get; set; } = false;

        private Point previousMousePosition;
        public CustomCanvas()
        {
            // Hook mouse and key events
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
        }
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                GridSnapping = true;
            }
            else if (e.Key == Key.LeftCtrl)
            {
                IsCtrlPressed = true;
            }
            else if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                IsAltPressed = true;
            }
            else if (e.Key == Key.Space)
            {
                IsSpacePressed = true;
                Mouse.OverrideCursor = Cursors.Hand;
            }
        }

        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                GridSnapping = false;
            }
            else if (e.Key == Key.LeftCtrl)
            {
                IsCtrlPressed = false;
                IsAltPressed = false;
            }
            else if (e.Key == Key.Space)
            {
                IsSpacePressed = false;
                Mouse.OverrideCursor = null;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsSpacePressed && IsLeftMouseButtonPressed)
            {
                var currentMousePosition = e.GetPosition(this);
                var deltaX = currentMousePosition.X - previousMousePosition.X;
                var deltaY = currentMousePosition.Y - previousMousePosition.Y;

                // Update offsets
                XOffset += deltaX;
                YOffset -= deltaY; // Invert Y for typical grid behavior

                previousMousePosition = currentMousePosition;

                // Redraw canvas
                InvalidateVisual();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            SelectedElements.Clear();
            InvalidateVisual();
            IsLeftMouseButtonPressed = true;
            Keyboard.ClearFocus();
            Keyboard.Focus(this);
            previousMousePosition = e.GetPosition(this);
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            IsLeftMouseButtonPressed = false;
            Mouse.OverrideCursor = null;
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (IsCtrlPressed)
            {
                // Determine delta multiplier based on whether Alt is pressed
                double deltaMultiplier = IsAltPressed ? 0.01 : 0.1;

                if (e.Delta != 0)
                {
                    // Adjust UnitSize based on delta multiplier
                    UnitSize += Math.Round(e.Delta * deltaMultiplier);
                    if (UnitSize <= 0) UnitSize = 1;
                }
            }
        }

    }
}


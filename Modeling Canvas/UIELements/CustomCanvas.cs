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

        public double GetDegreesBetweenMouseAndPoint(Point point)
        {
            var mousePosition = GetCanvasMousePosition();
            var angleInDegrees = Math.Atan2(-(mousePosition.Y - point.Y), mousePosition.X - point.X) * (180 / Math.PI);
            return (angleInDegrees + 360) % 360;
        }

        public Point GetCanvasMousePosition()
        {
            var mouseOnCanvas = Mouse.GetPosition(this);
            return GetCanvasUnitCoordinates(mouseOnCanvas);
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            DrawCoordinateGrid(dc); 
        }
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
        public int NumberFrequency { get; set; } = 1; // Default frequency is 1

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

            var calculatedFrequency = GridFrequency;
            if (UnitSize < 25) calculatedFrequency = 5;
            if (UnitSize < 15) calculatedFrequency = 10;
            if (UnitSize < 10) calculatedFrequency = 10;
            if (UnitSize < 5) calculatedFrequency = 50;
            if (UnitSize < 1) calculatedFrequency = 250;
            if (UnitSize < 0.2) calculatedFrequency = 500;

            // Vertical lines and coordinates
            for (double x = halfWidth; x < width; x += UnitSize * calculatedFrequency)
            {
                dc.DrawLine(gridPen, new Point(x, 0), new Point(x, height));
                DrawCoordinateLabel(dc, Math.Round((x - halfWidth) / UnitSize, 3), new Point(x, halfHeight), 15, false);
            }
            for (double x = halfWidth; x > 0; x -= UnitSize * calculatedFrequency)
            {
                dc.DrawLine(gridPen, new Point(x, 0), new Point(x, height));
                DrawCoordinateLabel(dc, Math.Round((x - halfWidth) / UnitSize, 3), new Point(x, halfHeight), 15, false);
            }

            // Horizontal lines and coordinates
            for (double y = halfHeight; y < height; y += UnitSize * calculatedFrequency)
            {
                dc.DrawLine(gridPen, new Point(0, y), new Point(width, y));
                DrawCoordinateLabel(dc, Math.Round(-(y - halfHeight) / UnitSize, 3), new Point(halfWidth, y), 15, true);
            }
            for (double y = halfHeight; y > 0; y -= UnitSize * calculatedFrequency)
            {
                dc.DrawLine(gridPen, new Point(0, y), new Point(width, y));
                DrawCoordinateLabel(dc, Math.Round(-(y - halfHeight) / UnitSize, 3), new Point(halfWidth, y), 15, true);
            }

            // Draw axes
            dc.DrawLine(axisPen, new Point(0, halfHeight), new Point(width, halfHeight)); // X-axis
            dc.DrawLine(axisPen, new Point(halfWidth, 0), new Point(halfWidth, height)); // Y-axis
            

        }

        private void DrawCoordinateLabel(DrawingContext dc, double value, Point position, double fontSize, bool isHorizontal)
        {
            if (value == 0)
                return;
            if (NumberFrequency > 0 && (int)value % NumberFrequency == 0)
            {
                var yOffset = isHorizontal ? fontSize/2 : fontSize;
                var xOffset = isHorizontal ? fontSize : fontSize/2;
                var formattedText = new FormattedText(
                    value.ToString(),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    fontSize,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                // Adjust position slightly to center the text on the grid line
                dc.DrawText(formattedText, new Point(position.X + xOffset, position.Y + yOffset));
            }
        }


        private Point previousMousePosition;
        public CustomCanvas()
        {
            // Hook mouse and key events
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
        }
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                Mouse.OverrideCursor = Cursors.Hand;
            }
            if (e.Key == Key.LeftCtrl)
            {
                Mouse.OverrideCursor = Cursors.SizeNESW;
            }
        }

        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }
        protected void ClearControlPanel()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ClearControlStack();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (InputManager.SpacePressed && InputManager.LeftMousePressed)
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
            UpdateMouseLabel();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            SelectedElements.Clear();
            InvalidateVisual();
            ClearControlPanel();
            previousMousePosition = e.GetPosition(this);
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Mouse.OverrideCursor = null;
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (InputManager.CtrlPressed)
            {
                // Determine delta multiplier based on whether Alt is pressed
                double deltaMultiplier = InputManager.AltPressed ? 0.01 : 0.1;
                if (UnitSize < 10) deltaMultiplier = InputManager.AltPressed ? 0.004 : 0.01;
                if (UnitSize < 5)
                {
                    deltaMultiplier = InputManager.AltPressed ? 0.0008 : 0.004; 
                }
                if (e.Delta != 0)
                {
                    // Adjust UnitSize based on delta multiplier
                    UnitSize += Math.Round(e.Delta * deltaMultiplier, 4);
                    if (UnitSize < 0.1) UnitSize = 0.1;
                    if (UnitSize > 5) UnitSize = Math.Round(UnitSize);
                }
                UpdateMouseLabel();
            }
        }

        public void UpdateMouseLabel()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var position = Mouse.GetPosition(this);
                var unitPosition = GetCanvasMousePosition();
                mainWindow.MousePositionLabel.Content = $"X: {Math.Round(unitPosition.X, 2)} Y: {Math.Round(unitPosition.Y, 2)}\np.X: {Math.Round(position.X, 2)} p.Y: {Math.Round(position.Y, 2)}";
            }
        }

    }
}


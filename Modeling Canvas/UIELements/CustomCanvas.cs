using Modeling_Canvas.Commands;
using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using Modeling_Canvas.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Modeling_Canvas.UIElements
{
    public class CustomCanvas : Canvas, INotifyPropertyChanged
    {
        public double XOffset { get; set; } = 0;
        public double YOffset { get; set; } = 0;

        public RenderMode _renderMode = RenderMode.Default;
        public RenderMode RenderMode
        {
            get => _renderMode;
            set
            {
                _renderMode = value;
                InvalidateVisual();
            }
        }
        public int RotationPrecision { get; set; } = 4;
        private AffineModel _affineParams = new AffineModel();
        public AffineModel AffineParams
        {
            get => _affineParams;
            set { _affineParams = value; OnPropertyChanged(nameof(AffineParams)); }
        }

        private ProjectiveModel _projectiveParams = new ProjectiveModel();
        public ProjectiveModel ProjectiveParams
        {
            get => _projectiveParams;
            set { _projectiveParams = value; OnPropertyChanged(nameof(ProjectiveParams)); }
        }

        public ICommand InvalidateCanvasCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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

        private Point previousMousePosition;
        public bool AllowInfinityRender { get; set; } = false;

        private DispatcherTimer timer;
        public CustomCanvas()
        {
            InvalidateCanvasCommand = new RelayCommand(_ =>
            {
                Keyboard.Focus(this);
                InvalidateVisual();
            });
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            switch (RenderMode)
            {
                case RenderMode.Default:
                    DrawCoordinateGrid(dc);
                    break;
                case RenderMode.Affine:
                    DrawAffineCoordinateGrid(dc);
                    break;
                case RenderMode.Projective:
                    DrawProjectiveCoordinateGrid(dc);
                    break;
            }
        }
        public Point GetUnitCoordinates(Point pixelCoordinates)
        {
            if (RenderMode is RenderMode.Affine)
            {
                pixelCoordinates = pixelCoordinates.ReverseAffineTransformation(AffineParams);
            }
            else if (RenderMode is RenderMode.Projective)
            {
                pixelCoordinates = pixelCoordinates.ReverseProjectiveTransformation(ProjectiveParams);
            }
            pixelCoordinates = new Point((pixelCoordinates.X - ActualWidth / 2 - XOffset) / UnitSize, (ActualHeight / 2 - pixelCoordinates.Y - YOffset) / UnitSize);
            return pixelCoordinates;
        }
        public Point GetPixelCoordinates(Point unitCoordinates)
        {
            return new Point(ActualWidth / 2 + unitCoordinates.X * UnitSize, ActualHeight / 2 - unitCoordinates.Y * UnitSize).AddCanvasOffsets();
        }
        public void ResetOffests()
        {
            XOffset = 0;
            YOffset = 0;
            InvalidateVisual();
        }
        public void ResetScaling()
        {
            UnitSize = 40;
            InvalidateVisual();
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
            return GetUnitCoordinates(mouseOnCanvas);
        }

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
                    element.Arrange(new Rect(point, element.DesiredSize));
                    element.InvalidateVisual();
                    element.InvalidateMeasure();
                    element.InvalidateArrange();
                }
            }

            return arrangeSize;
        }
        public int NumberFrequency { get; set; } = 1;

        protected double GetOptimalGridFrequency()
        {
            var calculatedFrequency = GridFrequency;
            if (UnitSize < 25) calculatedFrequency = 5;
            if (UnitSize < 15) calculatedFrequency = 10;
            if (UnitSize < 10) calculatedFrequency = 25;
            if (UnitSize < 5) calculatedFrequency = 100;
            if (UnitSize < 1) calculatedFrequency = 250;
            if (UnitSize < 0.5) calculatedFrequency = 500;
            if (UnitSize <= 0.1) calculatedFrequency = 2500;
            return calculatedFrequency;
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

            var calculatedFrequency = GetOptimalGridFrequency();

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
        protected void DrawAffineCoordinateGrid(DrawingContext dc)
        {
            double width = ActualWidth;
            double height = ActualHeight;
            double halfWidth = width / 2 + XOffset;
            double halfHeight = height / 2 - YOffset;

            Pen gridPen = new Pen(Brushes.Black, 0.2);
            Pen axisPen = new Pen(Brushes.Black, 2);

            if (UnitSize < 0 || GridFrequency < 0) return;

            var calculatedFrequency = GetOptimalGridFrequency();

            var corners = new[]
            {
                new Point(0, 0).ReverseAffineTransformation(AffineParams),
                new Point(width, 0).ReverseAffineTransformation(AffineParams),
                new Point(0, height).ReverseAffineTransformation(AffineParams),
                new Point(width, height).ReverseAffineTransformation(AffineParams),
            };

            // Calculate bounds of the transformed canvas
            double minX = corners.Min(c => c.X);
            double maxX = corners.Max(c => c.X);
            double minY = corners.Min(c => c.Y);
            double maxY = corners.Max(c => c.Y);

            // Draw vertical grid lines
            for (double x = halfWidth; x < maxX; x += UnitSize * calculatedFrequency)
            {
                var p1 = new Point(x, minY).ApplyAffineTransformation(AffineParams);
                var p2 = new Point(x, maxY).ApplyAffineTransformation(AffineParams);
                dc.DrawLine(gridPen, p1, p2);

                DrawCoordinateLabel(dc, Math.Round((x - halfWidth) / UnitSize, 3),
                    new Point(x, halfHeight).ApplyAffineTransformation(AffineParams), 15, false);
            }
            for (double x = halfWidth; x > minX; x -= UnitSize * calculatedFrequency)
            {
                var p1 = new Point(x, minY).ApplyAffineTransformation(AffineParams);
                var p2 = new Point(x, maxY).ApplyAffineTransformation(AffineParams);
                dc.DrawLine(gridPen, p1, p2);

                DrawCoordinateLabel(dc, Math.Round((x - halfWidth) / UnitSize, 3),
                    new Point(x, halfHeight).ApplyAffineTransformation(AffineParams), 15, false);
            }

            // Draw horizontal grid lines
            for (double y = halfHeight; y < maxY; y += UnitSize * calculatedFrequency)
            {
                var p1 = new Point(minX, y).ApplyAffineTransformation(AffineParams);
                var p2 = new Point(maxX, y).ApplyAffineTransformation(AffineParams);
                dc.DrawLine(gridPen, p1, p2);

                DrawCoordinateLabel(dc, Math.Round(-(y - halfHeight) / UnitSize, 3),
                    new Point(halfWidth, y).ApplyAffineTransformation(AffineParams), 15, true);
            }

            for (double y = halfHeight; y > minY; y -= UnitSize * calculatedFrequency)
            {
                var p1 = new Point(minX, y).ApplyAffineTransformation(AffineParams);
                var p2 = new Point(maxX, y).ApplyAffineTransformation(AffineParams);
                dc.DrawLine(gridPen, p1, p2);

                DrawCoordinateLabel(dc, Math.Round(-(y - halfHeight) / UnitSize, 3),
                    new Point(halfWidth, y).ApplyAffineTransformation(AffineParams), 15, true);
            }

            // Draw axes with transformation

            // x axis
            dc.DrawLine(axisPen,
                new Point(minX, halfHeight).ApplyAffineTransformation(AffineParams),
                new Point(maxX, halfHeight).ApplyAffineTransformation(AffineParams));

            // y axis
            dc.DrawLine(axisPen,
                new Point(halfWidth, minY).ApplyAffineTransformation(AffineParams),
                new Point(halfWidth, maxY).ApplyAffineTransformation(AffineParams));
        }

        protected void DrawProjectiveCoordinateGrid(DrawingContext dc)
        {
            double width = ActualWidth;
            double height = ActualHeight;
            double halfWidth = width / 2 + XOffset;
            double halfHeight = height / 2 - YOffset;

            Pen gridPen = new Pen(Brushes.Black, 0.2);
            Pen axisPen = new Pen(Brushes.Black, 2);

            if (UnitSize < 0 || GridFrequency < 0)
            {
                return;
            }
            var calculatedFrequency = GetOptimalGridFrequency();

            double minX = 0;
            double maxX = width;
            double minY = 0;
            double maxY = height;

            if (AllowInfinityRender)
            {
                var corners = new[]
                {
                    new Point(0, 0).ReverseProjectiveTransformation(ProjectiveParams),
                    new Point(width, 0).ReverseProjectiveTransformation(ProjectiveParams),
                    new Point(0, height).ReverseProjectiveTransformation(ProjectiveParams),
                    new Point(width, height).ReverseProjectiveTransformation(ProjectiveParams),
                };

                minX = corners.Min(c => c.X);
                maxX = corners.Max(c => c.X);
                minY = corners.Min(c => c.Y);
                maxY = corners.Max(c => c.Y);

            }

            // Vertical lines and labels
            for (double x = halfWidth; x < maxX; x += UnitSize * calculatedFrequency)
            {
                var start = new Point(x, minY).ApplyProjectiveTransformation(ProjectiveParams);
                var end = new Point(x, maxY).ApplyProjectiveTransformation(ProjectiveParams);
                dc.DrawLine(gridPen, start, end);
                var number = Math.Round(x - halfWidth) / UnitSize;
                if (number % 10 != 0) continue;
                DrawCoordinateLabel(dc, number, new Point(x, halfHeight).ApplyProjectiveTransformation(ProjectiveParams), 15, false);
            }
            for (double x = halfWidth; x > minY; x -= UnitSize * calculatedFrequency)
            {
                var start = new Point(x, minY).ApplyProjectiveTransformation(ProjectiveParams);
                var end = new Point(x, maxY).ApplyProjectiveTransformation(ProjectiveParams);
                dc.DrawLine(gridPen, start, end);
                var number = Math.Round(x - halfWidth) / UnitSize;
                if (number % 10 != 0) continue;
                DrawCoordinateLabel(dc, number, new Point(x, halfHeight).ApplyProjectiveTransformation(ProjectiveParams), 15, false);
            }
            // Horizontal lines and labels
            for (double y = halfHeight; y < maxY; y += UnitSize * calculatedFrequency)
            {
                var start = new Point(minX, y).ApplyProjectiveTransformation(ProjectiveParams);
                var end = new Point(maxX, y).ApplyProjectiveTransformation(ProjectiveParams);
                dc.DrawLine(gridPen, start, end);
                var number = Math.Round(-(y - halfHeight) / UnitSize);
                if (number % 10 != 0) continue;
                DrawCoordinateLabel(dc, number, new Point(halfWidth, y).ApplyProjectiveTransformation(ProjectiveParams), 15, true);
            }
            for (double y = halfHeight; y > minY; y -= UnitSize * calculatedFrequency)
            {
                var start = new Point(minX, y).ApplyProjectiveTransformation(ProjectiveParams);
                var end = new Point(maxX, y).ApplyProjectiveTransformation(ProjectiveParams);
                dc.DrawLine(gridPen, start, end);
                var number = Math.Round(-(y - halfHeight) / UnitSize);
                if (number % 10 != 0) continue;
                DrawCoordinateLabel(dc, number, new Point(halfWidth, y).ApplyProjectiveTransformation(ProjectiveParams), 15, true);
            }
            // Axes
            dc.DrawLine(axisPen, new Point(minX, halfHeight).ApplyProjectiveTransformation(ProjectiveParams), new Point(maxX, halfHeight).ApplyProjectiveTransformation(ProjectiveParams)); // X-axis
            dc.DrawLine(axisPen, new Point(halfWidth, minY).ApplyProjectiveTransformation(ProjectiveParams), new Point(halfWidth, maxY).ApplyProjectiveTransformation(ProjectiveParams)); // Y-axis
        }

        private void DrawCoordinateLabel(DrawingContext dc, double value, Point position, double fontSize, bool isHorizontal)
        {
            if (value == 0)
                return;
            if (NumberFrequency > 0 && (int)value % NumberFrequency == 0)
            {
                var yOffset = isHorizontal ? fontSize / 2 : fontSize;
                var xOffset = isHorizontal ? fontSize : fontSize / 2;
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
            if (e.Key == Key.E)
            {
                foreach (var element in SelectedElements)
                {
                    element.OverrideAnchorPoint = false;
                }
                InvalidateVisual();
            }
            if (e.Key == Key.LeftAlt)
            {
                InvalidateVisual();
            }
        }

        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            Mouse.OverrideCursor = null;
            if (e.Key == Key.LeftAlt)
            {
                InvalidateVisual();
            }
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

                if (RenderMode is RenderMode.Affine)
                {
                    var cmpA = currentMousePosition.ReverseAffineTransformation(AffineParams);
                    var pmpA = previousMousePosition.ReverseAffineTransformation(AffineParams);
                    deltaX = cmpA.X - pmpA.X;
                    deltaY = cmpA.Y - pmpA.Y;
                }
                else if (RenderMode is RenderMode.Projective)
                {

                }

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

                var focusedElement = Keyboard.FocusedElement;
                if (focusedElement is UIElement uiElement)
                {
                    mainWindow.MousePositionLabel.Content = $"X: {Math.Round(unitPosition.X, 2)} Y: {Math.Round(unitPosition.Y, 2)}\np.X: {Math.Round(position.X, 2)} p.Y: {Math.Round(position.Y, 2)}" + uiElement;
                }
                mainWindow.MousePositionLabel.Content = $"X: {Math.Round(unitPosition.X, 2)} Y: {Math.Round(unitPosition.Y, 2)}\np.X: {Math.Round(position.X, 2)} p.Y: {Math.Round(position.Y, 2)}";

            }
        }

    }
}


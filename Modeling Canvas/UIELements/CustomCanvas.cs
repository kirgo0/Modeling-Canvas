using Modeling_Canvas.Commands;
using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using Modeling_Canvas.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class CustomCanvas : Canvas, INotifyPropertyChanged
    {
        private Point previousMousePosition;

        private bool _allowInfinityRender = false;

        private AffineModel _affineParams = new AffineModel();

        private ProjectiveModel _projectiveParams = new ProjectiveModel();

        private RenderMode _renderMode = RenderMode.Default;

        public double XOffset { get; set; } = 0;

        public double YOffset { get; set; } = 0;

        public int RotationPrecision { get; set; } = 4;

        public int NumberFrequency { get; set; } = 1;

        public HashSet<Element> SelectedElements { get; set; } = new();

        public ICommand InvalidateCanvasCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler CanvasGridChanged;

        public event EventHandler<CanvasSelectionChangedEventArgs> SelectedElementsChanged;

        public AffineModel AffineParams
        {
            get => _affineParams;
            set
            {
                _affineParams = value;
                OnPropertyChanged(nameof(AffineParams));
            }
        }

        public ProjectiveModel ProjectiveParams
        {
            get => _projectiveParams;
            set
            {
                _projectiveParams = value;
                OnPropertyChanged(nameof(ProjectiveParams));
            }
        }

        public double UnitSize
        {
            get => (double)GetValue(UnitSizeProperty);
            set
            {
                CanvasGridChanged?.Invoke(this, new EventArgs());
                SetValue(UnitSizeProperty, value);
            }
        }

        public double GridFrequency
        {
            get => (double)GetValue(GridFrequencyProperty);
            set => SetValue(GridFrequencyProperty, value);
        }

        public bool AllowInfinityRender
        {
            get => _allowInfinityRender;
            set
            {
                if (_allowInfinityRender != value)
                {
                    _allowInfinityRender = value;
                    OnPropertyChanged(nameof(AllowInfinityRender));
                }
            }
        }

        public RenderMode RenderMode
        {
            get => _renderMode;
            set
            {
                _renderMode = value;
                CanvasGridChanged?.Invoke(this, new EventArgs());
                InvalidateVisual();
            }
        }

        public static readonly DependencyProperty UnitSizeProperty =
            DependencyProperty.Register(nameof(UnitSize), typeof(double), typeof(CustomCanvas),
                new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty GridFrequencyProperty =
            DependencyProperty.Register(nameof(GridFrequency), typeof(double), typeof(CustomCanvas),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public CustomCanvas()
        {
            InvalidateCanvasCommand = new RelayCommand(_ =>
            {
                Keyboard.Focus(this);
                InvalidateVisual();
            });
            AffineParams.PropertyChanged += (s, e) => CanvasGridChanged?.Invoke(this, new EventArgs());
            ProjectiveParams.PropertyChanged += (s, e) => CanvasGridChanged?.Invoke(this, new EventArgs());
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            CanvasGridChanged?.Invoke(this, new EventArgs());
            base.OnRenderSizeChanged(sizeInfo);
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            DrawGrid(dc);
        }

        public Point GetTransformedUnitCoordinates(Point pixelCoordinates)
        {
            pixelCoordinates = ReverseTransformPoint(pixelCoordinates);
            pixelCoordinates = new Point((pixelCoordinates.X - ActualWidth / 2 - XOffset) / UnitSize, (ActualHeight / 2 - pixelCoordinates.Y - YOffset) / UnitSize);
            return pixelCoordinates;
        }

        public Point GetUnitCoordinates(Point pixelCoordinates)
        {
            pixelCoordinates = new Point((pixelCoordinates.X - ActualWidth / 2 - XOffset) / UnitSize, (ActualHeight / 2 - pixelCoordinates.Y - YOffset) / UnitSize);
            return pixelCoordinates;
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
            return GetTransformedUnitCoordinates(mouseOnCanvas);
        }

        public Point TransformPoint(Point point)
        {
            switch (RenderMode)
            {
                case RenderMode.Affine:
                    return point.ApplyAffineTransformation(AffineParams);
                case RenderMode.Projective:
                    return point.ApplyProjectiveTransformation(ProjectiveParams);
                case RenderMode.ProjectiveV2:
                    return point.ApplyProjectiveV2Transformation(ProjectiveParams);
                default:
                    return point;
            }
        }

        public Point ReverseTransformPoint(Point point)
        {
            switch (RenderMode)
            {
                case RenderMode.Affine:
                    return point.ReverseAffineTransformation(AffineParams);
                case RenderMode.Projective:
                    return point.ReverseProjectiveTransformation(ProjectiveParams);
                case RenderMode.ProjectiveV2:
                    return point.ReverseProjectiveV2Transformation(ProjectiveParams);
                default:
                    return point;
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double centerX = arrangeSize.Width / 2;
            double centerY = arrangeSize.Height / 2;

            for (var i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                if (child is Element element)
                {
                    var point = element.GetOriginPoint(arrangeSize);
                    element.Arrange(new Rect(point, element.DesiredSize));
                    element.InvalidateVisual();
                }
            }

            return arrangeSize;
        }

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

        //protected void DrawGrid(DrawingContext dc)
        //{

        //    double width = ActualWidth;
        //    double height = ActualHeight;
        //    double halfWidth = width / 2 + XOffset;
        //    double halfHeight = height / 2 - YOffset;

        //    Pen gridPen = new Pen(Brushes.Black, 0.1);

        //    if (RenderMode is RenderMode.Projective || RenderMode is RenderMode.ProjectiveV2)
        //        gridPen = new Pen(Brushes.Black, 0.4);

        //    Pen axisPen = new Pen(Brushes.Black, 2);

        //    if (UnitSize < 0 || GridFrequency < 0)
        //    {
        //        return;
        //    }

        //    var calculatedFrequency = GetOptimalGridFrequency();

        //    double minX = 0, minY = 0, maxX = width, maxY = height;

        //    // Draw vertical grid lines
        //    for (double x = halfWidth; x < maxX; x += UnitSize * calculatedFrequency)
        //    {
        //        var p1 = TransformPoint(new Point(x, minY));
        //        var p2 = TransformPoint(new Point(x, maxY));
        //        dc.DrawLine(gridPen, p1, p2);

        //        DrawCoordinateLabel(dc, Math.Round((x - halfWidth) / UnitSize, 3),
        //            TransformPoint(new Point(x, halfHeight)), 15, false);
        //    }

        //    for (double x = halfWidth; x > minX; x -= UnitSize * calculatedFrequency)
        //    {
        //        var p1 = TransformPoint(new Point(x, minY));
        //        var p2 = TransformPoint(new Point(x, maxY));
        //        dc.DrawLine(gridPen, p1, p2);

        //        DrawCoordinateLabel(dc, Math.Round((x - halfWidth) / UnitSize, 3),
        //            TransformPoint(new Point(x, halfHeight)), 15, false);
        //    }

        //    //Draw horizontal grid lines
        //    for (double y = halfHeight; y < maxY; y += UnitSize * calculatedFrequency)
        //    {
        //        var p1 = TransformPoint(new Point(minX, y));
        //        var p2 = TransformPoint(new Point(maxX, y));
        //        dc.DrawLine(gridPen, p1, p2);

        //        DrawCoordinateLabel(dc, Math.Round(-(y - halfHeight) / UnitSize, 3),
        //            TransformPoint(new Point(halfWidth, y)), 15, true);
        //    }

        //    for (double y = halfHeight; y > minY; y -= UnitSize * calculatedFrequency)
        //    {
        //        var p1 = TransformPoint(new Point(minX, y));
        //        var p2 = TransformPoint(new Point(maxX, y));
        //        dc.DrawLine(gridPen, p1, p2);

        //        DrawCoordinateLabel(dc, Math.Round(-(y - halfHeight) / UnitSize, 3),
        //            TransformPoint(new Point(halfWidth, y)), 15, true);
        //    }

        //    // Draw axes with transformation

        //    // x axis
        //    dc.DrawLine(axisPen, TransformPoint(new Point(minX, halfHeight)), TransformPoint(new Point(maxX, halfHeight)));

        //    // y axis
        //    dc.DrawLine(axisPen, TransformPoint(new Point(halfWidth, minY)), TransformPoint(new Point(halfWidth, maxY)));

        //}

        protected void DrawGrid(DrawingContext dc)
        {
            double width = ActualWidth;
            double height = ActualHeight;
            double halfWidth = width / 2 + XOffset;
            double halfHeight = height / 2 - YOffset;

            Pen gridPen = new Pen(Brushes.Black, 0.1);

            if (RenderMode is RenderMode.Projective || RenderMode is RenderMode.ProjectiveV2)
                gridPen = new Pen(Brushes.Black, 0.4);

            Pen axisPen = new Pen(Brushes.Black, 2);

            if (UnitSize < 0 || GridFrequency < 0)
            {
                return;
            }

            var calculatedFrequency = GetOptimalGridFrequency();

            double minX = 0, minY = 0, maxX = width, maxY = height;

            // Draw vertical grid lines
            for (double x = halfWidth; x < maxX; x += UnitSize * calculatedFrequency)
            {
                var p1 = TransformPoint(new Point(x, minY));
                var p2 = TransformPoint(new Point(x, maxY));
                DrawClippedLine(dc, gridPen, p1, p2);

                DrawCoordinateLabel(dc, Math.Round((x - halfWidth) / UnitSize, 3),
                    TransformPoint(new Point(x, halfHeight)), 15, false);
            }

            for (double x = halfWidth; x > minX; x -= UnitSize * calculatedFrequency)
            {
                var p1 = TransformPoint(new Point(x, minY));
                var p2 = TransformPoint(new Point(x, maxY));
                DrawClippedLine(dc, gridPen, p1, p2);

                DrawCoordinateLabel(dc, Math.Round((x - halfWidth) / UnitSize, 3),
                    TransformPoint(new Point(x, halfHeight)), 15, false);
            }

            // Draw horizontal grid lines
            for (double y = halfHeight; y < maxY; y += UnitSize * calculatedFrequency)
            {
                var p1 = TransformPoint(new Point(minX, y));
                var p2 = TransformPoint(new Point(maxX, y));
                DrawClippedLine(dc, gridPen, p1, p2);

                DrawCoordinateLabel(dc, Math.Round(-(y - halfHeight) / UnitSize, 3),
                    TransformPoint(new Point(halfWidth, y)), 15, true);
            }

            for (double y = halfHeight; y > minY; y -= UnitSize * calculatedFrequency)
            {
                var p1 = TransformPoint(new Point(minX, y));
                var p2 = TransformPoint(new Point(maxX, y));
                DrawClippedLine(dc, gridPen, p1, p2);

                DrawCoordinateLabel(dc, Math.Round(-(y - halfHeight) / UnitSize, 3),
                    TransformPoint(new Point(halfWidth, y)), 15, true);
            }

            // Draw axes with transformation

            // x axis
            DrawClippedLine(dc, axisPen, TransformPoint(new Point(minX, halfHeight)), TransformPoint(new Point(maxX, halfHeight)));

            // y axis
            DrawClippedLine(dc, axisPen, TransformPoint(new Point(halfWidth, minY)), TransformPoint(new Point(halfWidth, maxY)));
        }

        /// <summary>
        /// Draws a line that is clipped to the bounds of the canvas.
        /// </summary>
        private void DrawClippedLine(DrawingContext dc, Pen pen, Point p1, Point p2)
        {
            double width = ActualWidth;
            double height = ActualHeight;

            (bool isVisible, Point clippedP1, Point clippedP2) = ClipLineToRect(p1, p2, new Rect(0, 0, width, height));

            if (isVisible)
            {
                dc.DrawLine(pen, clippedP1, clippedP2);
            }
        }

        /// <summary>
        /// Clips a line to the bounds of a rectangle using the Liang-Barsky algorithm.
        /// </summary>
        private (bool isVisible, Point clippedP1, Point clippedP2) ClipLineToRect(Point p1, Point p2, Rect bounds)
        {
            double xMin = bounds.Left;
            double xMax = bounds.Right;
            double yMin = bounds.Top;
            double yMax = bounds.Bottom;

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            double t0 = 0, t1 = 1;

            bool ClipTest(double p, double q, ref double t0, ref double t1)
            {
                if (p == 0)
                {
                    return q >= 0;
                }

                double r = q / p;
                if (p < 0)
                {
                    if (r > t1) return false;
                    if (r > t0) t0 = r;
                }
                else
                {
                    if (r < t0) return false;
                    if (r < t1) t1 = r;
                }

                return true;
            }

            if (ClipTest(-dx, p1.X - xMin, ref t0, ref t1) &&
                ClipTest(dx, xMax - p1.X, ref t0, ref t1) &&
                ClipTest(-dy, p1.Y - yMin, ref t0, ref t1) &&
                ClipTest(dy, yMax - p1.Y, ref t0, ref t1))
            {
                if (t1 < 1)
                {
                    p2 = new Point(p1.X + t1 * dx, p1.Y + t1 * dy);
                }
                if (t0 > 0)
                {
                    p1 = new Point(p1.X + t0 * dx, p1.Y + t0 * dy);
                }

                return (true, p1, p2);
            }

            return (false, p1, p2);
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

        public void SelectElement(Element element)
        {
            SelectedElements.Add(element);
            SelectedElementsChanged?.Invoke(this, new CanvasSelectionChangedEventArgs(element));
        }

        public void ClearSelection()
        {
            SelectedElements.Clear();
            SelectedElementsChanged?.Invoke(this, new CanvasSelectionChangedEventArgs(null));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (InputManager.SpacePressed && InputManager.LeftMousePressed)
            {
                var currentMousePosition = e.GetPosition(this);
                var deltaX = currentMousePosition.X - previousMousePosition.X;
                var deltaY = currentMousePosition.Y - previousMousePosition.Y;

                var cmpA = ReverseTransformPoint(currentMousePosition);
                var pmpA = ReverseTransformPoint(previousMousePosition);
                deltaX = cmpA.X - pmpA.X;
                deltaY = cmpA.Y - pmpA.Y;

                // Update offsets
                XOffset += deltaX;
                YOffset -= deltaY; // Invert Y for typical grid behavior

                previousMousePosition = currentMousePosition;

                CanvasGridChanged?.Invoke(this, new EventArgs());
                // Redraw canvas
                InvalidateVisual();
            }
            base.OnMouseMove(e);
            UpdateMouseLabel();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            ClearSelection();
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
                    var calculatedSize = UnitSize + Math.Round(e.Delta * deltaMultiplier, 4);
                    if (calculatedSize < 0.1) calculatedSize = 0.1;
                    if (calculatedSize > 5) calculatedSize = Math.Round(calculatedSize);
                    UnitSize = calculatedSize;
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
                mainWindow.MousePositionLabel.Content = $"X: {Math.Round(unitPosition.X, 2)} Y: {Math.Round(unitPosition.Y, 2)}\np.X: {Math.Round(position.X, 2)} p.Y: {Math.Round(position.Y, 2)}\nUnitSize: {UnitSize}";

            }
        }

        public void ResetOffests()
        {
            XOffset = 0;
            YOffset = 0;

            CanvasGridChanged?.Invoke(this, new EventArgs());
            InvalidateVisual();
        }

        public void ResetScaling()
        {
            UnitSize = 40;
            InvalidateVisual();
        }
    }
}


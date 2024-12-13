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

        public HashSet<Element> SelectedElements { get; set; } = new();

        public int RotationPrecision { get; set; } = 4;

        public ICommand InvalidateCanvasCommand { get; }

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

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler CanvasGridChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public static readonly DependencyProperty UnitSizeProperty =
            DependencyProperty.Register(nameof(UnitSize), typeof(double), typeof(CustomCanvas),
                new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty GridFrequencyProperty =
            DependencyProperty.Register(nameof(GridFrequency), typeof(double), typeof(CustomCanvas),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

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

            if (AllowInfinityRender)
            {
                var corners = new[]
                    {
                        ReverseTransformPoint(new Point(0, 0)),
                        ReverseTransformPoint(new Point(maxX, 0)),
                        ReverseTransformPoint(new Point(0, height)),
                        ReverseTransformPoint(new Point(maxX, height)),
                    };

                // Calculate bounds of the transformed canvas
                minX = corners.Min(c => c.X);
                maxX = corners.Max(c => c.X);
                minY = corners.Min(c => c.Y);
                maxY = corners.Max(c => c.Y);
            }

            // Draw vertical grid lines
            for (double x = halfWidth; x < maxX; x += UnitSize * calculatedFrequency)
            {
                var p1 = TransformPoint(new Point(x, minY));
                var p2 = TransformPoint(new Point(x, maxY));
                dc.DrawLine(gridPen, p1, p2);

                DrawCoordinateLabel(dc, Math.Round((x - halfWidth) / UnitSize, 3),
                    TransformPoint(new Point(x, halfHeight)), 15, false);
            }
            for (double x = halfWidth; x > minX; x -= UnitSize * calculatedFrequency)
            {
                var p1 = TransformPoint(new Point(x, minY));
                var p2 = TransformPoint(new Point(x, maxY));
                dc.DrawLine(gridPen, p1, p2);

                DrawCoordinateLabel(dc, Math.Round((x - halfWidth) / UnitSize, 3),
                    TransformPoint(new Point(x, halfHeight)), 15, false);
            }

            // Draw horizontal grid lines
            for (double y = halfHeight; y < maxY; y += UnitSize * calculatedFrequency)
            {
                var p1 = TransformPoint(new Point(minX, y));
                var p2 = TransformPoint(new Point(maxX, y));
                dc.DrawLine(gridPen, p1, p2);

                DrawCoordinateLabel(dc, Math.Round(-(y - halfHeight) / UnitSize, 3),
                    TransformPoint(new Point(halfWidth, y)), 15, true);
            }

            for (double y = halfHeight; y > minY; y -= UnitSize * calculatedFrequency)
            {
                var p1 = TransformPoint(new Point(minX, y));
                var p2 = TransformPoint(new Point(maxX, y));
                dc.DrawLine(gridPen, p1, p2);

                DrawCoordinateLabel(dc, Math.Round(-(y - halfHeight) / UnitSize, 3),
                    TransformPoint(new Point(halfWidth, y)), 15, true);
            }

            // Draw axes with transformation

            // x axis
            dc.DrawLine(axisPen, TransformPoint(new Point(minX, halfHeight)), TransformPoint(new Point(maxX, halfHeight)));

            // y axis
            dc.DrawLine(axisPen, TransformPoint(new Point(halfWidth, minY)), TransformPoint(new Point(halfWidth, maxY)));

        }
        protected void DrawGridCustomInfinityRender(DrawingContext dc)
        {
            double width = ActualWidth;
            double height = ActualHeight;

            // Центр екрана з урахуванням зміщення
            double halfWidth = width / 2 + XOffset;
            double halfHeight = height / 2 - YOffset;

            Pen gridPen = new Pen(Brushes.Black, 0.1);
            Pen axisPen = new Pen(Brushes.Black, 2);

            if (UnitSize <= 0 || GridFrequency <= 0)
            {
                return;
            }

            var calculatedFrequency = GetOptimalGridFrequency();
            double step = UnitSize * calculatedFrequency;

            // Межі видимої області в координатах сітки

            double startX = -halfWidth / UnitSize;
            double endX = (width - halfWidth) / UnitSize;
            double startY = (halfHeight - height) / UnitSize;
            double endY = halfHeight / UnitSize;

            double gridStartX = Math.Floor(startX / calculatedFrequency) * calculatedFrequency;
            double gridEndX = Math.Ceiling(endX / calculatedFrequency) * calculatedFrequency;
            double gridStartY = Math.Floor(startY / calculatedFrequency) * calculatedFrequency;
            double gridEndY = Math.Ceiling(endY / calculatedFrequency) * calculatedFrequency;

            if (AllowInfinityRender)
            {
                var corners = new[]
                    {
                        ReverseTransformPoint(new Point(0, 0)),
                        ReverseTransformPoint(new Point(width, 0)),
                        ReverseTransformPoint(new Point(0, height)),
                        ReverseTransformPoint(new Point(width, height)),
                    };

                // Calculate bounds of the transformed canvas
                var minX = 0;
                var maxX = corners.Max(c => c.X) / UnitSize;
                var minY = 0;
                var maxY = corners.Max(c => c.Y) / UnitSize;

                gridStartX = Math.Floor(minX / calculatedFrequency) * calculatedFrequency;
                gridEndX = Math.Ceiling(maxX / calculatedFrequency) * calculatedFrequency;
                gridStartY = Math.Floor(minY / calculatedFrequency) * calculatedFrequency;
                gridEndY = Math.Ceiling(maxY / calculatedFrequency) * calculatedFrequency;
            }
            
            // Малювання вертикальних ліній
            for (double x = gridStartX; x <= gridEndX; x += calculatedFrequency)
            {
                double screenX = halfWidth + x * UnitSize;
                var p1 = new Point(screenX, 0);
                var p2 = new Point(screenX, height);
                dc.DrawLine(gridPen, TransformPoint(p1), TransformPoint(p2));

                // Вивід міток для осі X
                DrawCoordinateLabel(dc, Math.Round(x, 3), TransformPoint(new Point(screenX, halfHeight)), 15, false);
            }

            // Малювання горизонтальних ліній
            for (double y = gridStartY; y <= gridEndY; y += calculatedFrequency)
            {
                double screenY = halfHeight - y * UnitSize; // Інверсія Y
                var p1 = new Point(0, screenY);
                var p2 = new Point(width, screenY);
                dc.DrawLine(gridPen, TransformPoint(p1), TransformPoint(p2));

                // Вивід міток для осі Y
                DrawCoordinateLabel(dc, Math.Round(y, 3), TransformPoint(new Point(halfWidth, screenY)), 15, true);
            }

            // Малювання осей
            // X-axis

            var xAxisStart = TransformPoint(new Point(0, halfHeight));
            var xAxisEnd = TransformPoint(new Point(width, halfHeight));
            dc.DrawLine(axisPen, xAxisStart, xAxisEnd);

            // Y-axis
            var yAxisStart = TransformPoint(new Point(halfWidth, 0));
            var yAxisEnd = TransformPoint(new Point(halfWidth, height));
            dc.DrawLine(axisPen, yAxisStart, yAxisEnd);

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

        public event EventHandler<CanvasSelectionChangedEventArgs> SelectedElementsChanged;

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


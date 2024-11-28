using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace Modeling_Canvas.UIELements
{
    public abstract class CustomElement : FrameworkElement
    {
        public Brush Fill { get; set; } = null; // Default fill color
        public Brush Stroke { get; set; } = Brushes.Black; // Default stroke color
        public double StrokeThickness { get; set; } = 1; // Default stroke thickness
        public Pen StrokePen { get => new Pen(Stroke, StrokeThickness); }
        public Visibility ShowControls { get => Canvas.SelectedElements.Contains(this) ? Visibility.Visible : Visibility.Hidden; }
        public Visibility AnchorVisibility { get; set; } = Visibility.Hidden;
        public bool HasAnchorPoint { get; set; } = true;

        private bool overrideAnchorPoint = false;
        public bool OverrideAnchorPoint
        {
            get => HasAnchorPoint ? overrideAnchorPoint : false;
            set
            {
                if (HasAnchorPoint && !value)
                {
                    AnchorPoint.Position = GetAnchorDefaultPosition();
                }
                overrideAnchorPoint = value;
            }
        }
        private DraggablePoint anchorPoint;
        public DraggablePoint AnchorPoint
        {
            get => HasAnchorPoint ? anchorPoint : null;
            set => anchorPoint = value;
        }
        public bool IsSelectable { get; set; } = true;
        public CustomCanvas Canvas { get; set; }
        public double UnitSize { get => Canvas.UnitSize; }

        protected Point _lastMousePosition;
        public bool AllowSnapping { get; set; } = true;
        protected virtual bool SnappingEnabled { get => AllowSnapping ? InputManager.ShiftPressed : false; }

        protected double _lastRotationDegrees = 0;
        protected bool _isDragging = false;
        protected bool _isRotating = false;

        protected CustomElement(CustomCanvas canvas, bool hasAnchorPoint = true)
        {
            Canvas = canvas;
            Canvas.KeyDown += OnCanvasKeyDown;
            Focusable = false;
            FocusVisualStyle = null;
            HasAnchorPoint = hasAnchorPoint;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (HasAnchorPoint)
            {
                if (!OverrideAnchorPoint)
                {
                    AnchorPoint.Position = GetAnchorDefaultPosition();
                }
                AnchorPoint.Visibility = AnchorVisibility;
            }
            base.OnRender(drawingContext);

        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitControls();
        }

        protected virtual void InitControls()
        {
            if (HasAnchorPoint)
            {
                AnchorPoint = new DraggablePoint(Canvas)
                {
                    Radius = 10,
                    HasAnchorPoint = false,
                    Focusable = false,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Green,
                    StrokeThickness = 2,
                    Shape = PointShape.Anchor,
                    MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                    MoveAction = OnAnchorPointMove,
                    OverrideToStringAction = (e) =>
                    {
                        return $"Anchor point\nX: {e.Position.X}\nY: {e.Position.Y}";
                    }
                };
                AnchorPoint.Position = GetAnchorDefaultPosition();
                Canvas.Children.Add(AnchorPoint);
                Panel.SetZIndex(AnchorPoint, Panel.GetZIndex(this) + 1);
            }
        }
        public virtual Point GetOriginPoint(Size arrangedSize)
        {
            return new Point(
                arrangedSize.Width / 2,
                arrangedSize.Height / 2
                );
        }
        protected virtual Point GetAnchorDefaultPosition()
        {
            return new Point(0, 0);
        }
        public virtual Point GetTopLeftPosition()
        {
            return new Point(0, 0);
        }
        public virtual Point GetBottomRightPosition()
        {
            return new Point(0, 0);
        }
        protected virtual void OnAnchorPointMove(DraggablePoint point, Vector offset)
        {
            OverrideAnchorPoint = true;
        }
        protected virtual void OnPointMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected virtual void RenderControlPanel()
        {
            ClearControlPanel();
            if (HasAnchorPoint)
            {
                AddAnchorControls();
            }
        }
        private void AddAnchorControls()
        {
            // Create and add StackPanel for X position
            var xPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };

            var label = new TextBlock { Text = "Anchor Point", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            AddElementToControlPanel(label);
            var xLabel = new TextBlock { Text = "Position X:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) };
            var xInput = new TextBox
            {
                Text = AnchorPoint.Position.X.ToString(),
                Width = 100
            };
            xInput.PreviewTextInput += Helpers.NumberValidationTextBox;
            xInput.TextChanged += (sender, e) =>
            {
                if (double.TryParse(xInput.Text, out double newX))
                {
                    OverrideAnchorPoint = true;
                    AnchorPoint.Position = new Point(newX, AnchorPoint.Position.Y);
                    InvalidateCanvas();
                }
            };

            xPanel.Children.Add(xLabel);
            xPanel.Children.Add(xInput);
            AddElementToControlPanel(xPanel);

            // Create and add StackPanel for Y position
            var yPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };

            var yLabel = new TextBlock { Text = "Position Y:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) };
            var yInput = new TextBox
            {
                Text = AnchorPoint.Position.Y.ToString(),
                Width = 100
            };
            yInput.PreviewTextInput += Helpers.NumberValidationTextBox;
            yInput.TextChanged += (sender, e) =>
            {
                if (double.TryParse(yInput.Text, out double newY))
                {
                    OverrideAnchorPoint = true;
                    AnchorPoint.Position = new Point(AnchorPoint.Position.X, newY);
                    InvalidateCanvas();
                }
            };

            yPanel.Children.Add(yLabel);
            yPanel.Children.Add(yInput);
            AddElementToControlPanel(yPanel);
        }
        
        protected virtual void AddFillColorControls()
        {

            // 1. Create controls for the Fill property using ColorPicker
            var fillLabel = new TextBlock { Text = "Fill Color:" };
            var fillColorPicker = new ColorPicker
            {
                SelectedColor = Fill is SolidColorBrush solidBrush ? solidBrush.Color : Colors.Transparent,
                //ShowTabHeaders = false,
                AdvancedTabHeader = string.Empty
            };

            fillColorPicker.SelectedColorChanged += (s, e) =>
            {
                Fill = new SolidColorBrush(fillColorPicker.SelectedColor ?? Colors.Transparent); // Update Fill
                InvalidateVisual();
            };
            AddElementToControlPanel(fillLabel);
            AddElementToControlPanel(fillColorPicker);
        }

        protected virtual void AddStrokeColorControls()
        {
            // 2. Create controls for the Stroke property using ColorPicker
            var strokeLabel = new TextBlock { Text = "Stroke Color:" };
            var strokeColorPicker = new ColorPicker
            {
                SelectedColor = Stroke is SolidColorBrush solidBrush2 ? solidBrush2.Color : Colors.Black,
                ShowAvailableColors = false
            };
            strokeColorPicker.SelectedColorChanged += (s, e) =>
            {
                Stroke = new SolidColorBrush(strokeColorPicker.SelectedColor ?? Colors.Black); // Update Stroke
                InvalidateVisual();
            };
            AddElementToControlPanel(strokeLabel);
            AddElementToControlPanel(strokeColorPicker);
        }

        protected virtual void AddStrokeThicknessControls()
        {
            // 3. Create controls for StrokeThickness
            var thicknessLabel = new TextBlock { Text = "Stroke Thickness:" };
            var thicknessSlider = new Slider
            {
                Minimum = 1,
                Maximum = 10,
                Value = StrokeThickness,
                TickFrequency = 0.1,
                IsSnapToTickEnabled = true
            };
            thicknessSlider.ValueChanged += (s, e) =>
            {
                StrokeThickness = e.NewValue; // Update StrokeThickness
                InvalidateVisual();
            };
            AddElementToControlPanel(thicknessLabel);
            AddElementToControlPanel(thicknessSlider);
        }

        protected void AddElementToControlPanel(UIElement control)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.AddControlToStackPanel(control);
            }
        }

        protected void ClearControlPanel()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ClearControlStack();
            }
        }

        protected void OnCanvasKeyDown(object sender, KeyEventArgs e)
        {
            if (!Canvas.SelectedElements.Contains(this)) return;
            if (e.Key == Key.E)
            {
                OverrideAnchorPoint = false;
                InvalidateCanvas();
            }
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
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var window = App.Current.MainWindow as MainWindow;
            if (window != null)
            {
                window.CurrentElementLabel.Content = ToString();
            }
            // Move and scale logic
            if (_isDragging && !InputManager.RightMousePressed)
            {
                Point currentMousePosition = e.GetPosition(Canvas);
                // Scale logic
                if (InputManager.CtrlPressed && HasAnchorPoint)
                {
                    // Calculate the distance change vector relative to the anchor point
                    Point anchorPosition = AnchorPoint.Position;
                    Point lastMousePosition = Canvas.GetCanvasUnitCoordinates(_lastMousePosition);
                    Point mousePosition = Canvas.GetCanvasMousePosition();

                    Vector lastVector = lastMousePosition - anchorPosition;
                    Vector currentVector = mousePosition - anchorPosition;

                    // Calculate the scale factors based on the ratio of the current vector to the last vector
                    double scaleX = currentVector.X / lastVector.X;
                    double scaleY = currentVector.Y / lastVector.Y;
                    double distanceToAnchor = (mousePosition - anchorPosition).Length;
                    double lastDistanceToAnchor = (lastMousePosition - anchorPosition).Length;
                    double scaleFactor = distanceToAnchor / lastDistanceToAnchor;
                    const double minVectorComponent = 0.01; // Minimal allowed value for vector components to avoid instability

                    if (Math.Abs(lastVector.X) < minVectorComponent || Math.Abs(lastVector.Y) < minVectorComponent)
                    {
                        scaleX = 1.1;
                        scaleY = 1.1;
                    }
                    else
                    {
                        scaleX = currentVector.X / lastVector.X;
                        scaleY = currentVector.Y / lastVector.Y;
                    }

                    // Ensure scale factors are valid
                    if (double.IsInfinity(scaleX) || double.IsNaN(scaleX)) scaleX = 1;
                    if (double.IsInfinity(scaleY) || double.IsNaN(scaleY)) scaleY = 1;

                    // Proportional scaling logic
                    if (InputManager.ShiftPressed)
                    {
                        double uniformScale = (Math.Abs(scaleX) + Math.Abs(scaleY)) / 2;
                        scaleX = Math.Sign(scaleX) * uniformScale;
                        scaleY = Math.Sign(scaleY) * uniformScale;
                    }

                    ScaleElement(AnchorPoint.Position, new Vector(scaleX, scaleY), scaleFactor);

                }
                // Move logic
                else
                {
                    Mouse.OverrideCursor = Cursors.SizeAll;
                    Vector offset = currentMousePosition - _lastMousePosition;
                    MoveElement(offset);
                }
                _lastMousePosition = currentMousePosition;
            }
            // Rotate logic
            else if (_isRotating && !InputManager.AnyKeyButShiftPressed && !InputManager.LeftMousePressed)
            {
                var angle = Canvas.GetDegreesBetweenMouseAndPoint(AnchorPoint.Position);
                RotateElement(AnchorPoint.Position, _lastRotationDegrees - angle);
                _lastRotationDegrees = angle;
            }
            else
            {
                Mouse.OverrideCursor = null;
            }
            InvalidateCanvas();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (!InputManager.SpacePressed && IsSelectable)
            {
                Canvas.SelectedElements.Clear();
                Canvas.SelectedElements.Add(this);
                e.Handled = true;
            }
            _isDragging = true;
            _lastMousePosition = e.GetPosition(Canvas);
            RenderControlPanel();
            CaptureMouse();
            InvalidateCanvas();
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            _isDragging = false;
            _isRotating = false;
            ReleaseMouseCapture();
        }
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            if (HasAnchorPoint)
            {
                _isRotating = true;
                _lastRotationDegrees = Canvas.GetDegreesBetweenMouseAndPoint(AnchorPoint.Position);
                //Keyboard.Focus(this);
                CaptureMouse();
            }
        }
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            if (HasAnchorPoint)
            {
                _isRotating = false;
                _isDragging = false;
                ReleaseMouseCapture();
            }
        }

        public virtual void MoveElement(Vector offset)
        {
            if (HasAnchorPoint && OverrideAnchorPoint)
            {
                AnchorPoint.MoveElement(offset);
            }
            else
            {
                AnchorPoint.Position = GetAnchorDefaultPosition();
            }
        }
        public abstract void RotateElement(Point anchorPoint, double degrees);
        public abstract void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor);

        // Helper method to invalidate the parent canvas
        public void InvalidateCanvas()
        {
            // Request the canvas to re-render by invalidating it
            Canvas.InvalidateVisual();
        }

    }
}

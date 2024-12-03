using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Modeling_Canvas.UIELements
{
    public abstract class CustomElement : FrameworkElement, INotifyPropertyChanged
    {
        public Brush Fill { get; set; } = null; // Default fill color
        public Brush Stroke { get; set; } = Brushes.Black; // Default stroke color
        public double StrokeThickness { get; set; } = 1; // Default stroke thickness
        private Pen _strokePen = null;
        public Pen StrokePen {
            get => _strokePen is null ? new Pen(Stroke, StrokeThickness) : _strokePen;
            set => _strokePen = value; 
        }
        public virtual Visibility ShowControls { get => Canvas.SelectedElements.Contains(this) ? Visibility.Visible : Visibility.Hidden; }
        public bool AnchorVisible { get; set; } = true;
        public Visibility AnchorVisibility { 
            get {
                if(AnchorVisible) return ShowControls;
                else return Visibility.Hidden;   
            } 
        }
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

        private DraggablePoint _anchorPoint;
        public DraggablePoint AnchorPoint
        {
            get => HasAnchorPoint ? _anchorPoint : null;
            set
            {
                _anchorPoint = value;
                OnPropertyChanged(nameof(AnchorPoint));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool IsSelectable { get; set; } = true;
        public CustomCanvas Canvas { get; set; }
        public double UnitSize { get => Canvas.UnitSize; }

        protected Point _lastMousePosition;
        public bool AllowSnapping { get; set; } = true;
        protected virtual bool SnappingEnabled { get => AllowSnapping ? InputManager.ShiftPressed : false; }

        public string LabelText { get; set; } = "Default Label";

        protected double _lastRotationDegrees = 0;
        protected bool _isDragging = false;
        protected bool _isRotating = false;

        protected CustomElement(CustomCanvas canvas, bool hasAnchorPoint = true)
        {
            Canvas = canvas;
            Focusable = false;
            FocusVisualStyle = null;
            HasAnchorPoint = hasAnchorPoint;
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (HasAnchorPoint)
            {
                if (!OverrideAnchorPoint)
                {
                    AnchorPoint.Position = GetAnchorDefaultPosition();
                }
                AnchorPoint.Visibility = AnchorVisibility;
            }
            switch (Canvas.RenderMode)
            {
                case RenderMode.Default:
                    DefaultRender(dc);
                    break;
                case RenderMode.Affine:
                    AffineRender(dc);
                    break;
                case RenderMode.Projective:
                    ProjectiveRender(dc);
                    break;
            }

        }
        protected abstract void DefaultRender(DrawingContext dc);

        protected abstract void AffineRender(DrawingContext dc);

        protected abstract void ProjectiveRender(DrawingContext dc);

        protected override void OnInitialized(EventArgs e)
        {
            //base.OnInitialized(e);
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
                    },
                    OverrideRenderControlPanelAction = true
                };
                AnchorPoint.Position = GetAnchorDefaultPosition();
                Canvas.Children.Add(AnchorPoint);
                Panel.SetZIndex(AnchorPoint, Canvas.Children.Count + 1);
            }
        }
        public virtual Point GetOriginPoint(Size arrangedSize)
        {
            return new Point(0, 0);
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

        protected virtual void RenderControlPanelLabel()
        {
            var label = new TextBlock { Text = $"| {LabelText} |", TextAlignment = TextAlignment.Center, FontWeight = FontWeight.FromOpenTypeWeight(600) };
            AddElementToControlPanel(label);
        }

        protected virtual void RenderControlPanel()
        {
            ClearControlPanel(); 
            RenderControlPanelLabel();
            AddRotateControls();
            AddOffsetControls();
            AddScaleControls();
            if (HasAnchorPoint)
            {
                AddAnchorControls();
            }
        }

        protected void AddDefaultPointControls(
            string labelText,
            object source,
            string xPath,
            string yPath,
            Action<double> xValueChanged, 
            Action<double> yValueChanged)
        {
            var panel = GetDefaultVerticalPanel();
            // Create and add StackPanel for X position
            var xPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };

            var label = new TextBlock { Text = labelText, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            var xLabel = new TextBlock { Text = "Position X:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) };
            var xInput = new TextBox
            {
                Width = 100
            };

            var xBinding = new Binding(xPath)
            {
                Source = source,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            xInput.SetBinding(TextBox.TextProperty, xBinding);

            xInput.PreviewKeyUp += (sender, e) =>
            {
                if (double.TryParse(xInput.Text, out double newX))
                {
                    xValueChanged(newX);
                }
            };

            xPanel.Children.Add(xLabel);
            xPanel.Children.Add(xInput);

            // Create and add StackPanel for Y position
            var yPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };

            var yLabel = new TextBlock { Text = "Position Y:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) };
            var yInput = new TextBox
            {
                Width = 100
            };

            var yBinding = new Binding(yPath)
            {
                Source = source,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            yInput.SetBinding(TextBox.TextProperty, yBinding);

            yInput.PreviewKeyUp += (sender, e) =>
            {
                if (double.TryParse(yInput.Text, out double newY))
                {
                    yValueChanged(newY);
                }
            };

            yPanel.Children.Add(yLabel);
            yPanel.Children.Add(yInput);
            panel.Children.Add(label);
            panel.Children.Add(xPanel);
            panel.Children.Add(yPanel);
            AddElementToControlPanel(panel);
        }
        
        protected virtual void AddAnchorControls()
        {
            var panel = GetDefaultHorizontalPanel();
            var isClosedLabel = new TextBlock { Text = "Anchor visible:" };

            var isClosedCheckBox = new CheckBox
            {
                IsChecked = AnchorVisible
            };

            isClosedCheckBox.Checked += (s, e) =>
            {
                AnchorVisible = true;
                InvalidateVisual();
            };
            isClosedCheckBox.Unchecked += (s, e) =>
            {
                AnchorVisible = false;
                InvalidateVisual();
            };

            panel.Children.Add(isClosedLabel);
            panel.Children.Add(isClosedCheckBox);
            AddElementToControlPanel(panel);

            AddDefaultPointControls(
                "Anchor Point",
                AnchorPoint,
                "Position.X",
                "Position.Y",
                (x) =>
                {
                    OverrideAnchorPoint = true;
                    AnchorPoint.Position = new Point(x, AnchorPoint.Position.Y);
                    InvalidateCanvas();
                },
                (y) =>
                {
                    OverrideAnchorPoint = true;
                    AnchorPoint.Position = new Point(AnchorPoint.Position.X, y);
                    InvalidateCanvas();
                }
            );
        }
        
        protected virtual void AddFillColorControls()
        {
            var panel = GetDefaultVerticalPanel();
            var fillLabel = new TextBlock { Text = "Fill Color:", TextAlignment=TextAlignment.Center };
            var fillColorPicker = new ColorPicker
            {
                SelectedColor = Fill is SolidColorBrush solidBrush ? solidBrush.Color : Colors.Transparent,
                AdvancedTabHeader = string.Empty,
                Width = 200
            };

            fillColorPicker.SelectedColorChanged += (s, e) =>
            {
                Fill = new SolidColorBrush(fillColorPicker.SelectedColor ?? Colors.Transparent); // Update Fill
                InvalidateVisual();
            };
            panel.Children.Add(fillLabel);
            panel.Children.Add(fillColorPicker);
            AddElementToControlPanel(panel);
        }

        protected virtual void AddStrokeColorControls()
        {
            var panel = GetDefaultVerticalPanel();
            var strokeLabel = new TextBlock { Text = "Stroke Color:", TextAlignment = TextAlignment.Center };
            var strokeColorPicker = new ColorPicker
            {
                SelectedColor = Stroke is SolidColorBrush solidBrush2 ? solidBrush2.Color : Colors.Black,
                ShowAvailableColors = false,
                Width = 200
            };
            strokeColorPicker.SelectedColorChanged += (s, e) =>
            {
                Stroke = new SolidColorBrush(strokeColorPicker.SelectedColor ?? Colors.Black); // Update Stroke
                InvalidateVisual();
            };
            panel.Children.Add(strokeLabel);
            panel.Children.Add(strokeColorPicker);
            AddElementToControlPanel(panel);
        }

        protected virtual void AddStrokeThicknessControls()
        {
            var panel = GetDefaultVerticalPanel();
            var thicknessLabel = new TextBlock { Text = "Stroke Thickness:", TextAlignment = TextAlignment.Center };
            var thicknessSlider = new Slider
            {
                Minimum = 1,
                Maximum = 10,
                Value = StrokeThickness,
                TickFrequency = 0.1,
                IsSnapToTickEnabled = true,
                Width = 200
            };
            thicknessSlider.ValueChanged += (s, e) =>
            {
                StrokeThickness = e.NewValue; // Update StrokeThickness
                InvalidateVisual();
            };
            panel.Children.Add(thicknessLabel);
            panel.Children.Add(thicknessSlider);
            AddElementToControlPanel(panel);
        }

        protected virtual void AddOffsetControls()
        {
            var panel = GetDefaultVerticalPanel();

            var label = new TextBlock { Text = "Offset", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

            var labelX = new TextBlock { Text = "X: ", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            var labelY = new TextBlock { Text = "Y: ", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

            var inputX = new TextBox
            {
                Width = 100,
                Text = "0"
            };
            var inputY = new TextBox
            {
                Width = 100,
                Text = "0"
            };

            var offsetButton = new Button
            {
                Content = "Offset",
                Width = 100,
                Height = 20,
                Margin = new Thickness(5)
            };

            offsetButton.Click += (s, e) =>
            {
                if (double.TryParse(inputX.Text, out double X) && double.TryParse(inputY.Text, out double Y))
                {
                    MoveElement(new Vector(X * UnitSize, -Y * UnitSize));
                    InvalidateCanvas();
                }
            };

            var panelX = GetDefaultHorizontalPanel(5);
            var panelY = GetDefaultHorizontalPanel(5);

            panelX.Children.Add(labelX);
            panelX.Children.Add(inputX);

            panelY.Children.Add(labelY);
            panelY.Children.Add(inputY);

            panel.Children.Add(label);
            panel.Children.Add(panelX);
            panel.Children.Add(panelY);
            panel.Children.Add(offsetButton);
            AddElementToControlPanel(panel);
        }
        protected virtual void AddRotateControls()
        {
            var panel = GetDefaultVerticalPanel();

            var label = new TextBlock { Text = "Rotating", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            var input = new TextBox
            {
                Width = 100
            };

            var rotateButton = new Button
            {
                Content = "Rotate",
                Width = 100,
                Height = 20,
                Margin = new Thickness(5)
            };

            rotateButton.Click += (s, e) =>
            {
                if(double.TryParse(input.Text, out double value))
                {
                    RotateElement(AnchorPoint.Position, -value);
                    InvalidateCanvas();
                }
            };

            panel.Children.Add(label);
            panel.Children.Add(input);
            panel.Children.Add(rotateButton);
            AddElementToControlPanel(panel);
        }
        protected virtual void AddScaleControls()
        {
            var panel = GetDefaultVerticalPanel();

            var label = new TextBlock { Text = "Scale", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

            var labelX = new TextBlock { Text = "X: ", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            var labelY = new TextBlock { Text = "Y: ", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

            var inputX = new TextBox
            {
                Width = 100,
                Text = "1"
            };
            var inputY = new TextBox
            {
                Width = 100,
                Text = "1"
            };

            var offsetButton = new Button
            {
                Content = "Scale",
                Width = 100,
                Height = 20,
                Margin = new Thickness(5)
            };

            offsetButton.Click += (s, e) =>
            {
                if (double.TryParse(inputX.Text, out double X) && double.TryParse(inputY.Text, out double Y))
                {
                    var factor = Math.Abs(X + Y) / 2;
                    ScaleElement(AnchorPoint.Position, new Vector(X, Y), factor);
                    InvalidateCanvas();
                }
            };

            var panelX = GetDefaultHorizontalPanel(5);
            var panelY = GetDefaultHorizontalPanel(5);

            panelX.Children.Add(labelX);
            panelX.Children.Add(inputX);

            panelY.Children.Add(labelY);
            panelY.Children.Add(inputY);

            panel.Children.Add(label);
            panel.Children.Add(panelX);
            panel.Children.Add(panelY);
            panel.Children.Add(offsetButton);
            AddElementToControlPanel(panel);
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

        protected StackPanel GetDefaultVerticalPanel(double marginBottom = 30)
        {
            return new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5, 5, 5, marginBottom), HorizontalAlignment = HorizontalAlignment.Center };
        }
        protected StackPanel GetDefaultHorizontalPanel(double marginBottom = 30)
        {
            return new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5, 5, 5, marginBottom), HorizontalAlignment = HorizontalAlignment.Center };
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

        private bool _lastAnchorState = false;
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
                    if(Canvas.RenderMode is RenderMode.Affine)
                    {
                        offset = currentMousePosition.ReverseAffineTransformation(Canvas.AffineParams) - _lastMousePosition.ReverseAffineTransformation(Canvas.AffineParams);
                    } else if(Canvas.RenderMode is RenderMode.Projective)
                    {
                        offset = currentMousePosition.ReverseProjectiveTransformation(Canvas.ProjectiveParams) - _lastMousePosition.ReverseProjectiveTransformation(Canvas.ProjectiveParams);
                    }
                    MoveElement(offset);

                    //foreach (var element in Canvas.SelectedElements.Where(e => !e.Equals(this)))
                    //{
                    //    element.MoveElement(offset);
                    //}
                }
                _lastMousePosition = currentMousePosition;
            }
            // Rotate logic
            else if (_isRotating && !InputManager.AnyKeyButShiftPressed && !InputManager.LeftMousePressed)
            {
                OverrideAnchorPoint = true;
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

        protected virtual void OnElementSelected(MouseButtonEventArgs e)
        {
            if (!InputManager.ShiftPressed && !Canvas.SelectedElements.Contains(this))
            {
                Canvas.SelectedElements.Clear();
            }
            Canvas.SelectedElements.Add(this);
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!InputManager.SpacePressed && IsSelectable)
            {
                OnElementSelected(e);
            }
            _isDragging = true;
            _lastMousePosition = e.GetPosition(Canvas);
            CaptureMouse();
            InvalidateCanvas();
            RenderControlPanel();
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
        public virtual void InvalidateCanvas()
        {
            // Request the canvas to re-render by invalidating it
            Canvas.InvalidateVisual();
            InvalidateVisual();
        }

    }
}

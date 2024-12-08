using Modeling_Canvas.Enums;
using Modeling_Canvas.UIElements.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public abstract partial class CustomElement : FrameworkElement, INotifyPropertyChanged, IMovableElement
    {
        public CustomElement? LogicalParent { get; set; } = null;

        private Brush _fill = null;
        public Brush Fill {
            get => _fill;
            set
            {
                if (_fill != value)
                {
                    _fill = value;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        private Brush _stroke = Brushes.Black;

        public Brush Stroke
        {
            get => _stroke;
            set
            {
                if (_stroke != value)
                {
                    _stroke = value;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public double MinStrokeThickness { get; set; } = 0.1;
        public double MaxStrokeThickness { get; set; } = 25;
        
        private double _strokeThickness = 1;
        public double StrokeThickness { 
            get => _strokeThickness;
            set
            {
                if (_strokeThickness != value)
                {
                    _strokeThickness = value;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        private Pen _strokePen = null;
        public Pen StrokePen
        {
            get => _strokePen is null ? new Pen(Stroke, StrokeThickness) : _strokePen;
            set => _strokePen = value;
        }
        public bool ControlsVisible { get; set; } = false;
        public virtual Visibility ControlsVisibility { get => Canvas.SelectedElements.Contains(this) || ControlsVisible ? Visibility.Visible : Visibility.Hidden; }

        private bool _anchorVisible = true;
        public bool AnchorVisible {
            get => _anchorVisible;
            set
            {
                if (_anchorVisible != value)
                {
                    _anchorVisible = value;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public Visibility AnchorVisibility
        {
            get
            {
                if (AnchorVisible) return ControlsVisibility;
                else return Visibility.Hidden;
            }
        }
        public bool HasAnchorPoint { get; } = true;

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
            private set
            {
                _anchorPoint = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool IsSelectable { get; set; } = true;
        public bool IsInteractable { get; set; } = true;
        public CustomCanvas Canvas { get; set; }
        public double UnitSize { get => Canvas.UnitSize; }

        protected Point _lastMousePosition;
        public bool AllowSnapping { get; set; } = true;
        protected virtual bool SnappingEnabled { get => AllowSnapping ? InputManager.ShiftPressed : false; }

        public string LabelText { get; set; } = "Default Label";

        protected double _lastRotationDegrees = 0;

        protected bool _isDragging = false;

        protected bool _isRotating = false;

        protected Dictionary<string, FrameworkElement> _controls = new();

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
            DefaultRender(dc);

        }
        protected abstract void DefaultRender(DrawingContext dc);

        protected override void OnInitialized(EventArgs e)
        {
            InitChildren();
            InitControlPanel();
        }
        
        protected virtual void InitChildren()
        {
            if (HasAnchorPoint)
            {
                AnchorPoint = new DraggablePoint(Canvas, false)
                {
                    Radius = 10,
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
        
        protected virtual void InitControlPanel()
        {
            //RenderControlPanelLabel();
            AddOffsetControls();
            AddRotateControls();
            AddScaleControls();
            if (HasAnchorPoint)
            {
                AddAnchorControls();
            }
        }
        protected virtual void RenderControlPanelLabel()
        {
            var label = new TextBlock { Text = $"| {LabelText} |", TextAlignment = TextAlignment.Center, FontWeight = FontWeight.FromOpenTypeWeight(600) };
            AddElementToControlPanel(label);
        }

        protected virtual void RenderControlPanel()
        {
            ClearControlPanel();
            foreach (var control in _controls)
            {
                AddElementToControlPanel(control.Value);
            }
        }

        protected virtual void AddChildren(CustomElement element)
        {
            element.LogicalParent = this;
            Canvas.Children.Add(element);
            Panel.SetZIndex(element, Canvas.Children.Count + 1);
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
            var window = App.Current.MainWindow as MainWindow;
            if (window != null)
            {
                window.CurrentElementLabel.Content = ToString();
            }
            if (!IsInteractable) return;
            // Move and scale logic
            if (_isDragging && !InputManager.RightMousePressed)
            {
                Point currentMousePosition = e.GetPosition(Canvas);
                // Scale logic
                if (InputManager.CtrlPressed && HasAnchorPoint)
                {
                    // Calculate the distance change vector relative to the anchor point
                    Point anchorPosition = AnchorPoint.Position;
                    Point lastMousePosition = Canvas.GetTransformedUnitCoordinates(_lastMousePosition);
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
                    offset = Canvas.ReverseTransformPoint(currentMousePosition) - Canvas.ReverseTransformPoint(_lastMousePosition);
                    MoveElement(offset);

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
            if (!InputManager.ShiftPressed && !Canvas.SelectedElements.Contains(this) && !Canvas.SelectedElements.Contains(LogicalParent))
            {
                Canvas.SelectedElements.Clear();
            }
            Canvas.SelectedElements.Add(this);
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!IsInteractable) return;
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
            if (!IsInteractable) return;
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
            else if(HasAnchorPoint) 
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

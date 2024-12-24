using Modeling_Canvas.Enums;
using Modeling_Canvas.Models;
using Modeling_Canvas.UIElements.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public abstract partial class Element : FrameworkElement, INotifyPropertyChanged, IMovableElement
    {
        private Brush _fill = null;

        private double _strokeThickness = 1;

        private Brush _stroke = Brushes.Black;

        private Pen _strokePen = null;

        private bool _overrideAnchorPoint = false;

        private bool _anchorVisible = true;

        private Visibility _controlsVisibility = Visibility.Hidden;

        protected Point _lastMousePosition;

        protected double _lastRotationDegrees = 0;

        protected bool _isDragging = false;

        protected bool _isRotating = false;

        protected Dictionary<string, FrameworkElement> _uiControls = new();

        private DraggablePoint _anchorPoint;

        public double MinStrokeThickness { get; set; } = 0.1;

        public double MaxStrokeThickness { get; set; } = 25;

        public bool HasAnchorPoint { get; } = true;

        public bool IsSelectable { get; set; } = true;

        public bool IsInteractable { get; set; } = true;

        public double UnitSize { get => Canvas.UnitSize; }

        public bool AllowSnapping { get; set; } = true;

        public string LabelText { get; set; } = "Default Label";

        public bool ControlsVisible { get; set; } = true;

        public double DragRadius { get; set; } = 10;

        public Pen DragzonePen { get; set; }

        public Element? LogicalParent { get; set; } = null;

        public CustomCanvas Canvas { get; set; }

        public Action<Element> AfterMoveAction;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SnappingEnabled { get => AllowSnapping ? InputManager.ShiftPressed : false; }

        public Brush Fill
        {
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

        public double StrokeThickness
        {
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

        public Pen StrokePen
        {
            get => _strokePen is null ? new Pen(Stroke, StrokeThickness) : _strokePen;
            set => _strokePen = value;
        }

        public virtual Visibility ControlsVisibility
        {
            get => ControlsVisible ? _controlsVisibility : Visibility.Hidden;
            set
            {
                if (_controlsVisibility != value)
                {
                    _controlsVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AnchorVisible
        {
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

        public bool OverrideAnchorPoint
        {
            get => HasAnchorPoint ? _overrideAnchorPoint : false;
            set
            {
                if (HasAnchorPoint && !value)
                {
                    AnchorPoint.Position = GetAnchorDefaultPosition();
                }
                _overrideAnchorPoint = value;
            }
        }

        public DraggablePoint AnchorPoint
        {
            get => HasAnchorPoint ? _anchorPoint : null;
            private set
            {
                if (HasAnchorPoint)
                {
                    _anchorPoint = value;
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected Element(CustomCanvas canvas, bool hasAnchorPoint = true)
        {
            Canvas = canvas;
            Focusable = false;
            FocusVisualStyle = null;
            HasAnchorPoint = hasAnchorPoint;
            DragzonePen = new Pen(Brushes.Transparent, DragRadius);
            Canvas.SelectedElementsChanged += (s, e) =>
            {
                if (e.SelectedElement is not null
                && Canvas.SelectedElements.Contains(this)
                || Canvas.SelectedElements.Contains(this.LogicalParent)
                )
                {
                    ControlsVisibility = Visibility.Visible;
                }
                else
                {
                    ControlsVisibility = Visibility.Hidden;
                }
            };

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

            var geometry = GetElementGeometry();

            TransformGroup transformGroup = new TransformGroup();

            transformGroup.Children.Add(new ScaleTransform(UnitSize, UnitSize));

            transformGroup.Children.Add(new TranslateTransform(Canvas.XOffset, Canvas.YOffset));

            transformGroup.Children.Add(new TranslateTransform(Canvas.ActualWidth / 2, Canvas.ActualHeight / 2));
            if(Canvas.RenderMode is RenderMode.Affine)
            {
                var affine = Canvas.AffineParams;

                var matrix = new Matrix(
                    affine.Xx, affine.Xy,  // Scale and skew
                    affine.Yx, affine.Yy,  // Skew and scale
                    affine.Ox, affine.Oy   // Translation
                );

                transformGroup.Children.Add(new TranslateTransform(-Canvas.ActualWidth / 2, -Canvas.ActualHeight / 2));

                transformGroup.Children.Add(new MatrixTransform(matrix));

                transformGroup.Children.Add(new TranslateTransform(Canvas.ActualWidth / 2, Canvas.ActualHeight / 2));
            }            
            // Apply the transformations to the geometry
            geometry.Transform = transformGroup;
            
            // Draw the transformed geometry
            dc.DrawGeometry(Fill, StrokePen, geometry);
            dc.DrawGeometry(null, DragzonePen, geometry);
        }

        private Point TransformPoint(Point point, ProjectiveModel projective)
        {
            double xx = projective.Xx * projective.wX;
            double xy = projective.Xy * projective.wX;
            double yy = projective.Yy * projective.wY;
            double yx = projective.Yx * projective.wY;
            double ox = projective.Ox * Canvas.UnitSize * projective.wO;
            double oy = projective.Oy * Canvas.UnitSize * projective.wO;
            double wx = projective.wX;
            double wy = projective.wY;
            double wo = projective.wO;

            double x = point.X;
            double y = point.Y;

            // Calculate denominator
            double w = x * wx + y * wy + wo;
            if (w == 0)
                return new Point(0, 0);

            // Transform coordinates
            double tx = (x * xx + y * yx + ox) / w;
            double ty = (x * xy + y * yy + oy) / w;

            return new Point(tx, ty);
        }


        protected abstract StreamGeometry GetElementGeometry();

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
                    Radius = 0.2,
                    Focusable = false,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Green,
                    StrokeThickness = 2,
                    Shape = PointShape.Anchor,
                    MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                    AfterMoveAction = OnAnchorPointMove,
                    OverrideToStringAction = (e) =>
                    {
                        return $"Anchor point\nX: {e.Position.X}\nY: {e.Position.Y}";
                    },
                    IsSelectable = false
                };
                AnchorPoint.Position = GetAnchorDefaultPosition();
                AddChildren(AnchorPoint);
            }
        }

        protected virtual void InitControlPanel()
        {
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
            foreach (var control in _uiControls)
            {
                AddElementToControlPanel(control.Value);
            }
        }

        protected virtual void AddChildren(Element element, int index = 999)
        {
            element.LogicalParent = this;
            Canvas.Children.Add(element);
            Panel.SetZIndex(element, index);
        }

        public virtual Point GetOriginPoint(Size arrangedSize) => new Point(0, 0);

        protected virtual Point GetAnchorDefaultPosition() => new Point(0, 0);

        protected virtual void OnAnchorPointMove(Element element)
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

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!IsInteractable) return;
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
                    AfterMoveAction?.Invoke(this);

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
                return;
            }
            InvalidateCanvas();
        }

        protected virtual void OnElementSelected(MouseButtonEventArgs e)
        {
            RenderControlPanel();
            if (
                !InputManager.ShiftPressed &&
                !Canvas.SelectedElements.Contains(this) &&
                !Canvas.SelectedElements.Contains(LogicalParent))
            {
                Canvas.ClearSelection();
            }
            Canvas.SelectElement(this);
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
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            _isDragging = false;
            _isRotating = false;
            ReleaseMouseCapture();
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (!IsInteractable) return;
            if (HasAnchorPoint)
            {
                _isRotating = true;
                _lastRotationDegrees = Canvas.GetDegreesBetweenMouseAndPoint(AnchorPoint.Position);
                CaptureMouse();
            }
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
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
            else if (HasAnchorPoint)
            {
                var a = this;
                AnchorPoint.Position = GetAnchorDefaultPosition();
            }
        }

        public abstract void RotateElement(Point anchorPoint, double degrees);

        public abstract void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor);

        // Helper method to invalidate the parent canvas
        public virtual void InvalidateCanvas()
        {
            Canvas.InvalidateVisual();
            InvalidateVisual();
        }

    }
}

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
    public abstract partial class Element : FrameworkElement, INotifyPropertyChanged
    {
        private FigureStyle _figureStyle;

        private bool _overrideAnchorPoint = false;

        private bool _anchorVisible = true;

        private Visibility _controlsVisibility = Visibility.Hidden;

        protected Point _lastMousePosition;

        protected double _lastRotationDegrees = 0;

        protected bool _isDragging = false;

        protected bool _isRotating = false;

        protected Dictionary<string, FrameworkElement> _uiControls = new();

        protected bool _transformGeometry = true;

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

        public FigureStyle Style
        {
            get => _figureStyle;
            set
            {
                if (_figureStyle != value)
                {
                    if (_figureStyle is not null)
                        _figureStyle.PropertyChanged -= FigureStyleChanged;

                    _figureStyle = value;

                    if (_figureStyle is not null)
                        _figureStyle.PropertyChanged += FigureStyleChanged;

                    OnPropertyChanged(nameof(Style));
                }
            }
        }

        private void FigureStyleChanged(object sender, PropertyChangedEventArgs e)
        {
            InvalidateCanvas(); // Refresh canvas when style changes
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
            Style = new();
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

            var geometryData = GetElementGeometry();

            foreach (var figure in geometryData)
            {
                var figurePoints = figure.Item2;
                var figureStyle = figure.Item1 ?? Style;
                var geometry = new StreamGeometry();
                if (_transformGeometry)
                {
                    var offsetToCenter = Canvas.RenderMode is not RenderMode.ProjectiveV2 && Canvas.RenderMode is not RenderMode.Projective;

                    using (var context = geometry.Open())
                    {
                        if (figurePoints is null || figurePoints.Length < 1)
                            continue;
                        // Begin a new figure at the first point
                        context.BeginFigure(TransformPoint(figurePoints[0], offsetToCenter), figureStyle.FillColor != null, false);
                        for (var i = 1; i < figurePoints.Length; i++)
                        {
                            context.LineTo(TransformPoint(figurePoints[i], offsetToCenter), isStroked: true, isSmoothJoin: true);
                        }
                    }
                }
                else
                {
                    using (var context = geometry.Open())
                    {
                        if (figurePoints is null || figurePoints.Length < 1)
                            continue;
                        context.BeginFigure(figurePoints[0], figureStyle.FillColor != null, false);
                        for (var i = 1; i < figurePoints.Length; i++)
                        {
                            context.LineTo(figurePoints[i], isStroked: true, isSmoothJoin: false);
                        }
                    }
                }
                dc.DrawGeometry(figureStyle.FillColor, figureStyle.StrokePen, geometry);
                dc.DrawGeometry(null, DragzonePen, geometry);
            }

        }

        protected Point TransformPoint(Point p, bool? offsetToCenter = null)
        {
            if (offsetToCenter == null)
                offsetToCenter = Canvas.RenderMode is not RenderMode.ProjectiveV2 && Canvas.RenderMode is not RenderMode.Projective;
            var debug = false;
            var canvasP = new Point(p.X * UnitSize, p.Y * UnitSize);
            if (offsetToCenter.Value)
                canvasP = new Point(canvasP.X + Canvas.XOffset + Canvas.ActualWidth / 2, canvasP.Y + Canvas.YOffset + Canvas.ActualHeight / 2);
            if (Canvas.RenderMode is RenderMode.Affine)
                canvasP = canvasP.ApplyAffineTransformation(Canvas.AffineParams);
            if (Canvas.RenderMode is RenderMode.Projective)
                canvasP = canvasP.ApplyProjectiveTransformation(Canvas.ProjectiveParams);
            if (Canvas.RenderMode is RenderMode.ProjectiveV2)
                canvasP = canvasP.ApplyProjectiveV2Transformation(Canvas.ProjectiveParams);
            return canvasP;
        }

        protected abstract List<(FigureStyle, Point[])> GetElementGeometry();

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
                    PixelRadius = 10,
                    Focusable = false,
                    Style = new()
                    {
                        FillColor = Brushes.Transparent,
                        StrokeColor = Brushes.Green,
                        StrokeThickness = 2,
                    },
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
            if (HasAnchorPoint)
            {
                AddAnchorControls();
                AddRotateControls();
                AddScaleControls();
            }
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
                RotateElement(AnchorPoint.Position, angle - _lastRotationDegrees);
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

﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIELements
{
    public abstract class CustomElement : FrameworkElement
    {
        public Brush Fill { get; set; } = Brushes.Transparent; // Default fill color
        public Brush Stroke { get; set; } = Brushes.Black; // Default stroke color
        public double StrokeThickness { get; set; } = 1; // Default stroke thickness

        public Visibility ShowControls { get => Canvas.SelectedElements.Contains(this) ? Visibility.Visible : Visibility.Hidden; }
        public Visibility AnchorVisibility { get; set; } = Visibility.Hidden;
        public bool HasAnchorPoint { get; set; } = true;

        private bool overrideAnchorPoint = false;
        public bool OverrideAnchorPoint
        {
            get => HasAnchorPoint ? overrideAnchorPoint : false;
            set {
                if (!value)
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

        public CustomCanvas Canvas { get; set; }
        public double UnitSize { get => Canvas.UnitSize; }

        protected Point _lastMousePosition;
        public bool AllowSnapping { get; set; } = true;
        protected virtual bool SnappingEnabled { get => AllowSnapping ? InputManager.ShiftPressed : false; }

        protected double _lastRotationDegrees { get; set; } = 0;

        protected bool _isDragging = false;
        protected bool _isRotating = false;

        protected CustomElement(CustomCanvas canvas, bool hasAnchorPoint = true)
        {
            Canvas = canvas;
            Focusable = true;
            FocusVisualStyle = null;
            HasAnchorPoint = hasAnchorPoint;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (HasAnchorPoint)
            {
                if(!OverrideAnchorPoint)
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
                    OverrideToStringAction = (e) => {
                        return $"Anchor point\nX: {e.Position.X}\nY: {e.Position.Y}";
                    }
                };
                AnchorPoint.Position = GetAnchorDefaultPosition();
                Canvas.Children.Add(AnchorPoint);
                Panel.SetZIndex(AnchorPoint, Panel.GetZIndex(this) + 1);
            }
        }

        protected virtual Point GetAnchorDefaultPosition()
        {
            return new Point(0,0);
        }

        protected virtual void OnAnchorPointMove(DraggablePoint point, Vector offset)
        {
            OverrideAnchorPoint = true;
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            if (HasAnchorPoint)
            {
                _isRotating = true;
                _lastRotationDegrees = Canvas.GetDegreesBetweenMouseAndPoint(AnchorPoint.Position);
                Keyboard.Focus(this);
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

        protected virtual void OnPointMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        public virtual Point GetOriginPoint(Size arrangedSize)
        {
            return new Point(
                arrangedSize.Width/2, 
                arrangedSize.Height/2
                );
        }

        protected virtual void RenderControlPanel()
        {
            ClearControlPanel();
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.E)
            {
                OverrideAnchorPoint = false;
                InvalidateCanvas();
            }
            base.OnKeyDown(e);
        }

        #region logic for dragging elements
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
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            _isDragging = true;
            _lastMousePosition = e.GetPosition(Canvas);
            Keyboard.Focus(this);
            CaptureMouse();
            RenderControlPanel();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var window = App.Current.MainWindow as MainWindow;
            if (window != null)
            {
                window.CurrentElementLabel.Content = ToString();
            }
            if (_isDragging && !InputManager.RightMousePressed)
            {
                Point currentMousePosition = e.GetPosition(Canvas);
                // scale logic
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
                // move logic
                else
                {
                    Vector offset = currentMousePosition - _lastMousePosition;
                    MoveElement(offset);
                }
                _lastMousePosition = currentMousePosition;
            }
            // rotate logic
            else if (_isRotating && !InputManager.AnyKeyButShiftPressed && !InputManager.LeftMousePressed)
            {
                var angle = Canvas.GetDegreesBetweenMouseAndPoint(AnchorPoint.Position);
                RotateElement(AnchorPoint.Position, _lastRotationDegrees - angle);
                _lastRotationDegrees = angle;
            }
            InvalidateCanvas();
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            _isDragging = false;
            _isRotating = false;
            ReleaseMouseCapture();
        }

        public virtual void MoveElement(Vector offset)
        {
            if (HasAnchorPoint && OverrideAnchorPoint)
            {
                AnchorPoint.MoveElement(offset);
            } else
            {
                AnchorPoint.Position = GetAnchorDefaultPosition();
            }
        }
        public abstract void RotateElement(Point anchorPoint, double degrees);
        public abstract void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor);

        #endregion

        // Helper method to invalidate the parent canvas
        public void InvalidateCanvas()
        {
            // Request the canvas to re-render by invalidating it
            Canvas.InvalidateVisual();
        }

        protected double SnapValue(double value)
        {
            const double lowerThreshold = 0.1;
            const double upperThreshold = 0.9;
            const double toHalfLower = 0.45;
            const double toHalfUpper = 0.55;

            double integerPart = Math.Floor(value);
            double fractionalPart = value - integerPart;

            if (fractionalPart <= lowerThreshold) return integerPart;
            if (fractionalPart >= upperThreshold) return integerPart + 1;
            if (fractionalPart >= toHalfLower && fractionalPart <= toHalfUpper) return integerPart + 0.5;
            return value;
        }

        protected Point RotatePoint(Point point1, Point point2, double degrees)
        {
            // Calculate rotation
            double dx = point1.X - point2.X;
            double dy = point1.Y - point2.Y;
            double radians = DegToRad(degrees);

            double rotatedX = Math.Cos(radians) * dx - Math.Sin(radians) * dy + point2.X;
            double rotatedY = Math.Sin(radians) * dx + Math.Cos(radians) * dy + point2.Y;

            // Update point position
            return new Point(Math.Round(rotatedX, Canvas.RotationPrecision), Math.Round(rotatedY, Canvas.RotationPrecision));
        }

        protected Point OffsetPoint(Point point, Vector offset)
        {
            return new Point(
                point.X + offset.X / UnitSize,
                point.Y - offset.Y / UnitSize
                );
        }

        protected Point OffsetAndSpanPoint(Point point, Vector offset)
        {
            return new Point(
                SnapValue(point.X + offset.X / UnitSize),
                SnapValue(point.Y - offset.Y / UnitSize)
            );
        }

        protected Point ScalePoint(Point point, Point anchor, Vector scaleVector)
        {
            // Apply scaling transformation
            double newX = anchor.X + scaleVector.X * (point.X - anchor.X);
            double newY = anchor.Y + scaleVector.Y * (point.Y - anchor.Y);
            
            // Update the point in the list
            return new Point(newX, newY);
        }

        protected double DegToRad(double deg) => Math.PI * deg / 180.0;
        protected double NormalizeAngle(double angle) => (angle % 360 + 360) % 360;

    }
}

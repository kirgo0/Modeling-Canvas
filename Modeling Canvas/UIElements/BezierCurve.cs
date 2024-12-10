using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using Modeling_Canvas.Models;
using Modeling_Canvas.UIElements.Abstract;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace Modeling_Canvas.UIElements
{
    public partial class BezierCurve : Path<BezierPoint>
    {
        public PointShape PointsShape { get; set; } = PointShape.Circle;

        public Dictionary<double, List<BezierPointFrameModel>> AnimationFrames { get; set; } = new();

        private int _curvePrecision = 30;

        private double _selectedFrameKey = 0;

        private bool _isNotAnimating = true;

        private bool _isInfiniteAnimation = false;

        public int CurvePrecision {
            get => _curvePrecision;
            set
            {
                if (_curvePrecision != value)
                {
                    _curvePrecision = value;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public bool IsNotAnimating
        {
            get => _isNotAnimating;
            set
            {
                if (_isNotAnimating != value)
                {
                    _isNotAnimating = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsInfiniteAnimation
        {
            get => _isInfiniteAnimation;
            set 
            { 
                if(_isInfiniteAnimation != value)
                {
                    _isInfiniteAnimation = value;
                    OnPropertyChanged();
                }
            }
        }

        public double SelectedFrameKey
        {
            get => _selectedFrameKey;
            set
            {
                _selectedFrameKey = value;
                OnPropertyChanged();
            }
        }

        public BezierCurve(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            PropertyChanged += (o, e) =>
            {
                if (e.PropertyName.Equals(nameof(IsClosed)))
                {
                    var p1 = Points.First();
                    var p2 = Points.Last();
                    if(p1 != null && p2 != null)
                    {
                        p1.ShowPrevControl = IsClosed;
                        p2.ShowNextControl = IsClosed;
                    }
                }
            };
        }

        protected override void OnInitialized(EventArgs e)
        {
            AnimationFrames.Add(SelectedFrameKey, Points.Select(p => p.GetFramePosition()).ToList());
            base.OnInitialized(e);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            foreach (var point in Points)
            {
                point.Shape = PointShape.Circle;
            }

            Points.First().Shape = PointShape.Square;
            Points.Last().Shape = PointShape.Square;
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            if (Points.Count < 2) return;

            for (int i = 0; i < Points.Count - 1; i++)
            {
                BezierPoint start = Points[i];
                BezierPoint end = Points[i + 1];

                Point p0 = start.PixelPosition;
                Point c1 = start.ControlNextPoint.PixelPosition;
                Point c2 = end.ControlPrevPoint.PixelPosition; 
                Point p3 = end.PixelPosition;

                dc.DrawBezierCurve(Canvas, StrokePen, p0, c1, c2, p3, CurvePrecision, 10);
            }

            if (IsClosed)
            {
                BezierPoint start = Points.Last();
                BezierPoint end = Points.First();

                Point p0 = start.PixelPosition;
                Point c1 = start.ControlNextPoint.PixelPosition;
                Point c2 = end.ControlPrevPoint.PixelPosition;
                Point p3 = end.PixelPosition;

                dc.DrawBezierCurve(Canvas, StrokePen, p0, c1, c2, p3, CurvePrecision, 10);
            }
        }

        public virtual void AddBezierPoint(Point position, Point prevControl, Point nextControl)
        {
            var point = AddPoint(position);
            point.ControlPrevPoint.Position = prevControl;
            point.ControlNextPoint.Position = nextControl;
        }

        protected override BezierPoint OnPointInit(Point point)
        {
            var customPoint = base.OnPointInit(point);

            customPoint.Shape = PointsShape;
            customPoint.Radius = PointsRadius;
            customPoint.IsSelectable = false;
            customPoint.ControlsVisibility = ControlsVisibility;
            customPoint.Visibility = ControlsVisibility;

            PropertyChanged += (s, e) => {
                if (e.PropertyName.Equals(nameof(ControlsVisibility)))
                {
                    customPoint.Visibility = ControlsVisibility;
                }
            };
            return customPoint;
        }

        public override void InsertPointAt(int pointIndex)
        {
            if (!IsClosed && (pointIndex <= 0 || pointIndex >= Points.Count - 1))
            {
                var p1 = Points.First();
                var p2 = Points.Last();
                p1.ShowPrevControl = true;
                p2.ShowNextControl = true;
            }
            base.InsertPointAt(pointIndex);
        }

        public override void RemovePoint(BezierPoint point)
        {
            base.RemovePoint(point);
            Canvas.Children.Remove(point.ControlNextPoint);
            Canvas.Children.Remove(point.ControlPrevPoint);
        }

        private void Animate()
        {
            if (AnimationFrames.Count < 2)
            {
                return;
            }

            var sortedFrameKeys = AnimationFrames.Keys.OrderBy(key => key).ToList();

            int currentFrameIndex = 0;
            var startTime = DateTime.Now;

            SelectFrame(sortedFrameKeys.First());

            IsNotAnimating = false;
            var timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (s, e) =>
            {
                if (currentFrameIndex >= sortedFrameKeys.Count - 1)
                {
                    if (IsInfiniteAnimation)
                    {
                        currentFrameIndex = 0;
                        return;
                    }
                    ((DispatcherTimer)s).Stop();
                    SelectedFrameKey = sortedFrameKeys.Last();
                    IsNotAnimating = true;
                    return;
                }

                // Поточний і наступний ключ кадру
                double startFrameKey = sortedFrameKeys[currentFrameIndex];
                double endFrameKey = sortedFrameKeys[currentFrameIndex + 1];

                // Визначення тривалості переходу між кадрами
                var transitionDuration = TimeSpan.FromSeconds(endFrameKey - startFrameKey);

                var startPoints = AnimationFrames[startFrameKey];
                var endPoints = AnimationFrames[endFrameKey];

                // Динамічне вирівнювання кількості точок
                if (endPoints.Count > Points.Count)
                {
                    // Додати нові точки
                    for (var i = Points.Count; i < endPoints.Count; i++)
                    {
                        AddPoint(endPoints[i].Position);
                    }
                }
                else if (endPoints.Count < Points.Count)
                {
                    // Видалити зайві точки
                    for (var i = Points.Count - 1; i >= endPoints.Count; i--)
                    {
                        RemovePoint(Points[i]);
                    }
                }

                var elapsedTime = DateTime.Now - startTime;
                var progress = Math.Min(1.0, elapsedTime.TotalMilliseconds / transitionDuration.TotalMilliseconds);

                SelectedFrameKey = startFrameKey;
                // Інтерполяція точок
                for (int i = 0; i < Points.Count; i++)
                {
                    var startPosition = i < startPoints.Count ? startPoints[i].Position : endPoints[i].Position;
                    var endPosition = i < endPoints.Count ? endPoints[i].Position : startPoints[i].Position;

                    Points[i].Position = Lerp(startPosition, endPosition, progress);

                    // Контрольні точки (якщо є)
                    if (i < startPoints.Count && i < endPoints.Count)
                    {
                        Points[i].ControlNextPoint.Position = Lerp(startPoints[i].ControlNextPosition, endPoints[i].ControlNextPosition, progress);
                        Points[i].ControlPrevPoint.Position = Lerp(startPoints[i].ControlPrevPosition, endPoints[i].ControlPrevPosition, progress);
                    }
                }

                InvalidateCanvas();

                if (progress >= 1.0)
                {
                    // Перехід на наступний кадр
                    currentFrameIndex++;
                    startTime = DateTime.Now;
                }
            }, Dispatcher.CurrentDispatcher);

            timer.Start();
        }

        private static Point Lerp(Point start, Point end, double t)
        {
            return new Point(
                start.X + (end.X - start.X) * t,
                start.Y + (end.Y - start.Y) * t
            );
        }

        protected void SelectFrame(double key)
        {

            if (AnimationFrames.TryGetValue(SelectedFrameKey, out var selectedFramePoints))
            {
                // Recreate the selected frame points to match the current list
                selectedFramePoints.Clear();
                selectedFramePoints.AddRange(Points.Select(p => p.GetFramePosition()));
            }

            if (AnimationFrames.TryGetValue(key, out var framePoints))
            {
                // Handle points count changes
                if (framePoints.Count > Points.Count)
                {
                    // Add missing points
                    for (var i = Points.Count; i < framePoints.Count; i++)
                    {
                        AddPoint(framePoints[i].Position);
                    }
                }
                else if (framePoints.Count < Points.Count)
                {
                    // Remove extra points
                    for (var i = Points.Count - 1; i >= framePoints.Count; i--)
                    {
                        RemovePoint(Points[i]);
                    }
                }

                // Update current points with frame points
                for (var i = 0; i < framePoints.Count; i++)
                {
                    Points[i].LoadFramePosition(framePoints[i]);
                }

                SelectedFrameKey = key;
            }

            InvalidateCanvas();
        }

        private void AddNewFrame()
        {
            var sortedKeys = AnimationFrames.Keys.OrderBy(k => k).ToList();

            var selectedIndex = sortedKeys.IndexOf(_selectedFrameKey);

            double newKey;
            if (selectedIndex < sortedKeys.Count - 1)
            {
                newKey = (sortedKeys[selectedIndex] + sortedKeys[selectedIndex + 1]) / 2.0;
            }
            else
            {
                newKey = sortedKeys.Last() + 1.0;
            }

            var newFrame = Points.Select(p => p.GetFramePosition()).ToList();

            AnimationFrames[newKey] = newFrame;
            SelectFrame(newKey);
        }

        private void RemoveFrame(double key)
        {
            if (AnimationFrames.Count > 1 && AnimationFrames.TryGetValue(key, out var frame)) {

                var keys = AnimationFrames.Select(f => f.Key).Order().ToList();
                var nextKeyIndex = keys.IndexOf(key);
                keys.Remove(key);

                if (nextKeyIndex > keys.Count - 1) nextKeyIndex--;
                var nextKey = keys[nextKeyIndex];

                AnimationFrames.Remove(key);

                SelectFrame(nextKey);
            }
        }

        public class SelectedFrameKeyToBackgroundConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is double selectedFrameKey && parameter is double frameKey)
                {
                    return selectedFrameKey == frameKey ? Brushes.LightGray : Brushes.White;
                }
                return Brushes.White;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }


        public override string ToString()
        {
            return $"Line\nPoints: {Points.Count}";
        }

    }
}

using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using Modeling_Canvas.Models;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class BezierCurve : Path<BezierPoint>
    {
        public PointShape PointsShape { get; set; } = PointShape.Circle;

        private int _curvePrecision = 100;
        public Dictionary<double, List<BezierPointFrameModel>> AnimationFrames { get; set; } = new();

        private double _selectedFrameKey = 0;

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

        private double _animationDuration = 3;
        public double AnimationDuration
        {
            get => _animationDuration;
            set
            {
                if (_animationDuration != value)
                {
                    _animationDuration = value;
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

        protected override void InitChildren()
        {
            var mainPanel = WpfHelper.CreateDefaultPanel();

            var scrollView = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Height = 300
            };

            var framesPanel = WpfHelper.CreateDefaultPanel(orientation: Orientation.Vertical);
            scrollView.Content = framesPanel;

            UpdateFramesPanel(framesPanel);

            var addFrameButton = WpfHelper.CreateButton(
                clickAction: () =>
                {
                    AddNewFrame();
                    UpdateFramesPanel(framesPanel);
                },
                content: "+"
            );

            mainPanel.Children.Add(scrollView);
            mainPanel.Children.Add(addFrameButton);

            _uiControls.Add("AnimationFrames", mainPanel);

            var precisionSlider =
                WpfHelper.CreateSliderControl(
                    "Precision",
                    this,
                    nameof(CurvePrecision),
                    5,
                    500,
                    2.5
                );

            _uiControls.Add("Presicion", precisionSlider);

            base.InitChildren();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var a = AnimationFrames;
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

                // Draw the segment
                dc.DrawBezierCurve(Canvas, StrokePen, p0, c1, c2, p3, CurvePrecision, 10);
            }

            // Handle closed curves
            if (IsClosed)
            {
                BezierPoint start = Points.Last();
                BezierPoint end = Points.First();

                Point p0 = start.PixelPosition;
                Point c1 = start.ControlNextPoint.PixelPosition;
                Point c2 = end.ControlPrevPoint.PixelPosition;
                Point p3 = end.PixelPosition;

                // Draw the closing segment
                dc.DrawBezierCurve(Canvas, StrokePen, p0, c1, c2, p3, CurvePrecision, 10);
            }
        }

        protected override BezierPoint OnPointInit(Point point)
        {
            var customPoint = base.OnPointInit(point);

            customPoint.Shape = PointsShape;
            customPoint.Radius = PointsRadius;
            customPoint.IsSelectable = false;
            customPoint.ControlsVisibility = ControlsVisibility;

            PropertyChanged += (s, e) => {
                if (e.PropertyName.Equals(nameof(ControlsVisibility))) {
                    customPoint.Visibility = ControlsVisibility;
                }
            };
            return customPoint;
        }

        public override void RemovePoint(BezierPoint point)
        {
            base.RemovePoint(point);
            Canvas.Children.Remove(point.ControlNextPoint);
            Canvas.Children.Remove(point.ControlPrevPoint);
        }

        private static Point Lerp(Point start, Point end, double t)
        {
            return new Point(
                start.X + (end.X - start.X) * t,
                start.Y + (end.Y - start.Y) * t
            );
        }

        private void UpdateFramesPanel(StackPanel framesPanel)
        {
            framesPanel.Children.Clear();

            foreach (var frame in AnimationFrames.OrderBy(x => x.Key))
            {
                var framePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5)
                };

                // Прив'язка до фону
                var binding = new Binding("SelectedFrameKey")
                {
                    Source = this,
                    Converter = new SelectedFrameKeyToBackgroundConverter(),
                    ConverterParameter = frame.Key
                };

                BindingOperations.SetBinding(framePanel, StackPanel.BackgroundProperty, binding);

                // Текстове поле для часу кадру
                var timeTextBox = new TextBox
                {
                    Width = 50,
                    Text = frame.Key.ToString()
                };

                var previousTime = frame.Key;

                timeTextBox.PreviewTextInput += (s, e) =>
                {
                    if (double.TryParse(timeTextBox.Text, out var newTime) && newTime != previousTime) {
                        timeTextBox.Background = Brushes.Gray;
                    } else
                    {
                        timeTextBox.Background = Brushes.White;
                    }
                };

                timeTextBox.PreviewKeyDown += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        if (double.TryParse(timeTextBox.Text, out var newTime) && !AnimationFrames.ContainsKey(newTime))
                        {
                            var points = AnimationFrames[frame.Key];
                            AnimationFrames.Remove(frame.Key);
                            AnimationFrames[newTime] = points;

                            if (SelectedFrameKey == previousTime) SelectedFrameKey = newTime;
                            previousTime = newTime;

                            UpdateFramesPanel(framesPanel);

                            timeTextBox.BorderBrush = SystemColors.ControlDarkBrush;
                        } else
                        {
                            timeTextBox.Background = Brushes.IndianRed;
                        }
                    }
                    else if (e.Key == Key.Escape) // Optional: Revert on Escape
                    {
                        timeTextBox.Text = previousTime.ToString();
                        timeTextBox.BorderBrush = SystemColors.ControlDarkBrush;
                    }
                };


                var switchButton = WpfHelper.CreateButton(
                    clickAction: () =>
                    {
                        if (AnimationFrames.TryGetValue(SelectedFrameKey, out var selectedFramePoints))
                        {
                            // Recreate the selected frame points to match the current list
                            selectedFramePoints.Clear();
                            selectedFramePoints.AddRange(Points.Select(p => p.GetFramePosition()));
                        }

                        if (AnimationFrames.TryGetValue(frame.Key, out var framePoints))
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

                            SelectedFrameKey = frame.Key;
                        }

                        InvalidateCanvas();
                    },
                    content: "Switch To"
                );


                framePanel.Children.Add(timeTextBox);
                framePanel.Children.Add(switchButton);
                framesPanel.Children.Add(framePanel);
            }
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

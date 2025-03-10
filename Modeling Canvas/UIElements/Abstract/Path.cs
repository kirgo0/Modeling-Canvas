﻿using Modeling_Canvas.Models;
using Modeling_Canvas.UIElements.Interfaces;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements.Abstract
{
    public abstract class Path<T> : GroupableElement where T : GroupableElement, IPoint
    {
        private bool _isClosed = true;

        private bool _showRemovePointButton;

        private bool _enableRemovePointButton;

        private T? _selectedPoint;

        public double PointsRadius { get; set; } = 5;

        public List<T> Points { get; set; } = new();

        public bool IsClosed
        {
            get => _isClosed;
            set
            {
                if (_isClosed != value)
                {
                    _isClosed = value;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public override Visibility ControlsVisibility
        {
            get => base.ControlsVisibility;
            set
            {
                if (value is Visibility.Hidden) SelectedPoint = null;
                base.ControlsVisibility = value;
            }
        }

        public T? SelectedPoint
        {
            get => _selectedPoint;
            set
            {
                if (_selectedPoint != value)
                {
                    _selectedPoint = value;
                    ShowRemovePointButton = value is not null;
                }
            }
        }

        public bool ShowRemovePointButton
        {
            get => _showRemovePointButton;
            set
            {
                if (_showRemovePointButton != value)
                {
                    _showRemovePointButton = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EnableRemovePointButton
        {
            get => _enableRemovePointButton;
            set
            {
                if (_enableRemovePointButton != value)
                {
                    _enableRemovePointButton = value;
                    OnPropertyChanged();
                }
            }
        }

        public Path(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            LabelText = "Path";
            Style = new()
            {
                StrokeThickness = 2,
                StrokeColor = Brushes.Cyan,
            };
        }

        public Path(CustomCanvas canvas, Point firstPoint, Point secondPoint, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            LabelText = "Path";
            Style = new()
            {
                StrokeThickness = 2,
                StrokeColor = Brushes.Cyan,
            };
            AddPoint(firstPoint);
            AddPoint(secondPoint);
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (Points.Count < 2) return;
            base.OnRender(dc);
        }

        protected override List<(FigureStyle, Point[])> GetElementGeometry()
        {
            var linePoint = Points.Select(p => p.Position).ToList();

            if (IsClosed)
            {
                linePoint.Add(Points.First().Position);
            }

            return new() { (Style, linePoint.ToArray()) };
        }

        protected override void InitControlPanel()
        {
            var addPointbutton =
                WpfHelper.CreateButton(
                    () =>
                    {
                        if (SelectedPoint is null)
                        {
                            AddPoint(GetAnchorDefaultPosition());
                        }
                        else
                        {
                            InsertPointAt(Points.IndexOf(SelectedPoint));
                        }
                    },
                    "Add point"
                );

            _uiControls.Add("Add Point", addPointbutton);

            var removePointbutton =
                WpfHelper.CreateButton(
                    () =>
                    {
                        RemovePoint(SelectedPoint);
                        SelectedPoint = null;
                        RenderControlPanel();
                    },
                    "Remove point"
                );

            removePointbutton.AddVisibilityBinding(
                this,
                nameof(ShowRemovePointButton)
                );

            removePointbutton.AddIsDisabledBinding(
                this,
                nameof(EnableRemovePointButton)
                );

            _uiControls.Add("Remove Point", removePointbutton);

            var isClosedCheckBox =
                WpfHelper.CreateLabeledCheckBox(
                    "Is Closed:",
                    this,
                    nameof(IsClosed)
                );

            _uiControls.Add("IsClosed", isClosedCheckBox);
            AddFillColorControls();
            AddStrokeColorControls();
            AddStrokeThicknessControls();
            base.InitControlPanel();
        }

        protected override Point GetAnchorDefaultPosition()
        {
            if (Points == null || !Points.Any()) return new Point(0, 0);

            // Calculate the average of X and Y coordinates
            double centerX = Points.Average(p => p.Position.X);
            double centerY = Points.Average(p => p.Position.Y);

            return new Point(centerX, centerY);
        }

        public override Point GetTopLeftPosition() => new Point(Points.Min(x => x.Position.X) - PointsRadius / UnitSize, Points.Max(y => y.Position.Y) + PointsRadius / UnitSize);

        public override Point GetBottomRightPosition() => new Point(Points.Max(x => x.Position.X) + PointsRadius / UnitSize, Points.Min(y => y.Position.Y) - PointsRadius / UnitSize);

        public virtual void AddPoint(double x, double y)
        {
            AddPoint(new Point(x, y));
        }

        protected virtual T OnPointInit(Point point)
        {
            T customPoint = (T)Activator.CreateInstance(typeof(T), Canvas, false);

            if (customPoint is null)
                throw new InvalidOperationException($"Failed to create an instance of type {typeof(T)}.");

            customPoint.Position = point;

            customPoint.MouseLeftButtonDownAction = (e) =>
            {
                SelectedPoint = customPoint;
            };
            EnableRemovePointButton = true;
            return customPoint;
        }

        public T AddPoint(Point point)
        {
            var customPoint = OnPointInit(point);

            Points.Add(customPoint);

            AddChildren(customPoint);

            return customPoint;
        }

        public virtual void RemovePoint(T point)
        {
            if (point is not null && Points.Contains(point))
            {
                Points.Remove(point);
                Canvas.Children.Remove(point);
                if (Points.Count < 2) EnableRemovePointButton = false;
            }
        }

        public virtual void InsertPointAt(int pointIndex)
        {
            if (pointIndex >= Points.Count) pointIndex = Points.Count - 1;

            var point = GetAvaragePoint(pointIndex);

            var customPoint = OnPointInit(point);

            Points.Insert(pointIndex, customPoint);

            AddChildren(customPoint);
        }

        public Point GetAvaragePoint(int index)
        {
            if (index <= 0)
            {
                var nextPoint = Points[0];
                if (Points.Count > 1)
                {
                    var prevPoint = Points[Points.Count - 1];
                    var avgX = (prevPoint.X + nextPoint.X) / 2.0;
                    var avgY = (prevPoint.Y + nextPoint.Y) / 2.0;
                    return new Point(avgX, avgY);
                }

                return new Point(nextPoint.X + 1, nextPoint.Y + 1);
            }
            else if (index >= Points.Count)
            {
                var prevPoint = Points[Points.Count - 1];
                return new Point(prevPoint.X, prevPoint.Y);
            }
            else
            {
                var prevPoint = Points[index - 1];
                var nextPoint = Points[index];
                var avgX = (prevPoint.X + nextPoint.X) / 2.0;
                var avgY = (prevPoint.Y + nextPoint.Y) / 2.0;
                return new Point(avgX, avgY);
            }
        }

        public override void MoveElement(Vector offset)
        {
            foreach (var point in Points)
            {
                point.MoveElement(offset);
            }
            base.MoveElement(offset);
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
            foreach (var point in Points)
            {
                point.RotateElement(anchorPoint, degrees);
            }
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
            foreach (var point in Points)
            {
                point.ScaleElement(anchorPoint, scaleVector, ScaleFactor);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            SelectedPoint = null;
            base.OnMouseLeftButtonDown(e);
        }

        public override string ToString()
        {
            return $"Path\nPoints: {Points.Count}";
        }

    }
}

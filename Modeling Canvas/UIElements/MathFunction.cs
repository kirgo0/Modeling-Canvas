﻿using AngouriMath.Extensions;
using MathNet.Numerics.LinearAlgebra;
using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using Modeling_Canvas.Models;
using System.Windows;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public class MathFunction : Element
    {
        private double[] xValues;

        private double _rangeStart = -0.6;

        private double _rangeEnd = 0.601;

        private double _step = 0.15;

        private bool _showParabola = true;

        private bool _showLine = true;

        private bool _showPoints = true;

        private bool _useFuncExpression = true;

        private string _funcExpression = "sin(x^2e)*cos(1/x)";

        private double _xToCalculate;

        private double? _calculatedValue;

        private List<(FigureStyle, Point[])> _cachedGeometry;

        private bool _isGeometryDirty = true;
        
        public CustomLine StartBound { get; set; }

        public CustomLine EndBound { get; set; }

        public FigureStyle ParabolaStyle { get; set; } = new()
        {
            StrokeColor = Brushes.Red,
            StrokeThickness = 2,
        };

        public FigureStyle LineStyle { get; set; } = new()
        {
            StrokeColor = Brushes.Green,
            StrokeThickness = 2,
        };

        public FigureStyle PointsStyle { get; set; } = new()
        {
            FillColor = Brushes.AliceBlue
        };

        public FigureStyle PointsLineStyle { get; set; } = new()
        {
            StrokePen = new Pen(Brushes.Black, 2) { DashStyle = DashStyles.Dash }
        };

        public List<Point> Points { get; set; }

        public double RangeStart
        {
            get => _rangeStart;
            set
            {
                if (_rangeStart != value)
                {
                    var oldStart = _rangeStart;
                    _rangeStart = value;
                    _isGeometryDirty = true;
                    OnPropertyChanged();
                }
            }
        }

        public double RangeEnd
        {
            get => _rangeEnd;
            set
            {
                if (_rangeEnd != value)
                {
                    var oldEnd = _rangeEnd;
                    _rangeEnd = value;
                    _isGeometryDirty = true;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }


        public double Step
        {
            get => _step;
            set
            {
                if (_step != value)
                {
                    _step = value;
                    _isGeometryDirty = true;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public bool ShowParabola
        {
            get => _showParabola;
            set
            {
                if (_showParabola != value)
                {
                    _showParabola = value;
                    _isGeometryDirty = true;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public bool ShowLine
        {
            get => _showLine;
            set
            {
                if (_showLine != value)
                {
                    _showLine = value;
                    _isGeometryDirty = true;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public bool ShowPoints
        {
            get => _showPoints;
            set
            {
                if (_showPoints != value)
                {
                    _showPoints = value;
                    _isGeometryDirty = true;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public bool UseFuncExpression
        {
            get => _useFuncExpression;
            set
            {
                if (_useFuncExpression != value)
                {
                    _useFuncExpression = value;
                    _isGeometryDirty = true;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public string FuncExpression
        {
            get => _funcExpression;
            set
            {
                if (_funcExpression != value)
                {
                    _funcExpression = value;
                    _isGeometryDirty = true;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public double XToCalculate
        {
            get => _xToCalculate;
            set
            {
                if (_xToCalculate != value)
                {
                    _xToCalculate = value;
                    CalculatedValue = CalculateFunctionValue(XToCalculate)?.Y;
                    OnPropertyChanged();
                }
            }
        }

        public double? CalculatedValue
        {
            get => _calculatedValue;
            set
            {
                if (_calculatedValue != value)
                {
                    _calculatedValue = value;
                    OnPropertyChanged();
                }
            }
        }


        public MathFunction(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            LabelText = "Fucntion";
            xValues = GenerateRange(RangeStart, RangeEnd, Step);
            Points = xValues.Select(x => new Point(x, Math.Asin(x * x) * Math.Acos(x * x))).ToList();
        }

        protected override void InitControlPanel()
        {
            var showLineCheckbox =
                WpfHelper.CreateLabeledCheckBox(
                    "Show Line:",
                    this,
                    nameof(ShowLine)
                );

            _uiControls.Add("ShowLine", showLineCheckbox);

            var showParabolaCheckbox =
                WpfHelper.CreateLabeledCheckBox(
                    "Show Parabola:",
                    this,
                    nameof(ShowParabola)
                );

            _uiControls.Add("ShowParabola", showParabolaCheckbox);

            var showPointsCheckbox =
                WpfHelper.CreateLabeledCheckBox(
                    "Show Points:",
                    this,
                    nameof(ShowPoints)
                );

            _uiControls.Add("ShowPoints", showPointsCheckbox);

            var useFuncCheckbox =
                WpfHelper.CreateLabeledCheckBox(
                    "Use function expression:",
                    this,
                    nameof(UseFuncExpression)
                );

            var expressionField =
                WpfHelper.CreateLabeledTextBox(
                    this,
                    nameof(FuncExpression),
                    labelText: "Function: ",
                    delay: 200
                );

            expressionField.AddVisibilityBinding(this, nameof(UseFuncExpression));

            _uiControls.Add("FuncExpressionText", expressionField);


            var xToCalculateTextbox =
                WpfHelper.CreateLabeledTextBox(
                    this,
                    nameof(XToCalculate),
                    labelText: "X to calculate: ",
                    delay: 200
                );

            xToCalculateTextbox.AddVisibilityBinding(this, nameof(UseFuncExpression));

            _uiControls.Add("XToCalculateText", xToCalculateTextbox);

            var calculatedValueLabel =
                WpfHelper.CreateValueTextBlock(
                    "Y",
                    this,
                    nameof(CalculatedValue)
                    );

            calculatedValueLabel.AddVisibilityBinding(this, nameof(UseFuncExpression));

            _uiControls.Add("CalculatedValue", calculatedValueLabel);
        }

        protected override void InitChildren()
        {
            StartBound = new CustomLine(Canvas, false)
            {
                Style = new FigureStyle()
                {
                    StrokeColor = Brushes.Purple,
                    StrokeThickness = 2
                },
                AfterMoveAction = (e) =>
                {
                    var line = (CustomLine)e;
                    RangeStart = line.Points[0].X;
                }
            };
            AddChildren(StartBound);

            StartBound.AddPoint(RangeStart, -7);
            StartBound.AddPoint(RangeStart, 7);

            EndBound = new CustomLine(Canvas, false)
            {
                Style = new FigureStyle()
                {
                    StrokeColor = Brushes.Purple,
                    StrokeThickness = 2
                },
                AfterMoveAction = (e) =>
                {
                    var line = (CustomLine)e;
                    RangeEnd = line.Points[0].X;
                }
            };
            AddChildren(EndBound);

            EndBound.AddPoint(RangeEnd, -7);
            EndBound.AddPoint(RangeEnd, 7);

            base.InitChildren();
        }

        protected override List<(FigureStyle, Point[])> GetElementGeometry()
        {
            if (!_isGeometryDirty)
            {
                return _cachedGeometry;
            }

            var geomtery = new List<(FigureStyle, Point[])>();

            var pointsLine = new List<Point>();

            try
            {

                if (UseFuncExpression && !string.IsNullOrEmpty(FuncExpression))
                {
                    for (double x = RangeStart; x < RangeEnd; x += Step)
                    {
                        var point = CalculateFunctionValue(x);
                        if (point.HasValue) pointsLine.Add(point.Value);
                    }
                    if (ShowPoints)
                    {
                        var accurateLinePoints = new List<Point>();
                        for (double x = RangeStart; x < RangeEnd; x += 0.01)
                        {
                            var point = CalculateFunctionValue(x);
                            if (point.HasValue) accurateLinePoints.Add(point.Value);
                        }
                        geomtery.Insert(0, (PointsLineStyle, accurateLinePoints.ToArray()));
                    }
                }
                else
                {
                    pointsLine = Points;
                    geomtery.Insert(0, (PointsLineStyle, pointsLine.ToArray()));
                }

                if (ShowParabola)
                {
                    geomtery.Add((ParabolaStyle, GetParabolaPoints(pointsLine)));
                }
                if (ShowLine)
                {
                    geomtery.Add((LineStyle, GetLinePoints(pointsLine)));
                }
                if (ShowPoints)
                {
                    if (ShowPoints)
                    {
                        foreach (var p in pointsLine)
                        {
                            geomtery.Add((PointsStyle, Canvas.GetCircleGeometry(p, 0.01, precision: 10)));
                        }
                    }
                }
                _isGeometryDirty = false;
                _cachedGeometry = geomtery;
                return geomtery;

            }
            catch
            {
                return [];
            }

        }

        private Point? CalculateFunctionValue(double x)
        {
            var expr = FuncExpression.ToEntity();
            var substituted = expr.Substitute("x", x);
            if (substituted.EvaluableNumerical)
            {
                double y = (double)substituted.EvalNumerical();
                return new Point(x, y);
            }
            return null;
        }

        private Point[] GetLinePoints(List<Point> inputPoints)
        {
            var points = new List<Point>();
            var lineParams = FitLine(inputPoints);
            if (lineParams.Length != 2) return [];

            points.Add(new Point(RangeStart, lineParams[0] + lineParams[1] * RangeStart));
            points.Add(new Point(RangeEnd, lineParams[0] + lineParams[1] * RangeEnd));
            return points.ToArray();
        }

        private Point[] GetParabolaPoints(List<Point> inputPoints)
        {
            var points = new List<Point>();
            var lineParams = FitParabola(inputPoints);
            if (lineParams.Length != 3) return [];
            for (double i = RangeStart; i < RangeEnd; i += 0.01)
            {
                points.Add(new Point(i, lineParams[0] + lineParams[1] * i + lineParams[2] * i * i));
            }
            return points.ToArray();
        }

        public double[] GenerateRange(double start, double end, double step)
        {
            if (step < 0) throw new ArgumentException("Step must be greater than zero.", nameof(step));

            return Enumerable.Range(0, (int)Math.Ceiling((end - start) / step) + 1)
                    .Select(i => Math.Round(start + i * step, 10))
                    .Where(value => value <= end)
                    .ToArray();
        }

        public double[] FitLine(List<Point> points)
        {
            var validPoints = points.Where(p => !double.IsNaN(p.X) && !double.IsNaN(p.Y)).ToList();

            if (validPoints.Count < 2) return [];

            int n = validPoints.Count;
            double sumX = validPoints.Sum(p => p.X);
            double sumY = validPoints.Sum(p => p.Y);
            double sumXY = validPoints.Sum(p => p.X * p.Y);
            double sumX2 = validPoints.Sum(p => p.X * p.X);

            double denom = n * sumX2 - sumX * sumX;
            double a1 = (n * sumXY - sumX * sumY) / denom;
            double a0 = (sumY - a1 * sumX) / n;

            return new double[] { a0, a1 };
        }

        public double[] FitParabola(List<Point> points)
        {
            var validPoints = points.Where(p => !double.IsNaN(p.X) && !double.IsNaN(p.Y)).ToList();

            if (validPoints.Count < 3) return [];

            int n = validPoints.Count;
            var A = Matrix<double>.Build.Dense(n, 3, (i, j) => j == 0 ? 1 : Math.Pow(validPoints[i].X, j));
            var Y = Vector<double>.Build.Dense(validPoints.Select(p => (double)p.Y).ToArray());

            var coeffs = (A.Transpose() * A).Inverse() * A.Transpose() * Y;
            return coeffs.ToArray();
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
        }
    }
}

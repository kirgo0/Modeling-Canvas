using AngouriMath.Extensions;
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
        public PointShape PointsShape { get; set; } = PointShape.Circle;

        private double[] xValues;

        private double _rangeStart = -0.6;

        private double _rangeEnd = 0.8;

        private double _step = 0.1;

        private bool _showParabola = true;

        private bool _showLine = true;

        private bool _showPoints = true;

        private bool _useFuncExpression = false;

        private string _funcExpression;

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
                    AdjustCachedGeometry(oldStart, _rangeEnd, _rangeStart, _rangeEnd);
                    OnPropertyChanged(nameof(RangeStart));
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
                    AdjustCachedGeometry(_rangeStart, oldEnd, _rangeStart, _rangeEnd);
                    OnPropertyChanged(nameof(RangeEnd));
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
                    OnPropertyChanged(nameof(Step));
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
                    OnPropertyChanged(nameof(ShowParabola));
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
                    OnPropertyChanged(nameof(ShowLine));
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
                    OnPropertyChanged(nameof(ShowPoints));
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
                    OnPropertyChanged(nameof(UseFuncExpression));
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
                    OnPropertyChanged(nameof(FuncExpression));
                    InvalidateCanvas();
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

            _uiControls.Add("UseFuncExpression", useFuncCheckbox);

            var expressionField =
                WpfHelper.CreateLabeledTextBox(
                    this,
                    nameof(FuncExpression),
                    labelText: "Function: ",
                    delay: 200
                );

            expressionField.AddVisibilityBinding(this, nameof(UseFuncExpression));

            _uiControls.Add("FuncExpressionText", expressionField);
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
                AfterMoveAction = (e) => { 
                    var line = (CustomLine) e;
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
                AfterMoveAction = (e) => {
                    var line = (CustomLine)e;
                    RangeEnd = line.Points[0].X;
                }
            };
            AddChildren(EndBound);

            EndBound.AddPoint(RangeEnd, -7);
            EndBound.AddPoint(RangeEnd, 7);

            base.InitChildren();
        }

        // Geometry references
        private (FigureStyle, Point[])? _pointsLineGeometry;
        private (FigureStyle, Point[])? _accurateLineGeometry;
        private (FigureStyle, Point[])? _parabolaGeometry;
        private (FigureStyle, Point[])? _lineGeometry;
        private List<(FigureStyle, Point[])> _pointsGeometries = new();

        protected override List<(FigureStyle, Point[])> GetElementGeometry()
        {
            if (!_isGeometryDirty)
            {
                return _cachedGeometry;
            }

            var geometry = new List<(FigureStyle, Point[])>();

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
                        _accurateLineGeometry = (PointsLineStyle, accurateLinePoints.ToArray());
                        geometry.Insert(0, _accurateLineGeometry.Value);
                    }
                }
                else
                {
                    pointsLine = Points;
                    _pointsLineGeometry = (PointsLineStyle, pointsLine.ToArray());
                    geometry.Insert(0, _pointsLineGeometry.Value);
                }

                if (ShowParabola)
                {
                    _parabolaGeometry = (ParabolaStyle, GetParabolaPoints(pointsLine));
                    geometry.Add(_parabolaGeometry.Value);
                }
                if (ShowLine)
                {
                    _lineGeometry = (LineStyle, GetLinePoints(pointsLine));
                    geometry.Add(_lineGeometry.Value);
                }
                if (ShowPoints)
                {
                    _pointsGeometries.Clear();
                    foreach (var p in pointsLine)
                    {
                        var pointGeometry = (PointsStyle, Canvas.GetCircleGeometry(p, 0.01, precision: 10));
                        _pointsGeometries.Add(pointGeometry);
                        geometry.Add(pointGeometry);
                    }
                }

                _isGeometryDirty = false;
                _cachedGeometry = geometry;
                return geometry;
            }
            catch
            {
                return [];
            }
        }
        private void AdjustCachedGeometry(double oldStart, double oldEnd, double newStart, double newEnd)
        {
            if (_cachedGeometry == null || _cachedGeometry.Count == 0)
            {
                _isGeometryDirty = true;
                return;
            }

            // Modify points line geometry
            if (_pointsLineGeometry.HasValue)
            {
                var pointsList = _pointsLineGeometry.Value.Item2.ToList();

                // Expand range (add missing points)
                if (newStart < oldStart)
                {
                    for (double x = newStart; x < oldStart; x += Step)
                    {
                        var point = CalculateFunctionValue(x);
                        if (point.HasValue) pointsList.Insert(0, point.Value);
                    }
                }
                if (newEnd > oldEnd)
                {
                    for (double x = oldEnd; x < newEnd; x += Step)
                    {
                        var point = CalculateFunctionValue(x);
                        if (point.HasValue) pointsList.Add(point.Value);
                    }
                }

                // Trim range
                pointsList.RemoveAll(p => p.X < newStart || p.X >= newEnd);

                _pointsLineGeometry = (_pointsLineGeometry.Value.Item1, pointsList.ToArray());
            }

            // Modify accurate line geometry
            if (_accurateLineGeometry.HasValue)
            {
                var accuratePointsList = _accurateLineGeometry.Value.Item2.ToList();

                if (newStart < oldStart)
                {
                    for (double x = newStart; x < oldStart; x += 0.01)
                    {
                        var point = CalculateFunctionValue(x);
                        if (point.HasValue) accuratePointsList.Insert(0, point.Value);
                    }
                }
                if (newEnd > oldEnd)
                {
                    for (double x = oldEnd; x < newEnd; x += 0.01)
                    {
                        var point = CalculateFunctionValue(x);
                        if (point.HasValue) accuratePointsList.Add(point.Value);
                    }
                }

                accuratePointsList.RemoveAll(p => p.X < newStart || p.X >= newEnd);
                _accurateLineGeometry = (_accurateLineGeometry.Value.Item1, accuratePointsList.ToArray());
            }

            // Modify parabola and line geometries
            if (_parabolaGeometry.HasValue)
            {
                _parabolaGeometry = (ParabolaStyle, GetParabolaPoints(_pointsLineGeometry.Value.Item2.ToList()));
            }
            if (_lineGeometry.HasValue)
            {
                _lineGeometry = (LineStyle, GetLinePoints(_pointsLineGeometry.Value.Item2.ToList()));
            }

            // Modify individual point geometries
            _pointsGeometries.Clear();
            foreach (var p in _pointsLineGeometry.Value.Item2)
            {
                _pointsGeometries.Add((PointsStyle, Canvas.GetCircleGeometry(p, 0.01, precision: 10)));
            }

            // Reconstruct _cachedGeometry in order
            _cachedGeometry = new List<(FigureStyle, Point[])>();

            if (_accurateLineGeometry.HasValue)
                _cachedGeometry.Add(_accurateLineGeometry.Value);
            if (_pointsLineGeometry.HasValue)
                _cachedGeometry.Add(_pointsLineGeometry.Value);
            if (_parabolaGeometry.HasValue)
                _cachedGeometry.Add(_parabolaGeometry.Value);
            if (_lineGeometry.HasValue)
                _cachedGeometry.Add(_lineGeometry.Value);
            _cachedGeometry.AddRange(_pointsGeometries);

            _isGeometryDirty = false;
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
                    .Select(i => Math.Round(start + i * step, 10)) // Rounding to avoid floating-point precision issues
                    .Where(value => value <= end) // Ensure we don't exceed the end value due to precision errors
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

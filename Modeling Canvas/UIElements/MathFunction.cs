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

        private double[] x;

        private double[] y;

        private double _rangeStart = -0.6;

        private double _rangeEnd = 0.8;

        private double _step = 0.1;

        private bool _showParabola = true;

        private bool _showLine = true;

        private bool _showPoints = true;

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

        public double RangeStart
        {
            get => _rangeStart;
            set
            {
                if (_rangeStart != value)
                {
                    _rangeStart = value;
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
                    _rangeEnd = value;
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
                    OnPropertyChanged(nameof(ShowPoints));
                    InvalidateCanvas();
                }
            }
        }

        public MathFunction(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            LabelText = "Fucntion";
            x = GenerateRange(RangeStart, RangeEnd, Step);
            y = x.Select(val => Math.Asin(val * val) * Math.Acos(val * val)).ToArray();
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
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
        }

        protected override List<(FigureStyle, Point[])> GetElementGeometry()
        {
            var geomtery = new List<(FigureStyle, Point[])>();
            if(ShowParabola)
            {
                geomtery.Add((ParabolaStyle, GetParabolaPoints()));
            }
            if (ShowLine)
            {
                geomtery.Add((LineStyle, GetLinePoints()));
            }
            var functionLine = new List<Point>();
            if (ShowPoints)
            {
                for(int i = 0; i < x.Length; i++)
                {
                    if (x[i] == double.NaN || y[i] == double.NaN) continue;
                    var functionPoint = new Point(x[i], y[i]);
                    functionLine.Add(functionPoint);
                    geomtery.Add((PointsStyle, Canvas.GetCircleGeometry(functionPoint, 0.01, precision: 10)));
                }
            }
            geomtery.Insert(0, (PointsLineStyle, functionLine.ToArray()));
            return geomtery;
        }

        private Point[] GetLinePoints()
        {
            var points = new List<Point>();
            var lineParams = FitLine(x, y);
            points.Add(new Point(RangeStart, lineParams[0] + lineParams[1] * RangeStart));
            points.Add(new Point(RangeEnd, lineParams[0] + lineParams[1] * RangeEnd));
            return points.ToArray();
        }

        private Point[] GetParabolaPoints()
        {
            var points = new List<Point>();
            var lineParams = FitParabola(x, y);
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

        private double[] FitLine(double[] x, double[] y)
        {
            var validPoints = x.Zip(y, (xi, yi) => new { xi, yi })
                    .Where(point => !double.IsNaN(point.xi) && !double.IsNaN(point.yi))
                    .ToList();

            if (validPoints.Count < 2)
                throw new InvalidOperationException("Not enough valid points to fit a line.");

            var xValid = validPoints.Select(p => p.xi).ToArray();
            var yValid = validPoints.Select(p => p.yi).ToArray();
            int n = xValid.Length;

            double sumX = xValid.Sum(), sumY = yValid.Sum(), sumXY = xValid.Zip(yValid, (xi, yi) => xi * yi).Sum(), sumX2 = xValid.Sum(xi => xi * xi);

            double denom = n * sumX2 - sumX * sumX;
            double a1 = (n * sumXY - sumX * sumY) / denom;
            double a0 = (sumY - a1 * sumX) / n;

            return new double[] { a0, a1 };
        }

        private double[] FitParabola(double[] x, double[] y)
        {
            var validPoints = x.Zip(y, (xi, yi) => new { xi, yi })
                    .Where(point => !double.IsNaN(point.xi) && !double.IsNaN(point.yi))
                    .ToList();

            if (validPoints.Count < 3)
                throw new InvalidOperationException("Not enough valid points to fit a parabola.");

            var xValid = validPoints.Select(p => p.xi).ToArray();
            var yValid = validPoints.Select(p => p.yi).ToArray();
            int n = xValid.Length;

            var A = Matrix<double>.Build.Dense(n, 3, (i, j) => j == 0 ? 1 : Math.Pow(xValid[i], j));
            var Y = Vector<double>.Build.Dense(yValid);

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

using Modeling_Canvas.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace Modeling_Canvas.UIElements
{
    public class KochCurve : Element
    {
        private int _iterations = 1;

        private double _stepSize = 1;

        private double _angle = 60;

        private string _axiom = "F++F++F";

        private string _ruleF = "F-F++F-F";

        protected Point _position;

        private Point[][] _cachedGeometry;

        private bool _isGeometryDirty = true;

        public int Iterations
        {
            get => _iterations;
            set
            {
                if (_iterations != value)
                {
                    _iterations = value;
                    _isGeometryDirty = true;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public double StepSize
        {
            get => _stepSize;
            set
            {
                if (_stepSize != value)
                {
                    _stepSize = value;
                    _isGeometryDirty = true;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public double Angle
        {
            get => _angle;
            set
            {
                if (_angle != value)
                {
                    _angle = value;
                    _isGeometryDirty = true;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public Point Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    _isGeometryDirty = true;
                    OnPropertyChanged();
                    InvalidateCanvas();
                }
            }
        }

        public KochCurve(CustomCanvas canvas) : base(canvas)
        {
        }

        protected override void InitControlPanel()
        {
            var iterationsBox =
                WpfHelper.CreateSliderControl<int>(
                    "Iterations",
                    this,
                    nameof(Iterations),
                    0,
                    7
                    );

            _uiControls.Add("Iterations", iterationsBox);

            var sizeBox =
                WpfHelper.CreateLabeledTextBox(
                    this,
                    nameof(StepSize),
                    "Step size",
                    delay: 400
                    );

            _uiControls.Add("StepSize", sizeBox);

            var angleBox =
                WpfHelper.CreateLabeledTextBox(
                    this,
                    nameof(Angle),
                    "Rotation angle"
                    );

            _uiControls.Add("Angle", angleBox);

            // Створення ComboBox
            ComboBox comboBox = new ComboBox
            {
                Width = 200,
                Margin = new Thickness(10)
            };

            // Додавання елементів у ComboBox
            comboBox.Items.Add(new ComboBoxItem { Content = "Сніжинка Коха", Tag = new ValueTuple<double, string, string>(60, "F++F++F", "F-F++F-F") });
            comboBox.Items.Add(new ComboBoxItem { Content = "Квадратична сніжинка Коха", Tag = new ValueTuple<double, string, string>(90, "F+F+F+F", "F+F-F-FF+F+F-F") });
            comboBox.Items.Add(new ComboBoxItem { Content = "Квадратична крива Коха", Tag = new ValueTuple<double, string, string>(90, "F", "F-F+F+F-F") });
            comboBox.Items.Add(new ComboBoxItem { Content = "Крива Коха", Tag = new ValueTuple<double, string, string>(60, "F", "F-F++F-F") });

            // Обробник подій для вибору елементів
            comboBox.SelectionChanged += ComboBox_SelectionChanged;
            _uiControls.Add("PatternsCombobox", comboBox);

            comboBox.SelectedIndex = 0;

            AddFillColorControls();
            AddStrokeColorControls();
            AddStrokeThicknessControls();
            base.InitControlPanel();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                // Явне приведення до типу кортежу
                if (selectedItem.Tag is ValueTuple<double, string, string> tag)
                {
                    (Angle, _axiom, _ruleF) = tag;
                    _isGeometryDirty = true;
                    if (selectedItem.Content.Equals("Квадратична сніжинка Коха") && Iterations > 5) Iterations = 5;
                    InvalidateCanvas();
                }
            }
        }


        protected override Point[][] GetElementGeometry()
        {
            if (_isGeometryDirty)
            {
                var lSystemSequence = GenerateLSystemSequence();
                _cachedGeometry = new[] { InterpretLSystem(lSystemSequence).ToArray() };
                _isGeometryDirty = false;
            }

            return _cachedGeometry;
        }

        private List<char> GenerateLSystemSequence()
        {
            var current = new List<char>(_axiom);

            for (int i = 0; i < Iterations; i++)
            {
                var next = new List<char>();
                foreach (var c in current)
                {
                    if (c == 'F')
                    {
                        next.AddRange(_ruleF);
                    }
                    else
                    {
                        next.Add(c);
                    }
                }
                current = next;
            }

            return current;
        }

        private List<Point> InterpretLSystem(List<char> lSystemSequence)
        {
            var points = new List<Point>();
            var currentPosition = Position;
            double currentAngle = 0;

            points.Add(currentPosition);

            foreach (var c in lSystemSequence)
            {
                switch (c)
                {
                    case 'F':
                        var newX = currentPosition.X + StepSize * Math.Cos(currentAngle * Math.PI / 180);
                        var newY = currentPosition.Y + StepSize * Math.Sin(currentAngle * Math.PI / 180);
                        currentPosition = new Point(newX, newY);
                        points.Add(currentPosition);
                        break;

                    case '+':
                        currentAngle -= Angle;
                        break;

                    case '-':
                        currentAngle += Angle;
                        break;
                }
            }

            return points;
        }

        protected override Point GetAnchorDefaultPosition() => Position;

        public override void MoveElement(Vector offset)
        {
            base.MoveElement(offset);

            if (InputManager.CtrlPressed || InputManager.SpacePressed)
            {
                return;
            }

            Position = SnappingEnabled ? Position.OffsetAndSpanPoint(offset) : Position.OffsetPoint(offset);
        }

        public override void RotateElement(Point anchorPoint, double degrees)
        {
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
        }
    }
}

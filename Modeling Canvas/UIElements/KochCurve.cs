using Modeling_Canvas.Extensions;
using System.Windows;

namespace Modeling_Canvas.UIElements
{
    public class KochCurve : Element
    {
        private int _iterations = 3;

        private double _stepSize = 10;

        private double _angle = 60;

        private const string Axiom = "F++F++F"; // Аксіома

        private const string RuleF = "F-F++F-F"; // Правило для "F"

        protected Point _position;

        public int Iterations
        {
            get => _iterations;
            set
            {
                if (_iterations != value)
                {
                    _iterations = value;
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
                    20
                    );

            _uiControls.Add("Iterations", iterationsBox);

            var sizeBox =
                WpfHelper.CreateLabeledTextBox(
                    this,
                    nameof(StepSize),
                    "Step size"
                    );

            _uiControls.Add("StepSize", sizeBox);

            var angleBox =
                WpfHelper.CreateLabeledTextBox(
                    this,
                    nameof(Angle),
                    "Rotation angle"
                    );

            _uiControls.Add("Angle", angleBox);

            AddFillColorControls();
            AddStrokeColorControls();
            AddStrokeThicknessControls();
            base.InitControlPanel();
        }


        protected override Point[][] GetElementGeometry()
        {
            string lSystemString = GenerateLSystemString();
            var points = InterpretLSystem(lSystemString);

            return new[] { points.ToArray() };
        }


        private string GenerateLSystemString()
        {
            string current = Axiom;

            for (int i = 0; i < Iterations; i++)
            {
                string next = "";
                foreach (char c in current)
                {
                    if (c == 'F')
                    {
                        next += RuleF;
                    }
                    else
                    {
                        next += c;
                    }
                }
                current = next;
            }

            return current;
        }

        private List<Point> InterpretLSystem(string lSystemString)
        {
            var points = new List<Point>();
            var currentPosition = Position;
            double currentAngle = 0;

            points.Add(currentPosition);

            foreach (char c in lSystemString)
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
            //Position = Position.RotatePoint(anchorPoint, degrees);
        }

        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double ScaleFactor)
        {
            //Position = Position.ScalePoint(anchorPoint, scaleVector);
        }
    }
}

using System.ComponentModel;
using System.Reflection;

namespace Modeling_Canvas.Models
{
    public class HypocycloidModel : INotifyPropertyChanged
    {
        public double MinRadius { get; } = 0.5;
        public double MinAngle { get; } = 0;
        public double MaxAmgle { get; } = 1080;

        private double _distance = 1;
        private double _angle = 720;
        private double _rotationAngle = 0;
        private double _largeRadius = 5;
        private double _smallRadius = 1;
        private double _maxLargeCircleRadius = 25;
        public double Distance
        {
            get => _distance;
            set
            {
                if (_distance != value)
                {
                    _distance = value;
                    OnPropertyChanged(nameof(Distance));
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
                    OnPropertyChanged(nameof(Angle));
                }
            }
        }
        public double RotationAngle
        {
            get => _rotationAngle;
            set
            {
                if (_rotationAngle != value)
                {
                    _rotationAngle = value;
                    OnPropertyChanged(nameof(RotationAngle));
                }
            }
        }
        public double LargeRadius
        {
            get => _largeRadius;
            set
            {
                if (_largeRadius != value)
                {
                    if (value > SmallRadius)
                    {
                        _largeRadius = value;
                    }
                    OnPropertyChanged(nameof(LargeRadius));
                }
            }
        }
        public double SmallRadius
        {
            get => _smallRadius;
            set
            {
                if (_smallRadius != value)
                {
                    if (value < Distance || (_smallRadius == Distance && value < LargeRadius))
                    {
                        Distance = value;
                    }
                    if (value < LargeRadius)
                    {
                        _smallRadius = value;
                    }
                    OnPropertyChanged(nameof(SmallRadius));
                }
            }
        }
        public double MaxLargeCircleRadius
        {
            get => _maxLargeCircleRadius;
            set
            {
                if (_maxLargeCircleRadius != value)
                {
                    _maxLargeCircleRadius = value;
                    OnPropertyChanged(nameof(MaxLargeCircleRadius));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool AreValuesEqual(HypocycloidModel model2)
        {
            if (model2 == null)
                throw new ArgumentNullException(nameof(model2));

            var properties = typeof(HypocycloidModel)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.PropertyType == typeof(double));

            // Compare the values of each property in both models
            foreach (var property in properties)
            {
                var value1 = (double)property.GetValue(this);
                var value2 = (double)property.GetValue(model2);

                if (value1 != value2)
                    return false;
            }

            return true;
        }
    }

}

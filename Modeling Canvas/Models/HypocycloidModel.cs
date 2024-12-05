using Modeling_Canvas.UIElements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Modeling_Canvas.Models
{
    public class HypocycloidModel : INotifyPropertyChanged
    {
        public double MinRadius { get; } = 0.5;
        public double MinAngle { get; } = 0;
        public double MaxAmgle { get; } = 1080;

        public double _maxLargeCircleRadius = 25;
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

        private double _distance = 1;
        private double _angle = 720;
        private double _rotationAngle = 0;
        private double _largeRadius = 5;
        private double _smallRadius = 1;
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
                    if(value > SmallRadius)
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}

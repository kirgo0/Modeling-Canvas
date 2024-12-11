using System.ComponentModel;
using System.Windows;

namespace Modeling_Canvas.Models
{
    public class HypocycloidCalculationsModel : INotifyPropertyChanged
    {
        private bool _showRadiusCurvature;
        private bool _showHypocycloidArea = true;
        private bool _showInfelctionPoints = false;
        private bool _showRingArea;
        private bool _showArcLength = true;

        private double _radiusCurvature;
        private double _hypocycloidArea;
        private List<Point> _infelctionPoints = new();
        private double _ringArea;
        private double _arcLength;
        public bool ShowRadiusCurvature
        {
            get => _showRadiusCurvature;
            set {
                if (_showRadiusCurvature != value)
                {
                    _showRadiusCurvature = value;
                    OnPropertyChanged(nameof(ShowRadiusCurvature));
                }
            }
        }
        public bool ShowHypocycloidArea
        {
            get => _showHypocycloidArea;
            set
            {
                if (_showHypocycloidArea != value)
                {
                    _showHypocycloidArea = value;
                    OnPropertyChanged(nameof(ShowHypocycloidArea));
                }
            }
        }
        public bool ShowInflectionPoints
        {
            get => _showInfelctionPoints;
            set
            {
                if (_showInfelctionPoints != value)
                {
                    _showInfelctionPoints = value;
                    OnPropertyChanged(nameof(ShowInflectionPoints));
                }
            }
        }
        public bool ShowRingArea
        {
            get => _showRingArea;
            set
            {
                if (_showRingArea != value)
                {
                    _showRingArea = value;
                    OnPropertyChanged(nameof(ShowRingArea));
                }
            }
        }
        public bool ShowArcLength
        {
            get => _showArcLength;
            set
            {
                if (_showArcLength != value)
                {
                    _showArcLength = value;
                    OnPropertyChanged(nameof(ShowArcLength));
                }
            }
        }

        public double RadiusCurvature
        {
            get => _radiusCurvature;
            set
            {
                if (_radiusCurvature != value)
                {
                    _radiusCurvature = value;
                    OnPropertyChanged(nameof(RadiusCurvature));
                }
            }
        }
        public double HypocycloidArea
        {
            get => _hypocycloidArea;
            set
            {
                if (_hypocycloidArea != value)
                {
                    _hypocycloidArea = value;
                    OnPropertyChanged(nameof(HypocycloidArea));
                }
            }
        }
        public List<Point> InflectionPoints
        {
            get => _infelctionPoints;
            set
            {
                if (_infelctionPoints != value)
                {
                    _infelctionPoints = value;
                    OnPropertyChanged(nameof(InflectionPoints));
                }
            }
        }
        public double RingArea
        {
            get => _ringArea;
            set
            {
                if (_ringArea != value)
                {
                    _ringArea = value;
                    OnPropertyChanged(nameof(RingArea));
                }
            }
        }
        public double ArcLength
        {
            get => _arcLength;
            set
            {
                if (_arcLength != value)
                {
                    _arcLength = value;
                    OnPropertyChanged(nameof(ArcLength));
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

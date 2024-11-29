using System.ComponentModel;

namespace Modeling_Canvas.Models
{
    public class ProjectiveModel : INotifyPropertyChanged
    {
        private double _xx, _yx, _ox, _xy, _yy, _oy, _wx, _wy, _wo;

        public double Xx { get => _xx; set { _xx = value; OnPropertyChanged(nameof(Xx)); } }
        public double Yx { get => _yx; set { _yx = value; OnPropertyChanged(nameof(Yx)); } }
        public double Ox { get => _ox; set { _ox = value; OnPropertyChanged(nameof(Ox)); } }
        public double Xy { get => _xy; set { _xy = value; OnPropertyChanged(nameof(Xy)); } }
        public double Yy { get => _yy; set { _yy = value; OnPropertyChanged(nameof(Yy)); } }
        public double Oy { get => _oy; set { _oy = value; OnPropertyChanged(nameof(Oy)); } }
        public double wX { get => _wx; set { _wx = value; OnPropertyChanged(nameof(wX)); } }
        public double wY { get => _wy; set { _wy = value; OnPropertyChanged(nameof(wY)); } }
        public double wO { get => _wo; set { _wo = value; OnPropertyChanged(nameof(wO)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

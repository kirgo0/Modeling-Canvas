using System.ComponentModel;

namespace Modeling_Canvas.Models
{
    public class AffineModel : INotifyPropertyChanged
    {
        private double _xx = 1;
        public double Xx
        {
            get => _xx;
            set { _xx = value; OnPropertyChanged(nameof(Xx)); }
        }

        private double _xy;
        public double Xy
        {
            get => _xy;
            set { _xy = value; OnPropertyChanged(nameof(Xy)); }
        }

        private double _yx;
        public double Yx
        {
            get => _yx;
            set { _yx = value; OnPropertyChanged(nameof(Yx)); }
        }

        private double _yy = 1;
        public double Yy
        {
            get => _yy;
            set { _yy = value; OnPropertyChanged(nameof(Yy)); }
        }

        private double _ox;
        public double Ox
        {
            get => _ox;
            set { _ox = value; OnPropertyChanged(nameof(Ox)); }
        }

        private double _oy;
        public double Oy
        {
            get => _oy;
            set { _oy = value; OnPropertyChanged(nameof(Oy)); }
        }

        public double CanvasHeight { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsDefaults { get => 
                Xx == 1 && Xy == 0 && Ox == 0 &&
                Yy == 1 && Yx == 0 && Oy == 0; 
        }

        public void Reset()
        {
            Xx = 1; 
            Xy = 0; 
            Ox = 0; 
            Yy = 1; 
            Yx = 0; 
            Oy = 0;
        }

        public override string ToString()
        {
            return $"Xx:{Xx}|Xy:{Xy}|Yx:{Yx}|Yy:{Yy}|Ox:{Ox}|Oy:{Oy}";
        }
    }
}

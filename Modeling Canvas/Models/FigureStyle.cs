using System.ComponentModel;
using System.Windows.Media;

namespace Modeling_Canvas.Models
{
    public class FigureStyle : INotifyPropertyChanged
    {
        private Brush _fillColor = null;


        private double _strokeThickness = 1;


        private Brush _strokeColor = Brushes.Black;


        private Pen _strokePen = null;


        public Brush FillColor
        {
            get => _fillColor;
            set
            {
                if (_fillColor != value)
                {
                    _fillColor = value;
                    OnPropertyChanged(nameof(FillColor));
                }
            }
        }

        public Brush StrokeColor
        {
            get => _strokeColor;
            set
            {
                if (_strokeColor != value)
                {
                    _strokeColor = value;
                    OnPropertyChanged(nameof(StrokeColor));
                    OnPropertyChanged(nameof(StrokePen));
                }
            }
        }

        public double StrokeThickness
        {
            get => _strokeThickness;
            set
            {
                if (_strokeThickness != value)
                {
                    _strokeThickness = value;
                    OnPropertyChanged(nameof(StrokeThickness));
                    OnPropertyChanged(nameof(StrokePen));
                }
            }
        }

        public Pen StrokePen
        {
            get => _strokePen ?? new Pen(StrokeColor, StrokeThickness);
            set
            {
                if (_strokePen != value)
                {
                    _strokePen = value;
                    OnPropertyChanged(nameof(StrokePen));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

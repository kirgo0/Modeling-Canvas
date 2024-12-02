using Modeling_Canvas.Extensions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIELements
{
    public class DraggablePoint : CustomPoint, INotifyPropertyChanged
    {
        public double DragRadius { get; set; } = 10;
        public Point Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = new Point(Math.Round(value.X, PositionPrecision), Math.Round(value.Y, PositionPrecision));
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public DraggablePoint(CustomCanvas canvas) : base(canvas)
        {
        }
        public DraggablePoint(CustomCanvas canvas, Point position) : base(canvas)
        {
            Position = position;
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            base.DefaultRender(dc);
            dc.DrawCircle(Brushes.Transparent, new Pen(Stroke, 0), new Point(0, 0), DragRadius, 100);

        }

        public Action<DraggablePoint, Vector> OverrideMoveAction;
        public Action<DraggablePoint, Vector> MoveAction;

        public override void MoveElement(Vector offset)
        {
            if (OverrideMoveAction is not null)
            {
                OverrideMoveAction?.Invoke(this, offset);
                return;
            }

            base.MoveElement(offset);
            MoveAction?.Invoke(this, offset);

            if (InputManager.CtrlPressed || InputManager.SpacePressed)
            {
                return;
            }

            Position = SnappingEnabled ? Position.OffsetAndSpanPoint(offset) : Position.OffsetPoint(offset);
        }

        public Point PixelPosition { get => new Point(Position.X * UnitSize, -Position.Y * UnitSize); }
            
        public Point CanvasPixelPosition { get => new Point(Canvas.ActualWidth / 2 + Position.X * UnitSize, Canvas.ActualHeight / 2 - Position.Y * UnitSize); }

        public Action<MouseButtonEventArgs> MouseLeftButtonDownAction { get; set; }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            MouseLeftButtonDownAction?.Invoke(e);
            e.Handled = true;
            base.OnMouseLeftButtonDown(e);
        }

        public Func<DraggablePoint, string>? OverrideToStringAction;
        public override string ToString()
        {
            if (OverrideToStringAction is not null) return OverrideToStringAction.Invoke(this);
            return base.ToString();
        }
    }

}
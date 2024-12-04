using System.Windows;
using System.Windows.Controls;

namespace Modeling_Canvas.UIElements
{
    public partial class CustomCircle
    {
        public virtual void AddRadiusControls()
        {
            var panel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };
            var radiusLabel = new TextBlock { Text = $"Radius: {Radius}", TextAlignment = TextAlignment.Center };
            var radiusSlider = new Slider
            {
                Minimum = 1,
                Maximum = Math.Max(Canvas.ActualWidth / 2 / UnitSize, Radius),
                Value = Radius,
                TickFrequency = 0.1,
                IsSnapToTickEnabled = true,
                Width = 200
            };
            radiusSlider.ValueChanged += (s, e) =>
            {
                Radius = Math.Round(e.NewValue, 2); // Update Radius
                radiusLabel.Text = $"Radius: {Radius}";
                InvalidateCanvas();
            };
            panel.Children.Add(radiusLabel);
            panel.Children.Add(radiusSlider);
            AddElementToControlPanel(panel);
        }
        protected virtual void AddCenterControls()
        {
            AddDefaultPointControls(
                "Center",
                this,
                "Center.X",
                "Center.Y",
                (x) =>
                {
                    OverrideAnchorPoint = true;
                    var difference = Center.X - x;
                    Center = new Point(x, Center.Y);
                    AnchorPoint.Position = new Point(AnchorPoint.Position.X - difference, AnchorPoint.Position.Y);
                    InvalidateCanvas();
                },
                (y) =>
                {
                    OverrideAnchorPoint = true;
                    var difference = Center.Y - y;
                    Center = new Point(Center.X, y);
                    AnchorPoint.Position = new Point(AnchorPoint.Position.X, AnchorPoint.Position.Y - difference);
                    InvalidateCanvas();
                }
                );
        }
    }
}

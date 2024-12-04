
using System.Windows.Controls;
using System.Windows;

namespace Modeling_Canvas.UIElements
{
    public partial class Hypocycloid
    {
        protected virtual void AddDistanceControls()
        {
            var panel = GetDefaultVerticalPanel();
            var distanceLabel = new TextBlock { Text = $"Distance: {Distance}", TextAlignment = TextAlignment.Center };
            var distanceSlider = new Slider
            {
                Minimum = 0.1,
                Maximum = SmallCircle.Radius,
                Value = Distance,
                TickFrequency = 0.1,
                IsSnapToTickEnabled = true,
                Width = 200
            };
            distanceSlider.ValueChanged += (s, e) =>
            {
                Distance = e.NewValue;
                distanceLabel.Text = $"Distance: {Distance}";
                InvalidateVisual();
            };
            panel.Children.Add(distanceLabel);
            panel.Children.Add(distanceSlider);
            AddElementToControlPanel(panel);
        }
        protected void AddRadiusControls()
        {
            // Large Radius Controls
            var largePanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };
            var largeRadiusLabel = new TextBlock { Text = $"Large Radius: {LargeRadius}", TextAlignment = TextAlignment.Center };

            var largeRadiusSlider = new Slider
            {
                Minimum = SmallRadius,
                Maximum = Math.Max(Canvas.ActualWidth / 2 / UnitSize, LargeRadius),
                Value = LargeRadius,
                TickFrequency = 0.1,
                IsSnapToTickEnabled = true,
                Width = 200
            };

            // Small Radius Controls
            var smallPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };
            var smallRadiusLabel = new TextBlock { Text = $"Small Radius: {SmallRadius}", TextAlignment = TextAlignment.Center };

            var smallRadiusSlider = new Slider
            {
                Minimum = 1,
                Maximum = LargeRadius, // Initially set to LargeRadius.
                Value = SmallRadius,
                TickFrequency = 0.1,
                IsSnapToTickEnabled = true,
                Width = 200
            };

            // Large Radius ValueChanged Event
            largeRadiusSlider.ValueChanged += (s, e) =>
            {
                var newLargeRadius = Math.Round(e.NewValue, 2);
                LargeCircle.Radius = newLargeRadius;

                largeRadiusLabel.Text = $"Large Radius: {LargeRadius}";
                smallRadiusSlider.Maximum = LargeRadius; // Update SmallRadius slider max value dynamically.

                InvalidateCanvas();
            };

            // Small Radius ValueChanged Event
            smallRadiusSlider.ValueChanged += (s, e) =>
            {
                var newSmallRadius = Math.Round(e.NewValue, 2);
                if (SmallRadius == Distance || SmallRadius < Distance)
                {
                    Distance = newSmallRadius;
                }
                SmallCircle.Radius = newSmallRadius;

                smallRadiusLabel.Text = $"Small Radius: {SmallRadius}";
                InvalidateCanvas();
            };

            // Add both panels to the control panel
            largePanel.Children.Add(largeRadiusLabel);
            largePanel.Children.Add(largeRadiusSlider);
            AddElementToControlPanel(largePanel);

            smallPanel.Children.Add(smallRadiusLabel);
            smallPanel.Children.Add(smallRadiusSlider);
            AddElementToControlPanel(smallPanel);
        }
        protected virtual void AddAngleControls()
        {
            var panel = GetDefaultVerticalPanel();
            var distanceLabel = new TextBlock { Text = $"Angle: {Angle}", TextAlignment = TextAlignment.Center };
            var distanceSlider = new Slider
            {
                Minimum = 0,
                Maximum = 1080,
                Value = Angle,
                TickFrequency = 1,
                IsSnapToTickEnabled = true,
                Width = 200
            };
            distanceSlider.ValueChanged += (s, e) =>
            {
                Angle = e.NewValue;
                distanceLabel.Text = $"Angle: {Angle}";
                UpdateUIControls();
                InvalidateCanvas();
            };
            panel.Children.Add(distanceLabel);
            panel.Children.Add(distanceSlider);
            AddElementToControlPanel(panel);
        }

    }
}

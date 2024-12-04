using System.Windows.Controls;
using System.Windows;

namespace Modeling_Canvas.UIElements
{
    public partial class SegmentedCircle
    {
        protected virtual void AddDegreesControls()
        {
            var startDegPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };
            var startDegLabel = new TextBlock { Text = $"StartDegrees: {StartDegrees}", TextAlignment = TextAlignment.Center };
            var startDegSlider = new Slider
            {
                Minimum = 0,
                Maximum = 360,
                Value = StartDegrees,
                TickFrequency = 1,
                IsSnapToTickEnabled = true,
                Width = 200
            };

            startDegSlider.ValueChanged += (s, e) =>
            {
                StartDegrees = Math.Round(e.NewValue); // Update Radius
                startDegLabel.Text = $"StartDegrees: {StartDegrees}";
                InvalidateCanvas();
            };
            startDegPanel.Children.Add(startDegLabel);
            startDegPanel.Children.Add(startDegSlider);

            AddElementToControlPanel(startDegPanel);
            var endDegPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };
            var endDegLabel = new TextBlock { Text = $"EndDegrees: {EndDegrees}", TextAlignment = TextAlignment.Center };
            var endDegSlider = new Slider
            {
                Minimum = 0,
                Maximum = 360,
                Value = EndDegrees,
                TickFrequency = 0.5,
                IsSnapToTickEnabled = true,
                Width = 200
            };

            endDegSlider.ValueChanged += (s, e) =>
            {
                EndDegrees = Math.Round(e.NewValue); // Update Radius
                endDegLabel.Text = $"EndDegrees: {EndDegrees}";
                InvalidateCanvas();
            };
            endDegPanel.Children.Add(endDegLabel);
            endDegPanel.Children.Add(endDegSlider);
            AddElementToControlPanel(endDegPanel);
        }
    }
}

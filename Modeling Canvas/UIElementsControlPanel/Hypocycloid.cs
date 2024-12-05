
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace Modeling_Canvas.UIElements
{
    public partial class Hypocycloid
    {
        private void CreateUIControls()
        {
            var distancePanel = CreateSliderControl(
                "Distance",
                nameof(Model.Distance),
                null,                      
                nameof(Model.SmallRadius),
                0.1
            );

            _controls[nameof(Model.Distance)] = distancePanel;

            var anglePanel = CreateSliderControl(
                "Angle",
                nameof(Model.Angle),
                nameof(Model.MinAngle),
                nameof(Model.MaxAmgle),
                1
            );

            _controls[nameof(Model.Angle)] = anglePanel;

            var largeRadiusPanel = CreateSliderControl(
                "Large Radius",
                nameof(Model.LargeRadius),
                nameof(Model.SmallRadius),
                nameof(Model.MaxLargeCircleRadius),
                0.1
            );
            _controls[nameof(Model.LargeRadius)] = largeRadiusPanel;

            var smallRadiusPanel = CreateSliderControl(
                "Small Radius",
                nameof(Model.SmallRadius),
                nameof(Model.MinRadius),
                nameof(Model.LargeRadius),
                0.1
            );
            _controls[nameof(Model.SmallRadius)] = smallRadiusPanel;
        }

        private UIElement CreateSliderControl(
    string labelText,
    string bindingPath,
    string minBindingPath,
    string maxBindingPath,
    double tickFrequency = 0.5)
        {
            var panel = GetDefaultVerticalPanel();

            var label = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var labelBinding = new Binding(bindingPath)
            {
                Source = Model,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                StringFormat = $"{labelText}: {{0:F2}}"
            };
            label.SetBinding(TextBlock.TextProperty, labelBinding);

            var slider = new Slider
            {
                TickFrequency = tickFrequency,
                IsSnapToTickEnabled = true,
                Width = 200
            };

            var valueBinding = new Binding(bindingPath)
            {
                Source = Model,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            slider.SetBinding(Slider.ValueProperty, valueBinding);

            if (!string.IsNullOrEmpty(minBindingPath))
            {
                var minBinding = new Binding(minBindingPath)
                {
                    Source = Model,
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                slider.SetBinding(Slider.MinimumProperty, minBinding);
            }

            if (!string.IsNullOrEmpty(maxBindingPath))
            {
                var maxBinding = new Binding(maxBindingPath)
                {
                    Source = Model,
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                slider.SetBinding(Slider.MaximumProperty, maxBinding);
            }

            panel.Children.Add(label);
            panel.Children.Add(slider);

            return panel;
        }
    }
}

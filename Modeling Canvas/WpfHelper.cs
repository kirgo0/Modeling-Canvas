using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Modeling_Canvas
{
    public static class WpfHelper
    {
        public static FrameworkElement CreateSliderControl(
            string labelText,
            object model,
            string bindingPath,
            string minBindingPath,
            string maxBindingPath,
            double tickFrequency = 0.5,
            object? visibilityBindingSource = null,
            string? visibilityBindingPath = null
        )
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };

            panel.AddVisibilityBinding(visibilityBindingSource, visibilityBindingPath);

            var label = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var labelBinding = new Binding(bindingPath)
            {
                Source = model,
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
                Source = model,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            slider.SetBinding(Slider.ValueProperty, valueBinding);

            if (!string.IsNullOrEmpty(minBindingPath))
            {
                var minBinding = new Binding(minBindingPath)
                {
                    Source = model,
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                slider.SetBinding(Slider.MinimumProperty, minBinding);
            }

            if (!string.IsNullOrEmpty(maxBindingPath))
            {
                var maxBinding = new Binding(maxBindingPath)
                {
                    Source = model,
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                slider.SetBinding(Slider.MaximumProperty, maxBinding);
            }

            panel.Children.Add(label);
            panel.Children.Add(slider);

            return panel;
        }
        public static FrameworkElement CreateLabeledCheckBox(string labelText, object bindingSource, string bindingPath)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };

            var label = new TextBlock
            {
                Text = labelText,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            };

            var checkBox = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            var binding = new Binding(bindingPath)
            {
                Source = bindingSource,
                Mode = BindingMode.TwoWay
            };
            checkBox.SetBinding(CheckBox.IsCheckedProperty, binding);

            panel.Children.Add(label);
            panel.Children.Add(checkBox);

            return panel;
        }


        public static Button CreateButton(
            Action clickAction,
            string? content = null,
            object? contentBindingSource = null,
            string? contentBindingPath = null,
            object? visibilityBindingSource = null,
            string? visibilityBindingPath = null,
            double width = 100,
            double height = 20,
            Thickness? margin = null
            )
        {
            var button = new Button
            {
                Width = width,
                Height = height,
                Margin = margin ?? new Thickness(5),
                IsTabStop = false
            };

            // Bind the Content property
            if (contentBindingSource != null && string.IsNullOrEmpty(contentBindingPath))
            {
                var contentBinding = new Binding(contentBindingPath)
                {
                    Source = contentBindingSource,
                    Mode = BindingMode.OneWay
                };
                button.SetBinding(Button.ContentProperty, contentBinding);
            }
            else
            {
                button.Content = content;
            }

            button.AddVisibilityBinding(visibilityBindingSource, visibilityBindingPath);

            button.Click += (s, e) => clickAction?.Invoke();

            return button;
        }

        public static void AddVisibilityBinding(this FrameworkElement element, object visibilityBindingSource, string visibilityBindingPath, BindingMode mode = BindingMode.OneWay)
        {
            if (visibilityBindingSource != null && !string.IsNullOrEmpty(visibilityBindingPath))
            {
                var visibilityBinding = new Binding(visibilityBindingPath)
                {
                    Source = visibilityBindingSource,
                    Mode = mode,
                    Converter = new BooleanToVisibilityConverter()
                };
                element.SetBinding(UIElement.VisibilityProperty, visibilityBinding);
            }
        }

        public static void AddIsDisabledBinding(this Button element, object isEnabledBindingSource, string isEnabledBindingPath, BindingMode mode = BindingMode.OneWay)
        {
            if (isEnabledBindingSource != null && !string.IsNullOrEmpty(isEnabledBindingPath))
            {
                var isEnabledBinding = new Binding(isEnabledBindingPath)
                {
                    Source = isEnabledBindingSource,
                    Mode = mode
                };
                element.SetBinding(UIElement.IsEnabledProperty, isEnabledBinding);
            }
        }
    }

}
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Modeling_Canvas
{
    public static class WpfHelper
    {
        public static FrameworkElement CreateSliderControl(
            string labelText,
            object model,
            string valueBindingPath,
            string? minBindingPath = null,
            string? maxBindingPath = null,
            double tickFrequency = 0.5,
            double marginBottom = 30,
            Orientation panelOrientation = Orientation.Vertical
        )
        {
            var panel = CreateDefaultPanel(marginBottom, panelOrientation);

            var label = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var labelBinding = new Binding(valueBindingPath)
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

            var valueBinding = new Binding(valueBindingPath)
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

        public static FrameworkElement CreateSliderControl<T>(
            string labelText,
            object model,
            string valueBindingPath,
            T minValue,
            T maxValue,
            double tickFrequency = 0.5,
            double marginBottom = 30,
            Orientation panelOrientation = Orientation.Vertical
        ) where T : struct, IComparable, IConvertible, IFormattable
        {
            var panel = CreateDefaultPanel(marginBottom, panelOrientation);

            var label = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var labelBinding = new Binding(valueBindingPath)
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

            var valueBinding = new Binding(valueBindingPath)
            {
                Source = model,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            slider.SetBinding(Slider.ValueProperty, valueBinding);

            slider.Minimum = Convert.ToDouble(minValue);

            slider.Maximum = Convert.ToDouble(maxValue);

            panel.Children.Add(label);
            panel.Children.Add(slider);

            return panel;
        }

        public static FrameworkElement CreateValueTextBlock(
            string labelText,
            object model,
            string bindingPath
        )
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };

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

            panel.Children.Add(label);

            return panel;
        }

        public static FrameworkElement CreateLabeledCheckBox(
            string labelText,
            object bindingSource,
            string bindingPath,
            double marginBottom = 5,
            Orientation panelOrientation = Orientation.Horizontal
        )
        {
            var panel = CreateDefaultPanel(marginBottom, panelOrientation);

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

            button.Click += (s, e) => clickAction?.Invoke();

            return button;
        }

        public static FrameworkElement CreateDefaultPointControls(
            string labelText,
            object source,
            string xPath,
            string yPath,
            Action<double> xValueChanged,
            Action<double> yValueChanged,
            double marginBottom = 30,
            Orientation panelOrientation = Orientation.Vertical

        )
        {
            var panel = CreateDefaultPanel(marginBottom, panelOrientation);

            // Create and add StackPanel for X position
            var xPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };

            var label = new TextBlock { Text = labelText, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            var xLabel = new TextBlock { Text = "X:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) };
            var xInput = new TextBox
            {
                Width = 100
            };

            var xBinding = new Binding(xPath)
            {
                Source = source,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            xInput.SetBinding(TextBox.TextProperty, xBinding);

            xInput.PreviewKeyUp += (sender, e) =>
            {
                if (double.TryParse(xInput.Text, out double newX))
                {
                    xValueChanged(newX);
                }
            };

            xPanel.Children.Add(xLabel);
            xPanel.Children.Add(xInput);

            // Create and add StackPanel for Y position
            var yPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };

            var yLabel = new TextBlock { Text = "Y:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) };
            var yInput = new TextBox
            {
                Width = 100
            };

            var yBinding = new Binding(yPath)
            {
                Source = source,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            yInput.SetBinding(TextBox.TextProperty, yBinding);

            yInput.PreviewKeyUp += (sender, e) =>
            {
                if (double.TryParse(yInput.Text, out double newY))
                {
                    yValueChanged(newY);
                }
            };

            yPanel.Children.Add(yLabel);
            yPanel.Children.Add(yInput);
            panel.Children.Add(label);
            panel.Children.Add(xPanel);
            panel.Children.Add(yPanel);

            return panel;
        }


        public static FrameworkElement CreateLabeledTextBox(
            object bindingSource,
            string bindingPath,
            string? labelText = null,
            double width = 100,
            double marginBottom = 30,
            Orientation panelOrientation = Orientation.Vertical
        )
        {
            var panel = CreateDefaultPanel(marginBottom, panelOrientation);

            if (!string.IsNullOrEmpty(labelText))
            {
                var label = new TextBlock
                {
                    Text = labelText,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                panel.Children.Add(label);
            }

            var textBox = new TextBox
            {
                Width = width,
                VerticalAlignment = VerticalAlignment.Center
            };

            var binding = new Binding(bindingPath)
            {
                Source = bindingSource,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            textBox.SetBinding(TextBox.TextProperty, binding);

            panel.Children.Add(textBox);

            return panel;
        }

        public static FrameworkElement CreateColorPicker(
            string labelText,
            object bindingSource,
            string bindingPath,
            double width = 200,
            double marginBottom = 30
        )
        {
            var panel = CreateDefaultPanel(marginBottom);

            if (!string.IsNullOrEmpty(labelText))
            {
                var label = new TextBlock
                {
                    Text = labelText,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                panel.Children.Add(label);
            }

            var colorPicker = new ColorPicker
            {
                Width = width,
                AdvancedTabHeader = string.Empty // Optional customization
            };

            // Bind the SelectedColor property of the ColorPicker to the source Brush property
            var colorBinding = new Binding(bindingPath)
            {
                Source = bindingSource,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new BrushToColorConverter() // You need to implement this converter
            };
            colorPicker.SetBinding(ColorPicker.SelectedColorProperty, colorBinding);

            panel.Children.Add(colorPicker);

            return panel;
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

        public static StackPanel CreateDefaultPanel(double marginBottom = 30, Orientation orientation = Orientation.Vertical)
        {
            return new StackPanel
            {
                Orientation = orientation,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5, 5, 5, marginBottom)
            };
        }

        public class BrushToColorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is SolidColorBrush brush)
                {
                    return brush.Color;
                }
                return Colors.Transparent; // Default color if not a SolidColorBrush
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is Color color)
                {
                    return new SolidColorBrush(color);
                }
                return new SolidColorBrush(Colors.Transparent); // Default brush
            }
        }

    }

}
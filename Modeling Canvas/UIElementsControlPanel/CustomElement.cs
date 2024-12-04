using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using Xceed.Wpf.Toolkit;

namespace Modeling_Canvas.UIElements
{
    public abstract partial class CustomElement
    {
        protected void AddDefaultPointControls(
            string labelText,
            object source,
            string xPath,
            string yPath,
            Action<double> xValueChanged,
            Action<double> yValueChanged)
        {
            var panel = GetDefaultVerticalPanel();
            // Create and add StackPanel for X position
            var xPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center };

            var label = new TextBlock { Text = labelText, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            var xLabel = new TextBlock { Text = "Position X:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) };
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

            var yLabel = new TextBlock { Text = "Position Y:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) };
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
            AddElementToControlPanel(panel);
        }

        protected virtual void AddAnchorControls()
        {
            var panel = GetDefaultHorizontalPanel();
            var isClosedLabel = new TextBlock { Text = "Anchor visible:" };

            var isClosedCheckBox = new CheckBox
            {
                IsChecked = AnchorVisible
            };

            isClosedCheckBox.Checked += (s, e) =>
            {
                AnchorVisible = true;
                InvalidateVisual();
            };
            isClosedCheckBox.Unchecked += (s, e) =>
            {
                AnchorVisible = false;
                InvalidateVisual();
            };

            panel.Children.Add(isClosedLabel);
            panel.Children.Add(isClosedCheckBox);
            AddElementToControlPanel(panel);

            AddDefaultPointControls(
                "Anchor Point",
                AnchorPoint,
                "Position.X",
                "Position.Y",
                (x) =>
                {
                    OverrideAnchorPoint = true;
                    AnchorPoint.Position = new Point(x, AnchorPoint.Position.Y);
                    InvalidateCanvas();
                },
                (y) =>
                {
                    OverrideAnchorPoint = true;
                    AnchorPoint.Position = new Point(AnchorPoint.Position.X, y);
                    InvalidateCanvas();
                }
            );
        }

        protected virtual void AddFillColorControls()
        {
            var panel = GetDefaultVerticalPanel();
            var fillLabel = new TextBlock { Text = "Fill Color:", TextAlignment = TextAlignment.Center };
            var fillColorPicker = new ColorPicker
            {
                SelectedColor = Fill is SolidColorBrush solidBrush ? solidBrush.Color : Colors.Transparent,
                AdvancedTabHeader = string.Empty,
                Width = 200
            };

            fillColorPicker.SelectedColorChanged += (s, e) =>
            {
                Fill = new SolidColorBrush(fillColorPicker.SelectedColor ?? Colors.Transparent); // Update Fill
                InvalidateVisual();
            };
            panel.Children.Add(fillLabel);
            panel.Children.Add(fillColorPicker);
            AddElementToControlPanel(panel);
        }

        protected virtual void AddStrokeColorControls()
        {
            var panel = GetDefaultVerticalPanel();
            var strokeLabel = new TextBlock { Text = "Stroke Color:", TextAlignment = TextAlignment.Center };
            var strokeColorPicker = new ColorPicker
            {
                SelectedColor = Stroke is SolidColorBrush solidBrush2 ? solidBrush2.Color : Colors.Black,
                ShowAvailableColors = false,
                Width = 200
            };
            strokeColorPicker.SelectedColorChanged += (s, e) =>
            {
                Stroke = new SolidColorBrush(strokeColorPicker.SelectedColor ?? Colors.Black); // Update Stroke
                InvalidateVisual();
            };
            panel.Children.Add(strokeLabel);
            panel.Children.Add(strokeColorPicker);
            AddElementToControlPanel(panel);
        }

        protected virtual void AddStrokeThicknessControls()
        {
            var panel = GetDefaultVerticalPanel();
            var thicknessLabel = new TextBlock { Text = "Stroke Thickness:", TextAlignment = TextAlignment.Center };
            var thicknessSlider = new Slider
            {
                Minimum = 1,
                Maximum = 10,
                Value = StrokeThickness,
                TickFrequency = 0.1,
                IsSnapToTickEnabled = true,
                Width = 200
            };
            thicknessSlider.ValueChanged += (s, e) =>
            {
                StrokeThickness = e.NewValue; // Update StrokeThickness
                InvalidateVisual();
            };
            panel.Children.Add(thicknessLabel);
            panel.Children.Add(thicknessSlider);
            AddElementToControlPanel(panel);
        }

        protected virtual void AddOffsetControls()
        {
            var panel = GetDefaultVerticalPanel();

            var label = new TextBlock { Text = "Offset", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

            var labelX = new TextBlock { Text = "X: ", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            var labelY = new TextBlock { Text = "Y: ", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

            var inputX = new TextBox
            {
                Width = 100,
                Text = "0"
            };
            var inputY = new TextBox
            {
                Width = 100,
                Text = "0"
            };

            var offsetButton = new Button
            {
                Content = "Offset",
                Width = 100,
                Height = 20,
                Margin = new Thickness(5)
            };

            offsetButton.Click += (s, e) =>
            {
                if (double.TryParse(inputX.Text, out double X) && double.TryParse(inputY.Text, out double Y))
                {
                    MoveElement(new Vector(X * UnitSize, -Y * UnitSize));
                    InvalidateCanvas();
                }
            };

            var panelX = GetDefaultHorizontalPanel(5);
            var panelY = GetDefaultHorizontalPanel(5);

            panelX.Children.Add(labelX);
            panelX.Children.Add(inputX);

            panelY.Children.Add(labelY);
            panelY.Children.Add(inputY);

            panel.Children.Add(label);
            panel.Children.Add(panelX);
            panel.Children.Add(panelY);
            panel.Children.Add(offsetButton);
            AddElementToControlPanel(panel);
        }
        protected virtual void AddRotateControls()
        {
            var panel = GetDefaultVerticalPanel();

            var label = new TextBlock { Text = "Rotating", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            var input = new TextBox
            {
                Width = 100
            };

            var rotateButton = new Button
            {
                Content = "Rotate",
                Width = 100,
                Height = 20,
                Margin = new Thickness(5)
            };

            rotateButton.Click += (s, e) =>
            {
                if (double.TryParse(input.Text, out double value))
                {
                    RotateElement(AnchorPoint.Position, -value);
                    InvalidateCanvas();
                }
            };

            panel.Children.Add(label);
            panel.Children.Add(input);
            panel.Children.Add(rotateButton);
            AddElementToControlPanel(panel);
        }
        protected virtual void AddScaleControls()
        {
            var panel = GetDefaultVerticalPanel();

            var label = new TextBlock { Text = "Scale", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

            var labelX = new TextBlock { Text = "X: ", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            var labelY = new TextBlock { Text = "Y: ", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

            var inputX = new TextBox
            {
                Width = 100,
                Text = "1"
            };
            var inputY = new TextBox
            {
                Width = 100,
                Text = "1"
            };

            var offsetButton = new Button
            {
                Content = "Scale",
                Width = 100,
                Height = 20,
                Margin = new Thickness(5)
            };

            offsetButton.Click += (s, e) =>
            {
                if (double.TryParse(inputX.Text, out double X) && double.TryParse(inputY.Text, out double Y))
                {
                    var factor = Math.Abs(X + Y) / 2;
                    ScaleElement(AnchorPoint.Position, new Vector(X, Y), factor);
                    InvalidateCanvas();
                }
            };

            var panelX = GetDefaultHorizontalPanel(5);
            var panelY = GetDefaultHorizontalPanel(5);

            panelX.Children.Add(labelX);
            panelX.Children.Add(inputX);

            panelY.Children.Add(labelY);
            panelY.Children.Add(inputY);

            panel.Children.Add(label);
            panel.Children.Add(panelX);
            panel.Children.Add(panelY);
            panel.Children.Add(offsetButton);
            AddElementToControlPanel(panel);
        }
        protected void AddElementToControlPanel(UIElement control)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.AddControlToStackPanel(control);
            }
        }

        protected void ClearControlPanel()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ClearControlStack();
            }
        }

        protected StackPanel GetDefaultVerticalPanel(double marginBottom = 30)
        {
            return new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5, 5, 5, marginBottom), HorizontalAlignment = HorizontalAlignment.Center };
        }
        protected StackPanel GetDefaultHorizontalPanel(double marginBottom = 30)
        {
            return new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5, 5, 5, marginBottom), HorizontalAlignment = HorizontalAlignment.Center };
        }

    }
}

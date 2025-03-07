using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public abstract partial class Element
    {
        protected virtual void AddAnchorControls()
        {
            var isAnchorVisibleCheckbox =
                WpfHelper.CreateLabeledCheckBox(
                    "Anchor visible:",
                    this,
                    nameof(AnchorVisible)
                );

            _uiControls.Add("Anchor visible", isAnchorVisibleCheckbox);

            var anchorControls =
                WpfHelper.CreateDefaultPointControls(
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

            _uiControls.Add("Anchor Point", anchorControls);
        }

        protected virtual void AddFillColorControls()
        {
            var fillColor =
                WpfHelper.CreateColorPicker(
                    "Fill Color:",
                    Style,
                    nameof(Style.FillColor)
                );

            _uiControls.Add("Fill Color", fillColor);
        }

        protected virtual void AddStrokeColorControls()
        {
            var strokeColor =
                WpfHelper.CreateColorPicker(
                    "Stroke Color:",
                    Style,
                    nameof(Style.StrokeColor)
                );

            _uiControls.Add("Stroke Color", strokeColor);
        }

        protected virtual void AddStrokeThicknessControls()
        {
            var strokeThickness =
                WpfHelper.CreateSliderControl(
                    "Stroke Thickness:",
                    Style,
                    nameof(Style.StrokeThickness),
                    nameof(MinStrokeThickness),
                    nameof(MaxStrokeThickness)

                );
            _uiControls.Add("Stroke Thickness", strokeThickness);
        }

        protected virtual void AddOffsetControls()
        {
            var panel = WpfHelper.CreateDefaultPanel();

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

            var panelX = WpfHelper.CreateDefaultPanel(5, Orientation.Horizontal);
            var panelY = WpfHelper.CreateDefaultPanel(5, Orientation.Horizontal);

            panelX.Children.Add(labelX);
            panelX.Children.Add(inputX);

            panelY.Children.Add(labelY);
            panelY.Children.Add(inputY);

            panel.Children.Add(label);
            panel.Children.Add(panelX);
            panel.Children.Add(panelY);
            panel.Children.Add(offsetButton);
            _uiControls.Add("Offset", panel);
        }

        protected virtual void AddRotateControls()
        {
            var panel = WpfHelper.CreateDefaultPanel();

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
            _uiControls.Add("Rotate", panel);
        }

        protected virtual void AddScaleControls()
        {
            var panel = WpfHelper.CreateDefaultPanel();

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

            inputX.PreviewTextInput += (s, e) =>
            {
                if (double.TryParse(inputX.Text, out double X) && X > 0)
                {
                    inputX.Background = Brushes.White;
                }
                else
                {
                    inputX.Background = Brushes.IndianRed;
                }
            };

            inputY.PreviewTextInput += (s, e) =>
            {
                if (double.TryParse(inputY.Text, out double Y) && Y > 0)
                {
                    inputY.Background = Brushes.White;
                }
                else
                {
                    inputY.Background = Brushes.IndianRed;
                }
            };

            offsetButton.Click += (s, e) =>
            {
                if (double.TryParse(inputX.Text, out double X) && double.TryParse(inputY.Text, out double Y))
                {
                    if (X <= 0 || Y <= 0) return;
                    var factor = Math.Abs(X + Y) / 2;
                    ScaleElement(AnchorPoint.Position, new Vector(X, Y), factor);
                    InvalidateCanvas();
                }
            };

            var panelX = WpfHelper.CreateDefaultPanel(5, Orientation.Horizontal);
            var panelY = WpfHelper.CreateDefaultPanel(5, Orientation.Horizontal);

            panelX.Children.Add(labelX);
            panelX.Children.Add(inputX);

            panelY.Children.Add(labelY);
            panelY.Children.Add(inputY);

            panel.Children.Add(label);
            panel.Children.Add(panelX);
            panel.Children.Add(panelY);
            panel.Children.Add(offsetButton);
            _uiControls.Add("Scale", panel);
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


    }
}

using Microsoft.Win32;
using Modeling_Canvas.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public partial class BezierCurve
    {
        private StackPanel _framesPanel;
        
        protected override void InitControlPanel()
        {
            var mainPanel = WpfHelper.CreateDefaultPanel();

            var scrollView = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Height = 300
            };

            _framesPanel = WpfHelper.CreateDefaultPanel(orientation: Orientation.Vertical);
            scrollView.Content = _framesPanel;

            UpdateFramesPanel();

            var addFrameButton = WpfHelper.CreateButton(
                clickAction: () =>
                {
                    AddNewFrame();
                    UpdateFramesPanel();
                },
                content: "+"
            );

            mainPanel.Children.Add(scrollView);
            mainPanel.Children.Add(addFrameButton);

            _uiControls.Add("AnimationFrames", mainPanel);

            var isInfinite =
                WpfHelper.CreateLabeledCheckBox(
                    "Inifinte",
                    this,
                    nameof(IsInfiniteAnimation)
                );

            _uiControls.Add("IsInfinite", isInfinite);

            var startAnimationButton =
                WpfHelper.CreateButton(
                    content: "Animate",
                    clickAction: () =>
                    {
                        Animate();
                    }
                );

            startAnimationButton.AddIsDisabledBinding(
                this,
                nameof(IsNotAnimating)
                );

            _uiControls.Add("StartAnimation", startAnimationButton);

            var saveCurveButton =
                WpfHelper.CreateButton(
                    clickAction: () =>
                    {

                        try
                        {
                            var saveFileDialog = new SaveFileDialog
                            {
                                Title = "Save a New File",
                                Filter = "JSON Files (*.json)|*.json",
                                DefaultExt = ".json",                                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) 
                            };

                            if (saveFileDialog.ShowDialog() == true)
                            {
                                string filePath = saveFileDialog.FileName;
                                BezierCurveSerializer.SerializeToFile(this, filePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"An error occurred while saving the file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    },
                    "Save as"
                );

            _uiControls.Add("Save", saveCurveButton);

            var precisionSlider =
                WpfHelper.CreateSliderControl(
                    "Precision",
                    this,
                    nameof(CurvePrecision),
                    5,
                    500,
                    2.5
                );

            _uiControls.Add("Presicion", precisionSlider);

            base.InitControlPanel();
        }

        public void UpdateFramesPanel()
        {
            _framesPanel.Children.Clear();

            foreach (var frame in AnimationFrames.OrderBy(x => x.Key))
            {
                var framePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5)
                };

                // Прив'язка до фону
                var binding = new Binding("SelectedFrameKey")
                {
                    Source = this,
                    Converter = new SelectedFrameKeyToBackgroundConverter(),
                    ConverterParameter = frame.Key
                };

                BindingOperations.SetBinding(framePanel, StackPanel.BackgroundProperty, binding);

                // Текстове поле для часу кадру
                var timeTextBox = new TextBox
                {
                    Width = 50,
                    Text = frame.Key.ToString(),
                    TextAlignment = TextAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    BorderBrush = null
                };

                var previousTime = frame.Key;

                timeTextBox.TextChanged += (s, e) =>
                {
                    if (double.TryParse(timeTextBox.Text, out var newTime) && newTime >= 0)
                    {
                        if (newTime != previousTime)
                        {
                            timeTextBox.Background = Brushes.Gray;
                        } else
                        {
                            timeTextBox.Background = Brushes.White;
                        }
                    }
                    else
                    {
                        timeTextBox.Background = Brushes.IndianRed;
                    }
                };

                timeTextBox.PreviewKeyDown += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        if (double.TryParse(timeTextBox.Text, out var newTime) && !AnimationFrames.ContainsKey(newTime) && newTime >= 0)
                        {
                            var points = AnimationFrames[frame.Key];
                            AnimationFrames.Remove(frame.Key);
                            AnimationFrames[newTime] = points;

                            if (SelectedFrameKey == previousTime) SelectedFrameKey = newTime;
                            previousTime = newTime;

                            UpdateFramesPanel();

                            timeTextBox.BorderBrush = SystemColors.ControlDarkBrush;
                        }
                    }
                    else if (e.Key == Key.Escape) // Optional: Revert on Escape
                    {
                        timeTextBox.Text = previousTime.ToString();
                        timeTextBox.BorderBrush = SystemColors.ControlDarkBrush;
                    }
                };


                var switchButton = 
                    WpfHelper.CreateButton(
                        clickAction: () =>
                        {
                            SelectFrame(frame.Key);
                        },
                        content: "<>",
                        width: 30
                    );

                var removeButton =
                    WpfHelper.CreateButton(
                        clickAction: () =>
                        {
                            RemoveFrame(frame.Key);
                            UpdateFramesPanel();
                        },
                        content: "-",
                        width: 30
                    );

                if (AnimationFrames.Count < 2) removeButton.IsEnabled = false;

                framePanel.Children.Add(timeTextBox);
                framePanel.Children.Add(switchButton);
                framePanel.Children.Add(removeButton);
                _framesPanel.Children.Add(framePanel);
            }
        }

    }
}

using System.Windows.Controls;
using System.Windows;

namespace Modeling_Canvas.UIElements
{
    public partial class CustomLine
    {

        protected virtual void AddAddPointButton(DraggablePoint point = null)
        {
            var panel = GetDefaultVerticalPanel();
            var addPointButton = new Button
            {
                Content = "Add Point",
                Width = 100,
                Height = 20,
                Margin = new Thickness(5),
                IsTabStop = false
            };

            addPointButton.Click += (s, e) =>
            {
                if (point is null)
                {
                    AddPoint(GetAnchorDefaultPosition());
                }
                else
                {
                    var pointIndex = Points.IndexOf(point);
                    InsertPointAt(pointIndex);
                }
            };
            panel.Children.Add(addPointButton);
            AddElementToControlPanel(panel);
        }

        protected virtual void AddRemovePointControls(DraggablePoint point)
        {
            var panel = GetDefaultVerticalPanel();
            var removePointButton = new Button
            {
                Content = "Remove Point",
                Width = 100,
                Height = 20,
                Margin = new Thickness(5),
                IsTabStop = false
            };

            removePointButton.Click += (s, e) =>
            {
                RemovePoint(point);
                RenderControlPanel();
            };

            if (Points.Count == 2)
            {
                removePointButton.IsEnabled = false;
            }

            panel.Children.Add(removePointButton);
            AddElementToControlPanel(panel);
        }

        protected virtual void AddIsClosedControls()
        {
            var panel = GetDefaultHorizontalPanel();
            var isClosedLabel = new TextBlock { Text = "Is Closed:" };

            var isClosedCheckBox = new CheckBox
            {
                IsChecked = IsClosed // Bind the initial value
            };

            isClosedCheckBox.Checked += (s, e) =>
            {
                IsClosed = true;
                InvalidateVisual();
            };
            isClosedCheckBox.Unchecked += (s, e) =>
            {
                IsClosed = false;
                InvalidateVisual();
            };

            panel.Children.Add(isClosedLabel);
            panel.Children.Add(isClosedCheckBox);
            AddElementToControlPanel(panel);
        }
    }
}

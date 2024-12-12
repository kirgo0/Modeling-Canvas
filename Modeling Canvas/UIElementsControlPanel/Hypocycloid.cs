using Modeling_Canvas.Models;
using System.Windows;

namespace Modeling_Canvas.UIElements
{
    public partial class Hypocycloid
    {
        private Dictionary<string, FrameworkElement> CreateHypocycloidControls(HypocycloidModel model, string labelPrefix = "", string namePrefix = "")
        {
            var controls = new Dictionary<string, FrameworkElement>();
            var distancePanel = WpfHelper.CreateSliderControl(
                $"{labelPrefix}Distance",
                model,
                nameof(Model.Distance),
                null,
                nameof(Model.SmallRadius),
                0.1
            );
            controls[namePrefix + nameof(Model.Distance)] = distancePanel;

            var anglePanel = WpfHelper.CreateSliderControl(
                $"{labelPrefix}Angle",
                model,
                nameof(Model.Angle),
                nameof(Model.MinAngle),
                nameof(Model.MaxAmgle),
                1
            );
            controls[namePrefix + nameof(Model.Angle)] = anglePanel;

            var largeRadiusPanel = WpfHelper.CreateSliderControl(
                $"{labelPrefix}Large Radius",
                model,
                nameof(Model.LargeRadius),
                nameof(Model.SmallRadius),
                nameof(Model.MaxLargeCircleRadius),
                0.1
            );
            controls[namePrefix + nameof(Model.LargeRadius)] = largeRadiusPanel;

            var smallRadiusPanel = WpfHelper.CreateSliderControl(
                $"{labelPrefix}Small Radius",
                model,
                nameof(Model.SmallRadius),
                nameof(Model.MinRadius),
                nameof(Model.LargeRadius),
                0.1
            );
            controls[namePrefix + nameof(Model.SmallRadius)] = smallRadiusPanel;
            return controls;
        }



    }
}

using Modeling_Canvas.Models;
using System.Windows;

namespace Modeling_Canvas.UIElements
{
    public partial class Hypocycloid
    {

        protected override void InitControlPanel()
        {
            var precisionSlider =
                WpfHelper.CreateSliderControl<double>(
                    "Max points count",
                    this,
                    nameof(MaxPointCount),
                    5,
                    10000,
                    5
                );
            _uiControls.Add(nameof(MaxPointCount), precisionSlider);

            var arcLengthText = WpfHelper.CreateValueTextBlock(
                "Arc Length",
                CalculatedValues,
                nameof(HypocycloidCalculationsModel.ArcLength)
                );

            arcLengthText.AddVisibilityBinding(CalculatedValues, nameof(HypocycloidCalculationsModel.ShowArcLength));

            _uiControls.Add(nameof(HypocycloidCalculationsModel.ArcLength), arcLengthText);

            var areaText = WpfHelper.CreateValueTextBlock(
                "Area",
                CalculatedValues,
                nameof(HypocycloidCalculationsModel.HypocycloidArea)
                );

            areaText.AddVisibilityBinding(CalculatedValues, nameof(HypocycloidCalculationsModel.ShowArcLength));

            _uiControls.Add(nameof(HypocycloidCalculationsModel.ShowHypocycloidArea), areaText);

            var showInflectionPointsCheckbox = WpfHelper.CreateLabeledCheckBox("Inflection points", CalculatedValues, nameof(HypocycloidCalculationsModel.ShowInflectionPoints));

            _uiControls.Add(nameof(HypocycloidCalculationsModel.ShowInflectionPoints), showInflectionPointsCheckbox);

            var animateMenuCheckbox = WpfHelper.CreateLabeledCheckBox("Animate", this, nameof(ShowAnimationControls));

            _uiControls.Add(nameof(ShowAnimationControls), animateMenuCheckbox);

            var hypoControls = CreateHypocycloidControls(Model);

            _uiControls = _uiControls.Concat(hypoControls).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            _animationControls = CreateHypocycloidControls(AnimationModel, "Animate to ");

            foreach (var ac in _animationControls.Values)
            {
                ac.AddVisibilityBinding(this, nameof(ShowAnimationControls));
            }

            var timeSlider = WpfHelper.CreateSliderControl(
                "Time",
                this,
                nameof(AnimationDuration),
                nameof(MinAnimationDuration),
                nameof(MaxAnimationDuration),
                0.1
                );
            timeSlider.AddVisibilityBinding(this, nameof(ShowAnimationControls));

            _uiControls.Add("TimeSlider", timeSlider);

            var startAnimationButton = WpfHelper.CreateButton(
                content: "Animate",
                clickAction: () => Animate()
                );

            startAnimationButton.AddVisibilityBinding(this, nameof(ShowAnimationControls));

            startAnimationButton.AddIsDisabledBinding(this, nameof(IsNotAnimating));

            _uiControls.Add("AnimateButton", startAnimationButton);

            base.InitControlPanel();

        }

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

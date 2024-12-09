using System.Windows;
using System.Windows.Controls;

namespace Modeling_Canvas.UIElements
{
    public partial class CustomCircle
    {
        public virtual void AddRadiusControls()
        {
            var radiusSlider =
                WpfHelper.CreateSliderControl(
                    "Radius",
                    this, 
                    nameof(Radius),
                    nameof(MinRadiusValue),
                    nameof(MaxRadiusValue)
                );

            _uiControls.Add("Radius", radiusSlider);
        }

        protected virtual void AddCenterControls()
        {
            var centerPosition = 
                WpfHelper.CreateDefaultPointControls(
                    "Center",
                    this,
                    "Center.X",
                    "Center.Y",
                    (x) =>
                    {
                        OverrideAnchorPoint = true;
                        var difference = Center.X - x;
                        Center = new Point(x, Center.Y);
                        AnchorPoint.Position = new Point(AnchorPoint.Position.X - difference, AnchorPoint.Position.Y);
                        InvalidateCanvas();
                    },
                    (y) =>
                    {
                        OverrideAnchorPoint = true;
                        var difference = Center.Y - y;
                        Center = new Point(Center.X, y);
                        AnchorPoint.Position = new Point(AnchorPoint.Position.X, AnchorPoint.Position.Y - difference);
                        InvalidateCanvas();
                    }
                );

            _uiControls.Add("Center", centerPosition);
        }
    }
}

namespace Modeling_Canvas.UIElements
{
    public partial class SegmentedCircle
    {
        protected virtual void AddDegreesControls()
        {
            var startDeg =
                WpfHelper.CreateSliderControl(
                    "Start Degress",
                    this,
                    nameof(StartDegrees),
                    nameof(MinDegrees),
                    nameof(MaxDegrees)
                );

            _uiControls.Add("Start Degrees", startDeg);

            var endDeg =
                WpfHelper.CreateSliderControl(
                    "End Degress",
                    this,
                    nameof(EndDegrees),
                    nameof(MinDegrees),
                    nameof(MaxDegrees)
                );

            _uiControls.Add("End Degrees", endDeg);
        }
    }
}

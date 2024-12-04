namespace Modeling_Canvas.Models
{
    public class RadiusChangeEventArgs : EventArgs
    {
        public double PreviousRadius { get; set; }
        public double NewRadius { get; set; }
        public double ChangeFactor { get; set; }
        public RadiusChangeEventArgs(double previousRadius, double radius, double changeFactor)
        {
            PreviousRadius = previousRadius;
            NewRadius = radius;
            ChangeFactor = changeFactor;
        }
    }
}

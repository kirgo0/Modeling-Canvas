using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Modeling_Canvas
{
    public class Helpers
    {
        public static double DegToRad(double deg) => Math.PI * deg / 180.0;
        public static double NormalizeAngle(double angle) => (angle % 360 + 360) % 360;
        public static double SnapValue(double value)
        {
            const double lowerThreshold = 0.1;
            const double upperThreshold = 0.9;
            const double toHalfLower = 0.45;
            const double toHalfUpper = 0.55;

            double integerPart = Math.Floor(value);
            double fractionalPart = value - integerPart;

            if (fractionalPart <= lowerThreshold) return integerPart;
            if (fractionalPart >= upperThreshold) return integerPart + 1;
            if (fractionalPart >= toHalfLower && fractionalPart <= toHalfUpper) return integerPart + 0.5;
            return value;
        }

        public static void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {

            // Regex to allow numbers, optional "-" at the start, and one "." or ","
            //Regex regex = new Regex(@"^[-]?[0-9]*[.,]?[0-9]*$");

            //e.Handled = !regex.IsMatch(e.Text);
        }

    }
}

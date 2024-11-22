using Microsoft.Extensions.Logging;
using Modeling_Canvas.UIELements;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Modeling_Canvas
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CenterWindowOnScreen();
            var customSegment = new CustomCircle
            {
                Radius = 6,
                Stroke = Brushes.DarkGreen,
                StrokeThickness = 3,
                Precision = 100,
                SegmentStartDegrees = 0, // Start at 45 degrees
                SegmentEndDegrees = 270,
                Center = new Point(-2, 0), // End at 270 degrees
                Canvas = MyCanvas,
            };

            MyCanvas.Children.Add(customSegment);

            PreviewKeyUp += MyCanvas.OnKeyUp;
            PreviewKeyDown += MyCanvas.OnKeyDown;
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = Width;
            double windowHeight = Height;
            Left = (screenWidth / 2) - (windowWidth / 2);
            Top = (screenHeight / 2) - (windowHeight / 2);
        }

    }

}

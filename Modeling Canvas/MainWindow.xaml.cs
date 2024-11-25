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
            var customSegment = new CustomCircle(MyCanvas)
            {
                Radius = 9,
                Stroke = Brushes.DarkGreen,
                StrokeThickness = 3,
                Precision = 100,
                StartDegrees = 0, // Start at 45 degrees
                EndDegrees = 70,
                Center = new Point(-2, 0), // End at 270 degrees
            };
            MyCanvas.Children.Insert(0,customSegment);

            //var customSegment2 = new CustomCircle(MyCanvas)
            //{
            //    Radius = 9,
            //    Stroke = Brushes.Red,
            //    StrokeThickness = 3,
            //    Precision = 100,
            //    StartDegrees = 0, // Start at 45 degrees
            //    EndDegrees = 90,
            //    Center = new Point(-2, 0), // End at 270 degrees
            //    Canvas = MyCanvas,
            //};
            //MyCanvas.Children.Add(customSegment2);

            var customLine = new CustomLine(MyCanvas, new Point(4, 1), new Point(-1, -6));
            customLine.AddPoint(5, 5);
            customLine.AddPoint(7, 7);
            MyCanvas.Children.Add(customLine);
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

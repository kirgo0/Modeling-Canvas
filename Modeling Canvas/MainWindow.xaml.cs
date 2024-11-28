using Modeling_Canvas.Extensions;
using Modeling_Canvas.UIELements;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

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

            InitFigure();

            var circle = new CustomCircle(MyCanvas);
            MyCanvas.Children.Add(circle);

            PointExtensions.Canvas = MyCanvas;
            
            Keyboard.Focus(MyCanvas);


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

        public void AddControlToStackPanel(UIElement control)
        {
            ControlStack.Children.Add(control);
        }

        public void ClearControlStack()
        {
            ControlStack.Children.Clear();
        }

        public void InitFigure()
        {
            var group = new ElementsGroup(MyCanvas);
            var line1 = new CustomLine(MyCanvas);
            line1.AddPoint(6, 0);
            line1.AddPoint(3, 5.2);
            line1.AddPoint(-3, 5.2);
            line1.AddPoint(-6, 0);
            line1.AddPoint(-3, -5.2);
            line1.AddPoint(3, -5.2);
            group.AddChild(line1);

            var line2 = new CustomLine(MyCanvas);
            line2.AddPoint(-9, 3);
            line2.AddPoint(-11, 4.5);
            line2.AddPoint(-9, 7);
            line2.AddPoint(-7, 5.5);
            line2.AddPoint(-1.5, 9);
            line2.AddPoint(-1.5, 11.5);

            line2.AddPoint(1.5, 11.5);
            line2.AddPoint(1.5, 9);
            line2.AddPoint(7, 5.5);
            line2.AddPoint(9, 7);
            line2.AddPoint(11, 4.5);
            line2.AddPoint(9, 3);

            line2.AddPoint(9, -3);
            line2.AddPoint(11, -4.5);
            line2.AddPoint(9, -7);
            line2.AddPoint(7, -5.5);
            line2.AddPoint(1.5, -9);
            line2.AddPoint(1.5, -11.5);

            line2.AddPoint(-1.5, -11.5);
            line2.AddPoint(-1.5, -9);
            line2.AddPoint(-7, -5.5);
            line2.AddPoint(-9, -7);
            line2.AddPoint(-11, -4.5);
            line2.AddPoint(-9, -3);
            group.AddChild(line2);
            MyCanvas.Children.Add(group);
        }

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Helpers.NumberValidationTextBox(sender, e);
        }

    }

}

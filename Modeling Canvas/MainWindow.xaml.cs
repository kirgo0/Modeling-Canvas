using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using Modeling_Canvas.UIElements;
using System.Windows;
using System.Windows.Controls;
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

            //InitFigure();
            var a = new Hypocycloid(MyCanvas, 4, 1);
            MyCanvas.Children.Add(a);
            a.Center = new Point(0, 0);

            //var b = new SegmentedCircle(MyCanvas);
            //MyCanvas.Children.Add(b);

            //var a = new CustomLine(MyCanvas, new Point(0,2), new Point(2, 7));
            //MyCanvas.Children.Add(a);

            //var c = new CustomCircle(MyCanvas);
            //MyCanvas.Children.Add(c);


            PointExtensions.Canvas = MyCanvas;

            PreviewKeyDown += MyCanvas.OnKeyDown;
            PreviewKeyUp += MyCanvas.OnKeyUp;
            ResetScaling(null, null);

            DrawModeControlTab.SelectionChanged += ChangeRenderMode;

            MyCanvas.SizeChanged += (s, e) =>
            {
                MyCanvas.AffineParams.CanvasHeight = MyCanvas.ActualHeight;
                MyCanvas.ProjectiveParams.CanvasHeight = MyCanvas.ActualHeight;
            };
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

        private void ResetOffsets(object sender, RoutedEventArgs e)
        {
            MyCanvas.ResetOffests();
        }

        private void ResetScaling(object sender, RoutedEventArgs e)
        {
            MyCanvas.ResetScaling();
        }

        private void ChangeRenderMode(object sender, SelectionChangedEventArgs e)
        {
            if (DefaultTab.IsSelected) MyCanvas.RenderMode = RenderMode.Default;
            else if (AffineTab.IsSelected) MyCanvas.RenderMode = RenderMode.Affine;
            else if (ProjectiveTab.IsSelected) MyCanvas.RenderMode = RenderMode.Projective;

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MyCanvas.AllowInfinityRender = true;
            MyCanvas.InvalidateVisual();
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MyCanvas.AllowInfinityRender = false;
            MyCanvas.InvalidateVisual();
        }
    }

}

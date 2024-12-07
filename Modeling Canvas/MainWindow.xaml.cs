using Modeling_Canvas.Enums;
using Modeling_Canvas.Extensions;
using Modeling_Canvas.UIElements;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

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
            //a.Center = new Point(5, 5);

            //var b = new SegmentedCircle(MyCanvas);
            //MyCanvas.Children.Add(b);

            //var a = new CustomLine(MyCanvas, new Point(0, 2), new Point(2, 7));
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
            if (DefaultTab.IsSelected) MyCanvas.RenderMode = Modeling_Canvas.Enums.RenderMode.Default;
            else if (AffineTab.IsSelected) MyCanvas.RenderMode = Enums.RenderMode.Affine;
            else if (ProjectiveTab.IsSelected) MyCanvas.RenderMode = Enums.RenderMode.Projective;

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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(WndProc);
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_SYSKEYDOWN = 0x0104; // System Key Down
            const int WM_SYSKEYUP = 0x0105;   // System Key Up
            const int WM_SYSCOMMAND = 0x0112; // System Command

            if (msg == WM_SYSKEYDOWN || msg == WM_SYSKEYUP)
            {
                if (wParam.ToInt32() == (int)Key.LeftAlt || wParam.ToInt32() == (int)Key.RightAlt)
                {
                    handled = true; // Suppress menu activation
                }
            }

            if (msg == WM_SYSCOMMAND)
            {
                // Prevent activation of the system menu when Alt is pressed
                if (wParam.ToInt32() == 0xF100) // SC_KEYMENU
                {
                    handled = true; // Suppress system menu
                }
            }

            return IntPtr.Zero;
        }
    }

}

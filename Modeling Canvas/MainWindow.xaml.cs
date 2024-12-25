using Microsoft.Win32;
using Modeling_Canvas.Extensions;
using Modeling_Canvas.UIElements;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Modeling_Canvas
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
            InitializeComponent();
            PointExtensions.Canvas = MainCanvas;
            CenterWindowOnScreen();

            //var grid = new CoordinateGrid(MainCanvas, false);
            //MainCanvas.Children.Add(grid);

            var koch = new KochCurve(MainCanvas);
            MainCanvas.Children.Add(koch);
            koch.Position = new Point(0.5, 2.625);


            //InitFigure();
            //var a = new Hypocycloid(MainCanvas, 4, 1);
            //MainCanvas.Children.Add(a);
            //a.Center = new Point(0, 0);

            //var b = new SegmentedCircle(MyCanvas);
            //MyCanvas.Children.Add(b);

            //var c = new CustomCircle(MainCanvas);
            //MainCanvas.Children.Add(c);

            //var d = new BezierCurve(MainCanvas);

            //d.AddPoint(2, 3);
            //d.AddPoint(3, 3);
            //MainCanvas.Children.Add(d);

            //var ball = new BezierCurve(MainCanvas);

            //ball.AddPoint(2, 3);
            //ball.AddPoint(1, 1);
            //ball.AddPoint(-2, 3);
            //ball.AddPoint(3, -4);
            //MainCanvas.Children.Add(ball);

            PreviewKeyDown += MainCanvas.OnKeyDown;
            PreviewKeyUp += MainCanvas.OnKeyUp;
            ResetScaling(null, null);

            DrawModeControlTab.SelectionChanged += ChangeRenderMode;

            MainCanvas.SizeChanged += (s, e) =>
            {
                MainCanvas.AffineParams.CanvasHeight = MainCanvas.ActualHeight;
                MainCanvas.ProjectiveParams.CanvasHeight = MainCanvas.ActualHeight;
            };
            KeyDown += MainWindow_KeyDown;
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
            var group = new ElementsGroup(MainCanvas);
            var line1 = new CustomLine(MainCanvas);
            line1.AddPoint(6, 0);
            line1.AddPoint(3, 5.2);
            line1.AddPoint(-3, 5.2);
            line1.AddPoint(-6, 0);
            line1.AddPoint(-3, -5.2);
            line1.AddPoint(3, -5.2);
            group.AddChild(line1);

            var line2 = new CustomLine(MainCanvas);
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

            MainCanvas.Children.Add(group);
        }

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Helpers.NumberValidationTextBox(sender, e);
        }

        private void ResetOffsets(object sender, RoutedEventArgs e)
        {
            MainCanvas.ResetOffests();
        }

        private void ResetScaling(object sender, RoutedEventArgs e)
        {
            MainCanvas.ResetScaling();
        }

        private void ChangeRenderMode(object sender, SelectionChangedEventArgs e)
        {
            if (DefaultTab.IsSelected) MainCanvas.RenderMode = Modeling_Canvas.Enums.RenderMode.Default;
            else if (AffineTab.IsSelected) MainCanvas.RenderMode = Enums.RenderMode.Affine;
            else if (ProjectiveTab.IsSelected) MainCanvas.RenderMode = Enums.RenderMode.Projective;
            else if (ProjectiveV2Tab.IsSelected) MainCanvas.RenderMode = Enums.RenderMode.ProjectiveV2;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MainCanvas.AllowInfinityRender = true;
            MainCanvas.InvalidateVisual();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MainCanvas.AllowInfinityRender = false;
            MainCanvas.InvalidateVisual();
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


        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.L)
            {
                OpenFileAndLoadBezierCurve();
            }
        }

        private void OpenFileAndLoadBezierCurve()
        {
            try
            {
                // Open file dialog to select a file
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                    Title = "Select a Bezier Curve File"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;

                    var bezierCurve = BezierCurveSerializer.DeserializeFromFile(filePath, MainCanvas);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and display error message
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

}

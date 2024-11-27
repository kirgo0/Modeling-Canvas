using System.Windows.Input;

namespace Modeling_Canvas
{
    public class InputManager
    {
        public static bool ShiftPressed
        {
            get => Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        }

        public static bool CtrlPressed
        {
            get => Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        }

        public static bool AltPressed
        {
            get => Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
        }

        public static bool SpacePressed
        {
            get => Keyboard.IsKeyDown(Key.Space);
        }

        public static bool AnyKeyButShiftPressed
        {
            get => CtrlPressed || AltPressed || SpacePressed;
        }

        public static bool LeftMousePressed
        {
            get => Mouse.LeftButton == MouseButtonState.Pressed;
        }
        public static bool RightMousePressed
        {
            get => Mouse.RightButton == MouseButtonState.Pressed;
        }

        public static bool OnlyLeftMousePressed { get => LeftMousePressed && !RightMousePressed; }
        public static bool OnlyRightMousePressed { get => RightMousePressed && !LeftMousePressed; }

    }
}

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Appalachia.CI.Integration.InputRemapping
{
    public class Input
    {
        public static bool GetMouseButton(int button)
        {
            var buttonControl = GetButtonControlFromMouseButtonIndex(button);
            return buttonControl.isPressed;
        }

        public static bool GetMouseButtonDown(int button)
        {
            var buttonControl = GetButtonControlFromMouseButtonIndex(button);
            return buttonControl.wasPressedThisFrame;
        }

        public static bool GetMouseButtonUp(int button)
        {
            var buttonControl = GetButtonControlFromMouseButtonIndex(button);
            return buttonControl.wasReleasedThisFrame;
        }

        private static ButtonControl GetButtonControlFromMouseButtonIndex(int i)
        {
            return i switch
            {
                0 => Mouse.current.leftButton,
                1 => Mouse.current.rightButton,
                2 => Mouse.current.middleButton,
                _ => null
            };
        }
    }
}

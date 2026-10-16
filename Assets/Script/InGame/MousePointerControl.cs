using UnityEngine;

namespace InGame
{

    public static class MousePointerControl
    {
        public static void Locked()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public static void Free()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
using UnityEngine;


namespace HnSF.ui.menus
{
    public enum NavigationDirections
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    public static class UIHelpers
    {
        public static NavigationDirections ConvertNavigationToDirection(Vector2 nav, float deadzone = 0.1f)
        {
            if(nav.magnitude < deadzone) return NavigationDirections.None;
            if (Mathf.Abs(nav.y) >= Mathf.Abs(nav.x))
            {
                return nav.y > 0 ? NavigationDirections.Up : NavigationDirections.Down;
            }
            return nav.x > 0 ? NavigationDirections.Right : NavigationDirections.Left;
        }
    }
}
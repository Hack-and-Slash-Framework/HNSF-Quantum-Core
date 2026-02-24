using CT.MenuNav;
using UnityEngine;

namespace HnSF.ui.menus
{
    public class ScreenSettings : MenuBase
    {
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return base.TryClose(direction, forceClose);
        }
    }
}
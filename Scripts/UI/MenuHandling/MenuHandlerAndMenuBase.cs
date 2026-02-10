using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace HnSF.ui
{
    public class MenuHandlerAndMenuBase : MenuBase, IMenuHandler
    {
        protected List<MenuBase> menus = new();

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }

        public virtual void ResetAndForwardTo(IMenu menuToForwardTo)
        {
            foreach (var m in menus)
            {
                m.gameObject.SetActive(false);
            }

            history.Clear();
            Forward(menuToForwardTo, false);
        }

        public bool Forward(IMenu menu, bool autoClose = true)
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (autoClose) GetCurrentMenu()?.TryClose(MenuDirection.FORWARDS, false);
            menu.MenuHandler = this;
            menu.Open(MenuDirection.FORWARDS, this);
            history.Add(menu);
            return true;
        }

        public bool Back()
        {
            if (history.Count == 0) return true;
            bool result = GetCurrentMenu().TryClose(MenuDirection.BACKWARDS);
            if (result == true)
            {
                history.RemoveAt(history.Count - 1);
                GetCurrentMenu()?.Open(MenuDirection.BACKWARDS, this);
            }

            return result;
        }

        public List<IMenu> GetHistory()
        {
            return history;
        }

        public IMenu GetCurrentMenu()
        {
            if (history.Count == 0) return null;
            return history[^1];
        }

        public bool HasSubmenuOpen()
        {
            return false;
        }
    }
}
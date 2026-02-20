using System.Collections.Generic;
using UnityEngine;

namespace HnSF.ui
{
    public class MenuBase : MonoBehaviour, IMenu
    {
        public IMenuHandler MenuHandler { get; set; } = null;
        
        [SerializeField] protected List<IMenu> history = new();

        public virtual void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            MenuHandler = menuHandler;
        }

        public virtual bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            return true;
        }
    }
}
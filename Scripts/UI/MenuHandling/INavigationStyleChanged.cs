using HnSF.Input;

namespace HnSF.ui
{
    public interface INavigationStyleChanged
    {
        public void NavigationStyleChanged(InputPlayerManager inputPlayer,
            InputPlayerManager.NavigationType navigationType);
    }
}
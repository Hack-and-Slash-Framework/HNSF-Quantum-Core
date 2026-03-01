using CT.LocalInputManagement;

namespace HnSF.ui
{
    public interface INavigationStyleChanged
    {
        public void NavigationStyleChanged(InputPlayerManagerBase inputPlayer,
            InputPlayerManagerBase.NavigationType navigationType);
    }
}
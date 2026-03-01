namespace CT.LocalInputManagement
{
    public partial class InputPlayerManagerUIM : InputPlayerManagerBase
    {
        public void DisableAllMaps()
        {
            inputActions.UI.Disable();
            inputActions.Player.Disable();
        }
        
        public void SwitchToUIMap()
        {
            playerInput.currentActionMap = inputActions.UI.Get();
            inputActions.UI.Enable();
            inputActions.Player.Disable();
        }

        public void SwitchToPlayerMap()
        {
            playerInput.currentActionMap = inputActions.Player.Get();
            inputActions.Player.Enable();
            inputActions.UI.Disable();
        }
    }
}

namespace CT.LocalInputManagement
{
    public partial class InputManager
    {
        public void SwitchToUIActionMap(int playerId = 0)
        {
            (playerInputManagers[playerId]).SwitchToUIMap();
        }

        public void SwitchToPlayerActionMap(int playerId = 0)
        {
            (playerInputManagers[playerId]).SwitchToPlayerMap();
        }

        public void SwitchAllToUIActionMap()
        {
            foreach (var pim in playerInputManagers)
            {
                if (pim.Id == 0) continue;
                (pim).SwitchToUIMap();
            }
        }
        
        public void SwitchAllToPlayerActionMap()
        {
            foreach (var pim in playerInputManagers)
            {
                if (pim.Id == 0) continue;
                (pim).SwitchToPlayerMap();
            }
        }
    }
}
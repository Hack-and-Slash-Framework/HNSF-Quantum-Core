namespace CT.LocalInputManagement
{
    public partial class InputManagerUIM
    {
        public void SwitchToUIActionMap(int playerId = 0)
        {
            (playerInputManagers[playerId] as InputPlayerManagerUIM).SwitchToUIMap();
        }

        public void SwitchToPlayerActionMap(int playerId = 0)
        {
            (playerInputManagers[playerId] as InputPlayerManagerUIM).SwitchToPlayerMap();
        }

        public void SwitchAllToUIActionMap()
        {
            foreach (var pim in playerInputManagers)
            {
                if (pim.Id == 0) continue;
                (pim as InputPlayerManagerUIM).SwitchToUIMap();
            }
        }
        
        public void SwitchAllToPlayerActionMap()
        {
            foreach (var pim in playerInputManagers)
            {
                if (pim.Id == 0) continue;
                (pim as InputPlayerManagerUIM).SwitchToPlayerMap();
            }
        }
    }
}
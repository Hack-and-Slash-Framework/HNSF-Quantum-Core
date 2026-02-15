using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HnSF.Input
{
    public class InputManager : MonoBehaviour
    {
        public enum ControlSchemeType
        {
            KEYBOARD_MOUSE,
            GAMEPAD
        }
        
        public List<InputPlayerManagerBase> playerInputManagers = new();
        public int autoAssignDevicesTo = 0;
        
        public void Initialize()
        {
            playerInputManagers = new(4);
            InitializeSystemPlayer();
            
            playerInputManagers[0].ActivateInput();
            playerInputManagers[0].ActivateUIHandling();
            
            InputSystem.onDeviceChange += onInputDeviceChange;
        }

        private void OnDestroy()
        {
            InputSystem.onDeviceChange -= onInputDeviceChange;
        }
        
        public void InitializeSystemPlayer()
        {
            GameObject go = new GameObject("System Player");
            go.transform.SetParent(transform, false);
            var ipm = go.AddComponent<InputPlayerManagerBase>();
            ipm.Initialize(0);

            playerInputManagers.Add(ipm);
            
            playerInputManagers[0].AssignInputDevices(Gamepad.all.ToArray());
            playerInputManagers[0].AssignKeyboardAndMouse();
        }
        
        public void AddPlayer()
        {
            GameObject go = new GameObject($"Player {playerInputManagers.Count}");
            go.transform.SetParent(transform, false);
            var ipm = go.AddComponent<InputPlayerManagerBase>();
            
            playerInputManagers.Add(ipm);
            ipm.Initialize(playerInputManagers.Count-1);
        }

        public void RemovePlayer(int player)
        {
            if (player == 0) return;
            playerInputManagers[player].Teardown();
            GameObject.Destroy(playerInputManagers[player].gameObject);
            playerInputManagers.RemoveAt(player);
            RefreshPlayerIDs();
        }

        public void SetPlayerCount(int count)
        {
            count += 1;
            while (playerInputManagers.Count < count)
            {
                AddPlayer();
            }

            while (playerInputManagers.Count > count)
            {
                RemovePlayer(playerInputManagers.Count-1);
            }
        }

        public int GetPlayerCount()
        {
            return playerInputManagers.Count - 1;
        }
        
        private void RefreshPlayerIDs()
        {
            for (int i = 0; i < playerInputManagers.Count; i++)
            {
                if (i == 0) continue;
                playerInputManagers[i].SetID(i);
            }
        }

        public InputPlayerManagerBase GetSystemPlayer()
        {
            return playerInputManagers[0];
        }
        
        public InputPlayerManagerBase GetPlayer(int playerId)
        {
            if (playerId == 0 || playerId >= playerInputManagers.Count) return null;
            return playerInputManagers[playerId];
        }

        public List<InputPlayerManagerBase> GetPlayers()
        {
            var l = new List<InputPlayerManagerBase>();
            for (int i = 1; i < playerInputManagers.Count; i++)
            {
                l.Add(playerInputManagers[i]);
            }
            return l;
        }
        
        public void SwitchToUIActionMap(int playerId = 0)
        {
            playerInputManagers[playerId].SwitchToUIMap();
        }

        public void SwitchToPlayerActionMap(int playerId = 0)
        {
            playerInputManagers[playerId].SwitchToPlayerMap();
        }

        public void SwitchAllToUIActionMap()
        {
            foreach (var pim in playerInputManagers)
            {
                if (pim.Id == 0) continue;
                pim.SwitchToUIMap();
            }
        }
        
        public void SwitchAllToPlayerActionMap()
        {
            foreach (var pim in playerInputManagers)
            {
                if (pim.Id == 0) continue;
                pim.SwitchToPlayerMap();
            }
        }
        
        public void ReturnAllDevicesToSystem()
        {
            for (int i = 1; i < playerInputManagers.Count; i++)
            {
                playerInputManagers[i].ClearAssignedDevices();
            }
            
            playerInputManagers[0].AssignInputDevices(Gamepad.all.ToArray());
            playerInputManagers[0].AssignKeyboardAndMouse();
        }

        public void ReturnPlayerDevicesToSystem(int player)
        {
            if (player == 0) return;
            var dList = playerInputManagers[player].assignedDevices.ToArray();
            playerInputManagers[player].RemoveAllDevices();
            playerInputManagers[0].AssignInputDevices(dList);
        }
        
        public void RemoveDeviceFromPlayers(InputDevice device, bool assignToSystem = true)
        {
            for (int i = 1; i < playerInputManagers.Count; i++)
            {
                playerInputManagers[i].RemoveDevice(device);
            }
            if(assignToSystem) playerInputManagers[0].AssignInputDevice(device);
        }

        public void AssignDevicesToPlayer(InputDevice[] devices, int player)
        {
            if (player == 0) return;
            playerInputManagers[0].RemoveDevices(devices);
            playerInputManagers[player].AssignInputDevices(devices);
        }

        public void AssignAllDevicesToPlayer(int player)
        {
            ReturnAllDevicesToSystem();
            TransferAllDevicesFromSystemTo(player);
        }

        public void TransferAllDevicesFromSystemTo(int player)
        {
            if (player == 0) return;
            var aDevices = playerInputManagers[0].assignedDevices.ToArray();
            playerInputManagers[0].RemoveDevices(aDevices);
            playerInputManagers[player].AssignInputDevices(aDevices);
        }
        
        public void onInputDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (autoAssignDevicesTo >= playerInputManagers.Count) autoAssignDevicesTo = 0;
            
            switch (change)
            {
                case InputDeviceChange.Added:
                    Debug.Log($"Device added {device}. Assigning to {autoAssignDevicesTo}.", playerInputManagers[autoAssignDevicesTo]);
                    playerInputManagers[0].RemoveDevice(device);
                    playerInputManagers[autoAssignDevicesTo].AssignInputDevice(device);
                    break;
                case InputDeviceChange.Removed:
                    Debug.Log("Device removed: " + device);
                    RemoveDeviceFromPlayers(device, assignToSystem: false);
                    playerInputManagers[0].RemoveDevice(device);
                    break;
                case InputDeviceChange.ConfigurationChanged:
                    Debug.Log("Device configuration changed: " + device);
                    break;
            }
        }

        public virtual void SetPlayersBasedOnDeviceLists(List<List<InputDevice>> players)
        {
            if (players.Count == 0) return;
            ReturnAllDevicesToSystem();
            SetPlayerCount(players.Count);
            
            for (int i = 0; i < players.Count; i++)
            {
                AssignDevicesToPlayer(players[i].ToArray(), i+1);
            }
        }
    }
}
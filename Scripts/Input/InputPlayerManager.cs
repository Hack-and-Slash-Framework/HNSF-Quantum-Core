using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;

namespace HnSF.Input
{
    public class InputPlayerManager : MonoBehaviour
    {
        public enum NavigationType
        {
            Controller_Or_Keyboard,
            Mouse
        }

        public NavigationType lastNavigationType = NavigationType.Controller_Or_Keyboard;

        public delegate void DelegateNavigationStyleChange(InputPlayerManager inputPlayer,
            NavigationType navigationType);

        public DelegateNavigationStyleChange onNavigationStyleChanged;

        public delegate void DelegateWhenControlSchemeChanged(InputPlayerManager inputPlayer,
            InputManager.ControlSchemeType controlScheme);

        public DelegateWhenControlSchemeChanged onControlSchemeChanged;

        public delegate void DelegateDeviceChanged();

        public DelegateDeviceChanged onCurrentDeviceChanged;

        public InputUser User => playerInput.user;
        public int Id { get; private set; } = 0;
        public string CurrentProfile { get; private set; } = "";

        public PlayerInput playerInput = null;
        public InputActions inputActions;

        public HashSet<InputDevice> assignedDevices = new();
        public List<InputDevice> currentDevices = new List<InputDevice>();

        public bool autoSwitchControlSchemes = true;

        public MultiplayerEventSystem mpEventSystem = null;
        public InputSystemUIInputModule uiInputModule = null;

        public int navigationStyleUpdateRate = 10;
        
        public void Initialize(int id)
        {
            Id = id;
            if(playerInput == null) playerInput = gameObject.AddComponent<PlayerInput>();
            inputActions?.Dispose();
            inputActions = new InputActions();
            playerInput.defaultActionMap = inputActions.UI.Get().name;
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            playerInput.neverAutoSwitchControlSchemes = true;
            playerInput.actions = inputActions.asset;

            if(mpEventSystem == null) mpEventSystem = gameObject.AddComponent<MultiplayerEventSystem>();
            if(uiInputModule == null) uiInputModule = gameObject.AddComponent<InputSystemUIInputModule>();
            uiInputModule.actionsAsset = inputActions.asset;
            mpEventSystem.playerRoot = null;
            playerInput.uiInputModule = uiInputModule;

            SwitchToUIMap();


            InputUser.onChange += onInputDeviceChange;
            ++InputUser.listenForUnpairedDeviceActivity;
            InputUser.onUnpairedDeviceUsed += WhenUnpairedDeviceUsed;

            inputActions.UI.Navigate.performed += WhenNavigationPerformed;
            inputActions.UI.Point.performed += WhenMouseNavigationPerformed;
        }

        public void Reinitalize()
        {
            if (playerInput.user.valid) return;
            playerInput.actions = null;
            playerInput.actions = inputActions.asset;
        }

        public void Teardown()
        {
            DeactivateInput();
            playerInput.user.UnpairDevicesAndRemoveUser();
        }

        private void OnDestroy()
        {
            InputUser.onUnpairedDeviceUsed -= WhenUnpairedDeviceUsed;
            InputUser.onChange -= onInputDeviceChange;
            --InputUser.listenForUnpairedDeviceActivity;
        }

        private void WhenNavigationPerformed(InputAction.CallbackContext obj)
        {
            if (lastNavigationType == NavigationType.Controller_Or_Keyboard) return;
            lastNavigationType = NavigationType.Controller_Or_Keyboard;
            onNavigationStyleChanged?.Invoke(this, NavigationType.Controller_Or_Keyboard);
        }

        private void WhenMouseNavigationPerformed(InputAction.CallbackContext obj)
        {
            if (lastNavigationType == NavigationType.Mouse) return;
            lastNavigationType = NavigationType.Mouse;
            onNavigationStyleChanged?.Invoke(this, NavigationType.Mouse);
        }

        public void Vibrate(float vibrateTime)
        {
            foreach (var id in currentDevices)
            {
                if (id is not Gamepad gamepad) continue;
            }
        }

        public void ActivateUIHandling()
        {
            uiInputModule.ActivateModule();
        }

        public void DeactivateUIHandling()
        {
            uiInputModule.DeactivateModule();
        }

        public bool EventDataIsMine(BaseEventData eventData)
        {
            return eventData.currentInputModule == uiInputModule;
        }

        public void SetUIRoot(GameObject uiRoot)
        {
            mpEventSystem.playerRoot = uiRoot;
        }

        private void WhenUnpairedDeviceUsed(InputControl arg1, InputEventPtr arg2)
        {
            if (!autoSwitchControlSchemes || !assignedDevices.Contains(arg1.device)) return;
            if (playerInput.user.valid == false)
            {
                Debug.LogError("Player Input user isn't valid.");
                return;
            }

            var dvs = arg1.device == Mouse.current || arg1.device == Keyboard.current
                ? new InputDevice[] { Keyboard.current, Mouse.current }
                : new InputDevice[] { arg1.device };

            playerInput.SwitchCurrentControlScheme(dvs);
            currentDevices = dvs.ToList();
            onCurrentDeviceChanged?.Invoke();
        }

        public void SwitchToDevice(InputDevice device)
        {
            if (device == null)
            {
                currentDevices.Clear();
                playerInput.SwitchCurrentControlScheme(Array.Empty<InputDevice>());
                onCurrentDeviceChanged?.Invoke();
                return;
            }

            if (!assignedDevices.Contains(device)) return;

            var dvs = device == Mouse.current || device == Keyboard.current
                ? new InputDevice[] { Keyboard.current, Mouse.current }
                : new InputDevice[] { device };

            try
            {
                playerInput.SwitchCurrentControlScheme(dvs);
            }
            catch (Exception e)
            {
                Debug.LogError("Exception throw while switching control scheme.");
                Debug.LogException(e);
            }
            currentDevices = dvs.ToList();
            onCurrentDeviceChanged?.Invoke();
        }

        public void ClearAssignedDevices(bool updateDevices = true)
        {
            assignedDevices.Clear();
            if(updateDevices) UpdateDevices();
        }

        public void RemoveDevice(InputDevice inputDevice, bool updateDevices = true)
        {
            playerInput.user.UnpairDevice(inputDevice);
            assignedDevices.Remove(inputDevice);
            if(updateDevices) UpdateDevices();
        }

        public void RemoveDevices(InputDevice[] inputDevices, bool updateDevices = true)
        {
            if (assignedDevices.Count == 0) return;
            
            foreach (var inputDevice in inputDevices)
            {
                if (inputDevice == Mouse.current || inputDevice == Keyboard.current)
                {
                    playerInput.user.UnpairDevice(Mouse.current);
                    playerInput.user.UnpairDevice(Keyboard.current);
                    assignedDevices.Remove(Mouse.current);
                    assignedDevices.Remove(Keyboard.current);
                    continue;
                }

                playerInput.user.UnpairDevice(inputDevice);
                assignedDevices.Remove(inputDevice);
            }

            if(updateDevices) UpdateDevices();
        }

        public void RemoveAllDevices(bool updateDevices = true)
        {
            assignedDevices.Clear();
            if(updateDevices) UpdateDevices();
        }

        public void AssignKeyboardAndMouse(bool updateDevices = true)
        {
            assignedDevices.Add(Keyboard.current);
            assignedDevices.Add(Mouse.current);
            if(updateDevices) UpdateDevices();
        }

        public void AssignInputDevice(InputDevice inputDevice, bool updateDevices = true)
        {
            assignedDevices.Add(inputDevice);
            if(updateDevices) UpdateDevices();
        }

        public void AssignInputDevices(InputDevice[] inputDeviceList, bool updateDevices = true)
        {
            foreach (var inputDevice in inputDeviceList)
            {
                assignedDevices.Add(inputDevice);
            }

            if(updateDevices) UpdateDevices();
        }

        public void AssignInputDevices(Gamepad[] gamepadList, bool updateDevices = true)
        {
            foreach (var gamepad in gamepadList)
            {
                assignedDevices.Add(gamepad);
            }

            if(updateDevices) UpdateDevices();
        }

        public virtual bool UpdateDevices()
        {
            inputActions.devices = assignedDevices.ToArray();
            for (int i = currentDevices.Count - 1; i >= 0; i--)
            {
                if (!assignedDevices.Contains(currentDevices[i])) currentDevices.RemoveAt(i);
            }

            Reinitalize();
            
            if (currentDevices.Count == 0 && assignedDevices.Count > 0)
                SwitchToDevice(assignedDevices.FirstOrDefault());
            else if (currentDevices.Count == 0)
                SwitchToDevice(null);
            return true;
        }

        public void ActivateInput()
        {
            playerInput.ActivateInput();
        }

        public void DeactivateInput()
        {
            playerInput.DeactivateInput();
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

        public void SetID(int id)
        {
            Id = id;
        }

        public string GetBindingOverridesAsJson()
        {
            return inputActions.SaveBindingOverridesAsJson();
        }

        public void ApplyBindingOverrides(string overrides)
        {
            inputActions.LoadBindingOverridesFromJson(overrides);
        }

        public void ResetBindingOverrides()
        {
            inputActions.RemoveAllBindingOverrides();
        }

        public void ApplyProfile(string profileName)
        {
            if (string.IsNullOrEmpty(profileName))
            {
                inputActions.RemoveAllBindingOverrides();
                return;
            }

            var pm = HnSFManagersContainer.instance.profilesManager;
            if (!pm.TryGetProfile(profileName, out var pd)) return;
            CurrentProfile = profileName;
            ApplyBindingOverrides(pd.overrides);
        }

        public InputManager.ControlSchemeType GetCurrentDeviceType()
        {
            if (currentDevices.Count == 0) return InputManager.ControlSchemeType.KEYBOARD_MOUSE;
            if (currentDevices[0] == Keyboard.current
                || currentDevices[0] == Mouse.current) return InputManager.ControlSchemeType.KEYBOARD_MOUSE;
            return InputManager.ControlSchemeType.GAMEPAD;
        }

        void onInputDeviceChange(InputUser user, InputUserChange change, InputDevice device)
        {
            if (User != user) return;


            if (change == InputUserChange.DeviceLost)
            {
                //Debug.Log("Device lost.");
            }

            if (change == InputUserChange.Removed)
            {
                //Debug.Log("Change removed");
            }

            /*
            if (change == InputUserChange.ControlSchemeChanged)
            {
                var oldControlScheme = controlScheme;
                switch (playerInput.currentControlScheme)
                {
                    case "Gamepad":
                        if (device?.description.manufacturer == "Sony Interactive Entertainment")
                        {
                            controlScheme = InputManager.ControlSchemeType.PS_GAMEPAD;
                        }
                        else if (device?.description.manufacturer == "Nintendo")
                        {
                            controlScheme = InputManager.ControlSchemeType.SWITCH_GAMEPAD;
                        }
                        else
                        {
                            controlScheme = InputManager.ControlSchemeType.XBOX_GAMEPAD;
                        }
                        break;
                    case "KeyboardMouse":
                        controlScheme = InputManager.ControlSchemeType.KEYBOARD_MOUSE;
                        break;
                }
                if (controlScheme != oldControlScheme)
                    onControlSchemeChanged?.Invoke(this, controlScheme);
            }*/
        }
    }
}
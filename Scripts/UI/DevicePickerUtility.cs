using System.Collections.Generic;
using System.Linq;
using HnSF.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace HnSF
{
    public class DevicePickerUtility : MonoBehaviour
    {
        public delegate void DelegatePickerEvent(DevicePickerUtility dpu);

        public DelegatePickerEvent OnPickerConfirm;
        public DelegatePickerEvent OnPickerCancel;

        protected InputManager inputManager;

        public GameObject canvasObject;
        
        protected int minimumPlayers = 0;
        protected int maximumPlayers = 0;

        private Dictionary<InputDevice, DevicePickerControllerInstance> inputDeviceItemMap = new();
        private Dictionary<InputDevice, Vector2> lastNavigateInput = new();

        protected List<List<InputDevice>> newDeviceLists = new();

        protected bool[] enabledRects = new bool[4];

        public Sprite genericControllerIcon;
        public Sprite genericKeyboardIcon;
        public Sprite genericMouseIcon;

        public DevicePickerControllerInstance controllerInstancePrefab;
        public DevicePickerPanelInstance unassignedPanelInstance;
        public DevicePickerPanelInstance[] newPlayerPanelInstances = new DevicePickerPanelInstance[4];

        protected virtual void Awake()
        {
            canvasObject?.SetActive(false);
        }

        public void Open(int minimumPlayers = 0, int maximumPlayers = 4)
        {
            this.minimumPlayers = minimumPlayers;
            this.maximumPlayers = maximumPlayers;
            inputManager = HnSFManagersContainer.instance.inputManager;
            EventSystem.current.SetSelectedGameObject(null);
            canvasObject.SetActive(true);
            Initialize();
        }

        public void Close()
        {
            var systemPlayer = inputManager.GetSystemPlayer();
            systemPlayer.inputActions.UI.Navigate.performed -= ReadSystemPlayerNavigate;
            systemPlayer.inputActions.UI.Submit.performed -= WhenSubmitPerformed;
            systemPlayer.inputActions.UI.Pause.performed -= WhenSubmitPerformed;
            systemPlayer.inputActions.UI.Cancel.performed -= WhenCancelPerformed;
            inputDeviceItemMap.Clear();
            lastNavigateInput.Clear();
            newDeviceLists.Clear();
            EventSystem.current.SetSelectedGameObject(null);
            canvasObject.SetActive(false);
        }

        protected virtual void Initialize()
        {
            for (int i = 0; i < 5; i++)
            {
                newDeviceLists.Add(new List<InputDevice>());
            }

            for (int i = 0; i < enabledRects.Length; i++)
            {
                enabledRects[i] = i <= (maximumPlayers - 1) ? true : false;
            }

            for (int i = 0; i < newPlayerPanelInstances.Length; i++)
            {
                if(newPlayerPanelInstances[i].initialColor == Color.black) newPlayerPanelInstances[i].initialColor = newPlayerPanelInstances[i].backgroundImage.color;
                if (!enabledRects[i]) newPlayerPanelInstances[i].backgroundImage.color *= Color.gray;
            }

            CleanupRect(unassignedPanelInstance);
            foreach (var ppr in newPlayerPanelInstances) CleanupRect(ppr);

            inputDeviceItemMap.Clear();
            lastNavigateInput.Clear();

            var systemPlayer = inputManager.GetSystemPlayer();
            newDeviceLists[0] = systemPlayer.assignedDevices.ToList();

            foreach (InputDevice inputDevice in systemPlayer.assignedDevices)
            {
                if (inputDevice == Mouse.current) continue;
               var go = GameObject.Instantiate(controllerInstancePrefab, unassignedPanelInstance.controllerContainerRect, false);
                go.controllerName.text = $"{inputDevice.displayName}";

                if (inputDevice.name == "Keyboard")
                {
                    go.controllerImage.sprite = genericKeyboardIcon;
                }
                else if (inputDevice.name == "Mouse")
                {
                    go.controllerImage.sprite = genericMouseIcon;
                }
                else
                {
                    go.controllerImage.sprite = genericControllerIcon;
                }

                inputDeviceItemMap.Add(inputDevice, go);
                lastNavigateInput.Add(inputDevice, new Vector2());
            }

            systemPlayer.SwitchToUIMap();

            systemPlayer.inputActions.UI.Navigate.performed += ReadSystemPlayerNavigate;
            systemPlayer.inputActions.UI.Submit.performed += WhenSubmitPerformed;
            systemPlayer.inputActions.UI.Pause.performed += WhenSubmitPerformed;
            systemPlayer.inputActions.UI.Cancel.performed += WhenCancelPerformed;
        }

        protected void WhenCancelPerformed(InputAction.CallbackContext obj)
        {
            OnPickerCancel?.Invoke(this);
        }

        protected void WhenSubmitPerformed(InputAction.CallbackContext obj)
        {
            if (GetValidInputPlayers().Count < minimumPlayers) return;
            OnPickerConfirm?.Invoke(this);
        }

        protected void ReadSystemPlayerNavigate(InputAction.CallbackContext obj)
        {
            var systemPlayer = inputManager.GetSystemPlayer();

            var input = CompressInput(obj.ReadValue<Vector2>());
            var movingDevice = systemPlayer.playerInput.GetDevice<InputDevice>();
            if (input == Vector2.zero)
            {
                lastNavigateInput[movingDevice] = Vector2.zero;
                return;
            }

            if (input != lastNavigateInput[movingDevice])
            {
                MoveDevice(movingDevice, input);
            }

            lastNavigateInput[movingDevice] = input;
        }

        protected virtual void MoveDevice(InputDevice movingDevice, Vector2 input)
        {
            if (newDeviceLists[0].Contains(movingDevice))
            {
                // SYSTEM
                if (input == Vector2.left && MoveDeviceToPlayer(2, movingDevice)
                    || input == Vector2.right && MoveDeviceToPlayer(3, movingDevice))
                {
                    newDeviceLists[0].Remove(movingDevice);
                }
            }
            else if (newDeviceLists[1].Contains(movingDevice))
            {
                // PLAYER 1
                if (input == Vector2.right && MoveDeviceToPlayer(2, movingDevice))
                {
                    newDeviceLists[1].Remove(movingDevice);
                }
            }
            else if (newDeviceLists[2].Contains(movingDevice))
            {
                // PLAYER 2
                if (input == Vector2.left && MoveDeviceToPlayer(1, movingDevice)
                    || input == Vector2.right && MoveDeviceToPlayer(0, movingDevice))
                {
                    newDeviceLists[2].Remove(movingDevice);
                }
            }
            else if (newDeviceLists[3].Contains(movingDevice))
            {
                // PLAYER 3
                if (input == Vector2.left && MoveDeviceToPlayer(0, movingDevice)
                    || input == Vector2.right && MoveDeviceToPlayer(4, movingDevice))
                {
                    newDeviceLists[3].Remove(movingDevice);
                }
            }
            else if (newDeviceLists[4].Contains(movingDevice))
            {
                // PLAYER 4
                if (input == Vector2.left && MoveDeviceToPlayer(3, movingDevice))
                {
                    newDeviceLists[4].Remove(movingDevice);
                }
            }
        }

        protected virtual bool MoveDeviceToPlayer(int playerId, InputDevice movingDevice)
        {
            if (playerId == 0)
            {
                inputDeviceItemMap[movingDevice].transform.SetParent(unassignedPanelInstance.controllerContainerRect, false);   
            }
            else
            {
                inputDeviceItemMap[movingDevice].transform.SetParent(newPlayerPanelInstances[playerId-1].controllerContainerRect, false);   
            }
            newDeviceLists[playerId].Add(movingDevice);
            return true;
        }
        
        protected void CleanupRect(DevicePickerPanelInstance panelInstance)
        {
            foreach (Transform child in panelInstance.controllerContainerRect.transform)
            {
                Destroy(child.gameObject);
            }
        }

        protected Vector2 CompressInput(Vector2 input)
        {
            if (input.magnitude < 0.25f) return Vector2.zero;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                input.y = 0;
            }
            else
            {
                input.x = 0;
            }

            input.Normalize();
            return input;
        }

        public List<List<InputDevice>> GetValidInputPlayers()
        {
            var c = newDeviceLists.ToList();
            c.RemoveAt(0); // System.
            for (int i = c.Count - 1; i >= 0; i--)
            {
                if (c[i].Count == 0) c.RemoveAt(i);
                else if (c[i].Contains(Keyboard.current)) c[i].Add(Mouse.current);
            }

            return c;
        }
    }
}
using System;
using System.Collections.Generic;
using CT.LocalInputManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HnSF.ui.menus.examples.mainmenu
{
    public class MainMenuScreenManagerInput : MonoBehaviour
    {
        public MainMenuScreenManager screenManager;

        private List<InputPlayerManager> listeningTo = new List<InputPlayerManager>();
        
        private async void OnEnable()
        {
            try
            {
                await UniTask.WaitUntil(() => InputManager.initialized == true,
                    cancellationToken: destroyCancellationToken);
                SetupInput();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnDisable()
        {
            TeardownInput();
        }
        
        private void OnDestroy()
        {
            TeardownInput();
        }

        private void SetupInput()
        {
            var inputManager = InputManager.instance;
            if (inputManager == null) return;
            inputManager.onPlayerAdded.AddListener(WhenPlayerAdded);
            inputManager.onPlayerRemoved.AddListener(WhenPlayerRemoved);
            UpdateListeningInputs();
        }

        private void UpdateListeningInputs()
        {
            var inputManager = InputManager.instance;
            if (inputManager == null) return;

            foreach (var player in inputManager.playerInputManagers)
            {
                RegisterInputs(player);
            }
        }
        
        private void WhenPlayerAdded(InputPlayerManager arg0)
        {
            RegisterInputs(arg0);
        }
        
        private void WhenPlayerRemoved(InputPlayerManager arg0)
        {
            DeregisterInputs(arg0);
            listeningTo.Remove(arg0);
        }

        private Dictionary<InputPlayerManager, Action<InputAction.CallbackContext>> actionsPause = new();
        private Dictionary<InputPlayerManager, Action<InputAction.CallbackContext>> actionsCancel = new();
        private Dictionary<InputPlayerManager, Action<InputAction.CallbackContext>> actionsSubmit = new();
        private Dictionary<InputPlayerManager, Action<InputAction.CallbackContext>> actionsNavigate = new();
        
        private void RegisterInputs(InputPlayerManager inputPlayerManager)
        {
            if (listeningTo.Contains(inputPlayerManager)) return;
            listeningTo.Add(inputPlayerManager);

            int id = inputPlayerManager.Id;
            
            Action<InputAction.CallbackContext> actionPause = (callbackContext) => { WhenInputPause(callbackContext, id); };
            Action<InputAction.CallbackContext> actionCancel = (callbackContext) => { WhenInputCancel(callbackContext, id); };
            Action<InputAction.CallbackContext> actionSubmit = (callbackContext) => { WhenInputSubmit(callbackContext, id); };
            Action<InputAction.CallbackContext> actionNavigate = (callbackContext) => { WhenNavigate(callbackContext, id); };
            
            inputPlayerManager.inputActions.UI.Pause.performed += actionPause;
            inputPlayerManager.inputActions.UI.Cancel.performed += actionCancel;
            inputPlayerManager.inputActions.UI.Submit.performed += actionSubmit;
            inputPlayerManager.inputActions.UI.Navigate.performed += actionNavigate;
            
            actionsPause.Add(inputPlayerManager, actionPause);
            actionsCancel.Add(inputPlayerManager, actionCancel);
            actionsSubmit.Add(inputPlayerManager, actionSubmit);
            actionsNavigate.Add(inputPlayerManager, actionNavigate);
        }

        private void WhenNavigate(InputAction.CallbackContext callbackContext, int id)
        {
            screenManager.OnNavigate(callbackContext.ReadValue<Vector2>(), id, null);
            screenManager.OnNavigateRaw(callbackContext.ReadValue<Vector2>(), id, null);
        }

        private void WhenInputSubmit(InputAction.CallbackContext callbackContext, int id)
        {
            screenManager.OnInputConfirmPressed(id, null);
        }

        private void WhenInputCancel(InputAction.CallbackContext callbackContext, int id)
        {
            screenManager.OnInputBackPressed(id, null);
        }

        private void WhenInputPause(InputAction.CallbackContext callbackContext, int id)
        {
            screenManager.OnInputStartPressed(id, null);
        }

        private void DeregisterInputs(InputPlayerManager inputPlayerManager)
        {
            if(!listeningTo.Contains(inputPlayerManager)) return;
            
            inputPlayerManager.inputActions.UI.Pause.performed -= actionsPause[inputPlayerManager];
            inputPlayerManager.inputActions.UI.Cancel.performed -= actionsCancel[inputPlayerManager];
            inputPlayerManager.inputActions.UI.Submit.performed -= actionsSubmit[inputPlayerManager];
            inputPlayerManager.inputActions.UI.Navigate.performed -= actionsNavigate[inputPlayerManager];
            
            actionsPause.Remove(inputPlayerManager);
            actionsCancel.Remove(inputPlayerManager);
            actionsSubmit.Remove(inputPlayerManager);
            actionsNavigate.Remove(inputPlayerManager);
        }

        private void TeardownInput()
        {
            foreach (var inputPlayerManager in listeningTo)
            {
                DeregisterInputs(inputPlayerManager);
            }
            
            listeningTo.Clear();
        }
    }
}
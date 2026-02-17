using Cysharp.Threading.Tasks;
using HnSF.Input;
using HnSF.ui.menus.examples.mainmenu;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HnSF
{
    public static class LobbyHelpers
    {
        public static async UniTask ReturnToTraditionalLobby()
        {
            await SceneManager.LoadSceneAsync("HnSF_MainMenu");
            await UniTask.NextFrame();
            var screenSetter = GameObject.FindFirstObjectByType<MainMenuScreenResetter>();
            screenSetter.ReturnToLobbyScreen();
        }

        public static async UniTask ReturnToMainMenu()
        {
            await SceneManager.LoadSceneAsync("HnSF_MainMenu");
            await UniTask.NextFrame();
            var screenSetter = GameObject.FindFirstObjectByType<MainMenuScreenResetter>();
            //screenSetter.ReturnToLobbyScreen();
            InputManager.instance.ReturnAllDevicesToSystem();
            InputManager.instance.SetPlayerCount(0);
        }
    }
}
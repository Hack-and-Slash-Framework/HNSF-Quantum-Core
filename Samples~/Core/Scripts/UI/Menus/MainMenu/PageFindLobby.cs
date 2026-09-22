using CT.MenuNav;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HnSF.ui.menus.examples.mainmenu
{
    public class PageFindLobby : MenuPage
    {
        public TMP_InputField inputFieldDirectConnection;

        public ScrollRect scrollRectRoomList;

        [FormerlySerializedAs("mainMenuScreenHandler")] public MainMenuHelper mainMenuHelper;

        public override UniTask<bool> TryOpenAsync(MenuNavContext context)
        {
            attemptingConnection = false;
            inputFieldDirectConnection.text = "localhost";
            return base.TryOpenAsync(context);
        }
        
        private bool attemptingConnection;
        public virtual async void BUTTON_DirectConnect()
        {
            if (attemptingConnection) return;
            if (string.IsNullOrEmpty(inputFieldDirectConnection.text))
            {
                return;
            }

            attemptingConnection = true;
            if (!await mainMenuHelper.AttemptCreateOrJoinRoom(inputFieldDirectConnection.text))
            {
                attemptingConnection = false;
            }
        }

        public virtual async void BUTTON_CreateRoom()
        {
            if (attemptingConnection) return;
            attemptingConnection = true;
            
            if (!await mainMenuHelper.AttemptCreateOrJoinRoom(string.Empty))
            {
                attemptingConnection = false;
                return;
            }
            attemptingConnection = false;
        }
        
        public virtual void BUTTON_RefreshRoomList()
        {
            
        }
    }
}
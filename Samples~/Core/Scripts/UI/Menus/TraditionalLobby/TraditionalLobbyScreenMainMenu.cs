using CT.MenuNav;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui.menus.traditionallobby
{
    public class TraditionalLobbyScreenMainMenu : MenuBase
    {
        public Canvas canvas;
        public Button buttonLeaveLobby;
        public ScrollRect scrollRectChat;
        public ScrollRect scrollRectPlayers;
        public ScrollRect scrollRectRooms;

        public GameObject roomRectViewItem;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            roomRectViewItem.gameObject.SetActive(false);

            var screenInstance = (menuHandler as TraditionalLobbyScreenHandler);
            screenInstance.lobbyRepresentation.onRoomOpened.AddListener(UpdateRoomList);
            screenInstance.lobbyRepresentation.onRoomClosed.AddListener(UpdateRoomList);
            UpdateRoomList(-1);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return base.TryClose(direction, forceClose);
        }
        
        private void UpdateRoomList(int arg0)
        {
            foreach (Transform child in scrollRectRooms.content)
            {
                if (child.gameObject == roomRectViewItem.gameObject) continue;
                Destroy(child.gameObject);
            }
            
            var lobbyRepresentation = (MenuHandler as TraditionalLobbyScreenHandler).lobbyRepresentation;

            foreach (var room in lobbyRepresentation.rooms)
            {
                var roomId = room.roomId;
                GameObject roomViewItem = Instantiate(roomRectViewItem, scrollRectRooms.content, false);
                roomViewItem.SetActive(true);
                roomViewItem.GetComponentInChildren<TextMeshProUGUI>().text = $"Join Room {room.roomTitle}";
                roomViewItem.GetComponentInChildren<Button>().onClick.AddListener(() => { GoTo_RoomScreen(roomId); });
            }
            
            GameObject roomCreateItem = Instantiate(roomRectViewItem, scrollRectRooms.content, false);
            roomCreateItem.SetActive(true);
            roomCreateItem.GetComponentInChildren<TextMeshProUGUI>().text = "Create Room";
            roomCreateItem.GetComponentInChildren<Button>().onClick.AddListener(() => { CreateRoom(); });
        }
        
        private void CreateRoom()
        {
            var instanceHanlder = (MenuHandler as TraditionalLobbyScreenHandler);
            instanceHanlder.Forward(instanceHanlder.screenCreateRoom);
        }

        private void GoTo_RoomScreen(int roomId)
        {
            var instanceHanlder = (MenuHandler as TraditionalLobbyScreenHandler);
            instanceHanlder.screenRoom.roomId = roomId;
            instanceHanlder.Forward(instanceHanlder.screenRoom);
        }
    }
}


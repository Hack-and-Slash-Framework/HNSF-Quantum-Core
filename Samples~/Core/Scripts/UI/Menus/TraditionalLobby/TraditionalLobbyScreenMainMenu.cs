using CT.MenuNav;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui.menus.traditionallobby
{
    public class TraditionalLobbyScreenMainMenu : MenuPage
    {
        public TraditionalLobbyScreenHelper helper;
        
        public Canvas canvas;
        public Button buttonLeaveLobby;
        public ScrollRect scrollRectChat;
        public ScrollRect scrollRectPlayers;
        public ScrollRect scrollRectRooms;

        public GameObject roomRectViewItem;

        public override UniTask<bool> TryOpenAsync(MenuNavDirection direction, int pageCount)
        {
            roomRectViewItem.gameObject.SetActive(false);

            var screenInstance = helper;
            screenInstance.lobbyRepresentation.onRoomOpened.AddListener(UpdateRoomList);
            screenInstance.lobbyRepresentation.onRoomClosed.AddListener(UpdateRoomList);
            UpdateRoomList(-1);
            return base.TryOpenAsync(direction, pageCount);
        }

        public override UniTask<bool> TryCloseAsync(MenuNavDirection direction)
        {
            return base.TryCloseAsync(direction);
        }
        
        private void UpdateRoomList(int arg0)
        {
            foreach (Transform child in scrollRectRooms.content)
            {
                if (child.gameObject == roomRectViewItem.gameObject) continue;
                Destroy(child.gameObject);
            }
            
            var lobbyRepresentation = helper.lobbyRepresentation;

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
            helper.screenManager.TryForwardPage(helper.screenCreateRoom);
        }

        private void GoTo_RoomScreen(int roomId)
        {
            helper.screenRoom.roomId = roomId;
            helper.screenManager.TryForwardPage(helper.screenRoom);
        }
    }
}


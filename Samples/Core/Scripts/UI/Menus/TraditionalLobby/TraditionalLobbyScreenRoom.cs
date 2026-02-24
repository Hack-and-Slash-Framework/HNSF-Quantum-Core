using System;
using System.Linq;
using CT.MenuNav;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HnSF.ui.menus.traditionallobby
{
    public class TraditionalLobbyScreenRoom : MenuBase
    {
        public Canvas canvas;
        
        public Button buttonReturnToLobby;
        public ScrollRect scrollRectPlayerList;
        public Button buttonGamemode;
        public Button buttonGamemodeSettings;
        public Button buttonMap;
        public Button buttonReadyUp;
        public Button buttonJoinRoom;

        public TextMeshProUGUI gamemodeNameText;
        public TextMeshProUGUI mapNameText;
        
        public int roomId;
        [NonSerialized] public TraditionalLobbyUIRepresentation.Room room;

        public TraditionalLobbyPlayerListContentItem playerListItemPrefab;

        public LoadedAssetHandleWrapper gamemodeAssetHandle;
        public LoadedAssetHandleWrapper mapAssetHandle;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            
            playerListItemPrefab.gameObject.SetActive(false);
            RegisterInputEvents();
            AssignRoom(roomId);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            if (direction == MenuDirection.BACKWARDS)
            {
                
            }
            gameObject.SetActive(false);
            UnregisterInputEvents();
            return base.TryClose(direction, forceClose);
        }

        private void OnDestroy()
        {
            UnregisterInputEvents();
        }

        public void RegisterInputEvents()
        {
            var tlsh = (TraditionalLobbyScreenHandler)MenuHandler;
            if (tlsh == null || tlsh.inputPlayer == null) return;
            tlsh.inputPlayer.inputActions.UI.PageLeft.performed += PageTeamLeft;
            tlsh.inputPlayer.inputActions.UI.PageRight.performed += PageTeamRight;
        }
        
        public void UnregisterInputEvents()
        {
            var tlsh = (TraditionalLobbyScreenHandler)MenuHandler;
            if (tlsh == null || tlsh.inputPlayer == null) return;
            tlsh.inputPlayer.inputActions.UI.PageLeft.performed -= PageTeamLeft;
            tlsh.inputPlayer.inputActions.UI.PageRight.performed -= PageTeamRight;
        }

        private void PageTeamLeft(InputAction.CallbackContext obj)
        {
            if (!gamemodeAssetHandle.IsValid()) return;
            var gamemodeDefinition = gamemodeAssetHandle.GetAsset<BaseGamemodeDefinition>();
            if (gamemodeDefinition == null) return;
            
            var instanceHandler = (MenuHandler as TraditionalLobbyScreenHandler);

            var myPlayer = instanceHandler.lobbyRepresentation.GetPlayer(instanceHandler.roomSessionHandler.localClientPlayerIds[instanceHandler.GetLocalPlayerID()]);
            if (myPlayer == null) return;
            var myCurrentTeam = myPlayer.selectedTeamId;

            var allTeams = gamemodeDefinition.GetDefaultTeamConfig().ToList();

            var currentTeamIndex = allTeams.FindIndex(x => x.team == myCurrentTeam);
            if (currentTeamIndex == -1)
            {
                instanceHandler.roomSessionHandler.ChangePlayerTeam(
                    localPlayer: 0,
                    team: allTeams[0].team);
            }
            else
            {
                var cti = currentTeamIndex - 1;
                if (cti == -1) cti = allTeams.Count - 1;
                instanceHandler.roomSessionHandler.ChangePlayerTeam(
                    localPlayer: 0,
                    team: allTeams[cti].team);
            }
        }
        
        private void PageTeamRight(InputAction.CallbackContext obj)
        {
            if (!gamemodeAssetHandle.IsValid()) return;
            var gamemodeDefinition = gamemodeAssetHandle.GetAsset<BaseGamemodeDefinition>();
            if (gamemodeDefinition == null) return;
            
            var instanceHandler = (MenuHandler as TraditionalLobbyScreenHandler);

            var myPlayer = instanceHandler.lobbyRepresentation.GetPlayer(instanceHandler.roomSessionHandler.localClientPlayerIds[instanceHandler.GetLocalPlayerID()]);
            if (myPlayer == null) return;
            var myCurrentTeam = myPlayer.selectedTeamId;

            var allTeams = gamemodeDefinition.GetDefaultTeamConfig().ToList();

            var currentTeamIndex = allTeams.FindIndex(x => x.team == myCurrentTeam);
            if (currentTeamIndex == -1)
            {
                instanceHandler.roomSessionHandler.ChangePlayerTeam(
                    localPlayer: 0,
                    team: allTeams[0].team);
            }
            else
            {
                instanceHandler.roomSessionHandler.ChangePlayerTeam(
                    localPlayer: 0,
                    team: allTeams[(currentTeamIndex+1) % allTeams.Count].team);
            }
        }

        public void Update()
        {
            if(UnityEngine.Input.GetKeyDown(KeyCode.F5)) UpdateAll();
            if (UnityEngine.Input.GetKeyDown(KeyCode.F6))
            {
                string lobbyPlayerPrintout = "";
                for (int i = 0; i < room.players.Count; i++) lobbyPlayerPrintout += $"{room.players[i]},\n";
                Debug.Log(lobbyPlayerPrintout);
            }
        }

        public void BUTTON_ReturnToLobby()
        {
            var instanceHandler = (MenuHandler as TraditionalLobbyScreenHandler);
            instanceHandler.roomSessionHandler.LeaveRoom();
            MenuHandler.Back();
        }

        public void BUTTON_GamemodeSelect()
        {
            if (!IsRoomMaster()) return;
         
            var instanceHandler = (MenuHandler as TraditionalLobbyScreenHandler);
            
            var contentPickerInstanceManager = GenericContentPickerInstanceManager.instance;
            instanceHandler.screenContentPicker = contentPickerInstanceManager.CreateInstance<BaseGamemodeDefinition>(instanceHandler.transform);
            instanceHandler.screenContentPicker.inputPlayer = instanceHandler.inputPlayer;
            instanceHandler.Forward(instanceHandler.screenContentPicker);
            instanceHandler.screenContentPicker.Initialize<BaseGamemodeDefinition>(instanceHandler.inputPlayer);
            instanceHandler.screenContentPicker.SetCameraTarget(instanceHandler.instanceCamera);
            
            instanceHandler.screenContentPicker.onContentPicked.AddListener(WhenGamemodeSubmitted);
            instanceHandler.screenContentPicker.onCancel.AddListener(WhenCancelContentPick);
        }

        private void WhenCancelContentPick(GenericContentPickerInstance arg0)
        {
            var instanceHandler = (MenuHandler as TraditionalLobbyScreenHandler);
            MenuHandler.Back();
            GameObject.Destroy(instanceHandler.screenContentPicker);
        }

        private async void WhenGamemodeSubmitted(GenericContentPickerInstance arg0)
        {
            DisableAllButtons();
            var instanceHandler = (MenuHandler as TraditionalLobbyScreenHandler);
            var contentManager = HnSFManagersContainer.instance.contentManager;
            
            var tempGamemodeAssetHandle = arg0.ConfirmWantedContentAndRemoveFromList();
            ModAssetSoftReference gamemodeReference = tempGamemodeAssetHandle.assetReference;
            contentManager.ReleaseAssetFromMod(tempGamemodeAssetHandle);
            instanceHandler.screenContentPicker.Uninitialize();
            MenuHandler.Back();
            
            var gamemodeSetResult = await instanceHandler.roomSessionHandler.ChangeRoomGamemode(gamemodeReference);
            EnableAllButtons();
        }

        public void BUTTON_GamemodeSettings()
        {
            if (!IsRoomMaster()) return;
        }

        public void BUTTON_MapSelect()
        {
            if (!IsRoomMaster()) return;
            
            var instanceHandler = (MenuHandler as TraditionalLobbyScreenHandler);
            
            var contentPickerInstanceManager = GenericContentPickerInstanceManager.instance;
            instanceHandler.screenContentPicker = contentPickerInstanceManager.CreateInstance<IMapDefinition>(instanceHandler.transform);
            instanceHandler.screenContentPicker.inputPlayer = instanceHandler.inputPlayer;
            instanceHandler.Forward(instanceHandler.screenContentPicker);
            instanceHandler.screenContentPicker.Initialize<IMapDefinition>(instanceHandler.inputPlayer);
            instanceHandler.screenContentPicker.SetCameraTarget(instanceHandler.instanceCamera);
            
            instanceHandler.screenContentPicker.onContentPicked.AddListener(WhenMapSubmitted);
            instanceHandler.screenContentPicker.onCancel.AddListener(WhenCancelContentPick);
        }
        
        private async void WhenMapSubmitted(GenericContentPickerInstance arg0)
        {
            DisableAllButtons();
            var instanceHandler = (MenuHandler as TraditionalLobbyScreenHandler);
            var contentManager = HnSFManagersContainer.instance.contentManager;
            
            var tempAssetHandle = arg0.ConfirmWantedContentAndRemoveFromList();
            ModAssetSoftReference mapReference = tempAssetHandle.assetReference;
            contentManager.ReleaseAssetFromMod(tempAssetHandle);
            instanceHandler.screenContentPicker.Uninitialize();
            MenuHandler.Back();

            var setResult = await instanceHandler.roomSessionHandler.ChangeRoomMap(mapReference);
            EnableAllButtons();
        }

        public void BUTTON_CharacterSelect()
        {
            if(!IsInRoom()) return;
            _ = SetFighters();
        }
        
        private async UniTask SetFighters()
        {
            Debug.Log($"Room fighter count of {room.GetPlayerFighterCount()}");
            var fightersToSet = room.GetPlayerFighterCount();
            
            var instanceHandler = (MenuHandler as TraditionalLobbyScreenHandler);

            var contentPickerInstanceManager = GenericContentPickerInstanceManager.instance;
            instanceHandler.screenContentPicker = contentPickerInstanceManager.CreateInstance<IFighterDefinition>(instanceHandler.transform);
            instanceHandler.screenContentPicker.inputPlayer = instanceHandler.inputPlayer;
            instanceHandler.Forward(instanceHandler.screenContentPicker);
            instanceHandler.screenContentPicker.SetCameraTarget(instanceHandler.instanceCamera);

            LoadedAssetHandleWrapper? fighterPickResult = null;
            LoadedAssetHandleWrapper[] fightersPicked = new LoadedAssetHandleWrapper[fightersToSet];
            ModAssetSoftReference[] fightersPickedReferences = new ModAssetSoftReference[fightersToSet];
            int i = 0;
            
            instanceHandler.screenContentPicker.onContentPicked.AddListener(WhenFighterSubmitted);
            instanceHandler.screenContentPicker.onCancel.AddListener(WhenCancelFighterPick);
            
            for (i = 0; i < fightersToSet; i++)
            {
                fighterPickResult = null;
                instanceHandler.screenContentPicker.Initialize<IFighterDefinition>(instanceHandler.inputPlayer);
                
                await UniTask.WaitUntil(() => fighterPickResult.HasValue);

                if (fighterPickResult.HasValue == false || fighterPickResult.Value.IsValid() == false)
                {
                    // Unload the fighters
                    return;
                }
                fightersPicked[i] = fighterPickResult.Value;
                fightersPickedReferences[i] = fighterPickResult.Value.assetReference;
                
                instanceHandler.screenContentPicker.Uninitialize();
            }
            
            instanceHandler.screenContentPicker.onContentPicked.RemoveListener(WhenFighterSubmitted);
            instanceHandler.screenContentPicker.onCancel.RemoveListener(WhenCancelFighterPick);

            var setResult = await instanceHandler.roomSessionHandler.ChangePlayerFighters(instanceHandler.GetLocalPlayerID(), fightersPickedReferences);
            MenuHandler.Back();
            
            void WhenFighterSubmitted(GenericContentPickerInstance arg0)
            {
                fighterPickResult = arg0.ConfirmWantedContentAndRemoveFromList();
            }
            
            void WhenCancelFighterPick(GenericContentPickerInstance arg0)
            {
                fighterPickResult = new LoadedAssetHandleWrapper();
            }
        }

        public void BUTTON_ReadyUp()
        {
            var instanceHandler = (MenuHandler as TraditionalLobbyScreenHandler);

            instanceHandler.roomSessionHandler.AttemptToggleReadyState(0);
        }

        public async void BUTTON_JoinRoom()
        {
            buttonJoinRoom.interactable = false;
            
            var instanceHandler = (MenuHandler as TraditionalLobbyScreenHandler);

            var joinResult = await instanceHandler.roomSessionHandler.TryJoinRoom(roomId);
            buttonJoinRoom.interactable = true;
            
            if (joinResult)
            {
                UpdateAll();
            }
        }

        private void DisableAllButtons()
        {
            buttonGamemode.interactable = false;
            buttonGamemodeSettings.interactable = false;
            buttonMap.interactable = false;
        }

        private void EnableAllButtons()
        {
            buttonGamemode.interactable = true;
            buttonGamemodeSettings.interactable = true;
            buttonMap.interactable = true;
            UpdateButtons();
        }

        public bool IsRoomMaster()
        {
            var menuHandler = (MenuHandler as TraditionalLobbyScreenHandler);

            return room.GetRoomMasterPlayerId() == menuHandler.roomSessionHandler.localClientPlayerIds[menuHandler.GetLocalPlayerID()];
        }

        public bool IsInRoom()
        {
            var menuHandler = (MenuHandler as TraditionalLobbyScreenHandler);
            return room.players.Contains(menuHandler.roomSessionHandler.localClientPlayerIds[menuHandler.GetLocalPlayerID()]);
        }

        public void AssignRoom(int roomId)
        {
            if (room != null)
            {
                room.onClosed.RemoveListener(WhenRoomClosed);
                room.onUpdated.RemoveListener(WhenRoomUpdated);
            }
            
            this.roomId = roomId;
            room = (MenuHandler as TraditionalLobbyScreenHandler).lobbyRepresentation.GetRoom(roomId);
            if (room == null)
            {
                MenuHandler.Back();
                return;
            }
            
            room.onClosed.AddListener(WhenRoomClosed);
            room.onUpdated.AddListener(WhenRoomUpdated);
            UpdatePlayerList();
            UpdateButtons();
            WhenRoomUpdated(room);
        }

        private bool updateLock = false;
        private async void WhenRoomUpdated(TraditionalLobbyUIRepresentation.Room arg0)
        {
            if (updateLock) await UniTask.WaitUntil(() => updateLock == false);
            updateLock = true;
            var instanceHandler = (MenuHandler as TraditionalLobbyScreenHandler);
            var contentManager = HnSFManagersContainer.instance.contentManager;
            
            // Gamemode
            gamemodeAssetHandle = await UpdateAssetHandle(arg0.selectedGamemode, gamemodeAssetHandle);
            mapAssetHandle = await UpdateAssetHandle(arg0.selectedMap, mapAssetHandle);
            
            UpdateAll();
            updateLock = false;

            async UniTask<LoadedAssetHandleWrapper> UpdateAssetHandle(string assetReference, LoadedAssetHandleWrapper handle)
            {
                if (string.IsNullOrEmpty(assetReference))
                {
                    if (handle.IsValid())
                    {
                        contentManager.ReleaseAssetFromMod(handle);
                    }
                    return default;
                }
                else if (handle.IsValid() && handle.assetReference.ToString() != assetReference)
                {
                    contentManager.ReleaseAssetFromMod(handle);
                    var loadResult = await contentManager.LoadAssetFromModAsync(new ModAssetSoftReference(assetReference));
                    if (loadResult.result)
                    {
                        handle = loadResult.handle;
                        return handle;
                    }
                    return default;
                }else if (handle.IsValid() == false)
                {
                    var loadResult = await contentManager.LoadAssetFromModAsync(new ModAssetSoftReference(assetReference));
                    if (loadResult.result)
                    {
                        handle = loadResult.handle;
                        return handle;
                    }
                    return default;
                }

                return handle;
            }
        }

        private void WhenRoomClosed(TraditionalLobbyUIRepresentation.Room arg0)
        {
            MenuHandler.Back();
        }

        public void UpdateAll()
        {
            UpdatePlayerList();
            UpdateButtons();
            
            gamemodeNameText.text = gamemodeAssetHandle.IsValid() ? gamemodeAssetHandle.GetAsset<BaseGamemodeDefinition>().Name : "?";
            mapNameText.text = mapAssetHandle.IsValid() ? mapAssetHandle.GetAsset<IMapDefinition>().Name : "?";
        }
        
        public void UpdatePlayerList()
        {
            var handler = (MenuHandler as TraditionalLobbyScreenHandler);
            
            foreach (Transform child in scrollRectPlayerList.content)
            {
                if (child.gameObject == playerListItemPrefab.gameObject) continue;
                Destroy(child.gameObject);
            }

            foreach (var playerId in room.players)
            {
                var playerInfo = handler.lobbyRepresentation.GetPlayer(playerId);
                if(playerInfo == null) continue;

                playerInfo.onUpdated.RemoveListener(WhenPlayerUpdated);
                playerInfo.onUpdated.AddListener(WhenPlayerUpdated);
                
                var playerListItem = Instantiate(playerListItemPrefab, scrollRectPlayerList.content, false);
                playerListItem.playerNameText.text = playerInfo.playerName;
                playerListItem.playerTeamText.text = playerInfo.selectedTeamId == 0 ? "No Team Selected" : playerInfo.selectedTeamId.ToString();
                playerListItem.gameObject.SetActive(true);

                playerListItem.GetComponentInChildren<Image>().color = playerInfo.ready ? Color.green : Color.white;
            }
        }

        private void WhenPlayerUpdated(TraditionalLobbyUIRepresentation.Player arg0)
        {
            UpdatePlayerList();
        }

        public void UpdateButtons()
        {
            buttonReadyUp.gameObject.SetActive(false);
            buttonJoinRoom.gameObject.SetActive(false);
            if (IsInRoom())
            {
                buttonReadyUp.gameObject.SetActive(true);
            }
            else
            {
                buttonJoinRoom.gameObject.SetActive(true);
            }
        }
    }
}


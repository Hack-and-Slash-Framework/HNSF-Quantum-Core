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
    public class TraditionalLobbyScreenRoom : MenuPage
    {
        [Space]
        public TraditionalLobbyScreenHelper helper;
        
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

        public override UniTask<bool> TryOpenAsync(MenuNavDirection direction, int pageCount)
        {
            playerListItemPrefab.gameObject.SetActive(false);
            RegisterInputEvents();
            AssignRoom(roomId);
            return base.TryOpenAsync(direction, pageCount);
        }

        public override UniTask<bool> TryCloseAsync(MenuNavDirection direction)
        {
            UnregisterInputEvents();
            return base.TryCloseAsync(direction);
        }
        

        private void OnDestroy()
        {
            UnregisterInputEvents();
        }

        public void RegisterInputEvents()
        {
            /*
            var tlsh = (TraditionalLobbyScreenHelper)MenuHandler;
            if (tlsh == null || tlsh.inputPlayer == null) return;
            tlsh.inputPlayer.inputActions.UI.PageLeft.performed += PageTeamLeft;
            tlsh.inputPlayer.inputActions.UI.PageRight.performed += PageTeamRight;*/
        }
        
        public void UnregisterInputEvents()
        {
            /*
            var tlsh = (TraditionalLobbyScreenHelper)MenuHandler;
            if (tlsh == null || tlsh.inputPlayer == null) return;
            tlsh.inputPlayer.inputActions.UI.PageLeft.performed -= PageTeamLeft;
            tlsh.inputPlayer.inputActions.UI.PageRight.performed -= PageTeamRight;*/
        }

        private void PageTeamLeft(InputAction.CallbackContext obj)
        {
            if (!gamemodeAssetHandle.IsValid()) return;
            var gamemodeDefinition = gamemodeAssetHandle.GetAsset<BaseGamemodeDefinition>();
            if (gamemodeDefinition == null) return;

            var myPlayer = helper.lobbyRepresentation.GetPlayer(helper.roomSessionHandler.localClientPlayerIds[helper.GetLocalPlayerIndex()]);
            if (myPlayer == null) return;
            var myCurrentTeam = myPlayer.selectedTeamId;

            var allTeams = gamemodeDefinition.GetDefaultTeamConfig().ToList();

            var currentTeamIndex = allTeams.FindIndex(x => x.team == myCurrentTeam);
            if (currentTeamIndex == -1)
            {
                helper.roomSessionHandler.ChangePlayerTeam(
                    localPlayer: 0,
                    team: allTeams[0].team);
            }
            else
            {
                var cti = currentTeamIndex - 1;
                if (cti == -1) cti = allTeams.Count - 1;
                helper.roomSessionHandler.ChangePlayerTeam(
                    localPlayer: 0,
                    team: allTeams[cti].team);
            }
        }
        
        private void PageTeamRight(InputAction.CallbackContext obj)
        {
            if (!gamemodeAssetHandle.IsValid()) return;
            var gamemodeDefinition = gamemodeAssetHandle.GetAsset<BaseGamemodeDefinition>();
            if (gamemodeDefinition == null) return;

            var myPlayer = helper.lobbyRepresentation.GetPlayer(helper.roomSessionHandler.localClientPlayerIds[helper.GetLocalPlayerIndex()]);
            if (myPlayer == null) return;
            var myCurrentTeam = myPlayer.selectedTeamId;

            var allTeams = gamemodeDefinition.GetDefaultTeamConfig().ToList();

            var currentTeamIndex = allTeams.FindIndex(x => x.team == myCurrentTeam);
            if (currentTeamIndex == -1)
            {
                helper.roomSessionHandler.ChangePlayerTeam(
                    localPlayer: 0,
                    team: allTeams[0].team);
            }
            else
            {
                helper.roomSessionHandler.ChangePlayerTeam(
                    localPlayer: 0,
                    team: allTeams[(currentTeamIndex+1) % allTeams.Count].team);
            }
        }

        public void Update()
        {
            if(Keyboard.current[Key.F5].wasPressedThisFrame) UpdateAll();
            if (Keyboard.current[Key.F6].wasPressedThisFrame)
            {
                string lobbyPlayerPrintout = "";
                for (int i = 0; i < room.players.Count; i++) lobbyPlayerPrintout += $"{room.players[i].ToString()},\n";
                Debug.Log(lobbyPlayerPrintout);
            }
        }

        public void BUTTON_ReturnToLobby()
        {
            helper.roomSessionHandler.LeaveRoom();
            _ = helper.screenManager.TryBackPageAsync();
        }

        public void BUTTON_GamemodeSelect()
        {
            if (!IsRoomMaster()) return;
            
            var contentPickerInstanceManager = GenericContentPickerInstanceManager.instance;
            helper.screenContentPicker = contentPickerInstanceManager.CreateInstance<BaseGamemodeDefinition>(helper.transform);
            _ = helper.screenManager.TryForwardPageAsync(helper.screenContentPicker);
            helper.screenContentPicker.Initialize<BaseGamemodeDefinition>();
            helper.screenContentPicker.SetCameraTarget(helper.instanceCamera);
            
            helper.screenContentPicker.onContentPicked.AddListener(WhenGamemodeSubmitted);
            helper.screenContentPicker.onCancel.AddListener(WhenCancelContentPick);
        }

        private void WhenCancelContentPick(GenericContentPickerInstance arg0)
        {
            _ = helper.screenManager.TryBackPageAsync();
            GameObject.Destroy(helper.screenContentPicker);
        }

        private async void WhenGamemodeSubmitted(GenericContentPickerInstance arg0)
        {
            DisableAllButtons();
            var contentManager = HnSFManagersContainer.instance.contentManager;
            
            var tempGamemodeAssetHandle = arg0.ConfirmWantedContentAndRemoveFromList();
            ModAssetSoftReference gamemodeReference = tempGamemodeAssetHandle.assetReference;
            contentManager.ReleaseAssetFromMod(tempGamemodeAssetHandle);
            helper.screenContentPicker.Uninitialize();
            await helper.screenManager.TryBackPageAsync();
            
            var gamemodeSetResult = await helper.roomSessionHandler.ChangeRoomGamemode(gamemodeReference);
            EnableAllButtons();
        }

        public void BUTTON_GamemodeSettings()
        {
            if (!IsRoomMaster()) return;
        }

        public void BUTTON_MapSelect()
        {
            if (!IsRoomMaster()) return;
            
            var contentPickerInstanceManager = GenericContentPickerInstanceManager.instance;
            helper.screenContentPicker = contentPickerInstanceManager.CreateInstance<IMapDefinition>(helper.transform);
            _ = helper.screenManager.TryForwardPageAsync(helper.screenContentPicker);
            helper.screenContentPicker.Initialize<IMapDefinition>();
            helper.screenContentPicker.SetCameraTarget(helper.instanceCamera);
            
            helper.screenContentPicker.onContentPicked.AddListener(WhenMapSubmitted);
            helper.screenContentPicker.onCancel.AddListener(WhenCancelContentPick);
        }
        
        private async void WhenMapSubmitted(GenericContentPickerInstance arg0)
        {
            DisableAllButtons();
            var contentManager = HnSFManagersContainer.instance.contentManager;
            
            var tempAssetHandle = arg0.ConfirmWantedContentAndRemoveFromList();
            ModAssetSoftReference mapReference = tempAssetHandle.assetReference;
            contentManager.ReleaseAssetFromMod(tempAssetHandle);
            helper.screenContentPicker.Uninitialize();
            await helper.screenManager.TryBackPageAsync();

            var setResult = await helper.roomSessionHandler.ChangeRoomMap(mapReference);
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

            var contentPickerInstanceManager = GenericContentPickerInstanceManager.instance;
            helper.screenContentPicker = contentPickerInstanceManager.CreateInstance<IFighterDefinition>(helper.transform);
            await helper.screenManager.TryForwardPageAsync(helper.screenContentPicker);
            helper.screenContentPicker.SetCameraTarget(helper.instanceCamera);

            LoadedAssetHandleWrapper? fighterPickResult = null;
            LoadedAssetHandleWrapper[] fightersPicked = new LoadedAssetHandleWrapper[fightersToSet];
            ModAssetSoftReference[] fightersPickedReferences = new ModAssetSoftReference[fightersToSet];
            int i = 0;
            
            helper.screenContentPicker.onContentPicked.AddListener(WhenFighterSubmitted);
            helper.screenContentPicker.onCancel.AddListener(WhenCancelFighterPick);
            
            for (i = 0; i < fightersToSet; i++)
            {
                fighterPickResult = null;
                helper.screenContentPicker.Initialize<IFighterDefinition>();
                
                await UniTask.WaitUntil(() => fighterPickResult.HasValue);

                if (fighterPickResult.HasValue == false || fighterPickResult.Value.IsValid() == false)
                {
                    // Unload the fighters
                    return;
                }
                fightersPicked[i] = fighterPickResult.Value;
                fightersPickedReferences[i] = fighterPickResult.Value.assetReference;
                
                helper.screenContentPicker.Uninitialize();
            }
            
            helper.screenContentPicker.onContentPicked.RemoveListener(WhenFighterSubmitted);
            helper.screenContentPicker.onCancel.RemoveListener(WhenCancelFighterPick);

            var setResult = await helper.roomSessionHandler.ChangePlayerFighters(helper.GetLocalPlayerIndex(), fightersPickedReferences);
            _ = helper.screenManager.TryBackPageAsync();
            
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
            helper.roomSessionHandler.AttemptToggleReadyState(0);
        }

        public async void BUTTON_JoinRoom()
        {
            buttonJoinRoom.interactable = false;
            
            var joinResult = await helper.roomSessionHandler.TryJoinRoom(roomId);
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
            if (helper.roomSessionHandler.localClientPlayerIds.Length != helper.roomSessionHandler.LocalPlayerCount)
            {
                return false;
            }
            return room.GetRoomMasterPlayerId() == helper.roomSessionHandler.localClientPlayerIds[helper.GetLocalPlayerIndex()];
        }

        public bool IsInRoom()
        {
            if (helper.roomSessionHandler.localClientPlayerIds.Length != helper.roomSessionHandler.LocalPlayerCount)
            {
                return false;
            }
            return room.players.Contains(helper.roomSessionHandler.localClientPlayerIds[helper.GetLocalPlayerIndex()]);
        }

        public void AssignRoom(int roomId)
        {
            if (room != null)
            {
                room.onClosed.RemoveListener(WhenRoomClosed);
                room.onUpdated.RemoveListener(WhenRoomUpdated);
            }
            
            this.roomId = roomId;
            room = helper.lobbyRepresentation.GetRoom(roomId);
            if (room == null)
            {
                _ = helper.screenManager.TryBackPageAsync();
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
            _ = helper.screenManager.TryBackPageAsync();
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
            foreach (Transform child in scrollRectPlayerList.content)
            {
                if (child.gameObject == playerListItemPrefab.gameObject) continue;
                Destroy(child.gameObject);
            }

            foreach (var playerId in room.players)
            {
                var playerInfo = helper.lobbyRepresentation.GetPlayer(playerId);
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


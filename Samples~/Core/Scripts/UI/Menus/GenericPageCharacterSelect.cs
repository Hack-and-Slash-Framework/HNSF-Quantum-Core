using System;
using System.Collections.Generic;
using CT.MenuNav;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace HnSF.ui.menus
{
    public class GenericPageCharacterSelect : MenuPage, IMenuInputOnPressedConfirm, IMenuInputOnPressedBack, IMenuInputOnPressedStart, IMenuInputOnNavigateRaw
    {
        public enum CssStates
        {
            CharacterSelect,
            PreparedToFight
        }

        public enum PlayerCssStates
        {
            CharacterSelect,
            CustomCharacterSelect
        }

        public UnityEvent OnConfirmCharacters = new UnityEvent();
        public UnityEvent OnCancel = new UnityEvent();

        public CharacterSelectScreenSelectable[] allCharacterUiItems = Array.Empty<CharacterSelectScreenSelectable>();

        public int playerCount;
        public List<List<CharacterSelectScreenSelectable>> playerSelections = new();
        public List<List<ModAssetSoftReference>> playerSelectionsFighterReferences = new();
        public NavigationDirections[] playerLastNavigation;
        public PlayerCssStates[] playerCssStates;

        public Canvas canvas;
        public GameObject preparedToFightUiItem;

        public CharacterSelectScreenSelectable[] defaultCharacterSelections =
            new CharacterSelectScreenSelectable[4];

        public bool selectionWrapAround;

        public int fighterSelectionCount = 1;

        public CssStates currentCssState;

        public CSSCustomCharacterSelectWidget[] customCharacterSelectWidgets = new CSSCustomCharacterSelectWidget[4];

        public override UniTask<bool> TryOpenAsync(MenuNavDirection direction, int pageCount)
        {
            allCharacterUiItems = gameObject.GetComponentsInChildren<CharacterSelectScreenSelectable>();
            foreach (var widget in customCharacterSelectWidgets) widget.gameObject.SetActive(false);
            foreach (var cuiItem in allCharacterUiItems) cuiItem.UpdateUi();
            return base.TryOpenAsync(direction, pageCount);
        }

        public override UniTask<bool> TryCloseAsync(MenuNavDirection direction)
        {
            Teardown();
            return base.TryCloseAsync(direction);
        }
        
        public async UniTask<bool> Initialize(int playerAmount, int wantedFighters = 1)
        {
            foreach (var acuitem in allCharacterUiItems)
            {
                if (!(await acuitem.PreloadAssets())) return false;
            }
            
            this.playerCount = playerAmount;
            playerSelections = new List<List<CharacterSelectScreenSelectable>>();
            playerLastNavigation = new NavigationDirections[playerCount];
            fighterSelectionCount = wantedFighters;
            playerCssStates = new PlayerCssStates[playerCount];
            playerSelectionsFighterReferences.Clear();

            currentCssState = CssStates.CharacterSelect;

            for (int i = 0; i < playerCount; i++)
            {
                playerSelectionsFighterReferences.Add(new List<ModAssetSoftReference>());
                playerSelections.Add(new List<CharacterSelectScreenSelectable>());
                playerSelections[i].Add(null);
                UpdateCurrentSelection(i, defaultCharacterSelections[i]);
            }
            
            currentManager.SetCurrentSelectedGameobject(null);
            return true;
        }

        public void Teardown()
        {
            foreach (var acuitem in allCharacterUiItems)
            {
                acuitem.UnloadAssets();
            }
            
            foreach (var cssWidget in customCharacterSelectWidgets) cssWidget.Close();
        }

        public List<List<ModAssetSoftReference>> GetCharactersPicked()
        {
            var lists = new List<List<ModAssetSoftReference>>();

            for (int i = 0; i < playerCount; i++)
            {
                List<ModAssetSoftReference> characterReferences = new List<ModAssetSoftReference>();

                for (int w = 0; w < playerSelectionsFighterReferences[i].Count; w++)
                {
                    characterReferences.Add(playerSelectionsFighterReferences[i][w]);
                }

                lists.Add(characterReferences);
            }

            return lists;
        }
        
        public void SelectCharacter(int playerIndex, CharacterSelectScreenSelectable selectable,
            ModAssetSoftReference selectedCharacter)
        {
            playerSelectionsFighterReferences[playerIndex].Add(selectedCharacter);
            //playerSelections[playerIndex][^1].ClearSelectionFlag(playerIndex);
            playerSelections[playerIndex][^1].SetSelectedFlag(playerIndex);
            playerSelections[playerIndex].Add(playerSelections[playerIndex][^1]);
            UpdateCurrentSelection(playerIndex, playerSelections[playerIndex][^1]);

            CheckIfCssShouldTransitionToReady();
        }

        private void CheckIfCssShouldTransitionToReady()
        {
            if (currentCssState == CssStates.PreparedToFight) return;

            bool valid = true;
            for (int i = 0; i < playerSelections.Count; i++)
            {
                if (playerSelections[i].Count == fighterSelectionCount + 1) continue;
                valid = false;
                break;
            }

            if (valid == false) return;
            UpdateCssState(CssStates.PreparedToFight);
        }
        
        private void UpdateCssState(CssStates nextState)
        {
            switch (nextState)
            {
                case CssStates.CharacterSelect:
                    preparedToFightUiItem.SetActive(false);
                    break;
                case CssStates.PreparedToFight:
                    preparedToFightUiItem.SetActive(true);
                    break;
            }

            currentCssState = nextState;
        }

        protected virtual void UpdateCurrentSelection(int playerIndex,
            CharacterSelectScreenSelectable destinationSelection)
        {
            if (destinationSelection == null || destinationSelection == playerSelections[playerIndex][^1]) return;
            if (playerSelections[playerIndex][^1] != null)
                playerSelections[playerIndex][^1].ClearSelectionFlag(playerIndex);
            playerSelections[playerIndex][^1] = destinationSelection;
            playerSelections[playerIndex][^1].SetSelectionFlag(playerIndex);
        }

        public void TransitionToCustomCharacterSelect(int playerIndex)
        {
            if (playerCssStates[playerIndex] == PlayerCssStates.CustomCharacterSelect) return;
            playerCssStates[playerIndex] = PlayerCssStates.CustomCharacterSelect;

            customCharacterSelectWidgets[playerIndex].Open();
            customCharacterSelectWidgets[playerIndex].OnCancel.AddListener(WhenCustomCharacterSelectCancel);
            customCharacterSelectWidgets[playerIndex].OnSubmit.AddListener(WhenCustomCharacterSelectSubmit);
        }

        private void WhenCustomCharacterSelectSubmit(CSSCustomCharacterSelectWidget arg0)
        {
            var playerIndex = Array.IndexOf(customCharacterSelectWidgets, arg0);
            customCharacterSelectWidgets[playerIndex].OnCancel.RemoveListener(WhenCustomCharacterSelectCancel);
            customCharacterSelectWidgets[playerIndex].OnSubmit.RemoveListener(WhenCustomCharacterSelectSubmit);

            var chara = customCharacterSelectWidgets[playerIndex].GetSelectedAssetReference();
            customCharacterSelectWidgets[playerIndex].Close();
            playerCssStates[playerIndex] = PlayerCssStates.CharacterSelect;

            SelectCharacter(playerIndex, null, chara);
        }

        private void WhenCustomCharacterSelectCancel(CSSCustomCharacterSelectWidget arg0)
        {
            var playerIndex = Array.IndexOf(customCharacterSelectWidgets, arg0);
            customCharacterSelectWidgets[playerIndex].OnCancel.RemoveListener(WhenCustomCharacterSelectCancel);
            customCharacterSelectWidgets[playerIndex].OnSubmit.RemoveListener(WhenCustomCharacterSelectSubmit);

            customCharacterSelectWidgets[playerIndex].Close();
            playerCssStates[playerIndex] = PlayerCssStates.CharacterSelect;
        }

        public virtual void OnInputConfirmPressed(int playerID, BaseEventData eventData)
        {
            var index = playerID - 1;
            if (index < 0 || index >= playerCount) return;
            
            if (playerSelections[index].Count == fighterSelectionCount + 1)
            {
                return;
            }

            switch (playerCssStates[index])
            {
                case PlayerCssStates.CharacterSelect:
                    if (playerSelections[index][^1] is CharacterSelectScreenSelectableCustom)
                        TransitionToCustomCharacterSelect(index);
                    else
                        playerSelections[index][^1].Submit(index);
                    break;
                case PlayerCssStates.CustomCharacterSelect:
                    customCharacterSelectWidgets[index].OnInputConfirmPressed(playerID, eventData);
                    break;
            }
        }

        public virtual void OnInputBackPressed(int playerID, BaseEventData eventData)
        {
            var index = playerID - 1;
            if (index < 0 || index >= playerCount) return;
            
            switch (playerCssStates[index])
            {
                case PlayerCssStates.CharacterSelect:
                    if (playerSelections[index].Count <= 1)
                    {
                        OnCancel.Invoke();
                        return;
                    }

                    //playerSelections[index][^2].ClearAllFlags(index);
                    playerSelections[index][^2].ClearSelectedFlag(index);
                    playerSelections[index].RemoveAt(playerSelections[index].Count - 2);
                    playerSelectionsFighterReferences[index]
                        .RemoveAt(playerSelectionsFighterReferences[index].Count - 1);
                    UpdateCurrentSelection(index, playerSelections[index][^1]);

                    UpdateCssState(CssStates.CharacterSelect);
                    break;
                case PlayerCssStates.CustomCharacterSelect:
                    customCharacterSelectWidgets[index].OnInputBackPressed(playerID, eventData);
                    break;
            }
        }

        public virtual void OnInputStartPressed(int playerID, BaseEventData eventData)
        {
            var index = playerID - 1;
            if (index < 0 || index >= playerCount) return;
            
            if (currentCssState == CssStates.PreparedToFight)
            {
                Debug.Log("Calling Characters Confirmed.");
                OnConfirmCharacters?.Invoke();
            }
            else
            {
                switch (playerCssStates[index])
                {
                    case PlayerCssStates.CharacterSelect:
                        break;
                    case PlayerCssStates.CustomCharacterSelect:
                        break;
                }
            }
        }

        public virtual void OnNavigateRaw(Vector2 navInput, int playerID, BaseEventData eventData)
        {
            var playerIndex = playerID - 1;
            if (currentCssState != CssStates.CharacterSelect || playerIndex < 0 || playerIndex >= playerCount) return;
            
            switch (playerCssStates[playerIndex])
            {
                case PlayerCssStates.CharacterSelect:
                    var navDir = UIHelpers.ConvertNavigationToDirection(navInput);
                    if (navDir == playerLastNavigation[playerIndex])
                    {
                        playerLastNavigation[playerIndex] = navDir;
                        return;
                    }

                    playerLastNavigation[playerIndex] = navDir;

                    switch (navDir)
                    {
                        case NavigationDirections.Up:
                            UpdateCurrentSelection(playerIndex,
                                GeneralHelpers.FindNextSelectableFromDirection(playerSelections[playerIndex][^1],
                                    new Vector3(0, 1, 0), allCharacterUiItems, true));
                            break;
                        case NavigationDirections.Down:
                            UpdateCurrentSelection(playerIndex,
                                GeneralHelpers.FindNextSelectableFromDirection(playerSelections[playerIndex][^1],
                                    new Vector3(0, -1, 0), allCharacterUiItems, true));
                            break;
                        case NavigationDirections.Left:
                            UpdateCurrentSelection(playerIndex,
                                GeneralHelpers.FindNextSelectableFromDirection(playerSelections[playerIndex][^1],
                                    new Vector3(-1, 0, 0), allCharacterUiItems, true));
                            break;
                        case NavigationDirections.Right:
                            UpdateCurrentSelection(playerIndex,
                                GeneralHelpers.FindNextSelectableFromDirection(playerSelections[playerIndex][^1],
                                    new Vector3(1, 0, 0), allCharacterUiItems, true));
                            break;
                    }

                    break;
                case PlayerCssStates.CustomCharacterSelect:
                    customCharacterSelectWidgets[playerIndex].OnInputNavigateRaw(navInput, playerID, eventData);
                    break;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HnSF.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace HnSF.ui.menus
{
    public class GenericScreenCharacterSelect : MenuBase
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


        public List<InputPlayerManager> inputPlayers = new();
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

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            allCharacterUiItems = gameObject.GetComponentsInChildren<CharacterSelectScreenSelectable>();

            foreach (var widget in customCharacterSelectWidgets) widget.gameObject.SetActive(false);

            foreach (var cuiItem in allCharacterUiItems) cuiItem.UpdateUi();
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            Teardown();
            gameObject.SetActive(false);
            return base.TryClose(direction, forceClose);
        }


        private List<Action<InputAction.CallbackContext>> navActions = new();
        private List<Action<InputAction.CallbackContext>> submitActions = new();
        private List<Action<InputAction.CallbackContext>> cancelActions = new();

        public async UniTask<bool> Initialize(List<InputPlayerManager> players, int wantedFighters = 1)
        {
            foreach (var acuitem in allCharacterUiItems)
            {
                if (!(await acuitem.PreloadAssets())) return false;
            }

            inputPlayers = players;
            playerSelections = new List<List<CharacterSelectScreenSelectable>>();
            playerLastNavigation = new NavigationDirections[players.Count];
            fighterSelectionCount = wantedFighters;
            playerCssStates = new PlayerCssStates[players.Count];
            playerSelectionsFighterReferences.Clear();

            currentCssState = CssStates.CharacterSelect;

            for (int i = 0; i < players.Count; i++)
            {
                playerSelectionsFighterReferences.Add(new List<ModAssetSoftReference>());
                playerSelections.Add(new List<CharacterSelectScreenSelectable>());
                playerSelections[i].Add(null);
                int index = i;
                navActions.Add((context) => { WhenNavigationPerformed(index, context); });
                submitActions.Add((context) => { WhenSubmitPerformed(index, context); });
                cancelActions.Add((context) => { WhenCancelPerformed(index, context); });

                players[i].inputActions.UI.Navigate.performed += navActions[i];
                players[i].inputActions.UI.Submit.performed += submitActions[i];
                players[i].inputActions.UI.Cancel.performed += cancelActions[i];
                players[i].inputActions.UI.Pause.performed += WhenStartPerformed;

                players[i].mpEventSystem.SetSelectedGameObject(null);

                UpdateCurrentSelection(i, defaultCharacterSelections[i]);
            }

            return false;
        }

        public void Teardown()
        {
            foreach (var acuitem in allCharacterUiItems)
            {
                acuitem.UnloadAssets();
            }

            for (int i = 0; i < inputPlayers.Count; i++)
            {
                inputPlayers[i].inputActions.UI.Navigate.performed -= navActions[i];
                inputPlayers[i].inputActions.UI.Submit.performed -= submitActions[i];
                inputPlayers[i].inputActions.UI.Cancel.performed -= cancelActions[i];
                inputPlayers[i].inputActions.UI.Pause.performed -= WhenStartPerformed;
            }

            foreach (var cssWidget in customCharacterSelectWidgets) cssWidget.Close();
        }

        public List<List<ModAssetSoftReference>> GetCharactersPicked()
        {
            var lists = new List<List<ModAssetSoftReference>>();

            for (int i = 0; i < inputPlayers.Count; i++)
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

        private void WhenStartPerformed(InputAction.CallbackContext obj)
        {
            if (currentCssState != CssStates.PreparedToFight) return;
            OnConfirmCharacters.Invoke();
        }

        private void WhenNavigationPerformed(int playerIndex, InputAction.CallbackContext context)
        {
            if (currentCssState != CssStates.CharacterSelect) return;

            switch (playerCssStates[playerIndex])
            {
                case PlayerCssStates.CharacterSelect:
                    var navDir = UIHelpers.ConvertNavigationToDirection(context.ReadValue<Vector2>());
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
                    break;
            }
        }

        private void WhenSubmitPerformed(int index, InputAction.CallbackContext context)
        {
            if (playerSelections[index].Count == fighterSelectionCount + 1)
            {
                return;
            }

            switch (playerCssStates[index])
            {
                case PlayerCssStates.CharacterSelect:
                    //playerSelections[index][^1].Submit(index, this);
                    break;
                case PlayerCssStates.CustomCharacterSelect:
                    break;
            }
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

        private void WhenCancelPerformed(int index, InputAction.CallbackContext context)
        {
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
                    break;
            }
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

            customCharacterSelectWidgets[playerIndex].Open(inputPlayers[playerIndex]);
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
    }
}
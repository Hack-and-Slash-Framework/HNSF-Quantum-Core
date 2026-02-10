using System;
using System.Collections.Generic;
using HnSF.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace HnSF.ui.menus
{
    public class CSSCustomCharacterSelectWidget : MonoBehaviour
    {
        public UnityEvent<CSSCustomCharacterSelectWidget> OnCancel = new UnityEvent<CSSCustomCharacterSelectWidget>();
        public UnityEvent<CSSCustomCharacterSelectWidget> OnSubmit = new UnityEvent<CSSCustomCharacterSelectWidget>();
        
        private InputPlayerManager inputPlayer;
        public CSSCustomCharacterSelectWidgetViewItem viewItemPrefab;
        public Transform viewItemParent;
        private List<CSSCustomCharacterSelectWidgetViewItem> items = new();
        private int currentlySelectedItemIndex = 0;

        [NonSerialized] private GenericContentListingUtilityBase contentListingUtility;
        private List<LoadedAssetHandleWrapper> loadedAssets = new List<LoadedAssetHandleWrapper>();
        private NavigationDirections playerLastNavigation = NavigationDirections.None;
        
        public void Open(InputPlayerManager player)
        {
            inputPlayer = player;
            viewItemPrefab.gameObject.SetActive(false);
            gameObject.SetActive(true);
            
            contentListingUtility = new GenericContentListingUtility<IFighterDefinition>();
            contentListingUtility.onPreContentListChanged.AddListener(BeforeContentListChanged);
            contentListingUtility.onPostContentListChanged.AddListener(WhenContentListChanged);
            contentListingUtility.Initialize(amountPerPage: 8);
            contentListingUtility.UpdateMaxPageCount();

            inputPlayer.inputActions.UI.Navigate.performed += WhenNavigationPerformed;
            inputPlayer.inputActions.UI.Submit.performed += WhenSubmitPerformed;
            inputPlayer.inputActions.UI.Cancel.performed += WhenCancelPerformed;
        }

        public void Close()
        {
            if (inputPlayer == null) return;
            inputPlayer.inputActions.UI.Navigate.performed -= WhenNavigationPerformed;
            inputPlayer.inputActions.UI.Submit.performed -= WhenSubmitPerformed;
            inputPlayer.inputActions.UI.Cancel.performed -= WhenCancelPerformed;
            
            Uninitialize();
            gameObject.SetActive(false);
            inputPlayer = null;
        }
        
        public void Uninitialize()
        {
            var contentManager = HnSFManagersContainer.instance?.contentManager;
            if (contentManager == null) return;
            
            foreach (var loadedAssetHandle in loadedAssets)
            {
                contentManager.ReleaseAssetFromMod(loadedAssetHandle);
            }
            
            loadedAssets.Clear();
            
            contentListingUtility?.Uninitialize();
            contentListingUtility = null;
        }

        public ModAssetSoftReference GetSelectedAssetReference()
        {
            return loadedAssets[items[currentlySelectedItemIndex].fighterIndex].assetReference;
        }
        
        private void WhenNavigationPerformed(InputAction.CallbackContext context)
        {
            var navDir = UIHelpers.ConvertNavigationToDirection(context.ReadValue<Vector2>());
            if (navDir == playerLastNavigation)
            {
                playerLastNavigation = navDir;
                return;
            }
            playerLastNavigation = navDir;
            
            switch (navDir)
            {
                case NavigationDirections.Up:
                    UpdateSelection(currentlySelectedItemIndex-1);
                    break;
                case NavigationDirections.Down:
                    UpdateSelection(currentlySelectedItemIndex+1);
                    break;
                case NavigationDirections.Left:
                    PageLeft();
                    break;
                case NavigationDirections.Right:
                    PageRight();
                    break;
            }
        }
        
        private void WhenSubmitPerformed(InputAction.CallbackContext context)
        {
            OnSubmit.Invoke(this);
        }
        
        private void WhenCancelPerformed(InputAction.CallbackContext obj)
        {
            OnCancel.Invoke(this);
        }

        private void UpdateSelection(int requestedIndex)
        {
            requestedIndex = Mathf.Clamp(requestedIndex, 0, items.Count - 1);
            if (items.Count == 0) return;

            items[currentlySelectedItemIndex].OnDeselected();
            currentlySelectedItemIndex = requestedIndex;
            items[currentlySelectedItemIndex].OnSelected();
        }

        private void PageLeft()
        {
            
        }

        private void PageRight()
        {
            
        }
        
        private void BeforeContentListChanged(GenericContentListingUtilityBase arg0)
        {
            var contentManager = HnSFManagersContainer.instance.contentManager;
            foreach (var loadedAsset in loadedAssets)
            {
                contentManager.ReleaseAssetFromMod(loadedAsset);
            }
            loadedAssets.Clear();
        }

        private async void WhenContentListChanged(GenericContentListingUtilityBase arg0)
        {
            var contentManager = HnSFManagersContainer.instance.contentManager;
            foreach (var assetRef in contentListingUtility.currentAssetList)
            {
                var loadResult = await contentManager.LoadAssetFromModAsync(assetRef);
                if (loadResult.result == false) continue;
                loadedAssets.Add(loadResult.handle);
            }
            
            BuildUIContentList();
        }
        
        public void BuildUIContentList()
        {
            foreach (Transform child in viewItemParent)
            {
                if (child.gameObject == viewItemPrefab.gameObject) continue;
                Destroy(child.gameObject);
            }
            
            items.Clear();

            for (var i = 0; i < loadedAssets.Count; i++)
            {
                var assetAsContentDefinition = loadedAssets[i].GetAsset<IFighterDefinition>();
                if (assetAsContentDefinition != null && assetAsContentDefinition.Selectable == false) continue;
                
                var index = i;
                var uiContentItem = GameObject.Instantiate(viewItemPrefab, viewItemParent, false);
                uiContentItem.gameObject.SetActive(true);
                uiContentItem.fighterNameTextObject.text = assetAsContentDefinition.Name;
                uiContentItem.fighterIndex = index;
                uiContentItem.Initialize();
                items.Add(uiContentItem);
            }

            currentlySelectedItemIndex = 0;
            UpdateSelection(currentlySelectedItemIndex);
        }
    }
}
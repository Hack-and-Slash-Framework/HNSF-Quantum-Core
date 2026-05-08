using System;
using System.Collections.Generic;
using CT.MenuNav;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HnSF.ui
{
    public class GenericContentPickerInstance : MenuPage, IMenuInputOnPressedConfirm, IMenuInputOnPressedBack, IMenuInputOnNavigateRaw
    {
        public UnityEvent<GenericContentPickerInstance> onContentPicked = new UnityEvent<GenericContentPickerInstance>();
        public UnityEvent<GenericContentPickerInstance> onCancel = new UnityEvent<GenericContentPickerInstance>();
        
        public Canvas canvas;
        
        public ScrollRect contentScrollRect;

        public GenericContentPickerContentViewItem contentItemPrefab;
        
        [NonSerialized] public GenericContentListingUtilityBase contentListingUtility;
        
        public List<LoadedAssetHandleWrapper> loadedAssets = new List<LoadedAssetHandleWrapper>();

        public bool changeContentListLockout;

        public int currentSelectedContentIndex = -1;

        public Button buttonConfirmSelection;

        public TextMeshProUGUI titleText;
        public TextMeshProUGUI contentTitleText;
        public TextMeshProUGUI contentDescriptionText;

        public override UniTask<bool> TryOpenAsync(MenuNavDirection direction, int pageCount)
        {
            PageState = MenuPageState.Opening;
            buttonConfirmSelection.interactable = false;
            currentSelectedContentIndex = -1;
            gameObject.SetActive(true);
            PageState = MenuPageState.Opened;

            currentManager.SetCurrentSelectedGameobject(null);
            return new UniTask<bool>(true);
        }

        public override UniTask<bool> TryCloseAsync(MenuNavDirection direction)
        {
            if(direction == MenuNavDirection.Back) Uninitialize();
            return base.TryCloseAsync(direction);
        }
        
        public virtual void Initialize<T>() where T : IContentDefinition
        {
            contentListingUtility = new GenericContentListingUtility<T>();
            contentListingUtility.UpdateMaxPageCount();
            contentListingUtility.onPreContentListChanged.AddListener(BeforeContentListChanged);
            contentListingUtility.onPostContentListChanged.AddListener(WhenContentListChanged);
            contentListingUtility.Initialize(amountPerPage: 10);
        }

        public virtual void Uninitialize()
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

        protected virtual void OnDestroy()
        {
            Uninitialize();
        }
        
        public virtual ModAssetSoftReference ConfirmWantedContent()
        {
            if (currentSelectedContentIndex == -1) return default;
            return loadedAssets[currentSelectedContentIndex].assetReference;
        }
        
        public virtual LoadedAssetHandleWrapper ConfirmWantedContentAndRemoveFromList()
        {
            if (currentSelectedContentIndex == -1) return default;
            var selectedHandle = loadedAssets[currentSelectedContentIndex];
            loadedAssets.Remove(selectedHandle);
            return selectedHandle;
        }
        
        public virtual void ConfirmWantedContentAndRemoveFromList(LoadedAssetHandleWrapper[] wantedAssetHandles)
        {
            for (int i = 0; i < wantedAssetHandles.Length; i++)
            {
                loadedAssets.Remove(wantedAssetHandles[i]);
            }
        }

        protected virtual void BeforeContentListChanged(GenericContentListingUtilityBase arg0)
        {
            buttonConfirmSelection.interactable = false;
            var contentManager = HnSFManagersContainer.instance.contentManager;
            foreach (var loadedAsset in loadedAssets)
            {
                contentManager.ReleaseAssetFromMod(loadedAsset);
            }
            loadedAssets.Clear();
        }

        protected virtual async void WhenContentListChanged(GenericContentListingUtilityBase arg0)
        {
            changeContentListLockout = true;
            var contentManager = HnSFManagersContainer.instance.contentManager;
            foreach (var assetRef in contentListingUtility.currentAssetList)
            {
                var loadResult = await contentManager.LoadAssetFromModAsync(assetRef);
                if (loadResult.result == false) continue;
                loadedAssets.Add(loadResult.handle);
            }
            changeContentListLockout = false;
            
            BuildUIContentList();
        }

        public virtual void BuildUIContentList()
        {
            foreach (Transform child in contentScrollRect.content.transform)
            {
                Destroy(child.gameObject);
            }

            for (var i = 0; i < loadedAssets.Count; i++)
            {
                var assetAsContentDefinition = loadedAssets[i].GetAsset<IContentDefinition>();
                if (assetAsContentDefinition != null && assetAsContentDefinition.Selectable == false) continue;
                
                var index = i;
                var uiContentItem = GameObject.Instantiate(contentItemPrefab, contentScrollRect.content.transform, false);
                uiContentItem.assetNameText.text = $"{loadedAssets[i].assetReference.ToString()}";
                
                var itemAsContentDefinition = loadedAssets[i].GetAsset<IContentDefinition>();
                if (itemAsContentDefinition != null)
                {
                    uiContentItem.assetNameText.text = itemAsContentDefinition.Name;
                }
                
                uiContentItem.button.onClick.AddListener(() => { OnSelectContentItem(index); });

                if (i == 0 && uiContentItem != null)
                    currentManager.SetCurrentSelectedGameobject(uiContentItem.gameObject);
            }
        }

        protected virtual void OnSelectContentItem(int index)
        {
            currentSelectedContentIndex = index;
            buttonConfirmSelection.interactable = true;
        }

        public virtual void SetCameraTarget(Camera target)
        {
            canvas.worldCamera = target;
        }

        public virtual void Button_Cancel()
        {
            onCancel.Invoke(this);
        }
        
        public virtual void Button_ConfirmSelection()
        {
            onContentPicked.Invoke(this);
        }
        
        public void OnInputConfirmPressed(int playerID, BaseEventData eventData)
        {
            if (currentSelectedContentIndex == -1) return;
            onContentPicked.Invoke(this);
        }

        public void OnInputBackPressed(int playerID, BaseEventData eventData)
        {
            onCancel.Invoke(this);
        }

        public void OnNavigateRaw(Vector2 navInput, int playerID, BaseEventData eventData)
        {
            if (currentManager.GetCurrentSelectedGameObject() == null)
            {
                if (contentScrollRect.content.transform.childCount <= 0) return;
                var cTransform = contentScrollRect.content.transform.GetChild(0);
                if (cTransform == null) return;
                currentManager.SetCurrentSelectedGameobject(cTransform.gameObject);
            }
        }
    }
}

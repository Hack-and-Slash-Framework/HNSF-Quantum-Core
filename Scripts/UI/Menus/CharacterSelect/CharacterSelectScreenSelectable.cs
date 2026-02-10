using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace HnSF.ui.menus
{
    public class CharacterSelectScreenSelectable : MonoBehaviour
    {
        [System.Serializable]
        [Flags]
        public enum SelectionFlags
        {
            None = 0,
            P1 = 1,
            P2 = 2,
            P3 = 4,
            P4 = 8
        }
        
        public GameObject p1SelectFlag;
        public GameObject p2SelectFlag;
        public GameObject p3SelectFlag;
        public GameObject p4SelectFlag;

        public SelectionFlags currentSelectionFlags;
        public SelectionFlags selectionFlags;

        [SerializeField] protected UnityEvent<CharacterSelectScreenSelectable, int> m_OnInitialSelection = new UnityEvent<CharacterSelectScreenSelectable, int>();
        [SerializeField] protected UnityEvent<CharacterSelectScreenSelectable, int> m_OnRepeatSelection = new UnityEvent<CharacterSelectScreenSelectable, int>();
        [SerializeField] protected UnityEvent<CharacterSelectScreenSelectable, int> m_OnSubmit = new UnityEvent<CharacterSelectScreenSelectable, int>();
        [SerializeField] protected UnityEvent<CharacterSelectScreenSelectable, int> m_OnInvidualDeselection = new UnityEvent<CharacterSelectScreenSelectable, int>();
        [SerializeField] protected UnityEvent<CharacterSelectScreenSelectable, int> m_OnLastDeselection = new UnityEvent<CharacterSelectScreenSelectable, int>();
        [SerializeField] protected UnityEvent<CharacterSelectScreenSelectable, int> m_OnSelectedFlagAssigned = new UnityEvent<CharacterSelectScreenSelectable, int>();
        [SerializeField] protected UnityEvent<CharacterSelectScreenSelectable, int> m_OnSelectedFlagUnassigned = new UnityEvent<CharacterSelectScreenSelectable, int>();
        
        [NonSerialized] public UnityEvent<int, ModAssetSoftReference> GetCharacterOnSubmit = new UnityEvent<int, ModAssetSoftReference>();
        
        /*
        [Header("Selection")]
        public CharacterSelectScreenSelectable selectLeft;
        public CharacterSelectScreenSelectable selectRight;
        public CharacterSelectScreenSelectable selectUp;
        public CharacterSelectScreenSelectable selectDown;*/

        public virtual UniTask<bool> PreloadAssets()
        {
            return new UniTask<bool>(true);
        }

        public virtual void UnloadAssets()
        {
            
        }

        protected virtual void OnDestroy()
        {
            UnloadAssets();
        }

        public virtual void Submit(int playerIndex)
        {
            GetCharacterOnSubmit.Invoke(playerIndex, default);
        }
        
        public virtual void UpdateUi()
        {
            UpdateFlagVisual(p1SelectFlag, SelectionFlags.P1);
            UpdateFlagVisual(p2SelectFlag, SelectionFlags.P2);
            UpdateFlagVisual(p3SelectFlag, SelectionFlags.P3);
            UpdateFlagVisual(p4SelectFlag, SelectionFlags.P4);
        }
        
        protected virtual void UpdateFlagVisual(GameObject flagObject, SelectionFlags flag)
        {
            if (currentSelectionFlags.HasFlag(flag))
            {
                flagObject.SetActive(true);
                flagObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            }else if (selectionFlags.HasFlag(flag))
            {
                flagObject.SetActive(true);
                flagObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
            }
            else
            {
                flagObject.SetActive(false);
            }
        }

        public void SetSelectedFlag(int playerIndex, bool updateUi = true, bool invokeEvent = true)
        {
            switch (playerIndex)
            {
                case 0:
                    selectionFlags |= SelectionFlags.P1;
                    break;
                case 1:
                    selectionFlags |= SelectionFlags.P2;
                    break;
                case 2:
                    selectionFlags |= SelectionFlags.P3;
                    break;
                case 3:
                    selectionFlags |= SelectionFlags.P4;
                    break;
            }

            if(updateUi) UpdateUi();

            if (!invokeEvent) return;
            m_OnSelectedFlagAssigned.Invoke(this, playerIndex);
        }
        
        public void ClearSelectedFlag(int playerIndex, bool updateUi = true, bool invokeEvent = true)
        {
            switch (playerIndex)
            {
                case 0:
                    selectionFlags &= ~SelectionFlags.P1;
                    break;
                case 1:
                    selectionFlags &= ~SelectionFlags.P2;
                    break;
                case 2:
                    selectionFlags &= ~SelectionFlags.P3;
                    break;
                case 3:
                    selectionFlags &= ~SelectionFlags.P4;
                    break;
            }

            if (updateUi) UpdateUi();

            if (!invokeEvent) return;
            m_OnSelectedFlagUnassigned.Invoke(this, playerIndex);
        }
        
        public void SetSelectionFlag(int playerIndex, bool updateUi = true, bool invokeEvent = true)
        {
            var oldFlags = currentSelectionFlags;
            
            switch (playerIndex)
            {
                case 0:
                    currentSelectionFlags |= SelectionFlags.P1;
                    break;
                case 1:
                    currentSelectionFlags |= SelectionFlags.P2;
                    break;
                case 2:
                    currentSelectionFlags |= SelectionFlags.P3;
                    break;
                case 3:
                    currentSelectionFlags |= SelectionFlags.P4;
                    break;
            }
            
            if(updateUi) UpdateUi();

            if (!invokeEvent || oldFlags == currentSelectionFlags) return;
            if (oldFlags == SelectionFlags.None)
            {
                m_OnInitialSelection.Invoke(this, playerIndex);
            }
            else
            {
                m_OnRepeatSelection.Invoke(this, playerIndex);
            }
        }

        public void ClearSelectionFlag(int playerIndex, bool updateUi = true, bool invokeEvent = true)
        {
            var oldFlags = currentSelectionFlags;
            
            switch (playerIndex)
            {
                case 0:
                    currentSelectionFlags &= ~SelectionFlags.P1;
                    break;
                case 1:
                    currentSelectionFlags &= ~SelectionFlags.P2;
                    break;
                case 2:
                    currentSelectionFlags &= ~SelectionFlags.P3;
                    break;
                case 3:
                    currentSelectionFlags &= ~SelectionFlags.P4;
                    break;
            }

            if(updateUi) UpdateUi();

            if (!invokeEvent || oldFlags == currentSelectionFlags) return;
            if (oldFlags != SelectionFlags.None && currentSelectionFlags == SelectionFlags.None)
            {
                m_OnLastDeselection.Invoke(this, playerIndex);
            }
            else
            {
                m_OnInvidualDeselection.Invoke(this, playerIndex);
            }
        }

        public void ClearAllFlags(int playerIndex, bool updateUi = true, bool invokeEvent = true)
        {
            ClearSelectedFlag(playerIndex, updateUi: false, invokeEvent: invokeEvent);
            ClearSelectionFlag(playerIndex, updateUi: false, invokeEvent: invokeEvent);
            if(updateUi) UpdateUi();
        }
    }
}
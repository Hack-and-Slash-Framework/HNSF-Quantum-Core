using HnSF.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HnSF.ui
{
    public class ExButton : Button
    {
        [SerializeField] private ButtonClickedEvent m_OnSelected = new ButtonClickedEvent();
        [SerializeField] private ButtonClickedEvent m_OnDeselected = new ButtonClickedEvent();
        [SerializeField] private ButtonClickedEvent m_OnPointerEnter = new ButtonClickedEvent();
        [SerializeField] private ButtonClickedEvent m_OnPointerExit = new ButtonClickedEvent();

        public ButtonClickedEvent onSelected
        {
            get { return m_OnSelected; }
            set { m_OnSelected = value; }
        }

        public ButtonClickedEvent onDeselected
        {
            get { return m_OnDeselected; }
            set { m_OnDeselected = value; }
        }

        public ButtonClickedEvent onPointerEnter
        {
            get { return m_OnPointerEnter; }
            set { m_OnPointerEnter = value; }
        }

        public ButtonClickedEvent onPointerExit
        {
            get { return m_OnPointerExit; }
            set { m_OnPointerExit = value; }
        }
        
        public InputPlayerManagerBase validInputPlayer;

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            if (validInputPlayer && !validInputPlayer.EventDataIsMine(eventData)) return;
            base.OnSelect(eventData);
            if (IsInteractable() && IsActive()) m_OnSelected.Invoke();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            if (validInputPlayer && !validInputPlayer.EventDataIsMine(eventData)) return;
            base.OnDeselect(eventData);
            if (IsInteractable() && IsActive()) m_OnDeselected.Invoke();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (validInputPlayer && !validInputPlayer.EventDataIsMine(eventData)) return;
            base.OnPointerEnter(eventData);
            if (IsInteractable() && IsActive()) m_OnPointerEnter.Invoke();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (validInputPlayer && !validInputPlayer.EventDataIsMine(eventData)) return;
            base.OnPointerExit(eventData);
            if (IsInteractable() && IsActive()) m_OnPointerExit.Invoke();
        }
    }
}
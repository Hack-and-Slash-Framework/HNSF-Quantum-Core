using System;
using System.Collections.Generic;
using System.Linq;
using HnSF.Input;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HnSF
{
    public class DevicePickerPanelInstance : MonoBehaviour
    {
        [NonSerialized] public Color initialColor = Color.black;
        public Image backgroundImage;
        public TextMeshProUGUI playerText;
        public RectTransform controllerContainerRect;
    }
}
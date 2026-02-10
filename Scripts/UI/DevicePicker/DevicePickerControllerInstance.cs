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
    public class DevicePickerControllerInstance : MonoBehaviour
    {
        public TextMeshProUGUI controllerName;
        public Image controllerImage;
        public Image controllerLeft;
        public Image controllerRight;
    }
}
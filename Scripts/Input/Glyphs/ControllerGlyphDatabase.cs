using System;
using System.Collections.Generic;
using HnSF;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HnSF.Input
{
    public class ControllerGlyphDatabase : MonoBehaviour
    {
        [NonSerialized] public Dictionary<string, ControllerGlyphs> glyphMap = new();

        public InputManager inputManager;
        [SerializeField] private ControllerGlyphs[] glyphs;

        public void Init()
        {
            glyphMap.Clear();
            foreach (var glyphMapper in glyphs)
            {
                foreach (var controllerName in glyphMapper.matchingControllers)
                {
                    glyphMap.Add(controllerName, glyphMapper);
                }
            }
        }

        public Sprite[] GetGlyphsForAction(InputDevice device, InputAction inputAction)
        {
            if (device == null) return null;
            return GetGlyphsForAction(device.displayName, inputAction);
        }

        public Sprite[] GetGlyphsForAction(string deviceName, InputAction inputAction)
        {
            if (!glyphMap.ContainsKey(deviceName)) return null;
            var inputString = inputAction.GetBindingDisplayString();
            if (string.IsNullOrEmpty(inputString)) return null;
            var inputs = inputString.Split(" | ");
            if (inputs.Length == 0) return null;
            Sprite[] sprites = new Sprite[inputs.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i] = glyphMap[deviceName].GetGlyphForBinding(inputs[i]);
            }

            return sprites;
        }

        public RectTransform[] BuildRectTransformsForAction(InputDevice device, InputAction inputAction)
        {
            if (device == null)
            {
                Debug.LogError("Device is null.");
                return null;
            }

            var rectTransforms = BuildRectTransformsForAction(device.displayName, inputAction);

            if (rectTransforms == null)
            {
                Debug.LogWarning(
                    $"Device \"{device.displayName}\" is not registered to have glyphs shown. Click for device info.\n" +
                    $"{device.description.ToString()}");
            }

            return rectTransforms;
        }

        public RectTransform[] BuildRectTransformsForAction(string deviceName, InputAction inputAction)
        {
            if (!glyphMap.ContainsKey(deviceName))
            {
                Debug.LogWarning($"Found no glyph mapping for {deviceName}.");
                return null;
            }

            var inputString = inputAction.GetBindingDisplayString();
            if (string.IsNullOrEmpty(inputString))
            {
                Debug.LogWarning($"Found no binding display string for {inputAction}.");
                return null;
            }

            var inputs = inputString.Split(" | ");
            if (inputs.Length == 0)
            {
                Debug.LogWarning($"Input string is invalid. {inputString}, {inputs.Length}");
                return null;
            }

            RectTransform[] rectTransforms = new RectTransform[inputs.Length];
            for (int i = 0; i < rectTransforms.Length; i++)
            {
                rectTransforms[i] = BuildRectTransformForAction(deviceName, inputs[i]);
            }

            return rectTransforms;
        }

        private RectTransform BuildRectTransformForAction(string deviceName, string bindingDisplayString)
        {
            if (!glyphMap.ContainsKey(deviceName) || string.IsNullOrEmpty(bindingDisplayString)) return null;
            var glyphEntry = glyphMap[deviceName].GetGlyphEntryForBinding(bindingDisplayString);
            if (glyphEntry == null) return null;

            GameObject rt = new GameObject("aa", typeof(RectTransform), typeof(Image));
            rt.GetComponent<Image>().sprite = glyphEntry.glyph;

            /*
            var bgSprite = glyphMap[deviceName].GetBackgroundSpriteForID(glyphEntry.glyphBackgroundID);

            if (bgSprite != null)
            {
                GameObject bgObject = new GameObject("bg", typeof(RectTransform), typeof(Image));
                bgObject.transform.SetParent(rt.transform, false);
                bgObject.GetComponent<RectTransform>().SetAndStretchToParentSize(rt.GetComponent<RectTransform>());
                bgObject.GetComponent<Image>().sprite = bgSprite;
            }*/

            GameObject buttonObject = new GameObject("button", typeof(RectTransform), typeof(Image));
            buttonObject.transform.SetParent(rt.transform, false);
            buttonObject.GetComponent<RectTransform>().SetAndStretchToParentSize(rt.GetComponent<RectTransform>());
            buttonObject.GetComponent<Image>().sprite = glyphEntry.glyph;

            return rt.GetComponent<RectTransform>();
        }
    }
}
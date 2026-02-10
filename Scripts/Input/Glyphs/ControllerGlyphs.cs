using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace HnSF.Input
{
    [CreateAssetMenu(fileName = "Controller Glyphs")]
    public class ControllerGlyphs : ScriptableObject
    {
        public static readonly string UnknownBindingName = "Unknown";

        public string[] matchingControllers;
        public string tmpSpritesheetName;
        public SerializedDictionary<string, GlyphEntry> glyphMapping = new();

        public SerializedDictionary<int, Sprite> glyphBackgrounds = new();

        public Sprite GetGlyphForBinding(string bindingString)
        {
            if (!glyphMapping.ContainsKey(bindingString)) return null;
            return glyphMapping[bindingString].glyph;
        }

        public GlyphEntry GetGlyphEntryForBinding(string bindingString)
        {
            if (!glyphMapping.ContainsKey(bindingString)) return null;
            return glyphMapping[bindingString];
        }

        public Sprite GetBackgroundSpriteForID(int id)
        {
            if (!glyphBackgrounds.ContainsKey(id)) return null;
            return glyphBackgrounds[id];
        }

        public string GetGlyphTextSpriteName(string bindingString)
        {
            if (!glyphMapping.ContainsKey(bindingString)) return UnknownBindingName;
            if (string.IsNullOrEmpty(glyphMapping[bindingString].textSpriteName))
                return glyphMapping[bindingString].glyph.name;
            return glyphMapping[bindingString].textSpriteName;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace HnSF
{
    public class HnSFConfigurationAsset : ScriptableObject
    {
        [System.Serializable]
        public class ModLocationDefinition
        {
            public BaseModInfoAsset modInfoAsset;
        }

        public BaseModInfoAsset localMod;
        public List<ModLocationDefinition> modLocations = new List<ModLocationDefinition>();

        public string fighterTemplatesLocation = "Assets/HnSFUser/ContentTemplates/Fighters";
    }
}
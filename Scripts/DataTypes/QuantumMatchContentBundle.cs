using System;
using System.Collections.Generic;

namespace HnSF
{
    [Serializable]
    public partial class QuantumMatchContentBundle
    {
        public ModAssetSoftReference gamemodeReference;
        public string gamemodeSettings;
        public ModAssetSoftReference mapReference;
        public ModAssetSoftReference musicReference;
        public int clientCount;
        public int playerCount;
        public List<PlayerMatchContentBundle> localPlayerBundles;
    }
}
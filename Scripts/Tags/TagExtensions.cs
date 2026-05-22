using System.Collections.Generic;
using Quantum;

namespace HnSF
{
    public static class TagExtensions
    {
        public static string GetFullTagString(this Quantum.Tag self)
        {
            List<string> parentStrings = new List<string>();
            AssetRef<Tag> nextParentTag = self.parent;
            while (nextParentTag != default)
            {
                if (!QuantumUnityDB.TryGetGlobalAssetEditorInstance(nextParentTag, out var nextParent)) break;
                parentStrings.Add(nextParent.label);
                nextParentTag = nextParent.parent;
            }
            parentStrings.Reverse();
            parentStrings.Add(self.label);
            return string.Join(".", parentStrings);
        }
    }
}

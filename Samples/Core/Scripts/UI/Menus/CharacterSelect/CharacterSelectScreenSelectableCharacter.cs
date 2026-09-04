using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HnSF.ui.menus
{
    public class CharacterSelectScreenSelectableCharacter : CharacterSelectScreenSelectable
    {
        [Header("Info")] 
        public ExternalModAssetSoftReference characterReference;
        public LoadedAssetHandleWrapper characterAssetHandle;
        
        public override async UniTask<bool> PreloadAssets()
        {
            var hnsfManagers = HnSFManagersContainer.instance;

            if (characterAssetHandle is {IsValid: true})
            {
                return true;
            }
            var loadResult = await hnsfManagers.contentManager.LoadAssetFromModAsync(characterReference.reference);
            if (loadResult == null) return false;
            characterAssetHandle = loadResult;
            return true;
        }

        public override void UnloadAssets()
        {
            characterAssetHandle.Release();
            characterAssetHandle = null;
        }

        public override void Submit(int playerIndex)
        {
            base.Submit(playerIndex);
            GetCharacterOnSubmit.Invoke(playerIndex, characterReference.reference);
        }
    }
}
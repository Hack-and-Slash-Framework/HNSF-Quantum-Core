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

            if (characterAssetHandle.IsValid())
            {
                return true;
            }
            var loadResult = await hnsfManagers.contentManager.LoadAssetFromModAsync(characterReference.reference);
            if (loadResult.result == false) return false;
            characterAssetHandle = loadResult.handle;
            return true;
        }

        public override void UnloadAssets()
        {
            characterAssetHandle.Teardown(releaseAsset: true);
        }

        public override void Submit(int playerIndex)
        {
            base.Submit(playerIndex);
            GetCharacterOnSubmit.Invoke(playerIndex, characterReference.reference);
        }
    }
}
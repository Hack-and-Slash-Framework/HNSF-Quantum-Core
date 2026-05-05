using HnSF.core.systems;
using Quantum;

namespace HnSF.core.statepreview
{
    public unsafe class StatePreviewSystem : SystemMainThread
    {
        EntityRef previewActorRef;
        EntityRef helperActorRef;

        //public int delay = 0;
        
        public override void OnEnabled(Frame f)
        {
            base.OnEnabled(f);

            var testingConfig = f.FindAsset<StatePreviewQuantumSettingsBase>(f.RuntimeConfig.gamemodeConfigAsset);
            GenericGamemodeStateSystem.SetState(f, GenericGamemodeStates.Game);
        }

        public override void Update(Frame f)
        {
            
        }
    }
}

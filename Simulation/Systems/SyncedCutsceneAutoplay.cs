using Quantum;

namespace HnSF.core.systems
{
    public unsafe class SyncedCutsceneAutoplay : SystemMainThreadFilter<SyncedCutsceneAutoplay.Filter>, ISignalOnComponentAdded<SyncedCutsceneSource>, ISignalOnComponentRemoved<SyncedCutsceneSource>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public SyncedCutsceneSource* syncedCutsceneSource;
        }
        
        public override void Update(Frame f, ref Filter filter)
        {
            if (filter.syncedCutsceneSource->autoEnd
                && filter.syncedCutsceneSource->frame >= filter.syncedCutsceneSource->endFrame)
            {
                f.Destroy(filter.Entity);
                return;
            }
            
            if (!filter.syncedCutsceneSource->autoPlay) return;

            if (f.Exists(filter.syncedCutsceneSource->sourcePlayer))
            {
                var hasLdt = f.Unsafe.TryGetPointer<LocalDeltaTime>(filter.syncedCutsceneSource->sourcePlayer, out var actorLocalDeltaTime);
                var hasHitstop = f.Unsafe.TryGetPointer<Hitstop>(filter.syncedCutsceneSource->sourcePlayer, out var actorHitstop);
                
                if (hasHitstop && actorHitstop->value > 0) return;

                if (hasLdt && filter.syncedCutsceneSource->ignorePlayerLdt == false)
                {
                    for (int i = 0; i < actorLocalDeltaTime->updatesThisTick; i++)
                    {
                        filter.syncedCutsceneSource->frame++;
                    }
                }
                else
                {
                    filter.syncedCutsceneSource->frame++;
                }
            }
            else
            {
                filter.syncedCutsceneSource->frame++;
            }
        } 

        public void OnAdded(Frame f, EntityRef entity, SyncedCutsceneSource* component)
        {
            component->cutsceneControls = f.AllocateDictionary<AssetRef<Tag>, CutsceneEntityControlDefinition>();
        }

        public void OnRemoved(Frame f, EntityRef entity, SyncedCutsceneSource* component)
        {
            f.FreeDictionary(component->cutsceneControls);
            component->cutsceneControls = default;
        }
    }
}
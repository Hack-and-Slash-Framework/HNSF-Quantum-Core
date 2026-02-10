using Quantum;

namespace HnSF.core.systems
{
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class ActorAnimatorAutoplay : SystemMainThreadFilter<ActorAnimatorAutoplay.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public BattleActorAnimator* ActorAnimator;
        }
        
        public override void Update(Frame f, ref Filter filter)
        {
            if ((f.Unsafe.TryGetPointer<Hitstop>(filter.Entity, out var hitstop) && hitstop->value > 0)) return;

            if (f.Unsafe.TryGetPointer<LocalDeltaTime>(filter.Entity, out var ldt))
            {
                for (int i = 0; i < ldt->updatesThisTick; i++)
                {
                    for (var w = 0; w < filter.ActorAnimator->state.layers.Length; w++)
                    {
                        if(!filter.ActorAnimator->state.layers[w].autoPlay) continue;
                        filter.ActorAnimator->state.layers[w].frame += filter.ActorAnimator->state.layers[w].autoPlayAdvanceAmount > 0 ? filter.ActorAnimator->state.layers[w].autoPlayAdvanceAmount : 1;
                    }
                }
            }
            else
            {
                for (var w = 0; w < filter.ActorAnimator->state.layers.Length; w++)
                {
                    if(!filter.ActorAnimator->state.layers[w].autoPlay) continue;
                    filter.ActorAnimator->state.layers[w].frame += filter.ActorAnimator->state.layers[w].autoPlayAdvanceAmount > 0 ? filter.ActorAnimator->state.layers[w].autoPlayAdvanceAmount : 1;
                }
            }
        }
    }
}
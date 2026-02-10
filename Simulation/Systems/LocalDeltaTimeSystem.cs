using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.systems
{
    public unsafe class LocalDeltaTimeSystem : SystemMainThread, ISignalOnComponentAdded<LocalDeltaTime>, ISignalOnComponentRemoved<LocalDeltaTime>
    {
        private List<EntityRef> exclusiveActorsThisFrame = new();
        
        public override void Update(Frame f)
        {
            var exclusiveDTActorFilter = f.Filter<ExclusiveDeltaTimeActor, LocalDeltaTime>();
            bool exclusiveActorExist = false;
            while (exclusiveDTActorFilter.NextUnsafe(out var entityRef, out var exclusiveDTActor, out var localDT))
            {
                exclusiveActorExist = true;
                ApplyLdt(f, localDT);
                exclusiveActorsThisFrame.Add(entityRef);
            }
            
            var ldtFilter = f.Filter<LocalDeltaTime>();
            while (ldtFilter.NextUnsafe(out var ldtEntityRef, out var localDeltaTime))
            {
                if (exclusiveActorExist)
                {
                    if (exclusiveActorsThisFrame.Contains(ldtEntityRef)) continue;
                    localDeltaTime->deltaTime = 0;
                    localDeltaTime->updatesThisTick = 0;
                }
                else
                {
                    ApplyLdt(f, localDeltaTime);
                }
            }
            
            exclusiveActorsThisFrame.Clear();
        }

        private static void ApplyLdt(Frame f, LocalDeltaTime* localDeltaTime)
        {
            if (localDeltaTime->updatesThisTick > 0)
            {
                localDeltaTime->deltaTime -= (f.DeltaTime * localDeltaTime->updatesThisTick);
                localDeltaTime->updatesThisTick = 0;
            }

            localDeltaTime->deltaTime += f.DeltaTime * localDeltaTime->multiplier;
            
            if (localDeltaTime->deltaTime >= f.DeltaTime)
            {
                localDeltaTime->updatesThisTick = FPMath.FloorToInt(localDeltaTime->deltaTime / f.DeltaTime);
            }
        }

        public void OnAdded(Frame f, EntityRef entity, LocalDeltaTime* component)
        {
            component->multiplier = 1;
        }

        public void OnRemoved(Frame f, EntityRef entity, LocalDeltaTime* component)
        {
        }
    }
}
/*
using Photon.Deterministic;

namespace Quantum
{
    using System;
    using System.Collections.Generic;

    partial class EventCauseScreenShake
    {
        public List<PlayerRef> playerFilter;
    }

    partial class Frame
    {
        partial struct FrameEvents
        {
            public EventCauseScreenShake CauseScreenShake(Int32 shakeType, QBoolean isGlobal, FPVector3 origin,
                FPVector3 shakeAmount, FPVector3 shakeSpeed, Int32 shakeFrames, Int32 shakeInterval,
                List<PlayerRef> playerFilterList)
            {
                var ev = CauseScreenShake(shakeType, isGlobal, origin, shakeAmount, shakeSpeed, shakeFrames,
                    shakeInterval);
                if (ev == null)
                {
                    // Synced or local events can be null for example during predicted frame.
                    return null;
                }

                // Reuse the list object of the pooled event.
                if (ev.playerFilter == null)
                {
                    ev.playerFilter = new List<PlayerRef>(playerFilterList.Count);
                }

                ev.playerFilter.Clear();

                // Copy the content into the event, to be independent from the input list object which can be cached.
                ev.playerFilter.AddRange(playerFilterList);
                return ev;
            }
        }
    }
}*/
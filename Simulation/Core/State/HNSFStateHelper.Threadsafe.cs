using System;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state
{
    public static unsafe partial class HNSFStateHelper
    {
        public static unsafe partial class ThreadSafe
        {
            public static void Update(FrameThreadSafe frame, FP deltaTime, HNSFStateAgentData* data, EntityRef entity, ref HNSFStateContext stateContext)
            {
                HNSFState currentState = frame.FindAsset<HNSFState>(data->state.Id);
                currentState.Execute(ref frame, deltaTime, data, entity, ref stateContext);
            }
        
            public static void Update(FrameThreadSafe frame, FP deltaTime, HNSFStateAgentData* data, EntityRef entity, HNSFState state, ref HNSFStateContext stateContext)
            {
                state.Execute(ref frame, deltaTime, data, entity, ref stateContext);
            }
        }
    }
}
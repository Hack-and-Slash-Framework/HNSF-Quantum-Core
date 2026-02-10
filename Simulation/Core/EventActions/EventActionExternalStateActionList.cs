using System;
using HnSF.core.state;
using HnSF.core.state.actions;
using HnSF.core.state.decisions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF
{
    public unsafe class EventActionExternalStateActionList : HNSFEventAction
    {
        public AssetRef<StateActionList> actionListRef;
        
        public override void Execute(Frame frame, EntityRef entity)
        {
            if (!frame.TryFindAsset(actionListRef, out var actionList)) return;
            actionList.Execute(frame, entity);
        }
    }
}
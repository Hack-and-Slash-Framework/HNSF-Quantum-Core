using System;
using System.Collections.Generic;
using HnSF.core.state.decisions;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state.actions
{
    [System.Serializable]
    public unsafe partial class HNSFStateAction
    {
        public int id;
        public string Label;
#if QUANTUM_UNITY
        [SerializeReference, HideInInspector]
#endif
        public HNSFStateAction parent;
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateAction[] children = Array.Empty<HNSFStateAction>();
        public ActionRange[] frameRanges = Array.Empty<ActionRange>();
        public bool actionDisabled;

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateDecision[] decision = Array.Empty<HNSFStateDecision>();

        public StateActionTargetContext actionTargetContext;

        public virtual void OnValidate()
        {
            
        }
        
        public virtual void Initialize()
        {
            
        }
        
        public virtual HNSFStateAction Copy()
        {
            return CopyTo(new HNSFStateAction());
        }

        public virtual HNSFStateAction CopyTo(HNSFStateAction target)
        {
            target.id = id;
            target.Label = Label;
            target.children = new HNSFStateAction[children.Length];
            target.frameRanges = new ActionRange[frameRanges.Length];
            Array.Copy(frameRanges, target.frameRanges, frameRanges.Length);
            target.actionDisabled = actionDisabled;
            target.decision = new HNSFStateDecision[decision.Length];
            target.actionTargetContext = actionTargetContext;
            
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i].Copy();
                child.parent = target;
                target.children[i] = child;
            }

            for (int i = 0; i < decision.Length; i++)
            {
                var d = decision[i].Copy();
                target.decision[i] = d;
            }
            return target;
        }
        
        public bool Execute(ref FrameThreadSafe frame, EntityRef entity, HNSFStateAgentData* data, ref HNSFStateContext stateContext)
        {
            bool exitEarly = Execute((Frame)frame, entity, data, ref stateContext);
            return exitEarly;
        }
        
        public bool Execute(Frame frame, EntityRef entity, HNSFStateAgentData* data, ref HNSFStateContext stateContext)
        {
            if (actionDisabled) return false;
            var state = frame.FindAsset<HNSFState>(stateContext.workingState.Id);

            FP percentage = 0;
            
            if (frameRanges.Length > 0 && !frameRanges.IsFrameWithinRanges(state.totalFrames, stateContext.stateFrame, out percentage)) return false;

            foreach (var deci in decision)
            {
                if (deci == null) continue;
                if (!deci.Decide(frame, entity, ref stateContext)) return false;
            }
            
            var exitEarly = ExecuteAction(frame, entity, percentage, ref stateContext);
            if (exitEarly) return true;

            foreach (var child in children)
            {
                child.Execute(frame, entity, data, ref stateContext);
            }

            return false;
        }
        
        public bool Execute(Frame frame, EntityRef entity, HNSFStateAgentData* data, FP rangePercent, ref HNSFStateContext stateContext)
        {
            if (actionDisabled) return false;
            
            foreach (var deci in decision)
            {
                if (deci == null) continue;
                if (!deci.Decide(frame, entity, ref stateContext)) return false;
            }
            
            var exitEarly = ExecuteAction(frame, entity, rangePercent, ref stateContext);
            if (exitEarly) return true;
            
            foreach (var child in children)
            {
                child.Execute(frame, entity, data, ref stateContext);
            }

            return false;
        }

        public virtual bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            return false;
        }

        public int GetHighestId()
        {
            int highestId = id;
            for (int i = 0; i < children.Length; i++)
            {
                highestId = Math.Max(highestId, children[i].GetHighestId());
            }
            return highestId;
        }

        public EntityRef GetActionTargetEntityRef(Frame frame, EntityRef callingEntity)
        {
            actionTargetContext.callingEntity = callingEntity;
            return HNSFStateHelper.GetStateTargetEntity(frame, ref actionTargetContext);
        }
        
        public EntityRef GetActionTargetEntityRef(Frame frame, EntityRef callingEntity, ref HNSFStateContext targetStateContext)
        {
            actionTargetContext.callingEntity = callingEntity;
            var targetEntity = HNSFStateHelper.GetStateTargetEntity(frame, ref actionTargetContext);
            if(targetEntity == EntityRef.None) return EntityRef.None;
            targetStateContext = new HNSFStateContext(frame, targetEntity);
            return targetEntity;
        }
    }
}
using System;
using System.Collections.Generic;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Functions;
using HnSF.Nodes;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Actions
{
    [Serializable]
    public unsafe partial class SetActorLDT : GroupControlAction
    {
        public bool applyToAll;
        public List<AssetRef<Tag>> actorsToApplyTo;
        public bool setDeltatime;
        public FP deltatime = 1;
        public bool setMultiplier;
        public FP multiplier = 1;
        public int setForFrames;
        public bool resetOnExit = true;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            if (setForFrames > 0)
            {
                frame.AddOrGet<GenericTimer>(infoEntityRef, out var gt);
                gt->countingType = TimerCountingType.CountDown;
                gt->value = setForFrames;
            }
        }
        
        public override bool Tick(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            if (applyToAll)
            {
                var filter = frame.Filter<LocalDeltaTime>();

                while (filter.NextUnsafe(out var eRef, out var ldt))
                {
                    if (setDeltatime)
                        ldt->deltaTime = deltatime;
                    if (setMultiplier)
                        ldt->multiplier = multiplier;
                }
            }
            else
            {
                
            }

            if (setForFrames > 0
                && frame.Unsafe.TryGetPointer<GenericTimer>(infoEntityRef, out var gt)
                && gt->value > 0)
                return false;

            return true;
        }
        
        public override void OnExit(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            frame.Remove<GenericTimer>(infoEntityRef);
            
            if (applyToAll)
            {
                if (!resetOnExit)
                    return;
                
                var filter = frame.Filter<LocalDeltaTime>();

                while (filter.NextUnsafe(out var eRef, out var ldt))
                {
                    if (setDeltatime)
                        ldt->deltaTime = 1;
                    if (setMultiplier)
                        ldt->multiplier = 1;
                }
            }
            else
            {
                
            }
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class SetActorLDT : ActorGroupControlNode
    {
        public const string OPTION_APPLYTOALL = "ApplyToAll";
        public const string OPTION_SETDT = "SetDeltaTime";
        public const string OPTION_DT = "DeltaTime";
        public const string OPTION_SETMULTI = "SetMultiplier";
        public const string OPTION_MULTI = "Multiplier";
        public const string OPTION_SETFORFRAMES = "SetForFrames";
        public const string OPTION_RESETONEXIT = "ResetOnExit";
        public const string PORT_TAGGEDENTITIES = "EntityTags";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<bool>(OPTION_APPLYTOALL)
                .WithDisplayName("Apply to All")
                .Build();

            context.AddOption<bool>(OPTION_SETDT)
                .WithDisplayName("Set DeltaTime?")
                .Build();

            context.AddOption<FP>(OPTION_DT)
                .WithDisplayName("DeltaTime")
                .WithDefaultValue(1)
                .Build();

            context.AddOption<bool>(OPTION_SETMULTI)
                .WithDisplayName("Set Multiplier?")
                .Build();

            context.AddOption<FP>(OPTION_MULTI)
                .WithDisplayName("Multiplier")
                .WithDefaultValue(1)
                .Build();

            context.AddOption<int>(OPTION_SETFORFRAMES)
                .WithDisplayName("Set For Frames")
                .Build();

            context.AddOption<bool>(OPTION_RESETONEXIT)
                .WithDisplayName("Reset On Exit?")
                .WithDefaultValue(true)
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
            
            context.AddInputPort(PORT_TAGGEDENTITIES)
                .WithDisplayName("Tagged Entities")
                .WithDataType<List<AssetRef<Tag>>>()
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }

        public override GroupControlAction Convert()
        {
            GetNodeOptionByName(OPTION_APPLYTOALL).TryGetValue(out bool applyToAll);
            GetNodeOptionByName(OPTION_SETDT).TryGetValue(out bool setDeltatime);
            GetNodeOptionByName(OPTION_DT).TryGetValue(out FP dt);
            GetNodeOptionByName(OPTION_SETMULTI).TryGetValue(out bool setMultiplier);
            GetNodeOptionByName(OPTION_MULTI).TryGetValue(out FP multi);
            GetNodeOptionByName(OPTION_SETFORFRAMES).TryGetValue(out int setForFrames);
            GetNodeOptionByName(OPTION_RESETONEXIT).TryGetValue(out bool resetOnExit);
            
            return new Actions.SetActorLDT()
            {
                applyToAll = applyToAll,
                setDeltatime = setDeltatime,
                deltatime = dt,
                setMultiplier =  setMultiplier,
                multiplier = multi,
                setForFrames = setForFrames,
                resetOnExit = resetOnExit,
                actorsToApplyTo = NodeHelper.GetInputPortValue<List<AssetRef<Tag>>>(GetInputPortByName(PORT_TAGGEDENTITIES))
            };
        }
    }
}
#endif
using System;
using HnSF.core.GroupControl.Actions;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Actions
{
    [Serializable]
    public unsafe partial class AwaitAllFinishedUnsyncedCutscene : GroupControlAction
    {
        public AssetRef<Tag> cutsceneSourceTag;
        public AssetRef<Tag> cutsceneTag;
        public int maxWaitFrames;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef)
        {
            frame.AddOrGet(infoEntityRef, out PlayerReadyMap* prm);
            prm->ClearMap(frame);

            if (maxWaitFrames > 0)
            {
                frame.AddOrGet(infoEntityRef, out GenericTimer* gt);
                gt->countingType = TimerCountingType.CountUp;
                gt->value = 0;
            }
        }
        
        public override bool Tick(Frame frame, EntityRef infoEntityRef)
        {
            var prm = frame.Unsafe.GetPointer<PlayerReadyMap>(infoEntityRef);
            return prm->CheckForAllFinishedWithUnsyncedCutscene(frame, cutsceneSourceTag, cutsceneTag);
        }
        
        public override void OnExit(Frame frame, EntityRef infoEntityRef)
        {
            frame.Remove<PlayerReadyMap>(infoEntityRef);
            frame.Remove<GenericTimer>(infoEntityRef);
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class AwaitAllFinishedUnsyncedCutscene : ActorGroupControlNode
    {
        public const string OPTION_CUTSCENE_SOURCE_TAG = "CutsceneSourceTag";
        public const string OPTION_CUTSCENE_TAG = "CutsceneTag";
        public const string OPTION_MAXWAITFRAMES = "MaxWaitFrames";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<AssetRef<Tag>>(OPTION_CUTSCENE_SOURCE_TAG)
                .WithDisplayName("Cutscene Source Tag")
                .Build();
            
            context.AddOption<AssetRef<Tag>>(OPTION_CUTSCENE_TAG)
                .WithDisplayName("Cutscene Tag")
                .Build();
            
            context.AddOption<int>(OPTION_MAXWAITFRAMES)
                .WithDisplayName("Timeout After Frames")
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
        }

        public override GroupControlAction Convert()
        {
            this.GetNodeOptionByName(OPTION_CUTSCENE_SOURCE_TAG).TryGetValue(out AssetRef<Tag> cutsceneSourceTag);
            this.GetNodeOptionByName(OPTION_CUTSCENE_TAG).TryGetValue(out AssetRef<Tag> cutsceneTag);
            this.GetNodeOptionByName(OPTION_MAXWAITFRAMES).TryGetValue(out int maxWaitFrames);
            
            
            return new Actions.AwaitAllFinishedUnsyncedCutscene()
            {
                cutsceneSourceTag = cutsceneSourceTag,
                cutsceneTag = cutsceneTag,
                maxWaitFrames = maxWaitFrames,
            };
        }
    }
}
#endif
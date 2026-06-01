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
    public unsafe partial class StopUnsyncedCutscene : GroupControlAction
    {
        public AssetRef<Tag> cutsceneSourceTag;
        public AssetRef<Tag> cutsceneTag;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref GroupControlContext context)
        {
        }
        
        public override bool Tick(Frame frame, EntityRef infoEntityRef, ref GroupControlContext context)
        {
            frame.Events.StopCutsceneUnsynced(
                cutsceneSource: default,
                cutsceneSourceTag: cutsceneSourceTag,
                cutsceneTag: cutsceneTag,
                onlyPlayOnPlayerLocalMachine: false);
            return true;
        }
        
        public override void OnExit(Frame frame, EntityRef infoEntityRef, ref GroupControlContext context)
        {
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class StopUnsyncedCutscene : ActorGroupControlNode
    {
        public const string OPTION_CUTSCENE_SOURCE_TAG = "CutsceneSourceTag";
        public const string OPTION_CUTSCENE_TAG = "CutsceneTag";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<AssetRef<Tag>>(OPTION_CUTSCENE_SOURCE_TAG)
                .WithDisplayName("Cutscene Source Tag")
                .Build();
            
            context.AddOption<AssetRef<Tag>>(OPTION_CUTSCENE_TAG)
                .WithDisplayName("Cutscene Tag")
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
            
            
            return new Actions.StopUnsyncedCutscene()
            {
                cutsceneSourceTag = cutsceneSourceTag,
                cutsceneTag = cutsceneTag
            };
        }
    }
}
#endif
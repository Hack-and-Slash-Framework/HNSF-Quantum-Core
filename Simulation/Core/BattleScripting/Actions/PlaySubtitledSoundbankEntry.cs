using System;
using HnSF.core.GroupControl.Actions;
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
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: true, sourceNamespace: "HnSF.core.scripting.VersusIntro.Actions")]
#endif
    public unsafe partial class PlaySubtitledSoundbankEntry : GroupControlAction
    {
        public AssetRef<Tag> voiceClip;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            //frame.Events.PlaySubtitledSoundbankEntry(infoEntityRef, voiceClip, voiceClip, 1);
        }

        public override bool Tick(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            return true;
        }

        public override void OnExit(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class PlaySubtitledSoundbankEntryNode : ActorGroupControlNode
    {
        public const string IN_PORT_SoundEntry_Tag = "SoundEntryTag";
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort<Tag>(IN_PORT_SoundEntry_Tag)
                .WithDisplayName("Sound Entry Tag")
                .Build();
        }

        public override GroupControlAction Convert()
        {
            var soundEntryTag = ActorGroupScriptDirectorImporter.GetInputPortValue<Tag>(this.GetInputPortByName(IN_PORT_SoundEntry_Tag));
            return new PlaySubtitledSoundbankEntry()
            {
                voiceClip = soundEntryTag
            };
        }
    }
}
#endif
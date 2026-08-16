using System;
using HnSF.core.GroupControl.Actions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Actions
{
    [Serializable]
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: true, sourceNamespace: "HnSF.core.scripting.VersusIntro.Actions")]
#endif
    public unsafe partial class PlaySubtitledSoundEntry : GroupControlAction
    {
        public AssetRef<SoundEntry> voiceClip;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            frame.Events.PlaySubtitledSoundEntry(infoEntityRef, voiceClip, 1);
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
    internal class PlaySubtitledSoundEntryNode : ActorGroupControlNode
    {
        public const string IN_PORT_SoundEntry_Tag = "SoundEntry";
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
            
            context.AddInputPort<SoundEntry>(IN_PORT_SoundEntry_Tag)
                .WithDisplayName("Sound Entry")
                .Build();
        }

        public override GroupControlAction Convert()
        {
            var soundEntryTag = NodeHelper.GetInputPortValue<SoundEntry>(this.GetInputPortByName(IN_PORT_SoundEntry_Tag));
            return new PlaySubtitledSoundEntry()
            {
                voiceClip = soundEntryTag
            };
        }
    }
}
#endif
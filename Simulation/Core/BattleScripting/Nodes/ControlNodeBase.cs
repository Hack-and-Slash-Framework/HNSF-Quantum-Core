#if UNITY_EDITOR
using System;
using HnSF.core.GroupControl.Actions;
using Unity.GraphToolkit.Editor;

namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    public class ControlNodeBase : Node
    {
        
    }
}
#endif
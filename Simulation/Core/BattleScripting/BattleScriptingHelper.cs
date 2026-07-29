using System;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Functions;
using HnSF.core.state;
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

namespace Quantum
{
    public static class BattleScriptingHelper
    {
        public static T GetFunctionNodeValue<T>(this IPort port) where T : GroupControlFunction
        {
            if (port.GetNode() is not FunctionNodeBase fnNodeBase)
                return null;
            return fnNodeBase.Convert() as T;
        }
    }
}
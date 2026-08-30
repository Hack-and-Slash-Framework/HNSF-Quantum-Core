using System;
using HnSF.core.GroupControl;
using HnSF.core.state;
using HnSF.core.state.actions;
using HnSF.core.state.decisions;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public unsafe partial class CreateGroupControlDefinition : AssetObject
    {
        public static HNSFStateContext emptyContext = new HNSFStateContext();
        
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateDecision[] conditions = Array.Empty<HNSFStateDecision>();

        public AssetRef identifier;

        public AssetRef<BattleActorGroupControlScript> controlScript;
        
        public bool ConditionsValid(Frame frame, EntityRef sourceEntity)
        {
            bool conditionsValid = true;
            foreach (var deci in conditions)
            {
                if (deci.Decide(frame, sourceEntity, ref emptyContext)) continue;
                conditionsValid = false;
                break;
            }
            
            return conditionsValid != false;
        }

        public bool TryCreateController(Frame frame, EntityRef sourceEntity, bool autoDestroy = true)
        {
            if(!ConditionsValid(frame, sourceEntity)) return false;
            
            var genericControlManager = frame.GetOrAddSingleton<GenericGroupControlManager>();
            if (genericControlManager.ContainsKey(frame, identifier))
                return false;
            
            var gcEntityRef = frame.Create();
            frame.Add(gcEntityRef, new GenericGroupControl()
            {
                data = new GroupControlStateData()
                {
                    script = controlScript,
                    currentAction = 0
                },
                autoDestroy = autoDestroy
            }, out var ggc);

            var groupControlContext = new BattleScriptContext();
            groupControlContext.SetScriptEntityAndBlackboard(frame, gcEntityRef, null);
            
            ggc->data.SetData(controlScript);
            ggc->data.Initialize(frame, gcEntityRef, ref groupControlContext);

            genericControlManager.Add(frame, identifier, gcEntityRef);
            return true;
        }
    }
}
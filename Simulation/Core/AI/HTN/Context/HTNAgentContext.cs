using System.Collections.Generic;

namespace Quantum
{
    public unsafe partial struct HTNAgentContext
    {
        public Frame frame;
        public EntityRef agentEntityRef;
        public BattleActorAI* battleActorAI;
        public HTNAgent* agent;
        public List<byte> currentMTR;
        public Dictionary<byte, Stack<KeyValuePair<EffectType, byte>>> worldStateChangeStack;
        public bool debug;
        
        public HTNAgentContext(Frame frame, EntityRef agentEntityRef, HTNAgent* agent, BattleActorAI* battleActorAI)
        {
            this.frame = frame;
            this.agentEntityRef = agentEntityRef;
            this.agent = agent;
            this.battleActorAI = battleActorAI;
            this.currentMTR = null;
            this.worldStateChangeStack = null;
            debug = false;
        }
        
        public HTNAgentContext(Frame frame, EntityRef agentEntityRef, HTNAgent* agent, BattleActorAI* battleActorAI, List<byte> currentMTR)
        {
            this.frame = frame;
            this.agentEntityRef = agentEntityRef;
            this.agent = agent;
            this.battleActorAI = battleActorAI;
            this.currentMTR = currentMTR;
            this.worldStateChangeStack = null;
            debug = false;
        }

        public HTNAgentContext(Frame frame, EntityRef agentEntityRef, HTNAgent* agent, BattleActorAI* battleActorAI, List<byte> currentMTR, Dictionary<byte, Stack<KeyValuePair<EffectType, byte>>> worldStateChangeStack)
        {
            this.frame = frame;
            this.agentEntityRef = agentEntityRef;
            this.agent = agent;
            this.battleActorAI = battleActorAI;
            this.currentMTR = currentMTR;
            this.worldStateChangeStack = worldStateChangeStack;
            debug = false;
        }
        
        /// <summary>
        /// Apply permanent world state changes to the actual world state used during plan execution.
        /// </summary>
        public void TrimForExecution()
        {
            if (agent->contextState == HTNContextState.Executing)
            {
                Log.Error("Can not trim a context when in execution mode");
                return;
            }

            foreach (var keyValueStack in worldStateChangeStack)
            {
                while (keyValueStack.Value.Count != 0 && keyValueStack.Value.Peek().Key != EffectType.Permanent)
                {
                    keyValueStack.Value.Pop();
                }
            }
        }
    }
}

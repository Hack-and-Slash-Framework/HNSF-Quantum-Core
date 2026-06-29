using System.Collections.Generic;
using Quantum.Collections;

namespace Quantum
{
    public unsafe partial struct HTNWorldState
    {
        public static void Reset(ref HTNAgentContext context)
        {
            var lastMTRecord = context.frame.ResolveList(context.agent->lastMTR);
            
            lastMTRecord.Clear();
            context.currentMTR.Clear();
        }
        
        public static bool HasState(ref HTNAgentContext context, byte state)
        {
            var worldState = context.frame.ResolveDictionary(context.agent->worldState.current);
            return HasState(ref context, state, ref worldState);
        }
        
        public static bool HasState(ref HTNAgentContext context, byte state, ref QDictionary<byte, byte> worldStateDictionary)
        {
            return worldStateDictionary.ContainsKey(state);
        }
        
        public static bool HasStateValue(ref HTNAgentContext context, byte state, byte value)
        {
            var worldState = context.frame.ResolveDictionary(context.agent->worldState.current);
            return HasStateValue(ref context, state, value, ref worldState);
        }
        
        public static bool HasStateValue(ref HTNAgentContext context, byte state, byte value, ref QDictionary<byte, byte> worldStateDictionary)
        {
            return worldStateDictionary.ContainsKey(state) && worldStateDictionary[state] == value;
        }
        
        public static bool TryGetState(ref HTNAgentContext context, byte state, out byte value)
        {
            var worldState = context.frame.ResolveDictionary(context.agent->worldState.current);
            return TryGetState(ref context, state, out value, ref worldState);
        }
        
        public static bool TryGetState(ref HTNAgentContext context, byte state, out byte value, ref QDictionary<byte, byte> worldState)
        {
            if (context.agent->contextState == HTNContextState.Executing)
            {
                return worldState.TryGetValue(state, out value);
            }

            if (!context.worldStateChangeStack.TryGetValue(state, out var stateStack)
                || stateStack.Count == 0)
            {
                return worldState.TryGetValue(state, out value);
            }

            value = stateStack.Peek().Value;
            return true;
        }
        
        public static void SetState(ref HTNAgentContext context, byte state, byte value, bool setAsDirty = true, EffectType e = EffectType.Permanent)
        {
            var worldState = context.frame.ResolveDictionary(context.agent->worldState.current);
            SetState(ref context, state, value, ref worldState, setAsDirty, e);
        }
        
        public static void SetState(ref HTNAgentContext context, byte state, byte value, ref QDictionary<byte, byte> worldState, bool setAsDirty = true, EffectType e = EffectType.Permanent)
        {
            if (context.agent->contextState == HTNContextState.Executing)
            {
                // Prevent setting the world state dirty if we're not changing anything.
                if (worldState.TryGetValue(state, out var currentValue)
                    && currentValue == value)
                    return;

                worldState[state] = value;
                if (setAsDirty)
                    context.agent->contextDirty = true; // When a state change during execution, we need to mark the context dirty for replanning!
            }
            else
            {
                if (!context.worldStateChangeStack.TryGetValue(state, out var stateStack))
                {
                    stateStack = new Stack<KeyValuePair<EffectType, byte>>();
                    context.worldStateChangeStack.Add(state, stateStack);
                }
                context.worldStateChangeStack[state].Push(new KeyValuePair<EffectType, byte>(e, value));
            }
        }
        
        public static void SetDirty(ref HTNAgentContext context, bool value = true)
        {
            context.agent->contextDirty = value; // When a state change during execution, we need to mark the context dirty for replanning!
        }
    }
}

namespace Quantum
{
    public unsafe partial struct HTNAgent
    {
        public void ClearCurrentScript()
        {
            currentActionData.script = default;
            currentActionData.currentAction = -1;
            uninterruptible = false;
        }

        public void ResetCooldown()
        {
            cooldown = 0;
        }
        
        public static void Update(Frame frame, EntityRef agentEntityRef, HTNAgent* agent)
        {
            if (!frame.Unsafe.TryGetPointer<BattleActorAI>(agentEntityRef, out var battleActorAI) 
                || !frame.TryFindAsset(agent->behaviourDefinition, out var behaviourDefinition)) return;
            
            behaviourDefinition.Tick(frame, agentEntityRef, agent, battleActorAI);
        }
    }
}

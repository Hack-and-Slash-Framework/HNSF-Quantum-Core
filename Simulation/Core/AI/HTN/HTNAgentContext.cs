namespace Quantum
{
    public unsafe partial struct HTNAgentContext
    {
        public BattleActorAI* battleActorAI;
        public HTNAgent* agent;

        public HTNAgentContext(HTNAgent* agent, BattleActorAI* battleActorAI)
        {
            this.agent = agent;
            this.battleActorAI = battleActorAI;
        }
    }
}

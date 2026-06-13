using Quantum;

namespace HnSF.core.AI.HTN.Conditions
{
    public interface ICondition
    {
        public string Label { get; set; }
        public bool IsValid(ref HTNAgentContext context);
    }
}
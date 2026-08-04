using Quantum;

namespace HnSF.core.AI.HTN.Sensors
{
    public interface ISensor
    {
        public string Label { get; set; }
        public void Execute(ref HTNAgentContext context);
    }
}
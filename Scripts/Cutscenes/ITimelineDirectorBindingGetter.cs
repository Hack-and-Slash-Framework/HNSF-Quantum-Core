using Quantum;

namespace HnSF
{
    public interface ITimelineDirectorBindingGetter
    {
        public void Bind(QuantumGame qGame, CutsceneBindingSource bindingSource);
    }
}
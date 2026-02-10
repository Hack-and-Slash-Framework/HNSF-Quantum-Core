using Quantum;

namespace HnSF
{
    public interface ITimelineDirectorBinder
    {
        public void Bind();
        public void Bind(ITimelineDirectorBindingSource bindingSource);
        public void Bind(QuantumGame qGame);
        public void Bind(QuantumGame qGame, ITimelineDirectorBindingSource bindingSource);
    }
}
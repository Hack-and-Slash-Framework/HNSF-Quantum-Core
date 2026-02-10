using Quantum;

namespace HnSF
{
    public interface ICutsceneBinding
    {
        public void Bind(QuantumGame qGame, CutsceneBindingSource bindingSource);
    }
}
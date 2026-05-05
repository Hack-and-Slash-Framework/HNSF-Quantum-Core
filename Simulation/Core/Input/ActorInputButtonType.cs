using System;

namespace Quantum
{
#if HNSF_DISABLE_DEFAULTS
#else
    [Flags]
    public enum ActorInputButtonType : int
    {
    }
#endif
}
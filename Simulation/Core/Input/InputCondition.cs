using UnityEngine.Serialization;

namespace Quantum
{
    [System.Serializable]
    public struct InputCondition
    {
        public InputBitmask[] sequence;
        public int impreciseInputCount;
        public bool ignoreDisableInput;
        public EnterInputMethod method;
    }
}
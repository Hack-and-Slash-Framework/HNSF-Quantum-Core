namespace Quantum
{
    [System.Serializable]
    public struct InputBitmask
    {
        public ActorInputButtonType input;
        public int lenience;
        public bool checkNot;
    }
}
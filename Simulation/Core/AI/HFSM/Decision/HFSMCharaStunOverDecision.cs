namespace Quantum
{
    [System.Serializable]
    public unsafe partial class HFSMCharaStunOverDecision : HFSMDecision
    {
        public enum StunType
        {
            Hitstun,
            Blockstun
        }

        public StunType stunType;
        
        public override bool Decide(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            switch (stunType)
            {
                case StunType.Hitstun:
                    return (!frame.Unsafe.TryGetPointer<Hitstun>(entity, out var hitstun)) || (hitstun->value <= 0);
                case StunType.Blockstun:
                    return (!frame.Unsafe.TryGetPointer<Blockstun>(entity, out var blockstun)) || (blockstun->value <= 0);
            }
            return false;
        }
    }
}
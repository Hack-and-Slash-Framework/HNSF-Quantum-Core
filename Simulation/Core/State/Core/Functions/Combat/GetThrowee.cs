using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetThrowee : StateFunctionEntityRef
    {
        public bool anyThrowee = true;
        [DrawIf(nameof(anyThrowee), false)]
        public int throweeId;
        
        public override EntityRef Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (frame.Unsafe.TryGetPointer<IsThrowing>(entity, out var isa))
            {
                var throwees = frame.ResolveDictionary(isa->throwees);
                if (anyThrowee)
                {
                    foreach (var throwee in throwees) return throwee.Value;
                }
                else
                {
                    foreach (var throwee in throwees)
                    {
                        if(throwee.Key == throweeId) return throwee.Value;
                    }
                }
            }
            return EntityRef.None;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetThrowee());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetThrowee;
            return base.CopyTo(target);
        }
    }
}
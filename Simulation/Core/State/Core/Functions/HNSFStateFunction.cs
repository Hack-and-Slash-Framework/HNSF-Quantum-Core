using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class HNSFStateFunction
    {

        public virtual HNSFStateFunction Copy()
        {
            throw new System.NotImplementedException();
        }

        public virtual HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            throw new System.NotImplementedException();
        }
    }
    
    [System.Serializable]
    public unsafe class HNSFStateFunction<T> : HNSFStateFunction
    {
        public string Label;
        
        public virtual T Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return default(T);
        }

        public virtual T Execute(FrameThreadSafe frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return Execute((Frame)frame, entity, ref stateContext);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new HNSFStateFunction<T>());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as HNSFStateFunction<T>;
            t.Label = Label;
            return target;
        }
    }
}
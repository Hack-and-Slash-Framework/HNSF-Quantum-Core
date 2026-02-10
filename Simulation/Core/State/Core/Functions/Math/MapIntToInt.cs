using System;
using System.Collections.Generic;
using System.Linq;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class MapIntToInt : StateFunctionInt
    {
        [System.Serializable]
        public struct IntToIntMapping
        {
            public int from;
            public int to;
        }

        public IntToIntMapping[] mappings = Array.Empty<IntToIntMapping>();

        public bool refresh;
        [NonSerialized] private Dictionary<int, int> _mappings = new Dictionary<int, int>();

        public HNSFParamInt intParam;
        
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (refresh || _mappings.Count != mappings.Length) BuildMappings();

            var fromValue = intParam.Resolve(frame, entity, ref stateContext);
            return _mappings.GetValueOrDefault(fromValue, 0);
        }

        private void BuildMappings()
        {
            refresh = false;
            foreach (var t in mappings) _mappings.Add(t.from, t.to);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new MapIntToInt());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as MapIntToInt;
            t.mappings = mappings.ToArray();
            t.refresh = refresh;
            t.intParam = intParam;
            return base.CopyTo(target);
        }
    }
}
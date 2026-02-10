using HnSF.core.state;

namespace Quantum
{
    public unsafe partial struct TrackingAttackString
    {
        public void RegisterAttackToString(Frame f, AssetRef<HNSFState> attackState)
        {
            var list = f.ResolveList(currentAttackString);
            list.Add(attackState);
        }

        public bool IsAttackInStringAtLeastXTimes(Frame f, AssetRef<HNSFState> attackState, int xTimesBeforeTrue = 1)
        {
            var list = f.ResolveList(currentAttackString);
        
            int cnt = 0;
            foreach (var state in list)
            {
                if (state == attackState) cnt++;
                if (cnt == xTimesBeforeTrue) return true;
            }

            return false;
        }

        public bool IsAttackLastInStringXTimes(Frame f, AssetRef<HNSFState> attackState, int checkLength = 1)
        {
            var list = f.ResolveList(currentAttackString);
        
            int cnt = 0;
            for (int i = list.Count - 1; i >= list.Count-1-checkLength; i--)
            {
                if (i < 0) return false;
                if (list[i] == attackState) cnt++;
                if (cnt == checkLength) return true;
            }

            return false;
        }

        public void PrintAttackString(Frame f)
        {
            var list = f.ResolveList(currentAttackString);

            var s = "";

            for (int i = 0; i < list.Count; i++)
            {
                var ass = f.FindAsset<HNSFState>(list[i].Id);
                s += $"{ass.Label}\n";
            }
        
            Log.Debug(s);
        }
    
        public void ClearAttackString(Frame f)
        {
            var list = f.ResolveList(currentAttackString);
            list.Clear();
        }
    }
}
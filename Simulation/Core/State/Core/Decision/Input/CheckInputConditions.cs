using System;
using System.Linq;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class CheckInputConditions : HNSFStateDecision
    {
        public AssetRef<InputConditionListAsset>[] inputConditionListAssetRef;
        public int offset;

        public enum CheckType
        {
            Sequence,
            Any
        }
        
        public CheckType checkType = CheckType.Sequence;
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<ActorInputBuffer>(entity, out var charaInputs)) return false;
            if (charaInputs->ignoreButtons == (ActorInputButtonType)~0) return false;
            if (offset >= Constants.INPUT_BUFFER_SIZE)
            {
                Log.DebugWarn($"Offset is larger or equal to the input buffer size ({Constants.INPUT_BUFFER_SIZE}), which is not allowed.");
                return false;
            }
            
            var lastBufferPos = charaInputs->bufferPosition - offset;

            switch (checkType)
            {
                case CheckType.Sequence:
                    for (var index = inputConditionListAssetRef.Length - 1; index >= 0; index--)
                    {
                        var inputConditionListAsset = frame.FindAsset<InputConditionListAsset>(inputConditionListAssetRef[index].Id);

                        lastBufferPos = InputHelper.CheckInputConditions(frame, charaInputs, inputConditionListAsset.conditions, lastBufferPos);
                        if (lastBufferPos == -1) return false;
                    }
                    return true;
                case CheckType.Any:
                    for (int i = 0; i < inputConditionListAssetRef.Length; i++)
                    {
                        var inputConditionListAsset = frame.FindAsset<InputConditionListAsset>(inputConditionListAssetRef[i].Id);
                        var bPos = InputHelper.CheckInputConditions(frame, charaInputs, inputConditionListAsset.conditions, lastBufferPos);
                        if (bPos == -1) continue;
                        return true;
                    }
                    break;
            }
            return false;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new CheckInputConditions());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as CheckInputConditions;
            t.inputConditionListAssetRef = inputConditionListAssetRef.ToArray();
            t.offset = offset;
            return base.CopyTo(target);
        }
    }
}
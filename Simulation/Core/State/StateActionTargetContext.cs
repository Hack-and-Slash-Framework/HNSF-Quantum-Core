using System;

namespace Quantum
{
    [System.Serializable]
    public struct StateActionTargetContext
    {
        [NonSerialized]
        public EntityRef callingEntity;
        public StateActionTargetType targetType;
        public AssetRef<Tag> mapTag;
        public int throweeId;

        public StateActionTargetContext(StateActionTargetType targetType, EntityRef callingEntity)
        {
            this.targetType = targetType;
            this.callingEntity = callingEntity;
            this.mapTag = default;
            this.throweeId = 0;
        }
        
        public StateActionTargetContext(StateActionTargetType targetType, EntityRef callingEntity, AssetRef<Tag> mapTag)
        {
            this.targetType = targetType;
            this.callingEntity = callingEntity;
            this.mapTag = mapTag;
            this.throweeId = 0;
        }
        
        public StateActionTargetContext(StateActionTargetType targetType, EntityRef callingEntity, AssetRef<Tag> mapTag, int throweeId)
        {
            this.targetType = targetType;
            this.callingEntity = callingEntity;
            this.mapTag = mapTag;
            this.throweeId = throweeId;
        }
    }
}

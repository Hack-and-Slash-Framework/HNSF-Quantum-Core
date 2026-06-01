using Quantum;

namespace HnSF.core.GroupControl
{
    public unsafe partial struct BattleScriptContext
    {
        private EntityRef _scriptEntity;
        private unsafe AIBlackboardComponent* _blackboard;
        private byte _CustomDataType;
        
        public EntityRef ScriptEntity => this._scriptEntity;
        public unsafe AIBlackboardComponent* Blackboard => this._blackboard;
        public unsafe byte CustomDataType => this._CustomDataType;
        public unsafe void* CustomData { get; private set; }
        
        public void SetScriptEntityAndBlackboard(Frame frame, EntityRef scriptEntity, AIBlackboardComponent* blackboard)
        {
            _scriptEntity = scriptEntity;
            _blackboard = blackboard;
        }
        
        public unsafe void SetUserData(byte customDataType, void* userData)
        {
            this._CustomDataType = customDataType;
            this.CustomData = userData;
        }
    }
}

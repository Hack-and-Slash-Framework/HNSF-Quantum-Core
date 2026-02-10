namespace Quantum
{
    [System.Serializable]
    public class AIBotComboConditionBase : AssetObject
    {
        public virtual bool Decide(Frame frame, ref AIBotComboConditionInfo info)
        {
            return true;
        }
    }
}

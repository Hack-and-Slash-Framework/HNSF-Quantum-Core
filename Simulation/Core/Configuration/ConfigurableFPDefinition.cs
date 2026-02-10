using Photon.Deterministic;

namespace Quantum
{
    [System.Serializable]
    public class ConfigurableFPDefinition : ConfigurableSettingBase
    {
        public FP value;
        public FP minValue;
        public FP maxValue;

        public ConfigurableFPDefinition(string key, FP value)
        {
            this.key = key;
            this.value = value;
        }
        
        public ConfigurableFPDefinition(string key, string title, string description, FP value, FP minValue, FP maxValue)
        {
            this.key = key;
            this.title = title;
            this.description = description;
            this.value = value;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
    }
}
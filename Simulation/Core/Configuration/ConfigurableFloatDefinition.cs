namespace Quantum
{
    [System.Serializable]
    public class ConfigurableFloatDefinition : ConfigurableSettingBase
    {
        public float value;
        public float minValue;
        public float maxValue;

        public ConfigurableFloatDefinition(string key, float value)
        {
            this.key = key;
            this.value = value;
        }
        
        public ConfigurableFloatDefinition(string key, string title, string description, float value, float minValue, float maxValue)
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

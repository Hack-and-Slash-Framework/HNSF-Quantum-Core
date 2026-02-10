namespace Quantum
{
    [System.Serializable]
    public class ConfigurableIntDefinition : ConfigurableSettingBase
    {
        public int value;
        public int minValue;
        public int maxValue;

        public ConfigurableIntDefinition(string key, int value)
        {
            this.key = key;
            this.value = value;
        }
        
        public ConfigurableIntDefinition(string key, string title, string description, int value, int minValue, int maxValue)
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

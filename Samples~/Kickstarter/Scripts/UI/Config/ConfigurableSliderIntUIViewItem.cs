using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui
{
    public class ConfigurableSliderIntUIViewItem : ConfigurableIntUIItemBase
    {
        public TextMeshProUGUI label;
        public TextMeshProUGUI value;
        public Slider slider;

        public void Initialize(string labelText, int currentValue, int minValue, int maxValue)
        {
            this.label.text = labelText;
            this.value.text = currentValue.ToString();
            this.slider.value = currentValue;
            this.slider.minValue = minValue;
            this.slider.maxValue = maxValue;
            this.slider.wholeNumbers = true;
            this.slider.SetValueWithoutNotify(currentValue);
            this.slider.onValueChanged.AddListener(WhenSliderValueChanged);
        }

        private void WhenSliderValueChanged(float arg0)
        {
            value.text = ((int)arg0).ToString();
        }

        public override int GetValue()
        {
            return (int)slider.value;
        }
    }
}
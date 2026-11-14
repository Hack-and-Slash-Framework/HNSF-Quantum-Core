using UnityEngine;

namespace HnSF.ui
{
    public class ConfigurableValueUIItemBase<T> : ConfigurableValueUIItemBase
    {
        public virtual T GetValue()
        {
            return default;
        }
    }
}
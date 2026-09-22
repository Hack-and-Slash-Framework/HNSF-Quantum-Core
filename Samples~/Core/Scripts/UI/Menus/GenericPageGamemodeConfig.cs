using System.Collections.Generic;
using CT.LocalInputManagement;
using CT.MenuNav;
using Cysharp.Threading.Tasks;
using Quantum;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HnSF.ui.menus
{
    public class GenericPageGamemodeConfig : MenuPage
    {
        public UnityEvent OnConfigurationCanceled;
        public UnityEvent<string> OnConfigurationConfirmed = new();
        
        public Canvas canvas;

        public ScrollRect optionsScrollRect;
        private GamemodeSettingsBase gamemodeSettings;
        public GameObject defaultSelection;

        [Header("Prefabs")]
        public ConfigurableSliderIntUIViewItem prefabConfigSliderInt;
        
        public Dictionary<string, ConfigurableValueUIItemBase> configItems = new Dictionary<string, ConfigurableValueUIItemBase>();

        private LoadedAssetHandleWrapper gamemodeAssetHandle;
        
        public bool initialized = false;

        public InputPlayerManager inputPlayer;

        public override UniTask<bool> TryOpenAsync(MenuNavContext context)
        {
            return base.TryOpenAsync(context);
        }

        public override UniTask<bool> TryCloseAsync(MenuNavContext context)
        {
            return base.TryCloseAsync(context);
        }
        
        public async UniTask Initialize(ModAssetSoftReference gamemodeReference)
        {
            initialized = false;
            var result = await HnSFManagersContainer.instance.contentManager.LoadAssetFromModAsync(gamemodeReference);
            if (result is null)
            {
                OnConfigurationCanceled?.Invoke();
                return;
            }

            if(gamemodeAssetHandle is {IsValid: true})
                gamemodeAssetHandle.Release();
            
            gamemodeAssetHandle = result;
            
            var ass = gamemodeAssetHandle.GetAsset<BaseGamemodeDefinition>();
            
            if (!await ass.LoadAssets())
            {
                OnConfigurationCanceled?.Invoke();
                return;
            }

            gamemodeSettings = ass.GetGamemodeSettingsInstance();

            InitializeSettings();
            initialized = true;
        }

        public void Uninitialize()
        {
            HnSFManagersContainer.instance.contentManager.ReleaseAssetFromMod(gamemodeAssetHandle);
            gamemodeAssetHandle = default;
            initialized = true;
        }

        protected void InitializeSettings()
        {
            var settings = gamemodeSettings.GetConfigurableSettings();
            for (int i = 0; i < settings.Length; i++)
            {
                var setting = settings[i];

                if (setting is ConfigurableIntDefinition configurableSetting)
                {
                    if (!configItems.ContainsKey(configurableSetting.key))
                    {
                        var item = GameObject.Instantiate(prefabConfigSliderInt, optionsScrollRect.content, false);
                        item.gameObject.SetActive(true);
                        item.Initialize(configurableSetting.title, configurableSetting.value, configurableSetting.minValue, configurableSetting.maxValue);
                        configItems.Add(configurableSetting.key, item);
                    }
                }
            }
        }

        public string ApplySettingsAndSave()
        {
            var settings = gamemodeSettings.GetConfigurableSettings();
            var configSettings = new List<ConfigurableSettingBase>();
            for (int i = 0; i < settings.Length; i++)
            {
                var setting = settings[i];

                if (setting is ConfigurableIntDefinition configurableSetting
                    && configItems.TryGetValue(configurableSetting.key, out var uiConfigItem))
                {
                    configSettings.Add(new ConfigurableIntDefinition(configurableSetting.key, (uiConfigItem as ConfigurableIntUIItemBase).GetValue() ));
                }
            }
            
            gamemodeSettings.SetConfigurableSettings(configSettings.ToArray());
            var jsonedSettings = JsonUtilityExtensions.ToJsonWithTypeAnnotation(gamemodeSettings);
            OnConfigurationConfirmed.Invoke(jsonedSettings);
            return jsonedSettings;
        }

        public void BUTTON_Back()
        {
            OnConfigurationCanceled.Invoke();
        }

        public void BUTTON_Apply()
        {
            ApplySettingsAndSave();
        }
    }
}

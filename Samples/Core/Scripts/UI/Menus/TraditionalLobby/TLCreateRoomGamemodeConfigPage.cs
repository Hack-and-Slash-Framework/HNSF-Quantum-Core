using System.Collections.Generic;
using CT.MenuNav;
using Cysharp.Threading.Tasks;
using Quantum;
using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui.menus.traditionallobby
{
    public class TLCreateRoomGamemodeConfigPage : MenuPage
    {
        [Space]
        public TraditionalLobbyScreenHelper helper;
        
        public Canvas canvas;

        public ScrollRect optionsScrollRect;
        private GamemodeSettingsBase gamemodeSettings;

        [Header("Prefabs")]
        public ConfigurableSliderIntUIViewItem prefabConfigSliderInt;
        
        public Dictionary<string, ConfigurableValueUIItemBase> configItems = new Dictionary<string, ConfigurableValueUIItemBase>();

        public override async UniTask<bool> TryOpenAsync(MenuNavContext context)
        {
            await AttemptInitialize();
            return await base.TryOpenAsync(context);
        }
        
        public async UniTask AttemptInitialize()
        {
            if (!helper.gamemodeHandle.IsValid())
            {
                await helper.screenManager.TryBackPageAsync();
                return;
            }

            var ass = helper.gamemodeHandle.GetAsset<BaseGamemodeDefinition>();
            
            if (!await ass.LoadAssets())
            {
                await helper.screenManager.TryBackPageAsync();
                return;
            }

            gamemodeSettings = ass.GetGamemodeSettingsInstance();

            InitializeSettings();
        }

        public void InitializeSettings()
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

        public void ApplySettingsAndSave()
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
            helper.gamemodeSettings = jsonedSettings;
        }

        public void BUTTON_Back()
        {
            _ = helper.screenManager.TryBackPageAsync();
        }

        public void BUTTON_Apply()
        {
            ApplySettingsAndSave();
            _ = helper.screenManager.TryBackPageAsync();
        }
    }
}

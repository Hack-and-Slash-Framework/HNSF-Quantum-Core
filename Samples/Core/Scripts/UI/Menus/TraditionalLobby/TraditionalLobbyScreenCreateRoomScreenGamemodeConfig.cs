using System.Collections.Generic;
using CT.MenuNav;
using Cysharp.Threading.Tasks;
using Quantum;
using UnityEngine;
using UnityEngine.UI;

namespace HnSF.ui.menus.traditionallobby
{
    public class TraditionalLobbyScreenCreateRoomScreenGamemodeConfig : MenuBase
    {
        public Canvas canvas;

        public ScrollRect optionsScrollRect;
        private GamemodeSettingsBase gamemodeSettings;

        [Header("Prefabs")]
        public ConfigurableSliderIntUIViewItem prefabConfigSliderInt;
        
        public Dictionary<string, ConfigurableValueUIItemBase> configItems = new Dictionary<string, ConfigurableValueUIItemBase>();
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            _ = AttemptInitialize();
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return base.TryClose(direction, forceClose);
        }

        public async UniTask AttemptInitialize()
        {
            var handler = (MenuHandler as TraditionalLobbyScreenCreateRoom);
            if (!handler.gamemodeHandle.IsValid())
            {
                MenuHandler.Back();
                return;
            }

            var ass = handler.gamemodeHandle.GetAsset<BaseGamemodeDefinition>();
            
            if (!await ass.LoadAssets())
            {
                MenuHandler.Back();
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
            var handler = (MenuHandler as TraditionalLobbyScreenCreateRoom);
            handler.gamemodeSettings = jsonedSettings;
        }

        public void BUTTON_Back()
        {
            MenuHandler.Back();
        }

        public void BUTTON_Apply()
        {
            ApplySettingsAndSave();
            MenuHandler.Back();
        }
    }
}
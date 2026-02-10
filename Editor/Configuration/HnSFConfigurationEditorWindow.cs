using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace HnSF
{
    public class HnSFConfigurationEditorWindow : EditorWindow
    {
        public static NamedBuildTarget CurrentNamedBuildTarget
        {
            get
            {
#if UNITY_SERVER
                    return NamedBuildTarget.Server;
#else
                BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
                BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
                NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
                return namedBuildTarget;
#endif
            }
        }

        public static readonly string HnSF_ScriptingDefine_UModSupport = "HNSF_UMOD";
        public static readonly string HnSF_ScriptingDefine_AnimancerSupport = "HNSF_ANIMANCER";
        public static readonly string HnSF_ScriptingDefine_PewEos = "HNSF_PEW_EOS";
        public static readonly string HnSF_ScriptingDefine_NetcodeForGameobjects = "HNSF_NGO";
        
        [SerializeField] private HnSFConfigurationAsset configAsset = null;
        [SerializeField] private VisualTreeAsset visualTree_NoConfigFound;
        [SerializeField] private VisualTreeAsset visualTree_foldoutItem;
        
        // TAB: Add Mod
        [SerializeField] private KnownModLoaderTypes addMod_ModType;
        [SerializeField] private string addMod_Location;
        [SerializeField] private string addMod_Identifier;
        [SerializeField] private string addMod_Name;
        
        [MenuItem("Tools/HnSF/Configuration")]
        public static void OpenWindow()
        {
            var wnd = CreateWindow<HnSFConfigurationEditorWindow>();
            wnd.titleContent = new GUIContent("HnSF Configuration");
            wnd.minSize = new Vector2(300, 300);
        }
        
        private void FindConfigurationAsset()
        {
            var cAssets = AssetDatabase.FindAssets($"t:{nameof(HnSFConfigurationAsset)}");
            if (cAssets.Length > 0) configAsset = AssetDatabase.LoadAssetAtPath<HnSFConfigurationAsset>(AssetDatabase.GUIDToAssetPath(cAssets[0]));
        }
        
        private void CreateGUI()
        {
            visualTree_NoConfigFound = Resources.Load<VisualTreeAsset>("UXML/UXML_HnSF_Configuration_NoFileFound");
            visualTree_foldoutItem = Resources.Load<VisualTreeAsset>("UXML/UXML_HnSF_Configuration_FoldoutItem");
         
            var so = new SerializedObject(this);
            
            FindConfigurationAsset();

            rootVisualElement.Clear();
            VisualElement root = rootVisualElement;
            if (configAsset == null)
            {
                visualTree_NoConfigFound.CloneTree(root);

                var b = root.Q<Button>();
                b.clicked += WhenCreateConfigFileClicked;
            }
            else
            {
                var tabView = new TabView();
                tabView.Q<VisualElement>(name: "unity-tab-view__header-container").style.marginBottom = 8;
                root.Add(tabView);

                var settingsTab = new Tab("Settings");
                settingsTab.name = "Tab_Settings";
                tabView.Add(settingsTab);
                CreateTabUI_Settings(settingsTab);
                UpdateTabUI_Settings();
                
                var configurePathTab = new Tab("Configure Paths");
                configurePathTab.name = "Tab_ConfigurePaths";
                tabView.Add(configurePathTab);
                CreateTabUI_ConfigurePaths(configurePathTab);
                UpdateTabUI_ConfigurePaths();
                
                var createModTab = new Tab("Create Mod");
                createModTab.name = "Tab_CreateMod";
                tabView.Add(createModTab);
                CreateTabUI_CreateModTab(createModTab);
                UpdateTabUI_CreateModTab();
            }
        }

        private void CreateTabUI_Settings(Tab tab)
        {
            var label = new Label("UMod Support");
            tab.Add(label);
            
            var addButton = new Button();
            addButton.name = "UModSupportToggle";
            addButton.text = "Enable UMod Support";
            addButton.clicked += Settings_ToggleUModSupport;
            tab.Add(addButton);
            
            /*
            var animancerLabel = new Label("Animancer Support");
            tab.Add(animancerLabel);
            
            var animancerSupportButton = new Button();
            animancerSupportButton.name = "AnimancerSupportToggle";
            animancerSupportButton.text = "Enable Animancer Support";
            animancerSupportButton.clicked += Settings_ToggleAnimancerSupport;
            tab.Add(animancerSupportButton);*/
            
            var pewEosLabel = new Label("PlayEveryWare Epic Online Services Support");
            tab.Add(pewEosLabel);
            
            var pewEosSupportButton = new Button();
            pewEosSupportButton.name = "PewEosSupportToggle";
            pewEosSupportButton.text = "Enable PlayEveryWare Epic Online Services Support";
            pewEosSupportButton.clicked += Settings_TogglePlayEveryWareEosSupport;
            tab.Add(pewEosSupportButton);
        }

        private void Settings_TogglePlayEveryWareEosSupport()
        {
            if (EditorApplication.isCompiling) return;
            PlayerSettings.GetScriptingDefineSymbols(CurrentNamedBuildTarget, out string[] defines);
            var definesList = defines.ToList();

            if (definesList.Contains(HnSF_ScriptingDefine_PewEos)) definesList.Remove(HnSF_ScriptingDefine_PewEos);
            else definesList.Add(HnSF_ScriptingDefine_PewEos);
            
            PlayerSettings.SetScriptingDefineSymbols(CurrentNamedBuildTarget, definesList.ToArray());
        }

        /*
        private void Settings_ToggleAnimancerSupport()
        {
            if (EditorApplication.isCompiling) return;
            PlayerSettings.GetScriptingDefineSymbols(CurrentNamedBuildTarget, out string[] defines);
            var definesList = defines.ToList();

            if (definesList.Contains(HnSF_ScriptingDefine_AnimancerSupport)) definesList.Remove(HnSF_ScriptingDefine_AnimancerSupport);
            else definesList.Add(HnSF_ScriptingDefine_AnimancerSupport);
            
            PlayerSettings.SetScriptingDefineSymbols(CurrentNamedBuildTarget, definesList.ToArray());
        }*/

        private void Settings_ToggleUModSupport()
        {
            if (EditorApplication.isCompiling) return;
            PlayerSettings.GetScriptingDefineSymbols(CurrentNamedBuildTarget, out string[] defines);
            var definesList = defines.ToList();

            if (definesList.Contains(HnSF_ScriptingDefine_UModSupport)) definesList.Remove(HnSF_ScriptingDefine_UModSupport);
            else definesList.Add(HnSF_ScriptingDefine_UModSupport);
            
            PlayerSettings.SetScriptingDefineSymbols(CurrentNamedBuildTarget, definesList.ToArray());
        }

        private void UpdateTabUI_Settings()
        {
            PlayerSettings.GetScriptingDefineSymbols(CurrentNamedBuildTarget, out string[] defines);
            var definesList = defines.ToList();
            
            var settingsTab = rootVisualElement.Q<Tab>("Tab_Settings");
            
            var umodToggleButton = settingsTab.Q<Button>("UModSupportToggle");
            umodToggleButton.text = definesList.Contains(HnSF_ScriptingDefine_UModSupport) ? "Disable UMod Support" : "Enable UMod Support";

            /*
            var animancerToggleButton = settingsTab.Q<Button>("AnimancerSupportToggle");
            animancerToggleButton.text = definesList.Contains(HnSF_ScriptingDefine_AnimancerSupport) ? "Disable Animancer Support" : "Enable Animancer Support";*/
            
            var pewEosToggleButton = settingsTab.Q<Button>("PewEosSupportToggle");
            pewEosToggleButton.text = definesList.Contains(HnSF_ScriptingDefine_PewEos) ? "Disable PlayEveryWare Epic Online Services Support" : "Enable PlayEveryWare Epic Online Services Support";
        }
        
        private void CreateTabUI_ConfigurePaths(Tab tab)
        {
            var label = new Label("Mod Info Asset Paths");
            tab.Add(label);
            
            var pathFoldout = new Foldout();

            for (int i = 0; i < configAsset.modLocations.Count; i++)
            {
                var index = i;
                var gottenPath = configAsset.modLocations[i].modInfoAsset != null ? AssetDatabase.GetAssetPath(configAsset.modLocations[i].modInfoAsset) : "...";
                    
                var modFoldoutItem = visualTree_foldoutItem.CloneTree();
                modFoldoutItem.Q<TextField>().value = gottenPath;
                modFoldoutItem.Q<Button>().clicked += () => { SetPathForModLocation(index); };
                pathFoldout.Add(modFoldoutItem);
            }
            
            var addButton = new Button();
            addButton.text = "Add";
            addButton.clicked += AddPathToList;
            pathFoldout.Add(addButton);
              
            tab.Add(pathFoldout);
        }

        private void UpdateTabUI_ConfigurePaths()
        {
            var configureTab = rootVisualElement.Q<Tab>("Tab_ConfigurePaths");
        }

        private void CreateTabUI_CreateModTab(Tab tab)
        {
            var so = new SerializedObject(this);
            
            var modTypeEnumField = new EnumField(addMod_ModType);
            modTypeEnumField.label = "Mod Type";
            modTypeEnumField.BindProperty(so.FindProperty(nameof(addMod_ModType)));
            tab.Add(modTypeEnumField);
            
            var modFoldoutItem = visualTree_foldoutItem.CloneTree();
            modFoldoutItem.Q<TextField>().BindProperty(so.FindProperty(nameof(addMod_Location)));
            modFoldoutItem.Q<TextField>().label = "Mod Path";
            modFoldoutItem.Q<Button>().clicked += () =>
            {
                string gotPath = EditorUtility.OpenFolderPanel("Select Mod Location", Application.dataPath, "");
                if (gotPath.Length == 0) return;
                if (gotPath == Application.dataPath)
                {
                    Debug.LogError("Mod Location Path can not be root of the Assets folder.");
                    return;
                }

                if (!gotPath.StartsWith(Application.dataPath))
                {
                    Debug.LogError("Path must be within project.");
                    return;
                }
                
                var rPath = gotPath.Substring(gotPath.IndexOf("Assets/", StringComparison.Ordinal));
                addMod_Location = rPath;
            };
            tab.Add(modFoldoutItem);
            
            var identifierField = new TextField();
            identifierField.BindProperty(so.FindProperty(nameof(addMod_Identifier)));
            identifierField.label = "Mod Identifier";
            tab.Add(identifierField);
            
            var modNameField = new TextField();
            modNameField.BindProperty(so.FindProperty(nameof(addMod_Name)));
            modNameField.label = "Mod Name";
            tab.Add(modNameField);
            
            var addButton = new Button();
            addButton.name = "CreateMod_CreateButton";
            addButton.text = "Create";
            //addButton.enabledSelf = false;
            addButton.clicked += CreateNewMod;
            tab.Add(addButton);
        }

        private void CreateNewMod()
        {
            var assetLocation = addMod_Location + "/" + $"ModInfoAsset_{addMod_Identifier.Replace(" ", "_")}" + ".asset";
            if (AssetDatabase.AssetPathExists(assetLocation))
            {
                Debug.LogError("Mod already exists at the given location.");
                return;
            }

            BaseModInfoAsset modInfoAsset = null;
            
            switch (addMod_ModType)
            {
                case KnownModLoaderTypes.ADDRESSABLES_LOCAL:
                    var localModInfoAsset = ScriptableObject.CreateInstance<AddressablesModInfoAsset>();
                    localModInfoAsset.SetInfo(addMod_Identifier, addMod_Name, "1.0.0", ModOnlineRequirement.NotRequiredByPlayers);
                    AssetDatabase.CreateAsset(localModInfoAsset, assetLocation);
                    modInfoAsset = localModInfoAsset;
                    break;
                case KnownModLoaderTypes.ADDRESSABLES:
                    var addressablesModInfoAsset = ScriptableObject.CreateInstance<AddressablesModInfoAsset>();
                    addressablesModInfoAsset.SetInfo(addMod_Identifier, addMod_Name, "1.0.0", ModOnlineRequirement.NotRequiredByPlayers);
                    AssetDatabase.CreateAsset(addressablesModInfoAsset, assetLocation);
                    modInfoAsset = addressablesModInfoAsset;
                    break;
                case KnownModLoaderTypes.UMOD:
#if HNSF_UMOD
                    var umodModInfoAsset = ScriptableObject.CreateInstance<UModModInfoAsset>();
                    umodModInfoAsset.modID = addMod_Identifier;
                    umodModInfoAsset.modName = addMod_Name;
                    AssetDatabase.CreateAsset(umodModInfoAsset, assetLocation);
                    modInfoAsset = umodModInfoAsset;
#endif
                    break;
            }
            
            AssetDatabase.SaveAssets();
            
            Undo.RecordObject(configAsset, "Add Path");
            configAsset.modLocations.Add(new HnSFConfigurationAsset.ModLocationDefinition()
            {
                modInfoAsset = AssetDatabase.LoadAssetAtPath<BaseModInfoAsset>(assetLocation)
            });
        }

        private void UpdateTabUI_CreateModTab()
        {
            var createModTab = rootVisualElement.Q<Tab>("Tab_CreateMod");

            switch (addMod_ModType)
            {
                case KnownModLoaderTypes.ADDRESSABLES_LOCAL:
                    break;
                case KnownModLoaderTypes.ADDRESSABLES:
                    break;
                case KnownModLoaderTypes.UMOD:
                    break;
            }

            //createModTab.Q<Button>("CreateMod_CreateButton").enabledSelf = false;
        }
        
        private void SetPathForModLocation(int index)
        {
            string gotPath = EditorUtility.OpenFilePanelWithFilters("Select ModInfoAsset", Application.dataPath, new string[]{ "asset", "asset" });
            if (gotPath.Length == 0) return;
            if (!gotPath.Contains(Application.dataPath))
            {
                Debug.LogError("Mod must be within project.");
                return;
            }
            Debug.Log(gotPath);
            
            var rPath = gotPath.Substring(gotPath.IndexOf("Assets/", StringComparison.Ordinal));
            
            var gotInfoAsset = AssetDatabase.LoadAssetAtPath<BaseModInfoAsset>(rPath);
            if (gotInfoAsset == null)
            {
                Debug.LogError("Invalid Asset.");
                return;
            }

            Undo.RecordObject(configAsset, "Set Path");
            configAsset.modLocations[index].modInfoAsset = gotInfoAsset;
            
            CreateGUI();
        }

        private void AddPathToList()
        {
            Undo.RecordObject(configAsset, "Add Path");
            configAsset.modLocations.Add(new HnSFConfigurationAsset.ModLocationDefinition()
            {
                modInfoAsset = null
            });
            CreateGUI();
        }

        private void WhenCreateConfigFileClicked()
        {
            string gotPath = EditorUtility.SaveFilePanelInProject("Create HnSF Configuration File", "hnsfconfig", "asset", "");
            if (gotPath.Length == 0) return;
            var defaultConfig = ScriptableObject.CreateInstance<HnSFConfigurationAsset>();
            AssetDatabase.CreateAsset(defaultConfig, gotPath);
            AssetDatabase.SaveAssets();
            
            FindConfigurationAsset();
            CreateGUI();
        }
    }
}
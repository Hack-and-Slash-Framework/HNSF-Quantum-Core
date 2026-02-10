using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HnSF
{
    public class FightersManagerWindow : EditorWindow
    {
        [SerializeField] private HnSFConfigurationAsset configAsset = null;
        [SerializeField] private int currentSelectedModIndex;
        [SerializeField] private List<BaseModInfoAsset> allMods = new List<BaseModInfoAsset>();
        [SerializeField] private List<string> allModsName = new List<string>();
        
        [SerializeField] private List<string> addressablesFighterTemplates = new List<string>();
        [SerializeField] private List<string> umodFighterTemplates = new List<string>();

        [SerializeField] private string createFighter_Path = "";
        
        [MenuItem("Tools/HnSF/Fighters Management")]
        public static void OpenWindow()
        {
            var wnd = CreateWindow<FightersManagerWindow>();
            wnd.titleContent = new GUIContent("Fighters Management");
            wnd.minSize = new Vector2(300, 300);
        }
        
        private void FindConfigurationAsset()
        {
            var cAssets = AssetDatabase.FindAssets($"t:{nameof(HnSFConfigurationAsset)}");
            if (cAssets.Length > 0) configAsset = AssetDatabase.LoadAssetAtPath<HnSFConfigurationAsset>(AssetDatabase.GUIDToAssetPath(cAssets[0]));
        }

        private void BuildModsList()
        {
            var modInfoAssetGUIDs = AssetDatabase.FindAssets($"t:{nameof(BaseModInfoAsset)}");
            allMods.Clear();
            allModsName.Clear();
            
            for (int i = 0; i < modInfoAssetGUIDs.Length; i++)
            {
                var modInfoAsset = AssetDatabase.LoadAssetAtPath<BaseModInfoAsset>(AssetDatabase.GUIDToAssetPath(modInfoAssetGUIDs[i]));
                if (modInfoAsset == null) continue;
                allMods.Add(modInfoAsset);
                allModsName.Add(modInfoAsset.ModName);
            }
        }
        
        private void FindFighterTemplates()
        {
            var templates = AssetDatabase.GetSubFolders(configAsset.fighterTemplatesLocation);

            foreach (var templateFolder in templates)
            {
                var splitTemplateString = templateFolder.Split('/');
                if (splitTemplateString[splitTemplateString.Length - 1].ToLower().Contains("addressables"))
                {
                    addressablesFighterTemplates.Add(templateFolder);
                }else if (splitTemplateString[splitTemplateString.Length - 1].ToLower().Contains("umod"))
                {
                    umodFighterTemplates.Add(templateFolder);
                }
            }
        }

        private void CreateGUI()
        {
            var so = new SerializedObject(this);
            rootVisualElement.Clear();
            
            FindConfigurationAsset();
            if (configAsset == null)
            {
                return;
            }

            BuildModsList();
            FindFighterTemplates();
            
            var tabViewScrollView = new ScrollView();
            tabViewScrollView.mode = ScrollViewMode.Horizontal;
            tabViewScrollView.horizontalScrollerVisibility = ScrollerVisibility.Auto;
            tabViewScrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            tabViewScrollView.style.flexGrow = 1;
            rootVisualElement.Add(tabViewScrollView);
            
            var topTabView = new TabView();
            topTabView.style.flexGrow = 1;
            topTabView.Q<VisualElement>(name: "unity-tab-view__header-container").style.marginBottom = 8;
            tabViewScrollView.Add(topTabView);

            var tabFightersList = new Tab("Fighters List");
            tabFightersList.name = "TabFightersList";
            topTabView.Add(tabFightersList);
            CreateTabUI_FightersList(tabFightersList);
            
            var tabCreateFighter = new Tab("Create Fighter");
            tabCreateFighter.name = "TabCreateFighter";
            topTabView.Add(tabCreateFighter);
            CreateTabUI_CreateFighter(tabCreateFighter);
        }

        private void CreateTabUI_FightersList(Tab tabFightersList)
        {
            for (int i = 0; i < allMods.Count; i++)
            {
                var modInfo = allMods[i];
                
                var modFightersListView = new FightersManagerModListViewItem();
                modFightersListView.Bind(modInfo);
                tabFightersList.Add(modFightersListView);
                modFightersListView.onSelectFighter += WhenFighterSelected;
            }
        }

        private void WhenFighterSelected(IFighterDefinition fighterDefinition)
        {
            var w = FighterEditorWindow.OpenWindow(fighterDefinition);
            w.CreateGUI();
        }

        private void CreateTabUI_CreateFighter(Tab tabCreateFighter)
        {
            var so = new SerializedObject(this);
            
            var visualTree = Resources.Load<VisualTreeAsset>("UXML/HnSF_FightersManager_CreateFighterTab");
            visualTree.CloneTree(tabCreateFighter);

            var modDropdown = tabCreateFighter.Q<DropdownField>("ModDropdown");
            modDropdown.choices = allModsName;
            modDropdown.RegisterValueChangedCallback(CreateFighterTab_WhenModSelected);
            
            var templateDropdown = tabCreateFighter.Q<DropdownField>("TemplateDropdown");
            templateDropdown.RegisterValueChangedCallback(CreateFighterTab_WhenTemplateSelected);
            
            var destinationPath = tabCreateFighter.Q<VisualElement>("DestinationPath");
            destinationPath.Q<TextField>().BindProperty(so.FindProperty(nameof(createFighter_Path)));
            destinationPath.Q<Button>().clicked += CreateFighterTab_OpenDestinationPathSetter;
            
            var createButton = tabCreateFighter.Q<Button>("CreateButton");
            createButton.enabledSelf = false;
            createButton.clicked += CreateFighterTab_OnCreateFighterClicked;
            CreateFighterTab_UpdateCreateButton();
        }

        private void CreateFighterTab_UpdateCreateButton()
        {
            var createFighterTab = rootVisualElement.Q<Tab>("TabCreateFighter");
            var modDropdown = createFighterTab.Q<DropdownField>("ModDropdown");
            var templateDropdown = createFighterTab.Q<DropdownField>("TemplateDropdown");
            var createFighterButton = createFighterTab.Q<Button>("CreateButton");

            if (string.IsNullOrEmpty(createFighter_Path) || string.IsNullOrEmpty(modDropdown.value) || string.IsNullOrEmpty(templateDropdown.value))
            {
                createFighterButton.enabledSelf = false;
                return;
            }

            createFighterButton.enabledSelf = true;
        }

        private void CreateFighterTab_OpenDestinationPathSetter()
        {
            var createFighterTab = rootVisualElement.Q<Tab>("TabCreateFighter");
            var modDropdown = createFighterTab.Q<DropdownField>("ModDropdown");
            
            if (string.IsNullOrEmpty(modDropdown.value))
            {
                Debug.LogError("Please select a mod.");
                return;
            }

            var modInfoAssetIndex = allModsName.IndexOf(modDropdown.value);
            if (modInfoAssetIndex == -1 || modInfoAssetIndex >= allMods.Count)
            {
                Debug.LogError("Invalid mod.");
                return;
            }

            var modInfo = allMods[modInfoAssetIndex];
            
            var folderLocation = EditorUtility.OpenFolderPanel("Select Root Fighters Folder", AssetDatabase.GetAssetPath(modInfo), "Fighters");
            Debug.Log(folderLocation);
            Debug.Log(folderLocation.Replace(AssetDatabase.GetAssetPath(modInfo), ""));

            if (folderLocation.Length == 0) return;
            if (!folderLocation.Contains(Application.dataPath))
            {
                Debug.Log("Folder must be within the mod's folder.");
                return;
            }

            createFighter_Path = folderLocation.Substring(folderLocation.IndexOf("Assets/", StringComparison.Ordinal));
            
            CreateFighterTab_UpdateCreateButton();
        }

        private void CreateFighterTab_OnCreateFighterClicked()
        {
            if (EditorApplication.isCompiling) return;
            
            var createFighterTab = rootVisualElement.Q<Tab>("TabCreateFighter");
            var fighterNameField = createFighterTab.Q<TextField>("NameField");
            var modDropdown = createFighterTab.Q<DropdownField>("ModDropdown");
            var templateDropdown = createFighterTab.Q<DropdownField>("TemplateDropdown");
            var createFighterButton = createFighterTab.Q<Button>("CreateButton");

            if (string.IsNullOrEmpty(modDropdown.value))
            {
                Debug.LogError("Invalid mod.");
                return;
            }

            if (string.IsNullOrEmpty(templateDropdown.value) || !AssetDatabase.AssetPathExists(templateDropdown.value))
            {
                Debug.LogError("Invalid template folder.");
                return;
            }

            createFighterButton.enabledSelf = false;

            var trueDestinationPath = createFighter_Path + $"/{fighterNameField.value.Replace(" ", "")}";

            if (!AssetDatabase.AssetPathExists(trueDestinationPath))
                AssetDatabase.CreateFolder(createFighter_Path, $"{fighterNameField.value.Replace(" ", "")}");

            if (AssetDatabase.FindAssets("", new string[] { trueDestinationPath }).Length > 0)
            {
                Debug.LogError("Fighter of that name already exists at the destination.");
                createFighterButton.enabledSelf = true;
                return;
            }
            
            // Deep Copy
            var templatePath = Application.dataPath.Replace("Assets", "") + templateDropdown.value;
            var destinationPath = Application.dataPath.Replace("Assets", "") + trueDestinationPath;
            DeepCopyUtility.CopyDirectoryDeep(templatePath, destinationPath);
            AssetDatabase.SaveAssets();
            createFighterButton.enabledSelf = true;
            
            foreach (var a in AssetDatabase.FindAssets($"t:{nameof(IFighterDefinition)}", new string[] { trueDestinationPath }))
            {
                if(string.IsNullOrEmpty(a)) continue;
                var assetPath = AssetDatabase.GUIDToAssetPath(a);
                var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(IFighterDefinition));
                if(asset == null) continue;
                
                if (asset is AddressablesFighterDefinition afd)
                {
                    afd.fighterName = fighterNameField.value;
                }
#if HNSF_UMOD
                else if (asset is UModFighterDefinition umfd)
                {
                    umfd.fighterName = fighterNameField.value;
                }
#endif
                
                EditorUtility.SetDirty(asset);
            }
        }

        private void CreateFighterTab_WhenTemplateSelected(ChangeEvent<string> evt)
        {
            CreateFighterTab_UpdateCreateButton();
        }

        private void CreateFighterTab_WhenModSelected(ChangeEvent<string> evt)
        {
            var modInfoIndex = allModsName.IndexOf(evt.newValue);
            if (modInfoIndex == -1) return;
            var mia = allMods[modInfoIndex];
            if (mia == null) return;
            
            var templateDropdown = rootVisualElement.Q<DropdownField>("TemplateDropdown");
            templateDropdown.index = -1;
            if (mia is AddressablesModInfoAsset)
            {
                templateDropdown.choices = addressablesFighterTemplates;
            }
#if HNSF_UMOD
            else if (mia is UModModInfoAsset)
            {
                templateDropdown.choices = umodFighterTemplates;
            }
#endif

            var assetPath = AssetDatabase.GetAssetPath(mia);
            var rPath = assetPath.Substring(assetPath.IndexOf("Assets/", StringComparison.Ordinal));
            var splitPath = rPath.Split('/');
            splitPath[splitPath.Length - 1] = "Fighters";
            
            createFighter_Path = string.Join("/", splitPath);
            
            CreateFighterTab_UpdateCreateButton();
        }
    }
}

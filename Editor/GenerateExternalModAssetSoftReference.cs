using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace HnSF
{
    public class GenerateExternalModAssetSoftReference : EditorWindow
    {
        private const string ExternalModAssetsPathPrefKey = "HnSF.GenerateExternalReferences.ExternalModAssetsPath";
        private const string BaseModInfoAssetPathPrefKey = "HnSF.GenerateExternalReferences.BaseModInfoAssetPath";

        private string _externalModAssetsPath = "Assets";
        private string _baseModInfoAssetPath = string.Empty;
        private Vector2 _scrollPosition;
        private string _lastResult;

        [MenuItem("Tools/HnSF/Generate External References")]
        public static void ShowWindow()
        {
            GetWindow(typeof(GenerateExternalModAssetSoftReference), false, "Generate External References");
        }

        private void OnEnable()
        {
            _externalModAssetsPath = EditorPrefs.GetString(ExternalModAssetsPathPrefKey, _externalModAssetsPath);
            _baseModInfoAssetPath = EditorPrefs.GetString(BaseModInfoAssetPathPrefKey, _baseModInfoAssetPath);
        }

        private void OnDisable()
        {
            SavePaths();
        }

        public virtual void OnGUI()
        {
            EditorGUILayout.LabelField("External Mod Asset References", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawProjectFolderField(
                "ExternalModAssets Path",
                "Folder where generated ExternalModAssetSoftReference assets will be created.",
                ref _externalModAssetsPath);

            DrawProjectAssetFileField(
                "BaseModInfoAsset",
                "Asset file whose folder will be searched for assets inheriting from IContentDefinition.",
                ref _baseModInfoAssetPath);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!PathsAreValid()))
            {
                if (GUILayout.Button("Generate References"))
                {
                    GenerateReferences();
                }
            }

            if (!string.IsNullOrEmpty(_lastResult))
            {
                EditorGUILayout.Space();
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MinHeight(80));
                EditorGUILayout.HelpBox(_lastResult, MessageType.Info);
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawProjectFolderField(string label, string tooltip, ref string path)
        {
            EditorGUILayout.LabelField(new GUIContent(label, tooltip));
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            path = EditorGUILayout.TextField(path);
            if (EditorGUI.EndChangeCheck())
            {
                SavePaths();
            }

            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                var selectedPath = EditorUtility.OpenFolderPanel(label, Application.dataPath, string.Empty);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    path = ConvertToAssetPath(selectedPath);
                    SavePaths();
                    GUI.FocusControl(null);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (!IsValidAssetFolder(path))
            {
                EditorGUILayout.HelpBox("Path must be an existing folder inside this project's Assets folder.", MessageType.Warning);
            }
        }

        private void DrawProjectAssetFileField(string label, string tooltip, ref string path)
        {
            EditorGUILayout.LabelField(new GUIContent(label, tooltip));
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            path = EditorGUILayout.TextField(path);
            if (EditorGUI.EndChangeCheck())
            {
                SavePaths();
            }

            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                var selectedPath = EditorUtility.OpenFilePanel(label, Application.dataPath, "asset");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    path = ConvertToAssetPath(selectedPath);
                    SavePaths();
                    GUI.FocusControl(null);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (!IsValidBaseModInfoAssetPath(path))
            {
                EditorGUILayout.HelpBox("Path must be a BaseModInfoAsset asset file inside this project's Assets folder.", MessageType.Warning);
            }
        }

        private void SavePaths()
        {
            EditorPrefs.SetString(ExternalModAssetsPathPrefKey, _externalModAssetsPath);
            EditorPrefs.SetString(BaseModInfoAssetPathPrefKey, _baseModInfoAssetPath);
        }

        private bool PathsAreValid()
        {
            return IsValidAssetFolder(_externalModAssetsPath)
                   && IsValidBaseModInfoAssetPath(_baseModInfoAssetPath);
        }

        private bool IsValidAssetFolder(string path)
        {
            return !string.IsNullOrWhiteSpace(path)
                   && path.StartsWith("Assets", StringComparison.Ordinal)
                   && AssetDatabase.IsValidFolder(path);
        }

        private bool IsValidBaseModInfoAssetPath(string path)
        {
            return !string.IsNullOrWhiteSpace(path)
                   && path.StartsWith("Assets", StringComparison.Ordinal)
                   && AssetDatabase.LoadAssetAtPath<BaseModInfoAsset>(path) != null;
        }

        private string ConvertToAssetPath(string absolutePath)
        {
            var normalizedPath = absolutePath.Replace('\\', '/');
            var dataPath = Application.dataPath.Replace('\\', '/');

            if (string.Equals(normalizedPath, dataPath, StringComparison.OrdinalIgnoreCase))
            {
                return "Assets";
            }

            if (normalizedPath.StartsWith(dataPath + "/", StringComparison.OrdinalIgnoreCase))
            {
                return "Assets" + normalizedPath.Substring(dataPath.Length);
            }

            return absolutePath;
        }

        private void GenerateReferences()
        {
            var modInfoAsset = AssetDatabase.LoadAssetAtPath<BaseModInfoAsset>(_baseModInfoAssetPath);
            if (modInfoAsset == null)
            {
                _lastResult = "Could not load BaseModInfoAsset.";
                return;
            }

            var modId = modInfoAsset.ModID;
            if (string.IsNullOrEmpty(modId))
            {
                _lastResult = "The selected BaseModInfoAsset has an empty ModID.";
                return;
            }

            var createdCount = 0;
            var updatedCount = 0;
            var skippedCount = 0;
            var skippedMissingAddressableCount = 0;
            var contentSearchPath = Path.GetDirectoryName(_baseModInfoAssetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(contentSearchPath))
            {
                _lastResult = "Could not determine the BaseModInfoAsset folder.";
                return;
            }

            var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { contentSearchPath });

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var contentDefinition = AssetDatabase.LoadAssetAtPath<IContentDefinition>(assetPath);
                if (contentDefinition == null)
                {
                    skippedCount++;
                    continue;
                }

                var addressableName = GetAddressableName(guid);
                if (string.IsNullOrEmpty(addressableName))
                {
                    skippedMissingAddressableCount++;
                    continue;
                }

                var referencePath = AssetDatabase.GenerateUniqueAssetPath(
                    Path.Combine(_externalModAssetsPath, $"{SanitizeFileName(contentDefinition.name)} Reference.asset")
                        .Replace('\\', '/'));

                var existingReference = FindExistingReference(assetPath, addressableName);
                if (existingReference != null)
                {
                    existingReference.reference = new ModAssetSoftReference(modId, addressableName, false);
                    EditorUtility.SetDirty(existingReference);
                    updatedCount++;
                    continue;
                }

                var reference = CreateInstance<ExternalModAssetSoftReference>();
                reference.reference = new ModAssetSoftReference(modId, addressableName, false);
                AssetDatabase.CreateAsset(reference, referencePath);
                createdCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _lastResult = $"Searched {contentSearchPath} for content definitions using mod ID {modId}.\n" +
                          $"Created {createdCount} reference assets.\nUpdated {updatedCount} existing reference assets.\nS" +
                          $"kipped {skippedCount} non-content ScriptableObjects.\n" +
                          $"Skipped {skippedMissingAddressableCount} content assets without an Addressables entry.";
        }

        private ExternalModAssetSoftReference FindExistingReference(string assetPath, string addressableName)
        {
            var referenceGuids = AssetDatabase.FindAssets("t:ExternalModAssetSoftReference", new[] { _externalModAssetsPath });
            foreach (var guid in referenceGuids)
            {
                var referencePath = AssetDatabase.GUIDToAssetPath(guid);
                var reference = AssetDatabase.LoadAssetAtPath<ExternalModAssetSoftReference>(referencePath);
                if (reference != null
                    && (reference.reference.assetID == addressableName
                        || reference.reference.assetID == assetPath))
                {
                    return reference;
                }
            }

            return null;
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            }

            return fileName;
        }
        
        // Addressables
        private string GetAddressableName(string guid)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entry = settings != null ? settings.FindAssetEntry(guid) : null;
            return entry?.address;
        }
    }
}

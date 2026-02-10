using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace HnSF
{
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX || UNITY_EMBEDDED_LINUX || UNITY_SERVER
    public static partial class FileSaveLoadService
    {
        public static bool SaveTextFile(string path, string text)
        {
            var basePath = new Uri(Application.persistentDataPath);
            var filePathUri = new Uri(Path.Combine(Application.persistentDataPath, path));
            var filePath = Uri.UnescapeDataString(filePathUri.AbsolutePath);
            if (!basePath.IsBaseOf(filePathUri)) return false;
            if (!filePathUri.IsFile) return false;

            try
            {
                using var streamWriter = File.CreateText(filePath);
                streamWriter.Write(text);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving text file: {e}");
                return false;
            }
            return true;
        }

        public static string LoadTextFile(string path)
        {
            var basePath = new Uri(Application.persistentDataPath);
            var filePathUri = new Uri(Path.Combine(Application.persistentDataPath, path));
            var filePath = Uri.UnescapeDataString(filePathUri.AbsolutePath);
            if (!basePath.IsBaseOf(filePathUri)) return "";
            if (!filePathUri.IsFile) return "";
            
            try
            {
                using var streamReader = File.OpenText(filePath);
                return streamReader.ReadToEnd();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading text file: {e}");
                return "";
            }
        }

        public static bool SaveFileAsJson<T>(string path, T obj, bool prettyPrint = false)
        {
            var basePath = new Uri(Application.persistentDataPath);
            var filePathUri = new Uri(Path.Combine(Application.persistentDataPath, path));
            var filePath = Uri.UnescapeDataString(filePathUri.AbsolutePath);
            if (!basePath.IsBaseOf(filePathUri)) return false;
            if (!filePathUri.IsFile) return false;

            try
            {
                var objAsJson = JsonConvert.SerializeObject(obj, prettyPrint ? Formatting.Indented : Formatting.None);
                using var streamWriter = File.CreateText(filePath);
                streamWriter.Write(objAsJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving file as json: {e}");
                return false;
            }
            return true;
        }

        public static T LoadFileFromJson<T>(string path)
        {
            var basePath = new Uri(Application.persistentDataPath);
            var filePathUri = new Uri(Path.Combine(Application.persistentDataPath, path));
            var filePath = Uri.UnescapeDataString(filePathUri.AbsolutePath);
            if (!basePath.IsBaseOf(filePathUri)) return default;
            if (!filePathUri.IsFile || !File.Exists(filePath)) return default;

            try
            {
                string jsonString;
                using (var streamReader = File.OpenText(filePath))
                {
                    jsonString = streamReader.ReadToEnd();
                }
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading file from json: {e}");
                return default;
            }
        }

        public static bool TryLoadFileFromJson<T>(string path, out T file)
        {
            file = default;
            var basePath = new Uri(Application.persistentDataPath);
            var filePathUri = new Uri(Path.Combine(Application.persistentDataPath, path));
            var filePath = Uri.UnescapeDataString(filePathUri.AbsolutePath);
            if (!basePath.IsBaseOf(filePathUri)) return false;
            if (!filePathUri.IsFile || !File.Exists(filePath)) return false;
            
            try
            {
                string jsonString;
                using (var streamReader = File.OpenText(filePath))
                {
                    jsonString = streamReader.ReadToEnd();
                }
                file = JsonConvert.DeserializeObject<T>(jsonString);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading file from json: {e}");
                return false;
            }
        }
    }
#endif
}
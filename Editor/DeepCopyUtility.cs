using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quantum.Editor;
using UnityEditor;

namespace HnSF
{
    public static class DeepCopyUtility
    {
        public static void CopyDirectoryDeep(string sourcePath, string destinationPath)
        {
            CopyDirectoryRecursively(sourcePath, destinationPath);
            
            List<string> nonmetaFiles = GetFilesRecursively(destinationPath, (f) => !f.EndsWith(".meta"));
            List<string> metaFiles = GetFilesRecursively(destinationPath, (f) => f.EndsWith(".meta"));
            List<(string originalQuantumIdentifier, string newQuantumIdentifier)> quantumIdTable =
                new List<(string originalQuantumIdentifier, string newQuantumIdentifier)>();
            List<(string originalGuid, string newGuid)> guidTable = new List<(string originalGuid, string newGuid)>();
            
            Dictionary<string, string> filePathToNewGuid = new Dictionary<string, string>();

            foreach (string metaFile in metaFiles)
            {
                StreamReader file = new StreamReader(metaFile);
                file.ReadLine();
                string guidLine = file.ReadLine();
                file.Close();
                string originalGuid = guidLine.Substring(6, guidLine.Length - 6);
                string newGuid = UnityEngine.GUID.Generate().ToString().Replace("-", "");
                guidTable.Add((originalGuid, newGuid));
                
                filePathToNewGuid.Add(Path.ChangeExtension(metaFile, "").Replace(".asset", ""), newGuid);
                //Debug.Log($"{metaFile} GUID: {originalGuid}, Quantum Identifier: {QuantumUnityDBUtilities.GetExpectedAssetGuid(new GUID(originalGuid), (long)11400000, out _)}");
            }

            foreach (string nonmetaFile in nonmetaFiles)
            {
                if(!nonmetaFile.EndsWith(".asset")) continue;
                
                StreamReader file = new StreamReader(nonmetaFile);
                string oldQuantumAssetId = file.ReadLine();
                while(oldQuantumAssetId != null && !oldQuantumAssetId.Contains("Identifier:")) oldQuantumAssetId = file.ReadLine();
                file.ReadLine();
                if (oldQuantumAssetId == null)
                {
                    file.Close();
                    continue;
                }
                file.ReadLine();
                file.ReadLine();
                oldQuantumAssetId = file.ReadLine();
                file.Close();
                
                if(oldQuantumAssetId == null || !oldQuantumAssetId.Contains("Value: ")) continue;
                oldQuantumAssetId = oldQuantumAssetId.Replace("Value: ", "");
                oldQuantumAssetId = oldQuantumAssetId.Replace(" ", "");

                var pathNoExtension = Path.ChangeExtension(nonmetaFile, "");
                //Debug.Log(pathNoExtension);
                if (!filePathToNewGuid.TryGetValue(pathNoExtension, out var value)) continue;
                var newQuantumAssetId = QuantumUnityDBUtilities.GetExpectedAssetGuid(new UnityEngine.GUID(value), (long)11400000, out _);
                //Debug.Log($": {oldQuantumAssetId} to {newQuantumAssetId.Value}");
                
                quantumIdTable.Add((oldQuantumAssetId, newQuantumAssetId.Value.ToString()));
            }

            List<string> allFiles = GetFilesRecursively(destinationPath);

            foreach (string fileToModify in allFiles)
            {
                string content = File.ReadAllText(fileToModify);

                foreach (var guidPair in guidTable)
                {
                    content = content.Replace(guidPair.originalGuid, guidPair.newGuid);
                }

                foreach (var quantumAssetIdPair in quantumIdTable)
                {
                    content = content.Replace($"Value: {quantumAssetIdPair.originalQuantumIdentifier}", $"Value: {quantumAssetIdPair.newQuantumIdentifier}");
                }

                File.WriteAllText(fileToModify, content);
            }

            AssetDatabase.Refresh();
        }

        private static void CopyDirectoryRecursively(string sourceDirName, string destDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                CopyDirectoryRecursively(subdir.FullName, temppath);
            }
        }

        private static List<string> GetFilesRecursively(string path, Func<string, bool> criteria = null, List<string> files = null)
        {
            if (files == null)
            {
                files = new List<string>();
            }

            files.AddRange(Directory.GetFiles(path).Where(f =>criteria == null || criteria(f)));

            foreach (string directory in Directory.GetDirectories(path))
            {
                GetFilesRecursively(directory, criteria, files);
            }

            return files;
        }

        public static T DeepCopyReflection<T>(T input)
        {
            var ignoredFields = new string[] { "identifier", "guid", "path" };
            var type = input.GetType();
            var properties = type.GetProperties();
            T clonedObj = (T)Activator.CreateInstance(type);
            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    object value = property.GetValue(input);
                    if (ignoredFields.Contains(property.Name.ToLower())) continue;
                
                    if (value != null && value.GetType().IsClass && !value.GetType().FullName.StartsWith("System."))
                    {
                        property.SetValue(clonedObj, DeepCopyReflection(value));
                    }
                    else
                    {
                        property.SetValue(clonedObj, value);
                    }
                }
            }
            return clonedObj;
        }
    }
}
using System.IO;
using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    public class GeNaEditorUpgrader : AssetPostprocessor
    {
        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            PerformUpgradeOperation(); 
        }

        public static void PerformUpgradeOperation()
        {
            if (Application.isPlaying)
                return;

            if (Directory.Exists(GetAssetPath("Dev Utilities")))
            {
                return;
            }

            string filePath = "Assets/Procedural Worlds/GeNa/Scripts/GeNaMaintenanceToken.dat";
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(filePath);
            if (asset == null)
            {
                return;
            }

            var spawners = Resources.FindObjectsOfTypeAll<GeNaSpawner>();
            int count = 0;
            foreach (var spawner in spawners)
            {
                EditorUtility.DisplayProgressBar("Processing GeNa Update", "Updating all your project spawners to the new GeNa version, this may take a moment...", (float)count / spawners.Length);
                if (spawner == null)
                    continue;
                spawner.Load();
                if (GeNaEditorUtility.IsPrefab(spawner.gameObject))
                {
                    var dataFile = spawner.GetDataFile();
                    if (dataFile != null)
                        continue;
                    dataFile = GeNaEditorUtility.GetMainDataBufferScriptable(spawner);
                    if (dataFile == null)
                        continue;
                    spawner.ConvertToFile(dataFile);
                }

                count++;
            }
            EditorUtility.ClearProgressBar();

            AssetDatabase.DeleteAsset(filePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="fileName">File name to search for</param>
        /// <returns></returns>
        private static string GetAssetPath(string fileName)
        {
            string fName = Path.GetFileNameWithoutExtension(fileName);
            string[] assets = AssetDatabase.FindAssets(fName, null);
            for (int idx = 0; idx < assets.Length; idx++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[idx]);
                if (Path.GetFileName(path) == fileName)
                {
                    return path;
                }
            }
            return "";
        }
    }
}
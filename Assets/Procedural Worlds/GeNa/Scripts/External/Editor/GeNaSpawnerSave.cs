using System.IO;
using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    public class GeNaSpawnerSave : EditorWindow
    {
        //[MenuItem("Window/GeNaSpawnerSave")]
        private static void ShowWindow()
        {
            GeNaSpawnerSave window = GetWindow<GeNaSpawnerSave>();
            window.titleContent = new GUIContent("GeNa Spawner Save");
            window.Show();
        }

        private int version = 1;
        private void OnGUI()
        {
            version = EditorGUILayout.IntField("Version", version);
            if (GUILayout.Button("Upgrade all GeNa Spawners"))
            {
                // find all the decor related prefabs
                string[] allPrefabs = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
                foreach(string prefabFile in allPrefabs)
                {
                    string assetPath = "Assets" + prefabFile.Replace(Application.dataPath, "").Replace('\\', '/');
                    GameObject gameObject = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
                    GeNaSpawner geNaSpawner = gameObject.GetComponent<GeNaSpawner>();
                    if (geNaSpawner != null)
                    {
                        geNaSpawner.Upgrade(version, new DataBufferScriptable());
                        EditorUtility.SetDirty(geNaSpawner);
                    }
                    // .. do whatever you like
                }
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("Refresh All SubSpawner Decorators"))
            {
                // find all the decor related prefabs
                string[] allPrefabs = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
                foreach(string prefabFile in allPrefabs)
                {
                    string assetPath = "Assets" + prefabFile.Replace(Application.dataPath, "").Replace('\\', '/');
                    GameObject gameObject = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
                    GeNaSubSpawnerDecorator subSpawnerDecorator = gameObject.GetComponent<GeNaSubSpawnerDecorator>();
                    if (subSpawnerDecorator != null)
                    {
                        subSpawnerDecorator.Refresh();
                        EditorUtility.SetDirty(subSpawnerDecorator);
                    }
                    // .. do whatever you like
                }
                AssetDatabase.SaveAssets();
            }
        }
    }
}
using UnityEngine;
using UnityEditor;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaSubSpawnerDecorator))]
    public class GeNaSubSpawnerDecoratorEditor : GeNaDecoratorEditor<GeNaSubSpawnerDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Sub Spawner Decorator")]
        public static void AddDecorator(MenuCommand command)
        {
            GameObject gameObject = command.context as GameObject;
            if (gameObject != null)
            {
                GeNaSubSpawnerDecorator decorator = gameObject.AddComponent<GeNaSubSpawnerDecorator>();
                GeNaDecoratorEditorUtility.RegisterDecorator(gameObject, decorator);
            }
        }
        protected override void RenderPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                GeNaSpawner spawnerPrefab = Decorator.SubSpawner;
                EditorGUI.BeginChangeCheck();
                {
                    spawnerPrefab = EditorUtils.ObjectField("Sub Spawner", spawnerPrefab, typeof(GeNaSpawner), false, helpEnabled) as GeNaSpawner;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (spawnerPrefab != null)
                    {
                        if (GeNaEditorUtility.IsPrefab(spawnerPrefab.gameObject))
                        {
                            GeNaSpawner subSpawner = GeNaEditorUtility.GetPrefabAsset(spawnerPrefab.gameObject).GetComponent<GeNaSpawner>();
                            if (subSpawner != null)
                            {
                                subSpawner.Deserialize();
                                Decorator.gameObject.name = $"Sub Spawner - {subSpawner.SpawnerData.Name}";
                            }
                            Decorator.SubSpawner = subSpawner;
                        }
                        else
                            GeNaDebug.LogWarning("Spawner must be a Prefab!");
                    }
                    else
                    {
                        Decorator.SubSpawner = spawnerPrefab;
                    }
                }
                if (Decorator.SubSpawner != null)
                {
                    GeNaSpawner subSpawner = Decorator.SubSpawner;
                    if (subSpawner.SpawnerData == null)
                        subSpawner.Deserialize();
                    GeNaSpawnerData spawnerData = subSpawner.SpawnerData;
                    EditorGUI.indentLevel++;
                    SpawnerSettings settings = spawnerData.Settings;
                    settings.MaxSubSpawnerDepth = EditorUtils.IntField("Max Sub Spawner Depth", settings.MaxSubSpawnerDepth, helpEnabled);
                    EditorGUI.indentLevel--;
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object @object in targets)
                {
                    if (@object is GeNaSubSpawnerDecorator decorator)
                    {
                        GeNaSpawner subSpawner = decorator.SubSpawner;
                        decorator.SubSpawner = subSpawner;
                        if (subSpawner != null)
                            decorator.SubSpawner.SpawnerData.Settings.MaxSubSpawnerDepth = subSpawner.SpawnerData.Settings.MaxSubSpawnerDepth;

                        EditorUtility.SetDirty(@object);
                    }
                }
            }
        }
        public override void OnSceneGUI()
        {
            GeNaSubSpawnerDecorator decorator = target as GeNaSubSpawnerDecorator;
            if (decorator == null)
                return;
            Transform transform = decorator.transform;
            GeNaSpawner spawner = decorator.SubSpawner;
            if (spawner == null)
                return;
            GeNaSpawnerData spawnerData = spawner.SpawnerData;
            Vector3 position = transform.position;
            GeNaEditorUtility.RenderSpawnRange(spawnerData, position);
        }
    }
}
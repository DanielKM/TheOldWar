using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaPhysicsDecorator))]
    public class GeNaPhysicsDecoratorEditor : GeNaDecoratorEditor<GeNaPhysicsDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Physics Decorator")]
        public static void AddDecorator(MenuCommand command)
        {
            GameObject gameObject = command.context as GameObject;
            if (gameObject != null)
            {
                GeNaPhysicsDecorator decorator = gameObject.AddComponent<GeNaPhysicsDecorator>();
                GeNaDecoratorEditorUtility.RegisterDecorator(gameObject, decorator);
            }
        }
        protected override void RenderPanel(bool helpEnabled)
        {
            PhysicsSimulatorSettings settings = Decorator.Settings;
            EditorGUI.BeginChangeCheck();
            {
                settings.Iterations = EditorUtils.IntField("Iterations", settings.Iterations, helpEnabled);
                settings.StepSize = EditorUtils.Slider("Step Size", settings.StepSize, 0.01f, 0.1f, helpEnabled);
                settings.SpawnOffsetY = EditorUtils.Slider("Spawn Offset Y", settings.SpawnOffsetY, -5f, 5f, helpEnabled);
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object @object in targets)
                {
                    if (@object is GeNaPhysicsDecorator decorator)
                    {
                        PhysicsSimulatorSettings otherSettings = decorator.Settings;
                        otherSettings.Iterations = settings.Iterations;
                        otherSettings.StepSize = settings.StepSize;
                        otherSettings.SpawnOffsetY = settings.SpawnOffsetY;
                        EditorUtility.SetDirty(@object);
                    }
                }
            }
        }
    }
}
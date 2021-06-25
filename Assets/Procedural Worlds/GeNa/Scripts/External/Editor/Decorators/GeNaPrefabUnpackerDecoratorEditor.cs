using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaPrefabUnpackerDecorator))]
    public class GeNaPrefabUnpackerDecoratorEditor : GeNaDecoratorEditor<GeNaPrefabUnpackerDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Prefab Unpacker Decorator")]
        public static void AddDecorator(MenuCommand command)
        {
            GameObject gameObject = command.context as GameObject;
            if (gameObject != null)
            {
                GeNaPrefabUnpackerDecorator decorator = gameObject.AddComponent<GeNaPrefabUnpackerDecorator>();
                GeNaDecoratorEditorUtility.RegisterDecorator(gameObject, decorator);
            }
        }
        protected override void RenderPanel(bool helpEnabled)
        {
            EditorUtils.InlineHelp("PrefabUnpackHelp", helpEnabled);

            // EditorGUI.BeginChangeCheck();
            // {
            //     Decorator.Enabled = EditorUtils.Toggle("Enabled", Decorator.Enabled, helpEnabled);
            // }
            // if (EditorGUI.EndChangeCheck())
            // {
            //     EditorUtility.SetDirty(Decorator);
            //     foreach (var o in targets)
            //     {
            //         GeNaPrefabUnpackerDecorator prefabUnpackerDecorator = (GeNaPrefabUnpackerDecorator) o;
            //         prefabUnpackerDecorator.Enabled = Decorator.Enabled;
            //     }
            // }
        }
    }
}
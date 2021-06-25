using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaTerrainTreeDecorator))]
    public class GeNaTerrainTreeDecoratorEditor : GeNaDecoratorEditor<GeNaTerrainTreeDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Terrain Tree Decorator")]
        public static void AddDecorator(MenuCommand command)
        {
            GameObject gameObject = command.context as GameObject;
            if (gameObject != null)
            {
                GeNaTerrainTreeDecorator decorator = gameObject.AddComponent<GeNaTerrainTreeDecorator>();
                GeNaDecoratorEditorUtility.RegisterDecorator(gameObject, decorator);
            }
        }
        protected override void RenderPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            Decorator.Enabled = EditorUtils.Toggle("Enabled", Decorator.Enabled, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object o in targets)
                {
                    GeNaTerrainTreeDecorator decorator = (GeNaTerrainTreeDecorator) o;
                    decorator.Enabled = Decorator.Enabled;
                    EditorUtility.SetDirty(o);
                }
            }
        }
    }
}
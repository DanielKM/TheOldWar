using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
#if GAIA_2_PRESENT
    [CanEditMultipleObjects]
    [CustomEditor((typeof(GeNaGaiaDecorator)))]
    public class GeNaGaiaDecoratorEditor : GeNaDecoratorEditor<GeNaGaiaDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Gaia Decorator")]
        public static void AddDecorator(MenuCommand command)
        {
            GameObject gameObject = command.context as GameObject;
            if (gameObject != null)
            {
                GeNaGaiaDecorator decorator = gameObject.AddComponent<GeNaGaiaDecorator>();
                GeNaDecoratorEditorUtility.RegisterDecorator(gameObject, decorator);
            }
        }
        protected override void RenderPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            Decorator.Enabled = EditorUtils.Toggle("Enabled", Decorator.Enabled, helpEnabled);
            if (Decorator.Enabled)
            {
                EditorGUI.indentLevel++;
                Decorator.GetSeaLevel = EditorUtils.Toggle("Get Sea Level", Decorator.GetSeaLevel, helpEnabled);
                if (Decorator.GetSeaLevel)
                {
                    EditorGUI.indentLevel++;
                    Decorator.ExtraSeaLevel = EditorUtils.FloatField("ExtraSeaLevel", Decorator.ExtraSeaLevel, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object o in targets)
                {
                    GeNaGaiaDecorator gaiaDecorator = (GeNaGaiaDecorator) o;
                    gaiaDecorator.Enabled = Decorator.Enabled;
                    gaiaDecorator.GetSeaLevel = Decorator.GetSeaLevel;
                    gaiaDecorator.ExtraSeaLevel = Decorator.ExtraSeaLevel;
                    EditorUtility.SetDirty(gaiaDecorator);
                }
            }
        }
    }
#endif
}
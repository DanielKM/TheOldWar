using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaChanceOfDecorator))]
    public class GeNaChanceOfDecoratorEditor : GeNaDecoratorEditor<GeNaChanceOfDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Chance Of Decorator")]
        public static void AddDecorator(MenuCommand command)
        {
            GameObject gameObject = command.context as GameObject;
            if (gameObject != null)
            {
                GeNaChanceOfDecorator decorator = gameObject.AddComponent<GeNaChanceOfDecorator>();
                GeNaDecoratorEditorUtility.RegisterDecorator(gameObject, decorator);
            }
        }
        
        protected override void RenderPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                Decorator.SuccessRate = EditorUtils.Slider("Res Success", Decorator.SuccessRate, 0.0f, 1.0f, helpEnabled);
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object @object in targets)
                {
                    if (@object is GeNaChanceOfDecorator decorator)
                    {
                        decorator.SuccessRate = Decorator.SuccessRate;
                        EditorUtility.SetDirty(decorator);
                    }
                }
            }
        }
    }
} 
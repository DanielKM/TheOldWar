using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaOneChildOfDecorator))]
    public class GeNaOneChildOfDecoratorEditor : GeNaDecoratorEditor<GeNaOneChildOfDecorator>
    {
        protected override void OnEnable()
        {
            base.OnEnable(); 
            if (Decorator != null)
                Decorator.RefreshChildren();
        }
        [MenuItem("GameObject/GeNa/Decorators/Child Of Decorator")]
        public static void AddDecorator(MenuCommand command)
        {
            GameObject gameObject = command.context as GameObject;
            if (gameObject != null)
            {
                GeNaOneChildOfDecorator decorator = gameObject.AddComponent<GeNaOneChildOfDecorator>();
                GeNaDecoratorEditorUtility.RegisterDecorator(gameObject, decorator);
            }
        }
        protected override void RenderPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                List<GeNaOneChildOfDecorator.Child> children = Decorator.Children;
                if (children.Count > 0)
                {
                    EditorUtils.LabelField("Weights", helpEnabled);
                    EditorGUI.indentLevel++;
                    foreach (GeNaOneChildOfDecorator.Child child in children)
                        child.weight = EditorGUILayout.Slider(child.transform.name, child.weight, 0.0f, 1.0f);
                    EditorGUI.indentLevel--;
                }
                else
                    GUILayout.Label("There are no Children attached to this GameObject.");
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(Decorator);
            }
        }
    }
}
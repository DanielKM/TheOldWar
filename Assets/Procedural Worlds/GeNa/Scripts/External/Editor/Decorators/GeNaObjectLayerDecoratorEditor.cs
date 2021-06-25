using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaObjectLayerDecorator))]
    public class GeNaObjectLayerDecoratorEditor : GeNaDecoratorEditor<GeNaObjectLayerDecorator>
    {
        private bool m_setLayer = false;
        [MenuItem("GameObject/GeNa/Decorators/Object Layer Decorator")]
        public static void AddDecorator(MenuCommand command)
        {
            GameObject gameObject = command.context as GameObject;
            if (gameObject != null)
            {
                GeNaObjectLayerDecorator decorator = gameObject.AddComponent<GeNaObjectLayerDecorator>();
                GeNaDecoratorEditorUtility.RegisterDecorator(gameObject, decorator);
            }
        }

        protected override void RenderPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                Decorator.Tags = GetTags();
                Decorator.TagIndex = EditorGUILayout.Popup(new GUIContent(EditorUtils.GetTextValue("ObjectTag"), EditorUtils.GetTooltip("ObjectTag")), Decorator.TagIndex, Decorator.Tags);
                EditorUtils.InlineHelp("ObjectTag", helpEnabled);
                Decorator.LayerMask = EditorGUILayout.Popup(new GUIContent(EditorUtils.GetTextValue("ObjectLayerMask"), EditorUtils.GetTooltip("ObjectLayerMask")), Decorator.LayerMask, GetLayerNames());
                EditorUtils.InlineHelp("ObjectLayerMask", helpEnabled);
                Decorator.ApplyToChildern = EditorUtils.Toggle("ApplyToChildren", Decorator.ApplyToChildern);
                EditorUtils.InlineHelp("ApplyToChildren", helpEnabled);
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object @object in targets)
                {
                    if (@object is GeNaObjectLayerDecorator decorator)
                    {
                        decorator.Tags = Decorator.Tags;
                        decorator.TagIndex = Decorator.TagIndex;
                        decorator.LayerMask = Decorator.LayerMask;
                        decorator.ApplyToChildern = Decorator.ApplyToChildern;
                        EditorUtility.SetDirty(decorator);
                    }
                }
            }

            if (m_setLayer)
            {
                Decorator.m_layerName = LayerMask.LayerToName(Decorator.LayerMask);
                m_setLayer = true;
            }
        }
        private string[] GetLayerNames()
        {
            List<string> layers = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                if (!string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                {
                    layers.Add(LayerMask.LayerToName(i));
                }
                else
                {
                    layers.Add("Empty Layer Mask (Do Not Select)");
                }
            }
            return layers.ToArray();
        }
        private static string[] GetTags()
        {
            string[] tags = InternalEditorUtility.tags;
            List<string> newTags = new List<string> {"Current"};
            foreach (string tag in tags)
            {
                newTags.Add(tag);
            }
            return newTags.ToArray();
        }
    }
}
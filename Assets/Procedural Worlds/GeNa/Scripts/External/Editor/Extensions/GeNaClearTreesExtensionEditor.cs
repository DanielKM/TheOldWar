using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaClearTreesExtension))]
    public class GeNaClearTreesExtensionEditor : GeNaSplineExtensionEditor
    {
        private void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!GeNaEditorUtility.ValidateComputeShader())
            {
                Color guiColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                EditorGUILayout.BeginVertical(Styles.box);
                m_editorUtils.Text("NoComputeShaderHelp");
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = guiColor;
                GUI.enabled = false;
            }
            GeNaClearTreesExtension clearTreesExtension = target as GeNaClearTreesExtension;
            clearTreesExtension.Width = m_editorUtils.FloatField("Width", clearTreesExtension.Width, HelpEnabled);
            clearTreesExtension.Smoothness = m_editorUtils.Slider("Smoothness", clearTreesExtension.Smoothness, 0f, 5f, HelpEnabled);
            clearTreesExtension.NoiseEnabled = m_editorUtils.Toggle("Fractal Noise Enabled", clearTreesExtension.NoiseEnabled, HelpEnabled);
            if (clearTreesExtension.NoiseEnabled)
            {
                EditorGUI.indentLevel++;
                clearTreesExtension.NoiseStrength = m_editorUtils.FloatField("Fractal Strength", clearTreesExtension.NoiseStrength, HelpEnabled);
                clearTreesExtension.ShoulderFalloff = m_editorUtils.CurveField("Fractal Falloff", clearTreesExtension.ShoulderFalloff, HelpEnabled);
                EditorGUI.indentLevel--;
                m_editorUtils.Fractal(clearTreesExtension.MaskFractal, HelpEnabled);
            }

            if (m_editorUtils.Button("Clear Trees Btn", HelpEnabled))
                clearTreesExtension.Clear();
        }
    }
}
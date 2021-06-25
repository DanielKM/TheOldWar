using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaClearDetailsExtension))]
    public class GeNaClearDetailsExtensionEditor : GeNaSplineExtensionEditor
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
            GeNaClearDetailsExtension clearDetailsExtension = target as GeNaClearDetailsExtension;
            clearDetailsExtension.Width = m_editorUtils.FloatField("Width", clearDetailsExtension.Width, HelpEnabled);
            clearDetailsExtension.Smoothness = m_editorUtils.Slider("Smoothness", clearDetailsExtension.Smoothness, 0f, 5f, HelpEnabled);
            clearDetailsExtension.NoiseEnabled = m_editorUtils.Toggle("Fractal Noise Enabled", clearDetailsExtension.NoiseEnabled, HelpEnabled);
            if (clearDetailsExtension.NoiseEnabled)
            {
                EditorGUI.indentLevel++;
                clearDetailsExtension.NoiseStrength = m_editorUtils.FloatField("Fractal Strength", clearDetailsExtension.NoiseStrength, HelpEnabled);
                clearDetailsExtension.ShoulderFalloff = m_editorUtils.CurveField("Fractal Falloff", clearDetailsExtension.ShoulderFalloff, HelpEnabled);
                EditorGUI.indentLevel--;
                m_editorUtils.Fractal(clearDetailsExtension.MaskFractal, HelpEnabled);
            }

            if (m_editorUtils.Button("Clear Details Btn", HelpEnabled))
                clearDetailsExtension.Clear();
        }
    }
}
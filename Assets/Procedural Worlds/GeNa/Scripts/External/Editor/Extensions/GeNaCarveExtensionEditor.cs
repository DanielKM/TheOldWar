using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaCarveExtension))]
    public class GeNaCarveExtensionEditor : GeNaSplineExtensionEditor
    {
        private void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");
        }
        public override void OnInspectorGUI()
        {
            Initialize();
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
            GeNaCarveExtension carveExtension = target as GeNaCarveExtension;
            carveExtension.Width = m_editorUtils.FloatField("Width", carveExtension.Width, HelpEnabled);
            if (carveExtension.Width < 0.05f)
            {
                carveExtension.Width = 0.05f;
            }
            carveExtension.HeightOffset = m_editorUtils.FloatField("Height Offset", carveExtension.HeightOffset, HelpEnabled);
            carveExtension.Smoothness = m_editorUtils.Slider("Smoothness", carveExtension.Smoothness, 0f, 10f, HelpEnabled);
            carveExtension.NoiseEnabled = m_editorUtils.Toggle("Fractal Noise Enabled", carveExtension.NoiseEnabled, HelpEnabled);
            if (carveExtension.NoiseEnabled)
            {
                EditorGUI.indentLevel++;
                carveExtension.NoiseStrength = m_editorUtils.FloatField("Fractal Strength", carveExtension.NoiseStrength, HelpEnabled);
                carveExtension.ShoulderFalloff = m_editorUtils.CurveField("Fractal Falloff", carveExtension.ShoulderFalloff, HelpEnabled);
                EditorGUI.indentLevel--;
                m_editorUtils.Fractal(carveExtension.MaskFractal, HelpEnabled);
            }
            if (m_editorUtils.Button("Carve Btn", HelpEnabled))
                carveExtension.Carve();
        }
    }
}
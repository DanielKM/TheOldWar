using System;
using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaTerrainExtension))]
    public class GeNaTerrainExtensionEditor : GeNaSplineExtensionEditor
    {
        private void OnEnable()
        {
            if (m_editorUtils == null)
            {
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");
            }
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
            GeNaTerrainExtension terrainExtension = target as GeNaTerrainExtension;
            terrainExtension.EffectType = (EffectType) m_editorUtils.EnumPopup("Effect Type", terrainExtension.EffectType, HelpEnabled);
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                TerrainData terrainData = terrain.terrainData;
                switch (terrainExtension.EffectType)
                {
                    case EffectType.Raise:
                    case EffectType.Lower:
                    case EffectType.Flatten:
                        break;
                    case EffectType.ClearTrees:
                    case EffectType.ClearDetails:
                        break;
                    case EffectType.Texture:
                        int alphamapLayers = terrainData.alphamapLayers;
                        TerrainLayer[] terrainLayers = terrainData.terrainLayers;
                        GUIContent[] textureChoices = new GUIContent[alphamapLayers];
                        for (int assetIdx = 0; assetIdx < textureChoices.Length; assetIdx++)
                            textureChoices[assetIdx] = new GUIContent(terrainLayers[assetIdx].diffuseTexture.name);
                        terrainExtension.TextureProtoIndex = m_editorUtils.Popup("Texture", terrainExtension.TextureProtoIndex, textureChoices, HelpEnabled);
                        break;
                    case EffectType.Detail:
                        DetailPrototype[] detailPrototypes = terrainData.detailPrototypes;
                        GUIContent[] detailChoices = new GUIContent[detailPrototypes.Length];
                        for (int assetIdx = 0; assetIdx < detailChoices.Length; assetIdx++)
                        {
                            DetailPrototype detailProto = detailPrototypes[assetIdx];
                            if (detailProto.prototype != null)
                                detailChoices[assetIdx] = new GUIContent(detailProto.prototype.name);
                            else
                                detailChoices[assetIdx] = new GUIContent("Unknown asset");
                        }
                        terrainExtension.DetailProtoIndex = m_editorUtils.Popup("Details", terrainExtension.DetailProtoIndex, detailChoices, HelpEnabled);
                        break;
                }
            }
            terrainExtension.Width = m_editorUtils.FloatField("Width", terrainExtension.Width, HelpEnabled);
            terrainExtension.Strength = m_editorUtils.Slider("Strength", terrainExtension.Strength, 0f, 1f, HelpEnabled);
            terrainExtension.Smoothness = m_editorUtils.Slider("Smoothness", terrainExtension.Smoothness, 0f, 5f, HelpEnabled);
            terrainExtension.NoiseEnabled = m_editorUtils.Toggle("Fractal Noise Enabled", terrainExtension.NoiseEnabled, HelpEnabled);
            if (terrainExtension.NoiseEnabled)
            {
                EditorGUI.indentLevel++;
                terrainExtension.NoiseStrength = m_editorUtils.FloatField("Fractal Strength", terrainExtension.NoiseStrength, HelpEnabled);
                terrainExtension.ShoulderFalloff = m_editorUtils.CurveField("Fractal Falloff", terrainExtension.ShoulderFalloff, HelpEnabled);
                EditorGUI.indentLevel--;
                m_editorUtils.Fractal(terrainExtension.MaskFractal, HelpEnabled);
            }
            if (m_editorUtils.Button("Clear Details Btn", HelpEnabled))
                terrainExtension.Clear();
        }
    }
}
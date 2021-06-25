using UnityEngine;
using UnityEditor;
using PWCommon5;

namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaRoadProfile))]
    public class GeNaRoadProfileEditor : PWEditor
    {
        public GeNaRoadProfile m_profile;
        private EditorUtils m_editorUtils;

        private void OnEnable()
        {
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this, null, null, null);
            }
        }

        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize();

            if (m_profile == null)
            {
                m_profile = (GeNaRoadProfile)target;
            }
            
            m_editorUtils.Panel("ProfileSettings", ProfilePanel, false);
        }

        private void ProfilePanel(bool helpEnabled)
        {
            if (m_profile == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            Constants.RenderPipeline pipeline = GeNaUtility.GetActivePipeline();

            m_profile.RoadParamaters.m_renderMode = (Constants.ProfileRenderMode)m_editorUtils.EnumPopup("RenderMode", m_profile.RoadParamaters.m_renderMode, helpEnabled);
            m_editorUtils.Heading("RoadShadersSettings");
            EditorGUI.indentLevel++;
            switch (m_profile.RoadParamaters.m_renderMode)
            {
                case Constants.ProfileRenderMode.PWShader:
                    m_profile.RoadParamaters.BuiltInRoadShader = (Shader)m_editorUtils.ObjectField("RoadBuilt-InShader", m_profile.RoadParamaters.BuiltInRoadShader, typeof(Shader), false, helpEnabled);
                    m_profile.RoadParamaters.UniversalRoadShader = (Shader)m_editorUtils.ObjectField("RoadUniversalShader", m_profile.RoadParamaters.UniversalRoadShader, typeof(Shader), false, helpEnabled);
                    m_profile.RoadParamaters.HighDefinitionRoadShader = (Shader)m_editorUtils.ObjectField("RoadHighDefinitionShader", m_profile.RoadParamaters.HighDefinitionRoadShader, typeof(Shader), false, helpEnabled);
                    break;
                case Constants.ProfileRenderMode.Material:
                    m_profile.RoadParamaters.m_builtInRoadMaterial = (Material)m_editorUtils.ObjectField("RoadBuilt-InMaterial", m_profile.RoadParamaters.m_builtInRoadMaterial, typeof(Material), false, helpEnabled);
                    m_profile.RoadParamaters.m_universalRoadMaterial = (Material)m_editorUtils.ObjectField("RoadUniversalMaterial", m_profile.RoadParamaters.m_universalRoadMaterial, typeof(Material), false, helpEnabled);
                    m_profile.RoadParamaters.m_highDefinitionRoadMaterial = (Material)m_editorUtils.ObjectField("RoadHighDefinitionMaterial", m_profile.RoadParamaters.m_highDefinitionRoadMaterial, typeof(Material), false, helpEnabled);
                    break;
            }

            EditorGUI.indentLevel--;
            m_editorUtils.Heading("IntersectionShadersSettings");
            EditorGUI.indentLevel++;
            switch (m_profile.RoadParamaters.m_renderMode)
            {
                case Constants.ProfileRenderMode.PWShader:
                    m_profile.RoadParamaters.BuiltInIntersectionRoadShader = (Shader)m_editorUtils.ObjectField("IntersectionBuilt-InShader", m_profile.RoadParamaters.BuiltInIntersectionRoadShader, typeof(Shader), false, helpEnabled);
                    m_profile.RoadParamaters.UniversalIntersectionRoadShader = (Shader)m_editorUtils.ObjectField("IntersectionUniversalShader", m_profile.RoadParamaters.UniversalIntersectionRoadShader, typeof(Shader), false, helpEnabled);
                    m_profile.RoadParamaters.HighDefinitionIntersectionRoadShader = (Shader)m_editorUtils.ObjectField("IntersectionHighDefinitionShader", m_profile.RoadParamaters.HighDefinitionIntersectionRoadShader, typeof(Shader), false, helpEnabled);
                    break;
                case Constants.ProfileRenderMode.Material:
                    m_profile.RoadParamaters.m_builtInIntersectionMaterial = (Material)m_editorUtils.ObjectField("IntersectionBuilt-InMaterial", m_profile.RoadParamaters.m_builtInIntersectionMaterial, typeof(Material), false, helpEnabled);
                    m_profile.RoadParamaters.m_universalIntersectionMaterial = (Material)m_editorUtils.ObjectField("IntersectionUniversalMaterial", m_profile.RoadParamaters.m_universalIntersectionMaterial, typeof(Material), false, helpEnabled);
                    m_profile.RoadParamaters.m_highDefinitionIntersectionMaterial = (Material)m_editorUtils.ObjectField("IntersectionHighDefinitionMaterial", m_profile.RoadParamaters.m_highDefinitionIntersectionMaterial, typeof(Material), false, helpEnabled);
                    break;
            }

            EditorGUI.indentLevel--;
            if (m_profile.RoadParamaters.m_renderMode == Constants.ProfileRenderMode.PWShader)
            {
                EditorGUILayout.Space();
                m_editorUtils.Heading("AlbedoSettings");
                EditorGUI.indentLevel++;
                m_editorUtils.LabelField("Road");
                EditorGUI.indentLevel++;
                m_profile.RoadParamaters.m_roadAlbedoMap = (Texture2D)m_editorUtils.ObjectField("AlbedoMap", m_profile.RoadParamaters.m_roadAlbedoMap, typeof(Texture2D), helpEnabled, GUILayout.MaxHeight(16f));
                m_profile.RoadParamaters.m_roadTintColor = m_editorUtils.ColorField("TintColor", m_profile.RoadParamaters.m_roadTintColor, helpEnabled);
                EditorGUI.indentLevel--;
                m_editorUtils.LabelField("Intersection");
                EditorGUI.indentLevel++;
                m_profile.RoadParamaters.m_intersectionAlbedoMap = (Texture2D)m_editorUtils.ObjectField("AlbedoMap", m_profile.RoadParamaters.m_intersectionAlbedoMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                m_profile.RoadParamaters.m_intersectionTintColor = m_editorUtils.ColorField("TintColor", m_profile.RoadParamaters.m_intersectionTintColor, helpEnabled);
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                m_editorUtils.Heading("NormalSettings");
                EditorGUI.indentLevel++;
                m_editorUtils.LabelField("Road");
                EditorGUI.indentLevel++;
                m_profile.RoadParamaters.m_roadNormalMap = (Texture2D)m_editorUtils.ObjectField("NormalMap", m_profile.RoadParamaters.m_roadNormalMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                m_profile.RoadParamaters.m_roadNormalStrength = m_editorUtils.Slider("NormalStrength", m_profile.RoadParamaters.m_roadNormalStrength, 0f, 5f, helpEnabled);
                EditorGUI.indentLevel--;
                m_editorUtils.LabelField("Intersection");
                EditorGUI.indentLevel++;
                m_profile.RoadParamaters.m_intersectionNormalMap = (Texture2D)m_editorUtils.ObjectField("NormalMap", m_profile.RoadParamaters.m_intersectionNormalMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                m_profile.RoadParamaters.m_intersectionNormalStrength = m_editorUtils.Slider("NormalStrength", m_profile.RoadParamaters.m_intersectionNormalStrength, 0f, 5f, helpEnabled);
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                m_editorUtils.Heading("PBRSettings");
                if (pipeline != Constants.RenderPipeline.HighDefinition)
                {
                    EditorGUI.indentLevel++;
                    m_editorUtils.LabelField("Road");
                    EditorGUI.indentLevel++;
                    if (m_profile.RoadParamaters.m_roadMetallicMap == null)
                    {
                        m_profile.RoadParamaters.m_roadMetallicMap = (Texture2D)m_editorUtils.ObjectField("MetallicMap", m_profile.RoadParamaters.m_roadMetallicMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                        m_profile.RoadParamaters.m_roadMetallic = m_editorUtils.Slider("Metallic", m_profile.RoadParamaters.m_roadMetallic, 0f, 1f, helpEnabled);
                    }
                    else
                    {
                        m_profile.RoadParamaters.m_roadMetallicMap = (Texture2D)m_editorUtils.ObjectField("MetallicMap", m_profile.RoadParamaters.m_roadMetallicMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                    }

                    m_profile.RoadParamaters.m_roadOcclusionMap = (Texture2D)m_editorUtils.ObjectField("OcclusionMap", m_profile.RoadParamaters.m_roadOcclusionMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                    m_profile.RoadParamaters.m_roadOcclusionStrength = m_editorUtils.Slider("OcclusionStrength", m_profile.RoadParamaters.m_roadOcclusionStrength, 0f, 1f, helpEnabled);
                    m_profile.RoadParamaters.m_roadSmoothness = m_editorUtils.Slider("Smoothness", m_profile.RoadParamaters.m_roadSmoothness, 0f, 1f, helpEnabled);
                    EditorGUI.indentLevel--;

                    m_editorUtils.LabelField("Intersection");
                    EditorGUI.indentLevel++;
                    if (m_profile.RoadParamaters.m_intersectionMetallicMap == null)
                    {
                        m_profile.RoadParamaters.m_intersectionMetallicMap = (Texture2D)m_editorUtils.ObjectField("MetallicMap", m_profile.RoadParamaters.m_intersectionMetallicMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                        m_profile.RoadParamaters.m_intersectionMetallic = m_editorUtils.Slider("Metallic", m_profile.RoadParamaters.m_intersectionMetallic, 0f, 1f, helpEnabled);
                    }
                    else
                    {
                        m_profile.RoadParamaters.m_intersectionMetallicMap = (Texture2D)m_editorUtils.ObjectField("MetallicMap", m_profile.RoadParamaters.m_intersectionMetallicMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                    }

                    m_profile.RoadParamaters.m_intersectionOcclusionMap = (Texture2D)m_editorUtils.ObjectField("OcclusionMap", m_profile.RoadParamaters.m_intersectionOcclusionMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                    m_profile.RoadParamaters.m_intersectionOcclusionStrength = m_editorUtils.Slider("OcclusionStrength", m_profile.RoadParamaters.m_intersectionOcclusionStrength, 0f, 1f, helpEnabled);
                    m_profile.RoadParamaters.m_intersectionSmoothness = m_editorUtils.Slider("Smoothness", m_profile.RoadParamaters.m_intersectionSmoothness, 0f, 1f, helpEnabled);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();

                    m_editorUtils.Heading("HeightSettings");
                    EditorGUI.indentLevel++;
                    m_editorUtils.LabelField("Road");
                    EditorGUI.indentLevel++;
                    m_profile.RoadParamaters.m_roadHeightMap = (Texture2D)m_editorUtils.ObjectField("HeightMap", m_profile.RoadParamaters.m_roadHeightMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                    m_profile.RoadParamaters.m_roadHeightStrength = m_editorUtils.Slider("HeightStrength", m_profile.RoadParamaters.m_roadHeightStrength, 0f, 1f, helpEnabled);
                    EditorGUI.indentLevel--;
                    m_editorUtils.LabelField("Intersection");
                    EditorGUI.indentLevel++;
                    m_profile.RoadParamaters.m_intersectionHeightMap = (Texture2D)m_editorUtils.ObjectField("HeightMap", m_profile.RoadParamaters.m_intersectionHeightMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                    m_profile.RoadParamaters.m_intersectionHeightStrength = m_editorUtils.Slider("HeightStrength", m_profile.RoadParamaters.m_intersectionHeightStrength, 0f, 1f, helpEnabled);
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
                else
                {
                    m_editorUtils.LabelField("Road");
                    EditorGUI.indentLevel++;
                    m_editorUtils.LabelField("MaskMapSettings");
                    EditorGUI.indentLevel++;
                    m_profile.RoadParamaters.m_roadMaskMap = (Texture2D)m_editorUtils.ObjectField("MaskMap", m_profile.RoadParamaters.m_roadMaskMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();

                    m_editorUtils.LabelField("Intersection");
                    EditorGUI.indentLevel++;
                    m_editorUtils.LabelField("MaskMapSettings");
                    EditorGUI.indentLevel++;
                    m_profile.RoadParamaters.m_intersectionMaskMap = (Texture2D)m_editorUtils.ObjectField("MaskMap", m_profile.RoadParamaters.m_intersectionMaskMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profile);
            }
        }

        /// <summary>
        /// Sets the profile when using extensions
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="editor"></param>
        public static void SetProfile(GeNaRoadProfile profile, GeNaRoadProfileEditor editor)
        {
            editor.m_profile = profile;
        }
    }
}
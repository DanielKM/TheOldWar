using UnityEngine;
using UnityEditor;
using PWCommon5;

namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaRiverProfile))]
    public class GeNaRiverProfileEditor : PWEditor
    {
        public GeNaRiverProfile m_profile;
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
                m_profile = (GeNaRiverProfile) target;
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

            m_profile.RiverParamaters.m_renderMode = (Constants.ProfileRenderMode)m_editorUtils.EnumPopup("RenderMode", m_profile.RiverParamaters.m_renderMode, helpEnabled);
            if (m_profile.RiverParamaters.m_renderMode == Constants.ProfileRenderMode.PWShader)
            {
                Constants.RenderPipeline pipeline = GeNaUtility.GetActivePipeline();
                if (pipeline != Constants.RenderPipeline.BuiltIn)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("SRPShaderModeHelp"), MessageType.Warning);
                }
                else
                {
                    m_editorUtils.Heading("WeatherSettings");
                    EditorGUI.indentLevel++;
                    m_profile.RiverParamaters.m_isUsedForWeather = m_editorUtils.Toggle("IsUsedForWeather", m_profile.RiverParamaters.m_isUsedForWeather, helpEnabled);
                    if (!m_profile.RiverParamaters.m_isUsedForWeather)
                    {
                        EditorGUI.indentLevel++;
                        m_profile.WeatherParamaters.m_rainRiverProfile = (GeNaRiverProfile)m_editorUtils.ObjectField("RainRiverProfile", m_profile.WeatherParamaters.m_rainRiverProfile, typeof(GeNaRiverProfile), false, helpEnabled);
                        m_profile.WeatherParamaters.m_snowRiverProfile = (GeNaRiverProfile)m_editorUtils.ObjectField("SnowRiverProfile", m_profile.WeatherParamaters.m_snowRiverProfile, typeof(GeNaRiverProfile), false, helpEnabled);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }

                m_editorUtils.Heading("ShadersSettings");
                EditorGUI.indentLevel++;
                m_profile.RiverParamaters.BuiltInRiverShader = (Shader)m_editorUtils.ObjectField("Built-InShader", m_profile.RiverParamaters.BuiltInRiverShader, typeof(Shader), false, helpEnabled);
                m_profile.RiverParamaters.UniversalRiverShader = (Shader)m_editorUtils.ObjectField("UniversalShader", m_profile.RiverParamaters.UniversalRiverShader, typeof(Shader), false, helpEnabled);
                m_profile.RiverParamaters.HighDefinitionRiverShader = (Shader)m_editorUtils.ObjectField("HighDefinitionShader", m_profile.RiverParamaters.HighDefinitionRiverShader, typeof(Shader), false, helpEnabled);
                EditorGUI.indentLevel--;

                switch (pipeline)
                {
                    case Constants.RenderPipeline.BuiltIn:
                    {
                        EditorGUILayout.Space();
                        m_editorUtils.Heading("ColorSettings");
                        EditorGUI.indentLevel++;
                        m_profile.RiverParamaters.m_mainColor = m_editorUtils.ColorField("AlbedoColor", m_profile.RiverParamaters.m_mainColor, helpEnabled);
                        m_profile.RiverParamaters.m_mainColorDepthStrength = m_editorUtils.Slider("AlbedoDepthStrength", m_profile.RiverParamaters.m_mainColorDepthStrength, 0f, 1f, helpEnabled);
                        m_profile.RiverParamaters.m_tintColor = m_editorUtils.ColorField("TintColor", m_profile.RiverParamaters.m_tintColor, helpEnabled);
                        m_profile.RiverParamaters.m_tintStrength = m_editorUtils.Slider("TintStrength", m_profile.RiverParamaters.m_tintStrength, 0f, 1f, helpEnabled);
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();
                        m_editorUtils.Heading("PBRSettings");
                        EditorGUI.indentLevel++;
                        m_profile.RiverParamaters.m_smoothness = m_editorUtils.Slider("Smoothness", m_profile.RiverParamaters.m_smoothness, 0f, 1f, helpEnabled);
                        m_profile.RiverParamaters.m_specularColor = m_editorUtils.ColorField("SpecularColor",
                            m_profile.RiverParamaters.m_specularColor, helpEnabled);
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();
                        m_editorUtils.Heading("FlowSettings");
                        EditorGUI.indentLevel++;
                        m_profile.RiverParamaters.m_speed = m_editorUtils.Slider("Speed", m_profile.RiverParamaters.m_speed, 0f, 2f, helpEnabled);
                        m_profile.RiverParamaters.m_normalShift = m_editorUtils.Slider("NormalShift", m_profile.RiverParamaters.m_normalShift, 0f, 0.5f, helpEnabled);
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();
                        m_editorUtils.Heading("BlendSettings");
                        EditorGUI.indentLevel++;
                        m_profile.RiverParamaters.m_shoreBlend = m_editorUtils.Slider("ShoreBlend", m_profile.RiverParamaters.m_shoreBlend, 0f, 1f, helpEnabled);
                        m_profile.RiverParamaters.m_shoreNormalBlend = m_editorUtils.Slider("ShoreNormalBlend", m_profile.RiverParamaters.m_shoreNormalBlend, 0f, 1f, helpEnabled);
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();
                        m_editorUtils.Heading("NormalAndHeightSettings");
                        EditorGUI.indentLevel++;
                        m_editorUtils.LabelField("NormalAndHeightMap");
                        EditorGUILayout.BeginHorizontal();
                        m_profile.RiverParamaters.m_normalAndHeightMapTiling = EditorGUILayout.Vector2Field("", m_profile.RiverParamaters.m_normalAndHeightMapTiling, GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 17f));
                        m_profile.RiverParamaters.m_normalAndHeightMap = (Texture2D) EditorGUILayout.ObjectField(m_profile.RiverParamaters.m_normalAndHeightMap, typeof(Texture2D), false, GUILayout.MaxHeight(16f), GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth));
                        EditorGUILayout.EndHorizontal();
                        m_editorUtils.InlineHelp("NormalAndHeightMap", helpEnabled);
                        m_profile.RiverParamaters.m_normalStrength = m_editorUtils.Slider("NormalStrength", m_profile.RiverParamaters.m_normalStrength, 0f, 1f, helpEnabled);
                        m_profile.RiverParamaters.m_shoreRippleHeight = m_editorUtils.Slider("ShoreRippleHeight", m_profile.RiverParamaters.m_shoreRippleHeight, 0f, 1f, helpEnabled);
                        m_profile.RiverParamaters.m_refractionStrength = m_editorUtils.Slider("RefractionStrength", m_profile.RiverParamaters.m_refractionStrength, 0f, 1f, helpEnabled);
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel++;
                        m_editorUtils.Heading("FoamSettings");
                        m_profile.RiverParamaters.m_foamColor = m_editorUtils.ColorField("FoamColor", m_profile.RiverParamaters.m_foamColor, helpEnabled);
                        m_editorUtils.LabelField("FoamAlbedoMap");
                        EditorGUILayout.BeginHorizontal();
                        m_profile.RiverParamaters.m_foamAlbedoMapTiling = EditorGUILayout.Vector2Field("", m_profile.RiverParamaters.m_foamAlbedoMapTiling, GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 17f));
                        m_profile.RiverParamaters.m_foamAlbedoMap = (Texture2D) EditorGUILayout.ObjectField(m_profile.RiverParamaters.m_foamAlbedoMap, typeof(Texture2D), false, GUILayout.MaxHeight(16f), GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth));
                        EditorGUILayout.EndHorizontal();
                        m_editorUtils.InlineHelp("FoamAlbedoMap", helpEnabled);
                        m_profile.RiverParamaters.m_foamNormalMap = (Texture2D) m_editorUtils.ObjectField("FoamNormalMap", m_profile.RiverParamaters.m_foamNormalMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                        m_profile.RiverParamaters.m_foamNormalStrength = m_editorUtils.Slider("FoamNormalStrength", m_profile.RiverParamaters.m_foamNormalStrength, 0f, 2f, helpEnabled);
                        m_profile.RiverParamaters.m_foamMaskMap = (Texture2D) m_editorUtils.ObjectField("FoamMaskMap", m_profile.RiverParamaters.m_foamMaskMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                        m_profile.RiverParamaters.m_foamShoreBlend = m_editorUtils.Slider("FoamShoreBlend", m_profile.RiverParamaters.m_foamShoreBlend, 0f, 1f, helpEnabled);
                        m_profile.RiverParamaters.m_foamHeight = m_editorUtils.Slider("FoamHeight", m_profile.RiverParamaters.m_foamHeight, 0f, 1f, helpEnabled);
                        m_profile.RiverParamaters.m_foamRipple = m_editorUtils.Slider("FoamRipple", m_profile.RiverParamaters.m_foamRipple, 0f, 1f, helpEnabled);
                        m_profile.RiverParamaters.m_foamSpeed = m_editorUtils.Slider("FoamSpeed", m_profile.RiverParamaters.m_foamSpeed, 0f, 2f, helpEnabled);
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();
                        m_editorUtils.Heading("SeaLevelSettings");
                        EditorGUI.indentLevel++;
                        m_profile.RiverParamaters.m_seaLevelBlend = m_editorUtils.Slider("SeaLevelFoamBlend", m_profile.RiverParamaters.m_seaLevelBlend, 0.001f, 10f, helpEnabled);
                        m_profile.RiverParamaters.m_seaLevelFoamColor = m_editorUtils.ColorField("SeaLevelFoamColor", m_profile.RiverParamaters.m_seaLevelFoamColor, helpEnabled);
                        m_profile.RiverParamaters.m_seaLevelFoamNormalStrength = m_editorUtils.Slider("SeaLevelFoamNormalStrength", m_profile.RiverParamaters.m_seaLevelFoamNormalStrength, 0f, 2f, helpEnabled);
                        m_profile.RiverParamaters.m_pBRColor = EditorGUILayout.ColorField(new GUIContent(m_editorUtils.GetTextValue("SeaLevelFoamPBR"), m_editorUtils.GetTooltip("SeaLevelFoamPBR")), m_profile.RiverParamaters.m_pBRColor, true, true, true);
                        m_editorUtils.InlineHelp("SeaLevelFoamPBR", helpEnabled);
                        EditorGUI.indentLevel--;
                        break;
                    }
                }
            }
            else
            {
                m_editorUtils.Heading("ShadersSettings");
                EditorGUI.indentLevel++;
                m_profile.RiverParamaters.m_builtInRiverMaterial = (Material)m_editorUtils.ObjectField("Built-InMaterial", m_profile.RiverParamaters.m_builtInRiverMaterial, typeof(Material), false, helpEnabled);
                m_profile.RiverParamaters.m_universalRiverMaterial = (Material)m_editorUtils.ObjectField("UniversalMaterial", m_profile.RiverParamaters.m_universalRiverMaterial, typeof(Material), false, helpEnabled);
                m_profile.RiverParamaters.m_highDefinitionRiverMaterial = (Material)m_editorUtils.ObjectField("HighDefinitionMaterial", m_profile.RiverParamaters.m_highDefinitionRiverMaterial, typeof(Material), false, helpEnabled);
                EditorGUI.indentLevel--;
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
        public static void SetProfile(GeNaRiverProfile profile, GeNaRiverProfileEditor editor)
        {
            editor.m_profile = profile;
        }
    }
}
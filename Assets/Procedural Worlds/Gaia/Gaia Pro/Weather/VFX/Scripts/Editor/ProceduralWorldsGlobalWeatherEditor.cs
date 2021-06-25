using UnityEngine;
using UnityEditor;
using PWCommon5;
using Gaia.Internal;
using UnityEngine.Rendering;
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Gaia
{
    public enum WeatherType { Rain, Snow }

    [CustomEditor(typeof(ProceduralWorldsGlobalWeather))]
    public class ProceduralWorldsGlobalWeatherEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private ProceduralWorldsGlobalWeather m_globalWeather;
        private SceneProfile m_profile;
#if HDPipeline
        private VolumeProfile m_hdrpVolumeProfile;
        private VisualEnvironment VisualEnvironment;
#endif

        private int hour;
        private float minute;

        private void OnEnable()
        {
            //Get Profile object
            m_globalWeather = (ProceduralWorldsGlobalWeather)target;

            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }

            #if HDPipeline

            if (VisualEnvironment == null)
            {

            }

            #endif

            m_globalWeather.m_renderPipeline = GaiaUtils.GetActivePipeline();
            if (!Application.isPlaying)
            {
                if (GaiaUtils.CheckIfSceneProfileExists())
                {
                    GaiaLightingProfileValues profile = GaiaGlobal.Instance.SceneProfile.m_lightingProfiles[GaiaGlobal.Instance.SceneProfile.m_selectedLightingProfileValuesIndex];
                    if (profile != null)
                    {
#if GAIA_PRO_PRESENT
                        profile.m_pwSkyAtmosphereData.Load(m_globalWeather);
                        profile.m_pwSkyWindData.Load(m_globalWeather);
                        profile.m_pwSkyCloudData.Load(m_globalWeather);
                        profile.m_pwSkySeasonData.Load(m_globalWeather);
                        profile.m_pwSkyWeatherData.Load(m_globalWeather);
#endif
                    }
                }

                m_globalWeather.UpdateAllSystems(false);
            }
        }

        public override void OnInspectorGUI()
        {
            //Initialization
            m_editorUtils.Initialize(); // Do not remove this!
            if (m_globalWeather != null)
            {
                m_globalWeather.m_renderPipeline = GaiaUtils.GetActivePipeline();
                Transform transform = m_globalWeather.gameObject.transform;
                transform.hideFlags = HideFlags.HideInInspector | HideFlags.NotEditable;
            }

            if (m_profile == null)
            {
                m_profile = GaiaGlobal.Instance.SceneProfile;
            }

#if HDPipeline
            m_hdrpVolumeProfile = GaiaUtils.GetVolumeProfile(false, "Environment", "Processing");
#endif

            //Monitor for changes
            EditorGUI.BeginChangeCheck();

            m_editorUtils.Panel("WeatherSettings", WeatherSettingsEnabled, false, true, true);

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                m_globalWeather.GetCurrentSettings();
                Undo.RecordObject(m_globalWeather, "Made changes");
                EditorUtility.SetDirty(m_globalWeather);
            }
        }

        private void WeatherSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            m_editorUtils.Heading("GlobalSetup");
            EditorGUI.indentLevel++;
            m_globalWeather.m_player = (Transform)m_editorUtils.ObjectField("PlayerObject", m_globalWeather.m_player, typeof(Transform), true, helpEnabled, GUILayout.Height(16f));
            m_globalWeather.m_instantStartStop = m_editorUtils.Toggle("InstantStop/Start", m_globalWeather.m_instantStartStop, helpEnabled);
            if (Application.isPlaying)
            {
                //Snow
                EditorGUILayout.BeginHorizontal();
                if (ProceduralWorldsGlobalWeather.Instance.EnableSnow)
                {
                    GUI.enabled = true;
                    EditorGUILayout.LabelField("Snow Chance: " + Mathf.RoundToInt(ProceduralWorldsGlobalWeather.Instance.m_snowSampleStrength * 100) + "%", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
                }
                else
                {
                    GUI.enabled = false;
                    EditorGUILayout.LabelField("Snow Chance: Not Enabled", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
                }

                if (ProceduralWorldsGlobalWeather.Instance.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    GUI.enabled = false;
                }

                if (ProceduralWorldsGlobalWeather.Instance.IsRaining)
                {
                    GUI.enabled = false;
                }

                if (ProceduralWorldsGlobalWeather.Instance.IsSnowing)
                {
                    if (GUILayout.Button("Stop Snow"))
                    {
                        ProceduralWorldsGlobalWeather.Instance.StopSnow();
                    }
                }
                else
                {
                    if (GUILayout.Button("Start Snow"))
                    {
                        ProceduralWorldsGlobalWeather.Instance.PlaySnow();
                    }
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                //Rain
                EditorGUILayout.BeginHorizontal();
                if (ProceduralWorldsGlobalWeather.Instance.EnableRain)
                {
                    GUI.enabled = true;
                    EditorGUILayout.LabelField("Rain Chance: " + Mathf.RoundToInt(ProceduralWorldsGlobalWeather.Instance.m_rainSampleStrength * 100) + "%", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
                }
                else
                {
                    GUI.enabled = false;
                    EditorGUILayout.LabelField("Rain Chance: Not Enabled", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
                }

                if (ProceduralWorldsGlobalWeather.Instance.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    GUI.enabled = false;
                }

                if (ProceduralWorldsGlobalWeather.Instance.IsSnowing)
                {
                    GUI.enabled = false;
                }
                if (ProceduralWorldsGlobalWeather.Instance.IsRaining)
                {
                    if (GUILayout.Button("Stop Rain"))
                    {
                        ProceduralWorldsGlobalWeather.Instance.StopRain();
                    }
                }
                else
                {
                    if (GUILayout.Button("Start Rain"))
                    {
                        ProceduralWorldsGlobalWeather.Instance.PlayRain();
                    }
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                //Snow
                EditorGUILayout.BeginHorizontal();
                if (ProceduralWorldsGlobalWeather.Instance.EnableSnow)
                {
                    EditorGUILayout.LabelField("Snow Chance: Runtime only", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
                }
                else
                {
                    EditorGUILayout.LabelField("Snow Chance: Not Enabled", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
                }

                GUI.enabled = false;

                if (ProceduralWorldsGlobalWeather.Instance.IsSnowing)
                {
                    if (GUILayout.Button("Stop Snow"))
                    {
                        ProceduralWorldsGlobalWeather.Instance.StopSnow();
                    }
                }
                else
                {
                    if (GUILayout.Button("Start Snow"))
                    {
                        ProceduralWorldsGlobalWeather.Instance.PlaySnow();
                    }
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                //Rain
                EditorGUILayout.BeginHorizontal();
                if (ProceduralWorldsGlobalWeather.Instance.EnableRain)
                {
                    EditorGUILayout.LabelField("Rain Chance: Runtime only", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
                }
                else
                {
                    EditorGUILayout.LabelField("Rain Chance: Not Enabled", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
                }

                GUI.enabled = false;

                if (ProceduralWorldsGlobalWeather.Instance.IsRaining)
                {
                    if (GUILayout.Button("Stop Rain"))
                    {
                        ProceduralWorldsGlobalWeather.Instance.StopRain();
                    }
                }
                else
                {
                    if (GUILayout.Button("Start Rain"))
                    {
                        ProceduralWorldsGlobalWeather.Instance.PlayRain();
                    }
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }

            if (m_globalWeather.EnableRain || m_globalWeather.EnableSnow)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Weather is currently enabled", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
                if (m_editorUtils.Button("DisableWeather"))
                {
                    m_globalWeather.SetWeatherStatus(false);
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Weather is currently disabled", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
                if (m_editorUtils.Button("EnableWeather"))
                {
                    m_globalWeather.SetWeatherStatus(true);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.BeginChangeCheck();

            if (m_profile != null)
            {
                m_editorUtils.LabelField("TimeOfDay", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                m_profile.m_gaiaTimeOfDay.m_todEnabled = m_editorUtils.Toggle("TODEnable", m_profile.m_gaiaTimeOfDay.m_todEnabled, helpEnabled);
                m_profile.m_gaiaTimeOfDay.m_todHour = m_editorUtils.IntSlider("TODHour", m_profile.m_gaiaTimeOfDay.m_todHour, 0, 23, helpEnabled);
                if (m_profile.m_gaiaTimeOfDay.m_todHour > 23)
                {
                    m_profile.m_gaiaTimeOfDay.m_todHour = 0;
                }
                m_profile.m_gaiaTimeOfDay.m_todMinutes = m_editorUtils.Slider("TODMinutes", m_profile.m_gaiaTimeOfDay.m_todMinutes, 0f, 59f, helpEnabled);
                if (m_profile.m_gaiaTimeOfDay.m_todMinutes > 60f)
                {
                    m_profile.m_gaiaTimeOfDay.m_todMinutes = 0f;
                }
                if (m_profile.m_gaiaTimeOfDay.m_todEnabled)
                {
                    EditorGUI.indentLevel++;
                    m_profile.m_gaiaTimeOfDay.m_todDayTimeScale = m_editorUtils.Slider("TODScale", m_profile.m_gaiaTimeOfDay.m_todDayTimeScale, 0f, 500f, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;

                if (EditorGUI.EndChangeCheck())
                {
                    if (GaiaGlobal.Instance != null)
                    {
                        EditorUtility.SetDirty(GaiaGlobal.Instance);
                        Undo.RecordObject(GaiaGlobal.Instance, "Changes Made");

                        GaiaGlobal.Instance.UpdateGaiaTimeOfDay(false);
                        GaiaGlobal.Instance.UpdateGaiaWeather();
                    }
                }
            }

            EditorGUI.indentLevel--;
            m_globalWeather.m_panelWindSettings = m_editorUtils.Panel("WindSettings", WindSettingsEnabled, m_globalWeather.m_panelWindSettings);
            m_globalWeather.m_panelSeasonSettings = m_editorUtils.Panel("SeasonSettings", SeasonSettingsEnabled, m_globalWeather.m_panelSeasonSettings);
            //m_globalWeather.m_panelCloudSettings = m_editorUtils.Panel("CloudSettings", CloudSettingsEnabled, m_globalWeather.m_panelCloudSettings);

            EditorGUILayout.Space();

            if (m_globalWeather.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
                EditorGUILayout.HelpBox("Rain and Snow is not available for HDRP, this feature will be available soon.", MessageType.Info);
                GUI.enabled = false;
            }
            m_editorUtils.Heading("WeatherSetup");
            EditorGUI.indentLevel++;
            m_globalWeather.m_weatherFadeDuration = m_editorUtils.Slider("WeatherFadeDuration", m_globalWeather.m_weatherFadeDuration, 0f, 250f, helpEnabled);
            m_globalWeather.m_modifyClouds = m_editorUtils.Toggle("ModifyClouds", m_globalWeather.m_modifyClouds, helpEnabled);
            m_globalWeather.m_modifyFog = m_editorUtils.Toggle("ModifyFog", m_globalWeather.m_modifyFog, helpEnabled);
            m_globalWeather.m_modifyPostProcessing = m_editorUtils.Toggle("ModifyPostProcessing", m_globalWeather.m_modifyPostProcessing, helpEnabled);
            m_globalWeather.m_modifySkybox = m_editorUtils.Toggle("ModifySkybox", m_globalWeather.m_modifySkybox, helpEnabled);
            m_globalWeather.m_modifySun = m_editorUtils.Toggle("ModifySun", m_globalWeather.m_modifySun, helpEnabled);
            m_globalWeather.m_modifyWind = m_editorUtils.Toggle("ModifyWind", m_globalWeather.m_modifyWind, helpEnabled);
            EditorGUI.indentLevel--;
            m_globalWeather.m_panelSnowSettings = m_editorUtils.Panel("SnowSettings", SnowSettingsEnabled, m_globalWeather.m_panelSnowSettings);
            m_globalWeather.m_panelRainSettings = m_editorUtils.Panel("RainSettings", RainSettingsEnabled, m_globalWeather.m_panelRainSettings);
            m_globalWeather.m_panelThunderSettings = m_editorUtils.Panel("ThunderSettings", ThunderSettingsEnabled, m_globalWeather.m_panelThunderSettings);

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_globalWeather, "Made changes");
                EditorUtility.SetDirty(m_globalWeather);
                if (GaiaUtils.CheckIfSceneProfileExists())
                {
                    GaiaLightingProfileValues profile = GaiaGlobal.Instance.SceneProfile.m_lightingProfiles[GaiaGlobal.Instance.SceneProfile.m_selectedLightingProfileValuesIndex];
                    ProceduralWorldsGlobalWeather.SaveValuesToLightProfile(profile, m_globalWeather);
                }
            }

            GUI.enabled = true;
            if (m_editorUtils.Button("RevertToDefaults"))
            {
                m_globalWeather.RevertToDefaults();
            }
        }
        private void WindSettingsEnabled(bool helpEnabled)
        {
            m_editorUtils.Heading("WindSettings");
            EditorGUI.indentLevel++;
            m_globalWeather.WindZone = (WindZone)m_editorUtils.ObjectField("WindZoneObject", m_globalWeather.WindZone, typeof(WindZone), true, helpEnabled, GUILayout.Height(16f));
            EditorGUILayout.BeginHorizontal();
            GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_windDirection = m_editorUtils.Slider("WindDirection", GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_windDirection, 0f, 1f);
            if (GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_windDirection < 0.25f || GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_windDirection == 1)
            {
                EditorGUILayout.LabelField("N ", GUILayout.Width(30f));
            }
            else if (GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_windDirection > 0.25f && GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_windDirection < 0.5f)
            {
                EditorGUILayout.LabelField("E ", GUILayout.Width(30f));
            }
            else if (GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_windDirection > 0.5 && GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_windDirection < 0.75f)
            {
                EditorGUILayout.LabelField("S ", GUILayout.Width(30f));
            }
            else
            {
                EditorGUILayout.LabelField("W ", GUILayout.Width(30f));
            }
            EditorGUILayout.EndHorizontal();
            m_globalWeather.WindDirection = GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_windDirection;

            m_editorUtils.InlineHelp("WindDirection", helpEnabled);
            m_globalWeather.WindSpeed = m_editorUtils.Slider("WindSpeed", m_globalWeather.WindSpeed, 0f, 5f, helpEnabled);
            m_globalWeather.WindTurbulence = m_editorUtils.Slider("WindTurbulence", m_globalWeather.WindTurbulence, 0f, 5f, helpEnabled);
            m_globalWeather.WindFrequency = m_editorUtils.Slider("WindFrequency", m_globalWeather.WindFrequency, 0f, 5f, helpEnabled);
            m_globalWeather.WindMultiplier = m_editorUtils.Slider("WindMultiplier", m_globalWeather.WindMultiplier, 1f, 50f, helpEnabled);
            m_editorUtils.InlineHelp("WindSetupHelp", helpEnabled);
            EditorGUI.indentLevel--;
            m_editorUtils.Heading("TerrainWindSettings");
            EditorGUI.indentLevel++;
            m_globalWeather.MinTerrainWind = m_editorUtils.FloatField("MinTerrainWind", m_globalWeather.MinTerrainWind, helpEnabled);
            if (m_globalWeather.MinTerrainWind < 0)
            {
                m_globalWeather.MinTerrainWind = 0f;
            }
            if (m_globalWeather.MinTerrainWind >= m_globalWeather.MaxTerrainWind)
            {
                m_globalWeather.MinTerrainWind = m_globalWeather.MaxTerrainWind - 0.05f;
            }
            m_globalWeather.MaxTerrainWind = m_editorUtils.FloatField("MaxTerrainWind", m_globalWeather.MaxTerrainWind, helpEnabled);
            if (m_globalWeather.MaxTerrainWind < 0.05)
            {
                m_globalWeather.MaxTerrainWind = 0.05f;
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
        }
        private void SeasonSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            m_editorUtils.Heading("SeasonSettings");
            m_globalWeather.EnableSeasons = m_editorUtils.Toggle("EnableSeasons", m_globalWeather.EnableSeasons, helpEnabled);
            if (m_globalWeather.EnableSeasons)
            {
                EditorGUI.indentLevel++;
                if (GaiaUtils.CheckIfSceneProfileExists())
                {
                    GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season = m_editorUtils.Slider("SeasonAmount", GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season, 0f, 3.9999f, helpEnabled);
                    EditorGUI.indentLevel++;
                    if (GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season < 1f)
                    {
                        EditorGUILayout.LabelField(string.Format("{0:0}% Winter {1:0}% Spring", (1f - GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season) * 100f, GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season * 100f));
                    }
                    else if (GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season < 2f)
                    {
                        EditorGUILayout.LabelField(string.Format("{0:0}% Spring {1:0}% Summer", (2f - GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season) * 100f, (GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season - 1f) * 100f));
                    }
                    else if (GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season < 3f)
                    {
                        EditorGUILayout.LabelField(string.Format("{0:0}% Summer {1:0}% Autumn", (3f - GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season) * 100f, (GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season - 2f) * 100f));
                    }
                    else
                    {
                        EditorGUILayout.LabelField(string.Format("{0:0}% Autumn {1:0}% Winter", (4f - GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season) * 100f, (GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season - 3f) * 100f));
                    }
                    EditorGUI.indentLevel--;
                }
                m_globalWeather.m_seasonTransitionDuration = m_editorUtils.FloatField("SeasonsDuration", m_globalWeather.m_seasonTransitionDuration, helpEnabled);
                m_globalWeather.SeasonWinterTint = m_editorUtils.ColorField("WinterTint", m_globalWeather.SeasonWinterTint, helpEnabled);
                m_globalWeather.SeasonSpringTint = m_editorUtils.ColorField("SpringTint", m_globalWeather.SeasonSpringTint, helpEnabled);
                m_globalWeather.SeasonSummerTint = m_editorUtils.ColorField("SummerTint", m_globalWeather.SeasonSummerTint, helpEnabled);
                m_globalWeather.SeasonAutumnTint = m_editorUtils.ColorField("AutumnTint", m_globalWeather.SeasonAutumnTint, helpEnabled);
                EditorGUI.indentLevel--;
            }

            if (GaiaGlobal.Instance != null)
            {
                EditorUtility.SetDirty(GaiaGlobal.Instance);
                Undo.RecordObject(GaiaGlobal.Instance, "Changes Made");

                GaiaGlobal.Instance.UpdateGaiaWeather();
            }
            EditorGUILayout.Space();
        }
        private void CloudSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            m_globalWeather.EnableClouds = m_editorUtils.Toggle("EnableClouds", m_globalWeather.EnableClouds, helpEnabled);
            if (m_globalWeather.EnableClouds)
            {
                m_editorUtils.LabelField("CloudSettings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                m_globalWeather.CloudRotationSpeedLow = m_editorUtils.Slider("CloudRotationSpeedLow", m_globalWeather.CloudRotationSpeedLow, -5f, 5f, helpEnabled);
                m_globalWeather.CloudRotationSpeedMiddle = m_editorUtils.Slider("CloudRotationSpeedMiddle", m_globalWeather.CloudRotationSpeedMiddle, -5f, 5f, helpEnabled);
                m_globalWeather.CloudRotationSpeedFar = m_editorUtils.Slider("CloudRotationSpeedFar", m_globalWeather.CloudRotationSpeedFar, -5f, 5f, helpEnabled);
                m_globalWeather.CloudHeight = m_editorUtils.IntField("CloudHeight", m_globalWeather.CloudHeight, helpEnabled);
                if (m_globalWeather.CloudHeight < 0)
                {
                    m_globalWeather.CloudHeight = 0;
                }
                m_globalWeather.CloudScale = m_editorUtils.Slider("CloudScale", m_globalWeather.CloudScale, 1f, 5f, helpEnabled);
                m_globalWeather.CloudOffset = m_editorUtils.Slider("CloudOffset", m_globalWeather.CloudOffset, 1f, 500f, helpEnabled);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                m_editorUtils.LabelField("LightingSettings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                //m_globalWeather.CloudBrightness = m_editorUtils.Slider("CloudBrightness", m_globalWeather.CloudBrightness, 0f, 8f, helpEnabled);
                m_globalWeather.CloudDomeBrightness = m_editorUtils.CurveField("CloudDomeBrightness", m_globalWeather.CloudDomeBrightness, helpEnabled);
                GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue());
                m_globalWeather.CloudFade = m_editorUtils.Slider("CloudFadeDistance", m_globalWeather.CloudFade, 0f, 500f, helpEnabled);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                m_editorUtils.LabelField("CloudSetup", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                m_globalWeather.TODCloudHeightLevelDensity = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODCloudHeightLevelDensity"), m_editorUtils.GetTooltip("TODCloudHeightLevelDensity")), m_globalWeather.TODCloudHeightLevelDensity);
                m_editorUtils.InlineHelp("TODCloudHeightLevelDensity", helpEnabled);

                m_globalWeather.TODCloudHeightLevelThickness = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODCloudHeightLevelThickness"), m_editorUtils.GetTooltip("TODCloudHeightLevelThickness")), m_globalWeather.TODCloudHeightLevelThickness);
                m_editorUtils.InlineHelp("TODCloudHeightLevelThickness", helpEnabled);

                m_globalWeather.TODCloudHeightLevelSpeed = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODCloudHeightLevelSpeed"), m_editorUtils.GetTooltip("TODCloudHeightLevelSpeed")), m_globalWeather.TODCloudHeightLevelSpeed);
                m_editorUtils.InlineHelp("TODCloudHeightLevelSpeed", helpEnabled);

                m_globalWeather.TODCloudOpacity = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODCloudOpacity"), m_editorUtils.GetTooltip("TODCloudOpacity")), m_globalWeather.TODCloudOpacity);
                m_editorUtils.InlineHelp("TODCloudOpacity", helpEnabled);
                GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 3.2f);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
            {

            }
        }
        private void AtmosphereSettingsEnabled(bool helpEnabled)
        {
            m_editorUtils.Heading("SunSettings");
            EditorGUI.indentLevel++;
            m_globalWeather.TODSunColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODSunColor"), m_editorUtils.GetTooltip("TODSunColor")), m_globalWeather.TODSunColor);
            m_editorUtils.InlineHelp("TODSunColor", helpEnabled);

            m_globalWeather.TODSunIntensity = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODSunIntensity"), m_editorUtils.GetTooltip("TODSunIntensity")), m_globalWeather.TODSunIntensity);
            m_editorUtils.InlineHelp("TODSunIntensity", helpEnabled);

            if (m_globalWeather.m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
                m_globalWeather.TODSunShadowStrength = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODSunShadowStregth"), m_editorUtils.GetTooltip("TODSunShadowStregth")), m_globalWeather.TODSunShadowStrength);
                m_editorUtils.InlineHelp("TODSunShadowStregth", helpEnabled);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            m_editorUtils.Heading("FogSettings");
            EditorGUI.indentLevel++;
            if (m_globalWeather.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
                m_globalWeather.TODFogColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODFogColor"), m_editorUtils.GetTooltip("TODFogColor")), m_globalWeather.TODFogColor);
                m_editorUtils.InlineHelp("TODFogColor", helpEnabled);
                m_globalWeather.TODHDRPFogAlbedo = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODHDRPFogAlbedo"), m_editorUtils.GetTooltip("TODHDRPFogAlbedo")), m_globalWeather.TODHDRPFogAlbedo);
                m_editorUtils.InlineHelp("TODHDRPFogAlbedo", helpEnabled);
                m_globalWeather.TODFogEndDistance = m_editorUtils.CurveField("TODFogEndDistance", m_globalWeather.TODFogEndDistance, helpEnabled);
                m_globalWeather.TODHDRPFogAnisotropy = m_editorUtils.CurveField("TODHDRPFogAnisotropy", m_globalWeather.TODHDRPFogAnisotropy, helpEnabled);
                m_globalWeather.TODHDRPFogBaseHeight = m_editorUtils.CurveField("TODHDRPFogBaseHeight", m_globalWeather.TODHDRPFogBaseHeight, helpEnabled);
                m_globalWeather.TODHDRPFogDepthExtent = m_editorUtils.CurveField("TODHDRPFogDepthExtent", m_globalWeather.TODHDRPFogDepthExtent, helpEnabled);
                m_globalWeather.TODHDRPFogLightProbeDimmer = m_editorUtils.CurveField("TODHDRPFogLightProbeDimmer", m_globalWeather.TODHDRPFogLightProbeDimmer, helpEnabled);
            }
            else
            {
                m_globalWeather.TODFogColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODFogColor"), m_editorUtils.GetTooltip("TODFogColor")), m_globalWeather.TODFogColor);
                m_editorUtils.InlineHelp("TODFogColor", helpEnabled);

                if (RenderSettings.fogMode == FogMode.Linear)
                {
                    m_globalWeather.TODFogStartDistance = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODFogStartDistance"), m_editorUtils.GetTooltip("TODFogStartDistance")), m_globalWeather.TODFogStartDistance);
                    m_editorUtils.InlineHelp("TODFogStartDistance", helpEnabled);

                    m_globalWeather.TODFogEndDistance = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODFogEndDistance"), m_editorUtils.GetTooltip("TODFogEndDistance")), m_globalWeather.TODFogEndDistance);
                    m_editorUtils.InlineHelp("TODFogEndDistance", helpEnabled);
                }
                else
                {
                    m_globalWeather.TODFogDensity = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODFogDensity"), m_editorUtils.GetTooltip("TODFogDensity")), m_globalWeather.TODFogDensity);
                    m_editorUtils.InlineHelp("TODFogDensity", helpEnabled);
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            m_editorUtils.Heading("AmbientSettings");
            EditorGUI.indentLevel++;
            if (m_globalWeather.m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
                switch (RenderSettings.ambientMode)
                {
                    case AmbientMode.Skybox:
                        m_globalWeather.TODAmbientIntensity = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODAmbientIntensity"), m_editorUtils.GetTooltip("TODAmbientIntensity")), m_globalWeather.TODAmbientIntensity);
                        m_editorUtils.InlineHelp("TODAmbientIntensity", helpEnabled);
                        break;
                    case AmbientMode.Trilight:
                        m_globalWeather.TODAmbientSkyColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODAmbientSkyColor"), m_editorUtils.GetTooltip("TODAmbientSkyColor")), m_globalWeather.TODAmbientSkyColor, true);
                        m_editorUtils.InlineHelp("TODAmbientSkyColor", helpEnabled);

                        m_globalWeather.TODAmbientEquatorColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODAmbientEquatorColor"), m_editorUtils.GetTooltip("TODAmbientEquatorColor")), m_globalWeather.TODAmbientEquatorColor, true);
                        m_editorUtils.InlineHelp("TODAmbientEquatorColor", helpEnabled);

                        m_globalWeather.TODAmbientGroundColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODAmbientGroundColor"), m_editorUtils.GetTooltip("TODAmbientGroundColor")), m_globalWeather.TODAmbientGroundColor, true);
                        m_editorUtils.InlineHelp("TODAmbientGroundColor", helpEnabled);
                        break;
                    default:
                        m_globalWeather.TODAmbientSkyColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODAmbientSkyColor"), m_editorUtils.GetTooltip("TODAmbientSkyColor")), m_globalWeather.TODAmbientSkyColor, true);
                        m_editorUtils.InlineHelp("TODAmbientSkyColor", helpEnabled);
                        break;
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            m_editorUtils.Heading("SkyboxSettings");
            EditorGUI.indentLevel++;
            if (m_globalWeather.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
                if (m_globalWeather.TODHDRPGroundTint != null)
                {
                    m_globalWeather.TODHDRPGroundTint = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODHDRPGroundTint"), m_editorUtils.GetTooltip("TODHDRPGroundTint")), m_globalWeather.TODHDRPGroundTint);
                    m_editorUtils.InlineHelp("TODHDRPGroundTint", helpEnabled);
                }

                m_globalWeather.TODSkyboxExposure = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODSkyboxExposure"), m_editorUtils.GetTooltip("TODSkyboxExposure")), m_globalWeather.TODSkyboxExposure);
                m_editorUtils.InlineHelp("TODSkyboxExposure", helpEnabled);
            }
            else
            {
                m_globalWeather.TODAtmosphereThickness = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODAtmosphereThickness"), m_editorUtils.GetTooltip("TODAtmosphereThickness")), m_globalWeather.TODAtmosphereThickness);
                m_editorUtils.InlineHelp("TODAtmosphereThickness", helpEnabled);

                m_globalWeather.TODSunSize = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODSunSize"), m_editorUtils.GetTooltip("TODSunSize")), m_globalWeather.TODSunSize);
                m_editorUtils.InlineHelp("TODSunSize", helpEnabled);

                m_globalWeather.TODSunSizeConvergence = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODSunSizeConvergence"), m_editorUtils.GetTooltip("TODSunSizeConvergence")), m_globalWeather.TODSunSizeConvergence);
                m_editorUtils.InlineHelp("TODSunSizeConvergence", helpEnabled);
            }
            EditorGUI.indentLevel--;
        }
        private void SnowSettingsEnabled(bool helpEnabled)
        {
            WeatherSettingsPanel(WeatherType.Snow, m_globalWeather.m_snowWeatherSettings, true, helpEnabled);
        }
        private void RainSettingsEnabled(bool helpEnabled)
        {
            WeatherSettingsPanel(WeatherType.Rain, m_globalWeather.m_rainWeatherSettings, false, helpEnabled);
        }
        private void ThunderSettingsEnabled(bool helpEnabled)
        {
            m_editorUtils.Heading("ThunderSettings");
            EditorGUI.indentLevel++;
            m_globalWeather.m_enableThunder = m_editorUtils.Toggle("EnableThunder", m_globalWeather.m_enableThunder, helpEnabled);
            if (m_globalWeather.m_enableThunder)
            {
                EditorGUI.indentLevel++;
                GaiaConstants.ThunderStormType stormType = m_globalWeather.m_stormType;
                stormType = (GaiaConstants.ThunderStormType)m_editorUtils.EnumPopup("StormType", stormType, helpEnabled);
                if (stormType != m_globalWeather.m_stormType)
                {
                    SetStormType(stormType);
                }
                m_globalWeather.ThunderWaitDuration = m_editorUtils.FloatField("ThunderWaitDuration", m_globalWeather.ThunderWaitDuration, helpEnabled);
                if (m_globalWeather.ThunderWaitDuration < 0)
                {
                    m_globalWeather.ThunderWaitDuration = 0f;
                }
                m_globalWeather.m_thunderChance = m_editorUtils.Slider("ThunderChance", m_globalWeather.m_thunderChance, 0f, 1f, helpEnabled);
                m_globalWeather.ThunderLightRadius = m_editorUtils.FloatField("ThunderLightRadius", m_globalWeather.ThunderLightRadius, helpEnabled);
                if (m_globalWeather.ThunderLightRadius < 0)
                {
                    m_globalWeather.ThunderLightRadius = 0;
                }
                m_globalWeather.ThunderLightColor = m_editorUtils.ColorField("ThunderColor", m_globalWeather.ThunderLightColor, helpEnabled);
                m_globalWeather.ThunderLightIntensity = m_editorUtils.Slider("ThunderIntensity", m_globalWeather.ThunderLightIntensity, 0f, 25f, helpEnabled);
                for (int idx = 0; idx < m_globalWeather.ThunderAudioSources.Count; idx++)
                {
                    EditorGUILayout.BeginHorizontal();
                    m_globalWeather.ThunderAudioSources[idx] = (AudioClip)EditorGUILayout.ObjectField(new GUIContent(m_editorUtils.GetTextValue("ThunderAudioClip"), m_editorUtils.GetTooltip("ThunderAudioClip")), m_globalWeather.ThunderAudioSources[idx], typeof(AudioClip), false);
                    if (m_editorUtils.Button("-", GUILayout.MaxWidth(35f)))
                    {
                        m_globalWeather.ThunderAudioSources.RemoveAt(idx);
                    }
                    EditorGUILayout.EndHorizontal();
                    m_editorUtils.InlineHelp("ThunderAudioClip", helpEnabled);
                }
                if (m_editorUtils.Button("+"))
                {
                    m_globalWeather.ThunderAudioSources.Add(null);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Displays the weather settings panel for Snow or Rain
        /// </summary>
        /// <param name="weatherType"></param>
        /// <param name="weatherSettings"></param>
        /// <param name="guiSpace"></param>
        /// <param name="helpEnabled"></param>
        private void WeatherSettingsPanel(WeatherType weatherType, WeatherSettings weatherSettings, bool guiSpace, bool helpEnabled)
        {
            if (weatherType == WeatherType.Rain)
            {
                m_globalWeather.EnableRain = m_editorUtils.Toggle("EnableRain", m_globalWeather.EnableRain, helpEnabled);
                if (m_globalWeather.EnableRain)
                {
                    m_editorUtils.Heading("RainEffects");
                    EditorGUI.indentLevel++;
                    m_globalWeather.m_rainMode = (RainMode)m_editorUtils.EnumPopup("RainMode", m_globalWeather.m_rainMode, helpEnabled);
                    weatherSettings.m_chance = m_editorUtils.Slider("RainChance", weatherSettings.m_chance, 0f, 1f, helpEnabled);
                    if (m_globalWeather.m_rainMode == RainMode.SampledHeight)
                    {
                        m_globalWeather.m_rainHeight = m_editorUtils.FloatField("RainHeight", m_globalWeather.m_rainHeight, helpEnabled);
                    }
                    else
                    {
                        weatherSettings.m_channelSelection = (ChannelSelection)m_editorUtils.EnumPopup("ChannelSelection", weatherSettings.m_channelSelection, helpEnabled);
                        weatherSettings.m_noiseMask = (Texture2D)m_editorUtils.ObjectField("RainNoiseMask", weatherSettings.m_noiseMask, typeof(Texture2D), false, helpEnabled, GUILayout.Height(16f));
                        weatherSettings.m_durationMinWaitTime = m_editorUtils.FloatField("RainMinDuration", weatherSettings.m_durationMinWaitTime, helpEnabled);
                        weatherSettings.m_durationMaxWaitTime = m_editorUtils.FloatField("RainMaxDuration", weatherSettings.m_durationMaxWaitTime, helpEnabled);
                        weatherSettings.m_minWaitTime = m_editorUtils.FloatField("RainMinWaitTime", weatherSettings.m_minWaitTime, helpEnabled);
                        weatherSettings.m_maxWaitTime = m_editorUtils.FloatField("RainMaxWaitTime", weatherSettings.m_maxWaitTime, helpEnabled);
                    }

                    weatherSettings.m_ambientAudio = (AudioClip)m_editorUtils.ObjectField("RainAmbientAudio", weatherSettings.m_ambientAudio, typeof(AudioClip), true, helpEnabled, GUILayout.Height(16f));
                    if (m_globalWeather.m_rainHeight < 0)
                    {
                        m_globalWeather.m_rainHeight = 0;
                    }
                    weatherSettings.m_weatherParticleAlpha = m_editorUtils.CurveField("WeatherParticlesAlpha", weatherSettings.m_weatherParticleAlpha, helpEnabled);
                    GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue());

                    if (m_globalWeather.m_modifyClouds)
                    {
                        m_editorUtils.LabelField("CloudSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        weatherSettings.m_cloudDomeBrightness = m_editorUtils.CurveField("CloudDomeBrightness", weatherSettings.m_cloudDomeBrightness, helpEnabled);
                        weatherSettings.m_cloudDensity = m_editorUtils.CurveField("CloudDensity", weatherSettings.m_cloudDensity, helpEnabled);
                        weatherSettings.m_cloudThickness = m_editorUtils.CurveField("CloudThickness", weatherSettings.m_cloudThickness, helpEnabled);
                        weatherSettings.m_cloudSpeed = m_editorUtils.CurveField("CloudSpeed", weatherSettings.m_cloudSpeed, helpEnabled);
                        weatherSettings.m_newCloudOpacity = m_editorUtils.CurveField("CloudOpacity", weatherSettings.m_newCloudOpacity, helpEnabled);
                        GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 4.2f);
                        EditorGUI.indentLevel--;
                    }
                    if (m_globalWeather.m_modifyAmbient)
                    {
                        m_editorUtils.LabelField("AmbientSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        if (m_globalWeather.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                        {
                            weatherSettings.m_ambientIntensity = m_editorUtils.CurveField("AmbientIntensity", weatherSettings.m_ambientIntensity, helpEnabled);
                            GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue());
                        }
                        else
                        {
                            if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Skybox)
                            {
                                weatherSettings.m_ambientIntensity = m_editorUtils.CurveField("AmbientIntensity", weatherSettings.m_ambientIntensity, helpEnabled);
                                GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue());
                            }
                            else if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Trilight)
                            {
                                weatherSettings.m_ambientSkyColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("AmbientSkyColor"), m_editorUtils.GetTooltip("AmbientSkyColor")), weatherSettings.m_ambientSkyColor, true);
                                m_editorUtils.InlineHelp("AmbientSkyColor", helpEnabled);
                                weatherSettings.m_ambientEquatorColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("AmbientEquatorColor"), m_editorUtils.GetTooltip("AmbientEquatorColor")), weatherSettings.m_ambientEquatorColor, true);
                                m_editorUtils.InlineHelp("AmbientEquatorColor", helpEnabled);
                                weatherSettings.m_ambientGroundColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("AmbientGroundColor"), m_editorUtils.GetTooltip("AmbientGroundColor")), weatherSettings.m_ambientGroundColor, true);
                                m_editorUtils.InlineHelp("AmbientGroundColor", helpEnabled);
                                GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 2.2f);
                            }
                            else
                            {
                                weatherSettings.m_ambientSkyColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("AmbientSkyColor"), m_editorUtils.GetTooltip("AmbientSkyColor")), weatherSettings.m_ambientSkyColor, true);
                                m_editorUtils.InlineHelp("AmbientSkyColor", helpEnabled);
                                GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue());
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    if (m_globalWeather.m_modifyFog)
                    {
                        m_editorUtils.LabelField("FogSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        weatherSettings.m_fogColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("FogColor"), m_editorUtils.GetTooltip("FogColor")), weatherSettings.m_fogColor);
                        m_editorUtils.InlineHelp("FogColor", helpEnabled);
                        if (m_globalWeather.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                        {
#if HDPipeline
                            if (m_hdrpVolumeProfile == null)
                            {
                                EditorGUILayout.HelpBox("Unable to find a Sky volume profile in the scene. Setting Gaia default lighting will add a sky volume to your scene", MessageType.Info);
                                return;
                            }

                            if (m_hdrpVolumeProfile.TryGet(out VisualEnvironment))
                            {
                                weatherSettings.m_fogEndDistance = m_editorUtils.CurveField("EndFogDistance", weatherSettings.m_fogEndDistance, helpEnabled);
                                weatherSettings.m_fogHeight = m_editorUtils.CurveField("FogHeight", weatherSettings.m_fogHeight, helpEnabled);
                                weatherSettings.m_volumetricGlobalAnisotropy = m_editorUtils.CurveField("GlobalAnisotropy", weatherSettings.m_volumetricGlobalAnisotropy, helpEnabled);
                                weatherSettings.m_volumetricGlobalProbeDimmer = m_editorUtils.CurveField("GlobalProbeDimmer", weatherSettings.m_volumetricGlobalProbeDimmer, helpEnabled);
                                weatherSettings.m_volumetricDepthExtent = m_editorUtils.CurveField("DepthExtent", weatherSettings.m_volumetricDepthExtent, helpEnabled);
                            }
#endif
                        }
                        else
                        {
                            if (RenderSettings.fogMode == FogMode.Linear)
                            {
                                weatherSettings.m_fogStartDistance = m_editorUtils.CurveField("StartFogDistance", weatherSettings.m_fogStartDistance, helpEnabled);
                                weatherSettings.m_fogEndDistance = m_editorUtils.CurveField("EndFogDistance", weatherSettings.m_fogEndDistance, helpEnabled);

                                GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 2.2f);
                            }
                            else
                            {
                                weatherSettings.m_fogDensity = m_editorUtils.CurveField("FogDensity", weatherSettings.m_fogDensity, helpEnabled);
                                GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 1.2f);
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    if (m_globalWeather.m_modifySkybox)
                    {
                        m_editorUtils.LabelField("SkyboxSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        weatherSettings.m_skyboxExposure = m_editorUtils.CurveField("SkyboxExposure", weatherSettings.m_skyboxExposure, helpEnabled);
                        weatherSettings.m_skyboxTint = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("SkyboxTint"), m_editorUtils.GetTooltip("SkyboxTint")), weatherSettings.m_skyboxTint);
                        m_editorUtils.InlineHelp("SkyboxTint", helpEnabled);
                        weatherSettings.m_skyboxAtmosphereThickness = m_editorUtils.CurveField("SkyboxAtmosphereThickness", weatherSettings.m_skyboxAtmosphereThickness, helpEnabled);
                        weatherSettings.m_skyboxSkyboxFogHeight = m_editorUtils.CurveField("SkyboxFogHeight", weatherSettings.m_skyboxSkyboxFogHeight, helpEnabled);
                        weatherSettings.m_skyboxSkyboxFogGradient = m_editorUtils.CurveField("SkyboxFogGradient", weatherSettings.m_skyboxSkyboxFogGradient, helpEnabled);
                        EditorGUI.indentLevel--;
                        GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 4.2f);
                    }
                    if (m_globalWeather.m_modifySun)
                    {
                        m_editorUtils.LabelField("SunSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        weatherSettings.m_sunIntensity = m_editorUtils.CurveField("SunIntensity", weatherSettings.m_sunIntensity, helpEnabled);
                        weatherSettings.m_sunColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("SunColor"), m_editorUtils.GetTooltip("SunColor")), weatherSettings.m_sunColor);
                        m_editorUtils.InlineHelp("SunColor", helpEnabled);
                        EditorGUI.indentLevel--;
                        GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 1.2f);
                    }
                    if (m_globalWeather.m_modifyWind)
                    {
                        m_editorUtils.LabelField("WindSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        weatherSettings.m_windSpeed = m_editorUtils.Slider("WindSpeed", weatherSettings.m_windSpeed, 0f, 5f, helpEnabled);
                        weatherSettings.m_windTurbulence = m_editorUtils.Slider("WindTurbulence", weatherSettings.m_windTurbulence, 0f, 5f, helpEnabled);
                        weatherSettings.m_windFrequency = m_editorUtils.Slider("WindFrequency", weatherSettings.m_windFrequency, 0f, 5f, helpEnabled);
                        weatherSettings.m_windMultiplier = m_editorUtils.Slider("WindMultiplier", weatherSettings.m_windMultiplier, 1f, 50f, helpEnabled);
                        EditorGUI.indentLevel--;
                    }
                    if (m_globalWeather.m_modifyPostProcessing)
                    {
                        m_editorUtils.LabelField("PostProcessingSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        weatherSettings.m_fXBloomIntensity = m_editorUtils.CurveField("BloomIntensity", weatherSettings.m_fXBloomIntensity, helpEnabled);
                        weatherSettings.m_fXTemerature = m_editorUtils.CurveField("Temperature", weatherSettings.m_fXTemerature, helpEnabled);
                        weatherSettings.m_fXTint = m_editorUtils.CurveField("Tint", weatherSettings.m_fXTint, helpEnabled);
                        weatherSettings.m_fXColorFilter = EditorGUILayout.GradientField("Color Filter", weatherSettings.m_fXColorFilter);
                        m_editorUtils.InlineHelp("ColorFilter", helpEnabled);
                        weatherSettings.m_fXVignetteIntensity = m_editorUtils.CurveField("VignetteIntensity", weatherSettings.m_fXVignetteIntensity, helpEnabled);
                        EditorGUI.indentLevel--;
                        GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 4.2f);
                    }

                    EditorGUI.indentLevel--;
                }
            }
            else if (weatherType == WeatherType.Snow)
            {
                m_globalWeather.EnableSnow = m_editorUtils.Toggle("EnableSnow", m_globalWeather.EnableSnow, helpEnabled);
                if (m_globalWeather.EnableSnow)
                {
                    m_editorUtils.Heading("SnowCoverage");
                    EditorGUI.indentLevel++;
                    m_globalWeather.SnowCoverAlwaysEnabled = m_editorUtils.Toggle("SnowAlwaysEnabled", m_globalWeather.SnowCoverAlwaysEnabled, helpEnabled);
                    if (m_globalWeather.SnowCoverAlwaysEnabled)
                    {
                        EditorGUI.indentLevel++;
                        m_globalWeather.PermanentSnowHeight = m_editorUtils.FloatField("PermanentSnowHeight", m_globalWeather.PermanentSnowHeight, helpEnabled);
                        EditorGUI.indentLevel--;
                    }

                    m_globalWeather.SnowingHeight = m_editorUtils.FloatField("SnowingHeight", m_globalWeather.SnowingHeight, helpEnabled);
                    m_globalWeather.SnowFadeHeight = m_editorUtils.FloatField("SnowFadeHeight", m_globalWeather.SnowFadeHeight, helpEnabled);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space();
                    m_editorUtils.Heading("SnowEffect");
                    EditorGUI.indentLevel++;
                    m_globalWeather.m_snowMode = (SnowMode)m_editorUtils.EnumPopup("SnowMode", m_globalWeather.m_snowMode, helpEnabled);
                    if (m_globalWeather.m_snowMode == SnowMode.SampledHeight)
                    {
                        m_globalWeather.SnowHeight = m_editorUtils.FloatField("SnowHeight", m_globalWeather.SnowHeight, helpEnabled);
                    }
                    weatherSettings.m_chance = m_editorUtils.Slider("SnowChance", weatherSettings.m_chance, 0f, 1f, helpEnabled);
                    m_globalWeather.m_snowStormChance = m_editorUtils.Slider("SnowStormChance", m_globalWeather.m_snowStormChance, 0f, 1f, helpEnabled);
                    weatherSettings.m_channelSelection = (ChannelSelection)m_editorUtils.EnumPopup("ChannelSelection", weatherSettings.m_channelSelection, helpEnabled);
                    weatherSettings.m_noiseMask = (Texture2D)m_editorUtils.ObjectField("RainNoiseMask", weatherSettings.m_noiseMask, typeof(Texture2D), false, helpEnabled, GUILayout.Height(16f));
                    weatherSettings.m_durationMinWaitTime = m_editorUtils.FloatField("SnowMinDuration", weatherSettings.m_durationMinWaitTime, helpEnabled);
                    weatherSettings.m_durationMaxWaitTime = m_editorUtils.FloatField("SnowMaxDuration", weatherSettings.m_durationMaxWaitTime, helpEnabled);
                    weatherSettings.m_minWaitTime = m_editorUtils.FloatField("SnowMinWaitTime", weatherSettings.m_minWaitTime, helpEnabled);
                    weatherSettings.m_maxWaitTime = m_editorUtils.FloatField("SnowMaxWaitTime", weatherSettings.m_maxWaitTime, helpEnabled);
                    weatherSettings.m_ambientAudio = (AudioClip)m_editorUtils.ObjectField("SnowAmbientAudio", weatherSettings.m_ambientAudio, typeof(AudioClip), true, helpEnabled, GUILayout.Height(16f));
                    weatherSettings.m_weatherParticleAlpha = m_editorUtils.CurveField("WeatherParticlesAlpha", weatherSettings.m_weatherParticleAlpha, helpEnabled);
                    GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue());

                    if (m_globalWeather.m_modifyClouds)
                    {
                        m_editorUtils.LabelField("CloudSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        weatherSettings.m_cloudDomeBrightness = m_editorUtils.CurveField("CloudDomeBrightness", weatherSettings.m_cloudDomeBrightness, helpEnabled);
                        weatherSettings.m_cloudDensity = m_editorUtils.CurveField("CloudDensity", weatherSettings.m_cloudDensity, helpEnabled);
                        weatherSettings.m_cloudThickness = m_editorUtils.CurveField("CloudThickness", weatherSettings.m_cloudThickness, helpEnabled);
                        weatherSettings.m_cloudSpeed = m_editorUtils.CurveField("CloudSpeed", weatherSettings.m_cloudSpeed, helpEnabled);
                        weatherSettings.m_newCloudOpacity = m_editorUtils.CurveField("CloudOpacity", weatherSettings.m_newCloudOpacity, helpEnabled);
                        EditorGUI.indentLevel--;
                        GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 4.2f);
                    }
                    if (m_globalWeather.m_modifyAmbient)
                    {
                        m_editorUtils.LabelField("AmbientSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        if (m_globalWeather.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                        {
                            weatherSettings.m_ambientIntensity = m_editorUtils.CurveField("AmbientIntensity", weatherSettings.m_ambientIntensity, helpEnabled);
                            GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue());
                        }
                        else
                        {
                            if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Skybox)
                            {
                                weatherSettings.m_ambientIntensity = m_editorUtils.CurveField("AmbientIntensity", weatherSettings.m_ambientIntensity, helpEnabled);
                                GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue());
                            }
                            else if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Trilight)
                            {
                                weatherSettings.m_ambientSkyColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("AmbientSkyColor"), m_editorUtils.GetTooltip("AmbientSkyColor")), weatherSettings.m_ambientSkyColor);
                                m_editorUtils.InlineHelp("AmbientSkyColor", helpEnabled);
                                weatherSettings.m_ambientEquatorColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("AmbientEquatorColor"), m_editorUtils.GetTooltip("AmbientEquatorColor")), weatherSettings.m_ambientEquatorColor);
                                m_editorUtils.InlineHelp("AmbientEquatorColor", helpEnabled);
                                weatherSettings.m_ambientGroundColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("AmbientGroundColor"), m_editorUtils.GetTooltip("AmbientGroundColor")), weatherSettings.m_ambientGroundColor);
                                m_editorUtils.InlineHelp("AmbientGroundColor", helpEnabled);
                                GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 2.2f);
                            }
                            else
                            {
                                weatherSettings.m_ambientSkyColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("AmbientSkyColor"), m_editorUtils.GetTooltip("AmbientSkyColor")), weatherSettings.m_ambientSkyColor);
                                m_editorUtils.InlineHelp("AmbientSkyColor", helpEnabled);
                                GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue());
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    if (m_globalWeather.m_modifyFog)
                    {
                        m_editorUtils.LabelField("FogSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        weatherSettings.m_fogColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("FogColor"), m_editorUtils.GetTooltip("FogColor")), weatherSettings.m_fogColor);
                        m_editorUtils.InlineHelp("FogColor", helpEnabled);
                        if (m_globalWeather.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                        {
#if HDPipeline
                            if (m_hdrpVolumeProfile.TryGet(out VisualEnvironment))
                            {
                                weatherSettings.m_fogEndDistance = m_editorUtils.CurveField("EndFogDistance", weatherSettings.m_fogEndDistance, helpEnabled);
                                weatherSettings.m_fogHeight = m_editorUtils.CurveField("FogHeight", weatherSettings.m_fogHeight, helpEnabled);
                                weatherSettings.m_volumetricGlobalAnisotropy = m_editorUtils.CurveField("GlobalAnisotropy", weatherSettings.m_volumetricGlobalAnisotropy, helpEnabled);
                                weatherSettings.m_volumetricGlobalProbeDimmer = m_editorUtils.CurveField("GlobalProbeDimmer", weatherSettings.m_volumetricGlobalProbeDimmer, helpEnabled);
                                weatherSettings.m_volumetricDepthExtent = m_editorUtils.CurveField("DepthExtent", weatherSettings.m_volumetricDepthExtent, helpEnabled);
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("Unable to find a Sky volume profile in the scene. Setting Gaia default lighting will add a sky volume to your scene", MessageType.Info);
                            }
#endif
                        }
                        else
                        {
                            if (RenderSettings.fogMode == FogMode.Linear)
                            {
                                weatherSettings.m_fogStartDistance = m_editorUtils.CurveField("StartFogDistance", weatherSettings.m_fogStartDistance, helpEnabled);
                                weatherSettings.m_fogEndDistance = m_editorUtils.CurveField("EndFogDistance", weatherSettings.m_fogEndDistance, helpEnabled);
                                GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 2.2f);
                            }
                            else
                            {
                                weatherSettings.m_fogDensity = m_editorUtils.CurveField("FogDensity", weatherSettings.m_fogDensity);
                                GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 1.2f);
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    if (m_globalWeather.m_modifySkybox)
                    {
                        m_editorUtils.LabelField("SkyboxSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        weatherSettings.m_skyboxExposure = m_editorUtils.CurveField("SkyboxExposure", weatherSettings.m_skyboxExposure);
                        weatherSettings.m_skyboxTint = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("SkyboxTint"), m_editorUtils.GetTooltip("SkyboxTint")), weatherSettings.m_skyboxTint);
                        m_editorUtils.InlineHelp("SkyboxTint", helpEnabled);
                        weatherSettings.m_skyboxAtmosphereThickness = m_editorUtils.CurveField("SkyboxAtmosphereThickness", weatherSettings.m_skyboxAtmosphereThickness, helpEnabled);
                        weatherSettings.m_skyboxSkyboxFogHeight = m_editorUtils.CurveField("SkyboxFogHeight", weatherSettings.m_skyboxSkyboxFogHeight, helpEnabled);
                        weatherSettings.m_skyboxSkyboxFogGradient = m_editorUtils.CurveField("SkyboxFogGradient", weatherSettings.m_skyboxSkyboxFogGradient, helpEnabled);
                        EditorGUI.indentLevel--;
                        GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 4.2f);
                    }
                    if (m_globalWeather.m_modifySun)
                    {
                        m_editorUtils.LabelField("SunSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        weatherSettings.m_sunIntensity = m_editorUtils.CurveField("SunIntensity", weatherSettings.m_sunIntensity);
                        weatherSettings.m_sunColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("SunColor"), m_editorUtils.GetTooltip("SunColor")), weatherSettings.m_sunColor);
                        m_editorUtils.InlineHelp("SunColor", helpEnabled);
                        EditorGUI.indentLevel--;
                        GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 1.2f);
                    }
                    if (m_globalWeather.m_modifyWind)
                    {
                        m_editorUtils.LabelField("WindSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        weatherSettings.m_windSpeed = m_editorUtils.Slider("WindSpeed", weatherSettings.m_windSpeed, 0f, 5f, helpEnabled);
                        weatherSettings.m_windTurbulence = m_editorUtils.Slider("WindTurbulence", weatherSettings.m_windTurbulence, 0f, 5f, helpEnabled);
                        weatherSettings.m_windFrequency = m_editorUtils.Slider("WindFrequency", weatherSettings.m_windFrequency, 0f, 5f, helpEnabled);
                        weatherSettings.m_windMultiplier = m_editorUtils.Slider("WindMultiplier", weatherSettings.m_windMultiplier, 1f, 50f, helpEnabled);
                        EditorGUI.indentLevel--;
                    }
                    if (m_globalWeather.m_modifyPostProcessing)
                    {
                        m_editorUtils.LabelField("PostProcessingSettings", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        weatherSettings.m_fXBloomIntensity = m_editorUtils.CurveField("BloomIntensity", weatherSettings.m_fXBloomIntensity);
                        weatherSettings.m_fXTemerature = m_editorUtils.CurveField("Temperature", weatherSettings.m_fXTemerature);
                        weatherSettings.m_fXTint = m_editorUtils.CurveField("Tint", weatherSettings.m_fXTint);
                        weatherSettings.m_fXColorFilter = EditorGUILayout.GradientField("Color Filter", weatherSettings.m_fXColorFilter);
                        m_editorUtils.InlineHelp("ColorFilter", helpEnabled);
                        weatherSettings.m_fXVignetteIntensity = m_editorUtils.CurveField("VignetteIntensity", weatherSettings.m_fXVignetteIntensity);
                        EditorGUI.indentLevel--;
                        GaiaEditorUtils.DrawTimeOfDayLine(GaiaGlobal.GetTimeOfDayMainValue(), 4.2f);
                    }
                    EditorGUI.indentLevel--;
                }
            }

            if (guiSpace)
            {
                EditorGUILayout.Space();
            }
        }
        /// <summary>
        /// Sets the settings of the thunder based ont he storm type
        /// </summary>
        /// <param name="stormType"></param>
        private void SetStormType(GaiaConstants.ThunderStormType stormType)
        {
            switch (stormType)
            {
                case GaiaConstants.ThunderStormType.Light:
                    m_globalWeather.ThunderWaitDuration = 70f;
                    m_globalWeather.m_thunderLightRadius = 350f;
                    m_globalWeather.ThunderLightIntensity = 1.7f;
                    break;
                case GaiaConstants.ThunderStormType.Moderate:
                    m_globalWeather.ThunderWaitDuration = 30;
                    m_globalWeather.m_thunderLightRadius = 500f;
                    m_globalWeather.ThunderLightIntensity = 2f;
                    break;
                case GaiaConstants.ThunderStormType.Continuous:
                    m_globalWeather.ThunderWaitDuration = 10f;
                    m_globalWeather.m_thunderLightRadius = 700f;
                    m_globalWeather.ThunderLightIntensity = 3f;
                    break;
            }

            m_globalWeather.m_stormType = stormType;
        }
    }
}
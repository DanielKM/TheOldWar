using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if CTS_PRESENT
using CTS;
#endif
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif
using ProceduralWorlds.WaterSystem;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Gaia
{
    public enum SeasonMode { Autumn, Winter, Spring, Summer }
    public enum RainMode { RandomChance, SampledHeight }
    public enum SnowMode { RandomChance, SampledHeight }
    public enum ChannelSelection { R, G, B, A }
    public enum CollisionDetectionType { Box, Spherical }
    public enum SurfaceType { Opaque, Transparent }
    public enum BlendMode { Alpha, Premultiply, Additive, Multiply }

    [System.Serializable]
    public class WeatherSettings
    {
        public ChannelSelection m_channelSelection;
        public AudioClip m_ambientAudio;
        public Texture2D m_noiseMask;
        //Fog
        public Gradient m_fogColor;
        public AnimationCurve m_fogStartDistance;
        public AnimationCurve m_fogEndDistance;
        public AnimationCurve m_fogDensity;
        //Skybox
        public AnimationCurve m_skyboxExposure;
        public Gradient m_skyboxTint;
        public AnimationCurve m_skyboxAtmosphereThickness;
        public AnimationCurve m_skyboxSkyboxFogHeight;
        public AnimationCurve m_skyboxSkyboxFogGradient;
        //Sun
        public Gradient m_sunColor;
        public AnimationCurve m_sunIntensity;
        //Post fx
        public AnimationCurve m_fXTemerature;
        public AnimationCurve m_fXTint;
        public Gradient m_fXColorFilter;
        public AnimationCurve m_fXBloomIntensity;
        public AnimationCurve m_fXVignetteIntensity;
        //Wind
        public float m_windSpeed;
        public float m_windTurbulence;
        public float m_windFrequency;
        public float m_windMultiplier;
        public AnimationCurve m_ambientIntensity;
        public Gradient m_ambientSkyColor;
        public Gradient m_ambientEquatorColor;
        public Gradient m_ambientGroundColor;
        public float m_chance;
        public float m_minWaitTime;
        public float m_maxWaitTime;
        public float m_durationMinWaitTime;
        public float m_durationMaxWaitTime;
        //HDRP
        public AnimationCurve m_volumetricDepthExtent;
        public AnimationCurve m_volumetricGlobalAnisotropy;
        public AnimationCurve m_volumetricGlobalProbeDimmer;
        public AnimationCurve m_fogHeight;
        //PW Cloud System
        public AnimationCurve m_cloudDomeBrightness;
        public AnimationCurve m_cloudDensity;
        public AnimationCurve m_cloudThickness;
        public AnimationCurve m_cloudSpeed;
        public AnimationCurve m_newCloudOpacity;
        //Weather particles
        public AnimationCurve m_weatherParticleAlpha;

        public void Load(WeatherSettings settings, ProceduralWorldsGlobalWeather globalWeather, bool isRain)
        {
            if (isRain)
            {
                globalWeather.m_rainWeatherSettings.m_channelSelection = settings.m_channelSelection;
                globalWeather.m_rainWeatherSettings.m_ambientAudio = settings.m_ambientAudio;
                globalWeather.m_rainWeatherSettings.m_noiseMask = settings.m_noiseMask;
                globalWeather.m_rainWeatherSettings.m_fogColor = settings.m_fogColor;
                globalWeather.m_rainWeatherSettings.m_fogStartDistance = settings.m_fogStartDistance;
                globalWeather.m_rainWeatherSettings.m_fogEndDistance = settings.m_fogEndDistance;
                globalWeather.m_rainWeatherSettings.m_fogDensity = settings.m_fogDensity;
                globalWeather.m_rainWeatherSettings.m_skyboxExposure = settings.m_skyboxExposure;
                globalWeather.m_rainWeatherSettings.m_skyboxTint = settings.m_skyboxTint;
                globalWeather.m_rainWeatherSettings.m_skyboxAtmosphereThickness = settings.m_skyboxAtmosphereThickness;
                globalWeather.m_rainWeatherSettings.m_sunColor = settings.m_sunColor;
                globalWeather.m_rainWeatherSettings.m_sunIntensity = settings.m_sunIntensity;
                globalWeather.m_rainWeatherSettings.m_fXTemerature = settings.m_fXTemerature;
                globalWeather.m_rainWeatherSettings.m_fXTint = settings.m_fXTint;
                globalWeather.m_rainWeatherSettings.m_fXColorFilter = settings.m_fXColorFilter;
                globalWeather.m_rainWeatherSettings.m_fXBloomIntensity = settings.m_fXBloomIntensity;
                globalWeather.m_rainWeatherSettings.m_fXVignetteIntensity = settings.m_fXVignetteIntensity;
                globalWeather.m_rainWeatherSettings.m_windSpeed = settings.m_windSpeed;
                globalWeather.m_rainWeatherSettings.m_windTurbulence = settings.m_windTurbulence;
                globalWeather.m_rainWeatherSettings.m_windFrequency = settings.m_windFrequency;
                globalWeather.m_rainWeatherSettings.m_windMultiplier = settings.m_windMultiplier;
                globalWeather.m_rainWeatherSettings.m_ambientIntensity = settings.m_ambientIntensity;
                globalWeather.m_rainWeatherSettings.m_ambientSkyColor = settings.m_ambientSkyColor;
                globalWeather.m_rainWeatherSettings.m_ambientEquatorColor = settings.m_ambientEquatorColor;
                globalWeather.m_rainWeatherSettings.m_ambientGroundColor = settings.m_ambientGroundColor;
                globalWeather.m_rainWeatherSettings.m_chance = settings.m_chance;
                globalWeather.m_rainWeatherSettings.m_minWaitTime = settings.m_minWaitTime;
                globalWeather.m_rainWeatherSettings.m_maxWaitTime = settings.m_maxWaitTime;
                globalWeather.m_rainWeatherSettings.m_durationMinWaitTime = settings.m_durationMinWaitTime;
                globalWeather.m_rainWeatherSettings.m_durationMaxWaitTime = settings.m_durationMaxWaitTime;
                globalWeather.m_rainWeatherSettings.m_cloudDomeBrightness = settings.m_cloudDomeBrightness;
                globalWeather.m_rainWeatherSettings.m_volumetricDepthExtent = settings.m_volumetricDepthExtent;
                globalWeather.m_rainWeatherSettings.m_volumetricGlobalAnisotropy = settings.m_volumetricGlobalAnisotropy;
                globalWeather.m_rainWeatherSettings.m_volumetricGlobalProbeDimmer = settings.m_volumetricGlobalProbeDimmer;
                globalWeather.m_rainWeatherSettings.m_fogHeight = settings.m_fogHeight;
                globalWeather.m_rainWeatherSettings.m_cloudDensity = settings.m_cloudDensity;
                globalWeather.m_rainWeatherSettings.m_cloudThickness = settings.m_cloudThickness;
                globalWeather.m_rainWeatherSettings.m_cloudSpeed = settings.m_cloudSpeed;
                globalWeather.m_rainWeatherSettings.m_newCloudOpacity = settings.m_newCloudOpacity;
                globalWeather.m_rainWeatherSettings.m_weatherParticleAlpha = settings.m_weatherParticleAlpha;
                globalWeather.m_rainWeatherSettings.m_skyboxSkyboxFogHeight = settings.m_skyboxSkyboxFogHeight;
                globalWeather.m_rainWeatherSettings.m_skyboxSkyboxFogGradient = settings.m_skyboxSkyboxFogGradient;
            }
            else
            {
                globalWeather.m_snowWeatherSettings.m_channelSelection = settings.m_channelSelection;
                globalWeather.m_snowWeatherSettings.m_ambientAudio = settings.m_ambientAudio;
                globalWeather.m_snowWeatherSettings.m_noiseMask = settings.m_noiseMask;
                globalWeather.m_snowWeatherSettings.m_fogColor = settings.m_fogColor;
                globalWeather.m_snowWeatherSettings.m_fogStartDistance = settings.m_fogStartDistance;
                globalWeather.m_snowWeatherSettings.m_fogEndDistance = settings.m_fogEndDistance;
                globalWeather.m_snowWeatherSettings.m_fogDensity = settings.m_fogDensity;
                globalWeather.m_snowWeatherSettings.m_skyboxExposure = settings.m_skyboxExposure;
                globalWeather.m_snowWeatherSettings.m_skyboxTint = settings.m_skyboxTint;
                globalWeather.m_snowWeatherSettings.m_skyboxAtmosphereThickness = settings.m_skyboxAtmosphereThickness;
                globalWeather.m_snowWeatherSettings.m_sunColor = settings.m_sunColor;
                globalWeather.m_snowWeatherSettings.m_sunIntensity = settings.m_sunIntensity;
                globalWeather.m_snowWeatherSettings.m_fXTemerature = settings.m_fXTemerature;
                globalWeather.m_snowWeatherSettings.m_fXTint = settings.m_fXTint;
                globalWeather.m_snowWeatherSettings.m_fXColorFilter = settings.m_fXColorFilter;
                globalWeather.m_snowWeatherSettings.m_fXBloomIntensity = settings.m_fXBloomIntensity;
                globalWeather.m_snowWeatherSettings.m_fXVignetteIntensity = settings.m_fXVignetteIntensity;
                globalWeather.m_snowWeatherSettings.m_windSpeed = settings.m_windSpeed;
                globalWeather.m_snowWeatherSettings.m_windTurbulence = settings.m_windTurbulence;
                globalWeather.m_snowWeatherSettings.m_windFrequency = settings.m_windFrequency;
                globalWeather.m_snowWeatherSettings.m_windMultiplier = settings.m_windMultiplier;
                globalWeather.m_snowWeatherSettings.m_ambientIntensity = settings.m_ambientIntensity;
                globalWeather.m_snowWeatherSettings.m_ambientSkyColor = settings.m_ambientSkyColor;
                globalWeather.m_snowWeatherSettings.m_ambientEquatorColor = settings.m_ambientEquatorColor;
                globalWeather.m_snowWeatherSettings.m_ambientGroundColor = settings.m_ambientGroundColor;
                globalWeather.m_snowWeatherSettings.m_chance = settings.m_chance;
                globalWeather.m_snowWeatherSettings.m_minWaitTime = settings.m_minWaitTime;
                globalWeather.m_snowWeatherSettings.m_maxWaitTime = settings.m_maxWaitTime;
                globalWeather.m_snowWeatherSettings.m_durationMinWaitTime = settings.m_durationMinWaitTime;
                globalWeather.m_snowWeatherSettings.m_durationMaxWaitTime = settings.m_durationMaxWaitTime;
                globalWeather.m_snowWeatherSettings.m_cloudDomeBrightness = settings.m_cloudDomeBrightness;
                globalWeather.m_snowWeatherSettings.m_volumetricDepthExtent = settings.m_volumetricDepthExtent;
                globalWeather.m_snowWeatherSettings.m_volumetricGlobalAnisotropy = settings.m_volumetricGlobalAnisotropy;
                globalWeather.m_snowWeatherSettings.m_volumetricGlobalProbeDimmer = settings.m_volumetricGlobalProbeDimmer;
                globalWeather.m_snowWeatherSettings.m_fogHeight = settings.m_fogHeight;
                globalWeather.m_snowWeatherSettings.m_cloudDensity = settings.m_cloudDensity;
                globalWeather.m_snowWeatherSettings.m_cloudThickness = settings.m_cloudThickness;
                globalWeather.m_snowWeatherSettings.m_cloudSpeed = settings.m_cloudSpeed;
                globalWeather.m_snowWeatherSettings.m_newCloudOpacity = settings.m_newCloudOpacity;
                globalWeather.m_snowWeatherSettings.m_weatherParticleAlpha = settings.m_weatherParticleAlpha;
                globalWeather.m_snowWeatherSettings.m_skyboxSkyboxFogHeight = settings.m_skyboxSkyboxFogHeight;
                globalWeather.m_snowWeatherSettings.m_skyboxSkyboxFogGradient = settings.m_skyboxSkyboxFogGradient;
            }
        }
        public void Save(WeatherSettings settings, ProceduralWorldsGlobalWeather globalWeather, bool isRain)
        {
            if (isRain)
            {
                settings.m_channelSelection = globalWeather.m_rainWeatherSettings.m_channelSelection;
                settings.m_ambientAudio = globalWeather.m_rainWeatherSettings.m_ambientAudio;
                settings.m_noiseMask = globalWeather.m_rainWeatherSettings.m_noiseMask;
                settings.m_fogColor = globalWeather.m_rainWeatherSettings.m_fogColor;
                settings.m_fogStartDistance = globalWeather.m_rainWeatherSettings.m_fogStartDistance;
                settings.m_fogEndDistance = globalWeather.m_rainWeatherSettings.m_fogEndDistance;
                settings.m_fogDensity = globalWeather.m_rainWeatherSettings.m_fogDensity;
                settings.m_skyboxExposure = globalWeather.m_rainWeatherSettings.m_skyboxExposure;
                settings.m_skyboxTint = globalWeather.m_rainWeatherSettings.m_skyboxTint;
                settings.m_skyboxAtmosphereThickness = globalWeather.m_rainWeatherSettings.m_skyboxAtmosphereThickness;
                settings.m_sunColor = globalWeather.m_rainWeatherSettings.m_sunColor;
                settings.m_sunIntensity = globalWeather.m_rainWeatherSettings.m_sunIntensity;
                settings.m_fXTemerature = globalWeather.m_rainWeatherSettings.m_fXTemerature;
                settings.m_fXTint = globalWeather.m_rainWeatherSettings.m_fXTint;
                settings.m_fXColorFilter = globalWeather.m_rainWeatherSettings.m_fXColorFilter;
                settings.m_fXBloomIntensity = globalWeather.m_rainWeatherSettings.m_fXBloomIntensity;
                settings.m_fXVignetteIntensity = globalWeather.m_rainWeatherSettings.m_fXVignetteIntensity;
                settings.m_windSpeed = globalWeather.m_rainWeatherSettings.m_windSpeed;
                settings.m_windTurbulence = globalWeather.m_rainWeatherSettings.m_windTurbulence;
                settings.m_windFrequency = globalWeather.m_rainWeatherSettings.m_windFrequency;
                settings.m_windMultiplier = globalWeather.m_rainWeatherSettings.m_windMultiplier;
                settings.m_ambientIntensity = globalWeather.m_rainWeatherSettings.m_ambientIntensity;
                settings.m_ambientSkyColor = globalWeather.m_rainWeatherSettings.m_ambientSkyColor;
                settings.m_ambientEquatorColor = globalWeather.m_rainWeatherSettings.m_ambientEquatorColor;
                settings.m_ambientGroundColor = globalWeather.m_rainWeatherSettings.m_ambientGroundColor;
                settings.m_chance = globalWeather.m_rainWeatherSettings.m_chance;
                settings.m_minWaitTime = globalWeather.m_rainWeatherSettings.m_minWaitTime;
                settings.m_maxWaitTime = globalWeather.m_rainWeatherSettings.m_maxWaitTime;
                settings.m_durationMinWaitTime = globalWeather.m_rainWeatherSettings.m_durationMinWaitTime;
                settings.m_durationMaxWaitTime = globalWeather.m_rainWeatherSettings.m_durationMaxWaitTime;
                settings.m_cloudDomeBrightness = globalWeather.m_rainWeatherSettings.m_cloudDomeBrightness;
                settings.m_volumetricDepthExtent = globalWeather.m_rainWeatherSettings.m_volumetricDepthExtent;
                settings.m_volumetricGlobalAnisotropy = globalWeather.m_rainWeatherSettings.m_volumetricGlobalAnisotropy;
                settings.m_volumetricGlobalProbeDimmer = globalWeather.m_rainWeatherSettings.m_volumetricGlobalProbeDimmer;
                settings.m_fogHeight = globalWeather.m_rainWeatherSettings.m_fogHeight;
                settings.m_cloudDensity = globalWeather.m_rainWeatherSettings.m_cloudDensity;
                settings.m_cloudThickness = globalWeather.m_rainWeatherSettings.m_cloudThickness;
                settings.m_cloudSpeed = globalWeather.m_rainWeatherSettings.m_cloudSpeed;
                settings.m_newCloudOpacity = globalWeather.m_rainWeatherSettings.m_newCloudOpacity;
                settings.m_weatherParticleAlpha = globalWeather.m_rainWeatherSettings.m_weatherParticleAlpha;
                settings.m_skyboxSkyboxFogHeight = globalWeather.m_rainWeatherSettings.m_skyboxSkyboxFogHeight;
                settings.m_skyboxSkyboxFogGradient = globalWeather.m_rainWeatherSettings.m_skyboxSkyboxFogGradient;
            }
            else
            {
                settings.m_channelSelection = globalWeather.m_snowWeatherSettings.m_channelSelection;
                settings.m_ambientAudio = globalWeather.m_snowWeatherSettings.m_ambientAudio;
                settings.m_noiseMask = globalWeather.m_snowWeatherSettings.m_noiseMask;
                settings.m_fogColor = globalWeather.m_snowWeatherSettings.m_fogColor;
                settings.m_fogStartDistance = globalWeather.m_snowWeatherSettings.m_fogStartDistance;
                settings.m_fogEndDistance = globalWeather.m_snowWeatherSettings.m_fogEndDistance;
                settings.m_fogDensity = globalWeather.m_snowWeatherSettings.m_fogDensity;
                settings.m_skyboxExposure = globalWeather.m_snowWeatherSettings.m_skyboxExposure;
                settings.m_skyboxTint = globalWeather.m_snowWeatherSettings.m_skyboxTint;
                settings.m_skyboxAtmosphereThickness = globalWeather.m_snowWeatherSettings.m_skyboxAtmosphereThickness;
                settings.m_sunColor = globalWeather.m_snowWeatherSettings.m_sunColor;
                settings.m_sunIntensity = globalWeather.m_snowWeatherSettings.m_sunIntensity;
                settings.m_fXTemerature = globalWeather.m_snowWeatherSettings.m_fXTemerature;
                settings.m_fXTint = globalWeather.m_snowWeatherSettings.m_fXTint;
                settings.m_fXColorFilter = globalWeather.m_snowWeatherSettings.m_fXColorFilter;
                settings.m_fXBloomIntensity = globalWeather.m_snowWeatherSettings.m_fXBloomIntensity;
                settings.m_fXVignetteIntensity = globalWeather.m_snowWeatherSettings.m_fXVignetteIntensity;
                settings.m_windSpeed = globalWeather.m_snowWeatherSettings.m_windSpeed;
                settings.m_windTurbulence = globalWeather.m_snowWeatherSettings.m_windTurbulence;
                settings.m_windFrequency = globalWeather.m_snowWeatherSettings.m_windFrequency;
                settings.m_windMultiplier = globalWeather.m_snowWeatherSettings.m_windMultiplier;
                settings.m_ambientIntensity = globalWeather.m_snowWeatherSettings.m_ambientIntensity;
                settings.m_ambientSkyColor = globalWeather.m_snowWeatherSettings.m_ambientSkyColor;
                settings.m_ambientEquatorColor = globalWeather.m_snowWeatherSettings.m_ambientEquatorColor;
                settings.m_ambientGroundColor = globalWeather.m_snowWeatherSettings.m_ambientGroundColor;
                settings.m_chance = globalWeather.m_snowWeatherSettings.m_chance;
                settings.m_minWaitTime = globalWeather.m_snowWeatherSettings.m_minWaitTime;
                settings.m_maxWaitTime = globalWeather.m_snowWeatherSettings.m_maxWaitTime;
                settings.m_durationMinWaitTime = globalWeather.m_snowWeatherSettings.m_durationMinWaitTime;
                settings.m_durationMaxWaitTime = globalWeather.m_snowWeatherSettings.m_durationMaxWaitTime;
                settings.m_cloudDomeBrightness = globalWeather.m_snowWeatherSettings.m_cloudDomeBrightness;
                settings.m_volumetricDepthExtent = globalWeather.m_snowWeatherSettings.m_volumetricDepthExtent;
                settings.m_volumetricGlobalAnisotropy = globalWeather.m_snowWeatherSettings.m_volumetricGlobalAnisotropy;
                settings.m_volumetricGlobalProbeDimmer = globalWeather.m_snowWeatherSettings.m_volumetricGlobalProbeDimmer;
                settings.m_fogHeight = globalWeather.m_snowWeatherSettings.m_fogHeight;
                settings.m_cloudDensity = globalWeather.m_snowWeatherSettings.m_cloudDensity;
                settings.m_cloudThickness = globalWeather.m_snowWeatherSettings.m_cloudThickness;
                settings.m_cloudSpeed = globalWeather.m_snowWeatherSettings.m_cloudSpeed;
                settings.m_newCloudOpacity = globalWeather.m_snowWeatherSettings.m_newCloudOpacity;
                settings.m_weatherParticleAlpha = globalWeather.m_snowWeatherSettings.m_weatherParticleAlpha;
                settings.m_skyboxSkyboxFogHeight = globalWeather.m_snowWeatherSettings.m_skyboxSkyboxFogHeight;
                settings.m_skyboxSkyboxFogGradient = globalWeather.m_snowWeatherSettings.m_skyboxSkyboxFogGradient;
            }
        }
    }

    [System.Serializable]
    public class WeatherCurrentSettings
    {
        //Fog
        public Color m_fogColor;
        public float m_fogDensity;
        public float m_fogStart;
        public float m_fogEnd;

        //Sun
        public Color m_sunColor;
        public float m_sunIntensity;

        //Snow
        public float m_snowHeight;

        //Ambient
        public Color m_skyAmbient;
        public Color m_equatorAmbient;
        public Color m_groundAmbient;
        public float m_intensityAmbient;

        //Skybox
        public float m_skyboxExposure;
        public Color m_skyboxTint;
        public float m_skyboxAtmosphereThickness;
        public float m_skyboxFogHeight;
        public float m_skyboxFogGradient;

        //Cloud
        public float m_cloudDomeBrightness;
        public float m_startCloudDomeBrightness;
        public float m_cloudDensity;
        public float m_cloudThickness;
        public float m_cloudSpeed;
        public float m_cloudOpacity;

        //Post FX
        public float m_fxBloomIntensity;
        public float m_fxTemperature;
        public float m_fxTint;
        public float m_fxVignetteIntensity;
        public Color m_fxColorFilter;

        //Volume
        public float m_rainAudioVolume;
        public float m_snowAudioVolume;

        //Wind
        public float m_windSpeed;
        public float m_windTurbulence;
        public float m_windFrequency;
        public float m_windMultiplier;

        //HDRP Fog
        public float m_hdrpFogHeight;
        public float m_hdrpFogAnisotropy;
        public float m_hdrpFogLightProbeDimmer;
        public float m_hdrpFogDepthExtent;
    }

    [System.Serializable]
    public class WeatherPostFXSceneSettings
    {
        //Post FX
        public float m_fxBloomIntensity;
        public float m_fxTemperature;
        public float m_fxTint;
        public float m_fxVignetteIntensity;
        public Color m_fxColorFilter;
        //Wind
        public float m_windSpeed;
        public float m_windTurbulence;
        public float m_windFrequency;
        public float m_windMultiplier;
    }

    [ExecuteAlways]
    public class ProceduralWorldsGlobalWeather : MonoBehaviour
    {
        public static ProceduralWorldsGlobalWeather Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = FindObjectOfType<ProceduralWorldsGlobalWeather>();
                }
                return m_instance;
            }
        }
        [SerializeField]
        private static ProceduralWorldsGlobalWeather m_instance;

        #region Public Values

        //Sun DOF
        public bool m_enableAutoDOF;
        public Camera m_camera;
        public LayerMask m_depthOfFieldDetectionLayers;

        //Volume
        public float GlobalVolume
        {
            get { return m_globalVolume; }
            set
            {
                if (m_globalVolume != value)
                {
                    m_globalVolume = value;
                    UpdateAudioVolume();
                }
            }
        }
        public float RainVolume
        {
            get { return m_rainVolume; }
            set
            {
                if (m_rainVolume != value)
                {
                    m_rainVolume = value;
                    UpdateAudioVolume();
                }
            }
        }
        public float SnowVolume
        {
            get { return m_snowVolume; }
            set
            {
                if (m_snowVolume != value)
                {
                    m_snowVolume = value;
                    UpdateAudioVolume();
                }
            }
        }
        public float m_thunderVolume = 1f;

        //Global
        public float AdditionalFogDistanceLinear
        {
            get { return m_additionalFogDistanceLinear; }
            set
            {
                if (m_additionalFogDistanceLinear != value)
                {
                    m_additionalFogDistanceLinear = value;
                    if (AtmosphereSystemValid)
                    {
                        PW_VFX_Atmosphere.Instance.AdditionalFogDistanceLinear = value;
                        PW_VFX_Atmosphere.Instance.UpdateSystem();
                    }
                }
            }
        }
        [SerializeField] private float m_additionalFogDistanceLinear = 0f;

        public float AdditionalFogDistanceExponential
        {
            get { return m_additionalFogDistanceExponential; }
            set
            {
                if (m_additionalFogDistanceExponential != value)
                {
                    m_additionalFogDistanceExponential = value;
                    if (AtmosphereSystemValid)
                    {
                        PW_VFX_Atmosphere.Instance.AdditionalFogDistanceExponential = value;
                        PW_VFX_Atmosphere.Instance.UpdateSystem();
                    }
                }
            }
        }
        [SerializeField] private float m_additionalFogDistanceExponential = 0f;
        public bool m_instantStartStop = true;
        public bool m_instantVFX = false;
        public bool m_disableWeatherFX = false;
        public bool m_weatherSystemActive;
        public bool m_getCurrentSnowValue;
        public float m_currentSnowIntensity;
        public float m_currentSnowVolume;
        public bool m_getCurrentRainValue;
        public float m_currentRainVolume;
        public float m_currentRainIntensity;
        public float m_currentRainParticleAlpha;
        public GaiaConstants.EnvironmentRenderer m_renderPipeline = GaiaConstants.EnvironmentRenderer.BuiltIn;
        public bool RunInEditor
        {
            get { return m_runInEditor; }
            set
            {
                if (m_runInEditor != value)
                {
                    m_runInEditor = value;
                    CheckEditorRuntimeValues();
                }
            }
        }
        public float SystemStepSize
        {
            get { return m_stepStepSize; }
            set
            {
                if (m_stepStepSize != value)
                {
                    m_stepStepSize = value;
                    m_rainStepSize = value;
                    m_snowStepSize = value;
                    //m_weatherStepSize = value;
                }
            }
        }
        public bool IsRaining;
        public bool IsSnowing;
        public bool DoesAtmosphereNeedUpdate;
        public WeatherCurrentSettings CurrentSettings = new WeatherCurrentSettings();
        public WeatherPostFXSceneSettings CurrentScenePostFXSettings = new WeatherPostFXSceneSettings();
        public bool IsNetworkSynced;
        public bool ForceUpdateSystem = false;

        //Player
        public Transform m_player;
        public Vector3 LastKnownLocation
        {
            get { return m_lastKnownLocation; }
            set
            {
                if (m_lastKnownLocation != value)
                {
                    m_lastKnownLocation = m_player.transform.position;
                    //Resample the stregnth on the mask at location
                    m_rainSampleStrength = SampleLocationWithMask(m_rainWeatherSettings.m_noiseMask, m_lastKnownLocation, m_rainWeatherSettings.m_channelSelection);
                    m_snowSampleStrength = SampleLocationWithMask(m_snowWeatherSettings.m_noiseMask, m_lastKnownLocation, m_snowWeatherSettings.m_channelSelection);
                }
            }
        }

        //Weather Particles
        public ParticleSystem m_snowParticles;
        public ParticleSystem.EmissionModule m_snowParticleEmission;
        public ParticleSystem m_snowVFX;
        public ParticleSystem.EmissionModule m_snowVFXEmission;
        public MeshRenderer m_rainParticles;
        public ParticleSystem m_rainVFX;
        public GameObject m_cloudsMeshRenderer;

        //Atmosphere
        public Gradient TODSkyboxTint
        {
            get { return m_todSkyboxTint; }
            set
            {
                if (m_todSkyboxTint != value)
                {
                    m_todSkyboxTint = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODSkyboxFogHeight
        {
            get { return m_todSkyboxFogHeight; }
            set
            {
                if (m_todSkyboxFogHeight != value)
                {
                    m_todSkyboxFogHeight = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODSkyboxFogGradient
        {
            get { return m_todSkyboxFogGradient; }
            set
            {
                if (m_todSkyboxFogGradient != value)
                {
                    m_todSkyboxFogGradient = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODSkyboxExposure
        {
            get { return m_todSkyboxExposure; }
            set
            {
                if (m_todSkyboxExposure != value)
                {
                    m_todSkyboxExposure = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public Gradient TODSunColor
        {
            get { return m_todSunColor; }
            set
            {
                if (m_todSunColor != value)
                {
                    m_todSunColor = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public Gradient TODFogColor
        {
            get { return m_todFogColor; }
            set
            {
                if (m_todFogColor != value)
                {
                    m_todFogColor = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public Gradient TODAmbientSkyColor
        {
            get { return m_todAmbientSkyColor; }
            set
            {
                if (m_todAmbientSkyColor != value)
                {
                    m_todAmbientSkyColor = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public Gradient TODAmbientEquatorColor
        {
            get { return m_todAmbientEquatorColor; }
            set
            {
                if (m_todAmbientEquatorColor != value)
                {
                    m_todAmbientEquatorColor = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public Gradient TODAmbientGroundColor
        {
            get { return m_todAmbientGroundColor; }
            set
            {
                if (m_todAmbientGroundColor != value)
                {
                    m_todAmbientGroundColor = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODSunIntensity
        {
            get { return m_todSunIntensity; }
            set
            {
                if (m_todSunIntensity != value)
                {
                    m_todSunIntensity = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODSunShadowStrength
        {
            get { return m_todSunShadowStrength; }
            set
            {
                if (m_todSunShadowStrength != value)
                {
                    m_todSunShadowStrength = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODCloudHeightLevelDensity
        {
            get { return m_todCloudHeightLevelDensity; }
            set
            {
                if (m_todCloudHeightLevelDensity != value)
                {
                    m_todCloudHeightLevelDensity = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODCloudHeightLevelThickness
        {
            get { return m_todCloudHeightLevelThickness; }
            set
            {
                if (m_todCloudHeightLevelThickness != value)
                {
                    m_todCloudHeightLevelThickness = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODCloudHeightLevelSpeed
        {
            get { return m_todCloudHeightLevelSpeed; }
            set
            {
                if (m_todCloudHeightLevelSpeed != value)
                {
                    m_todCloudHeightLevelSpeed = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODCloudOpacity
        {
            get { return m_todCloudOpacity; }
            set
            {
                if (m_todCloudOpacity != value)
                {
                    m_todCloudOpacity = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODAmbientIntensity
        {
            get { return m_todAmbientIntensity; }
            set
            {
                if (m_todAmbientIntensity != value)
                {
                    m_todAmbientIntensity = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODAtmosphereThickness
        {
            get { return m_todAtmosphereThickness; }
            set
            {
                if (m_todAtmosphereThickness != value)
                {
                    m_todAtmosphereThickness = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODFogDensity
        {
            get { return m_todFogDensity; }
            set
            {
                if (m_todFogDensity != value)
                {
                    m_todFogDensity = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODFogStartDistance
        {
            get { return m_todFogStartDistance; }
            set
            {
                if (m_todFogStartDistance != value)
                {
                    m_todFogStartDistance = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODFogEndDistance
        {
            get { return m_todFogEndDistance; }
            set
            {
                if (m_todFogEndDistance != value)
                {
                    m_todFogEndDistance = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODHDRPFogBaseHeight
        {
            get { return m_todHDRPFogBaseHeight; }
            set
            {
                if (m_todHDRPFogBaseHeight != value)
                {
                    m_todHDRPFogBaseHeight = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODHDRPFogAnisotropy
        {
            get { return m_todHDRPFogAnisotropy; }
            set
            {
                if (m_todHDRPFogAnisotropy != value)
                {
                    m_todHDRPFogAnisotropy = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODHDRPFogLightProbeDimmer
        {
            get { return m_todHDRPFogLightProbeDimmer; }
            set
            {
                if (m_todHDRPFogLightProbeDimmer != value)
                {
                    m_todHDRPFogLightProbeDimmer = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODHDRPFogDepthExtent
        {
            get { return m_todHDRPFogDepthExtent; }
            set
            {
                if (m_todHDRPFogDepthExtent != value)
                {
                    m_todHDRPFogDepthExtent = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public Gradient TODHDRPGroundTint
        {
            get { return m_todHDRPGroundTint; }
            set
            {
                if (m_todHDRPGroundTint != value)
                {
                    m_todHDRPGroundTint = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public Gradient TODHDRPFogAlbedo
        {
            get { return m_todHDRPFogAlbedo; }
            set
            {
                if (m_todHDRPFogAlbedo != value)
                {
                    m_todHDRPFogAlbedo = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODSunSize
        {
            get { return m_todSunSize; }
            set
            {
                if (m_todSunSize != value)
                {
                    m_todSunSize = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve TODSunSizeConvergence
        {
            get { return m_todSunSizeConvergence; }
            set
            {
                if (m_todSunSizeConvergence != value)
                {
                    m_todSunSizeConvergence = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }

        public AnimationCurve TODPostProcessExposure
        {
            get { return m_todPostProcessExposure; }
            set
            {
                if (m_todPostProcessExposure != value)
                {
                    m_todPostProcessExposure = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }

        //Wind Settings
        public WindZone WindZone
        {
            get { return m_windZone; }
            set
            {
                if (m_windZone != value)
                {
                    m_windZone = value;
                    m_updateWind = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float WindSpeed
        {
            get { return m_windSpeed; }
            set
            {
                if (m_windSpeed != value)
                {
                    m_windSpeed = value;
                    m_updateWind = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float WindTurbulence
        {
            get { return m_windTurbulence; }
            set
            {
                if (m_windTurbulence != value)
                {
                    m_windTurbulence = value;
                    m_updateWind = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float WindFrequency
        {
            get { return m_windFrequency; }
            set
            {
                if (m_windFrequency != value)
                {
                    m_windFrequency = value;
                    m_updateWind = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float WindDirection
        {
            get { return m_windDirection; }
            set
            {
                if (m_windDirection != value)
                {
                    m_windDirection = value;
                    m_updateWind = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float WindMultiplier
        {
            get { return m_windMultiplier; }
            set
            {
                if (m_windMultiplier != value)
                {
                    m_windMultiplier = value;
                    m_updateWind = true;
                    UpdateWeatherEvents();
                }
            }
        }

        public float MinTerrainWind
        {
            get { return m_minTerrainWind; }
            set
            {
                if (m_minTerrainWind != value)
                {
                    m_minTerrainWind = value;
                    m_updateWind = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float MaxTerrainWind
        {
            get { return m_maxTerrainWind; }
            set
            {
                if (m_maxTerrainWind != value)
                {
                    m_maxTerrainWind = value;
                    m_updateWind = true;
                    UpdateWeatherEvents();
                }
            }
        }

        //Snow Settings
        public bool EnableSnow
        {
            get { return m_enableSnow; }
            set
            {
                if (m_enableSnow != value)
                {
                    m_enableSnow = value;
                    m_updateSnow = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public bool SnowCoverAlwaysEnabled
        {
            get { return m_snowCoverAlwaysEnabled; }
            set
            {
                if (m_snowCoverAlwaysEnabled != value)
                {
                    m_snowCoverAlwaysEnabled = value;
                    m_updateSnow = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float SnowIntensity
        {
            get { return m_snowIntensity; }
            set
            {
                if (m_snowIntensity != value)
                {
                    m_snowIntensity = value;
                    m_newSnowIntensity = value;
                    m_updateSnow = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float SnowHeight
        {
            get { return m_snowHeight; }
            set
            {
                if (m_snowHeight != value)
                {
                    m_snowHeight = value;
                    m_updateSnow = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float PermanentSnowHeight
        {
            get { return m_permanentSnowHeight; }
            set
            {
                if (m_permanentSnowHeight != value)
                {
                    m_permanentSnowHeight = value;
                    m_updateSnow = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float SnowingHeight
        {
            get { return m_snowingHeight; }
            set
            {
                if (m_snowingHeight != value)
                {
                    m_snowingHeight = value;
                    m_updateSnow = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float SnowFadeHeight
        {
            get { return m_snowFadeHeight; }
            set
            {
                if (m_snowFadeHeight != value)
                {
                    m_snowFadeHeight = value;
                    m_updateSnow = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float m_snowStormChance = 0.2f;
        public float m_snowStepSize = 0.05f;
        public SnowMode m_snowMode = SnowMode.SampledHeight;
        public WeatherSettings m_snowWeatherSettings = new WeatherSettings();

        //Rain Settings
        public bool EnableRain
        {
            get { return m_enableRain; }
            set
            {
                if (m_enableRain != value)
                {
                    m_enableRain = value;
                    m_updateRain = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float RainIntensity
        {
            get { return m_rainIntensity; }
            set
            {
                if (m_rainIntensity != value)
                {
                    m_rainIntensity = value;
                    m_newRainIntensity = value;
                    m_updateRain = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public RainMode m_rainMode = RainMode.RandomChance;
        public float m_rainHeight = 400f;
        public float m_rainStepSize = 0.05f;
        public WeatherSettings m_rainWeatherSettings = new WeatherSettings();

        //Thunder
        public GaiaConstants.ThunderStormType m_stormType = GaiaConstants.ThunderStormType.Moderate;
        public bool m_enableThunder = true;
        public float ThunderWaitDuration = 30f;
        public float m_thunderChance = 0.45f;
        public Color ThunderLightColor
        {
            get { return m_thunderLightColor; }
            set
            {
                if (m_thunderLightColor != value)
                {
                    m_thunderLightColor = value;
                    UpdateThunderSystem();
                }
            }
        }
        public float ThunderLightIntensity
        {
            get { return m_thunderLightIntensity; }
            set
            {
                if (m_thunderLightIntensity != value)
                {
                    m_thunderLightIntensity = value;
                    UpdateThunderSystem();
                }
            }
        }
        public List<AudioClip> ThunderAudioSources
        {
            get { return m_thunderAudioSources; }
            set
            {
                if (m_thunderAudioSources != value)
                {
                    m_thunderAudioSources = value;
                    UpdateThunderSystem();
                }
            }
        }
        public float ThunderLightRadius
        {
            get { return m_thunderLightRadius; }
            set
            {
                if (m_thunderLightRadius != value)
                {
                    m_thunderLightRadius = value;
                    UpdateThunderSystem();
                }
            }
        }

        //Weather
        //public float m_weatherStepSize = 0.05f;
        public float m_weatherFadeDuration = 30f;
        public bool m_modifyFog = true;
        public bool m_modifyWind = true;
        public bool m_modifySkybox = true;
        public bool m_modifyPostProcessing = true;
        public bool m_modifySun = true;
        public bool m_modifyAmbient = true;
        public bool m_modifyClouds = true;

        //Cloud Particles
        public bool EnableClouds
        {
            get { return m_enableClouds; }
            set
            {
                if (m_enableClouds != value)
                {
                    m_enableClouds = value;
                    UpdateCloudSettings();
                }
            }
        }
        public int CloudHeight
        {
            get { return m_cloudHeight; }
            set
            {
                if (m_cloudHeight != value)
                {
                    m_cloudHeight = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public Color CloudAmbientColor
        {
            get { return m_cloudAmbientColor; }
            set
            {
                if (m_cloudAmbientColor != value)
                {
                    m_cloudAmbientColor = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float CloudScale
        {
            get { return m_cloudScale; }
            set
            {
                if (m_cloudScale != value)
                {
                    m_cloudScale = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float CloudOffset
        {
            get { return m_cloudOffset; }
            set
            {
                if (m_cloudOffset != value)
                {
                    m_cloudOffset = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float CloudBrightness
        {
            get { return m_cloudBrightness; }
            set
            {
                if (m_cloudBrightness != value)
                {
                    m_cloudBrightness = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public AnimationCurve CloudDomeBrightness
        {
            get { return m_cloudDomeBrightness; }
            set
            {
                if (m_cloudDomeBrightness != value)
                {
                    m_cloudDomeBrightness = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float CloudFade
        {
            get { return m_cloudFade; }
            set
            {
                if (m_cloudFade != value)
                {
                    m_cloudFade = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }

        //X-Y tile, Z wind speed
        public Vector4 CloudTilingAndWind
        {
            get { return m_cloudTilingAndWind; }
            set
            {
                if (m_cloudTilingAndWind != value)
                {
                    m_cloudTilingAndWind = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        //X cutout, Y opacity, Z density, W dome opacity
        public Vector4 CloudOpacity
        {
            get { return m_cloudOpacity; }
            set
            {
                if (m_cloudOpacity != value)
                {
                    m_cloudOpacity = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float CloudRotationSpeedLow
        {
            get { return m_cloudRotationSpeedLow; }
            set
            {
                if (m_cloudRotationSpeedLow != value)
                {
                    m_cloudRotationSpeedLow = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float CloudRotationSpeedMiddle
        {
            get { return m_cloudRotationSpeedMiddle; }
            set
            {
                if (m_cloudRotationSpeedMiddle != value)
                {
                    m_cloudRotationSpeedMiddle = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float CloudRotationSpeedFar
        {
            get { return m_cloudRotationSpeedFar; }
            set
            {
                if (m_cloudRotationSpeedFar != value)
                {
                    m_cloudRotationSpeedFar = value;
                    m_updateClouds = true;
                    UpdateWeatherEvents();
                }
            }
        }

        //Season
        public bool EnableSeasons
        {
            get { return m_enableSeasons; }
            set
            {
                if (m_enableSeasons != value)
                {
                    m_enableSeasons = value;
                    m_updateSeason = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public SeasonMode SeasonMode
        {
            get { return m_seasonMode; }
            set
            {
                if (m_seasonMode != value)
                {
                    m_seasonMode = value;
                    m_updateSeason = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float Season
        {
            get { return m_season; }
            set
            {
                if (m_season != value)
                {
                    m_season = value;
                    m_updateSeason = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public Color SeasonWinterTint
        {
            get { return m_seasonWinterTint; }
            set
            {
                if (m_seasonWinterTint != value)
                {
                    m_seasonWinterTint = value;
                    m_updateSeason = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public Color SeasonSpringTint
        {
            get { return m_seasonSpringTint; }
            set
            {
                if (m_seasonSpringTint != value)
                {
                    m_seasonSpringTint = value;
                    m_updateSeason = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public Color SeasonSummerTint
        {
            get { return m_seasonSummerTint; }
            set
            {
                if (m_seasonSummerTint != value)
                {
                    m_seasonSummerTint = value;
                    m_updateSeason = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public Color SeasonAutumnTint
        {
            get { return m_seasonAutumnTint; }
            set
            {
                if (m_seasonAutumnTint != value)
                {
                    m_seasonAutumnTint = value;
                    m_updateSeason = true;
                    UpdateWeatherEvents();
                }
            }
        }
        public float m_seasonTransitionDuration;

        //Panel Bools
        public bool m_panelWindSettings;
        public bool m_panelWeatherSettings = true;
        public bool m_panelSeasonSettings;
        public bool m_panelAtmosphereSettings;
        public bool m_panelParticlesSettings;
        public bool m_panelCloudSettings;
        public bool m_panelSnowSettings;
        public bool m_panelRainSettings;
        public bool m_panelThunderSettings;

        #endregion
        #region Private Values

        //Audio
        [SerializeField]
        private AudioReverbFilter AudioReverbFilter;
        [SerializeField]
        private AudioReverbPreset AudioReverbPreset;

        //Volume
        [SerializeField]
        private float m_globalVolume = 1f;
        [SerializeField]
        private float m_rainVolume = 1f;
        [SerializeField]
        private float m_snowVolume = 1f;

        //Global
        [SerializeField]
        private bool m_runInEditor = false;
        [SerializeField]
        private float m_stepStepSize = 0.05f;
        private float TransationTime = 0;
        [SerializeField]
        private float m_currentTime;

        //Player
        [SerializeField]
        private Vector3 m_lastKnownLocation;

        //Atmosphere
        [SerializeField]
        private Gradient m_todSkyboxTint;
        [SerializeField]
        private AnimationCurve m_todSkyboxFogHeight;
        [SerializeField]
        private AnimationCurve m_todSkyboxFogGradient;
        [SerializeField]
        private AnimationCurve m_todSkyboxExposure;
        [SerializeField]
        private Gradient m_todSunColor;
        [SerializeField]
        private Gradient m_todFogColor;
        [SerializeField]
        private Gradient m_todAmbientSkyColor;
        [SerializeField]
        private Gradient m_todAmbientEquatorColor;
        [SerializeField]
        private Gradient m_todAmbientGroundColor;
        [SerializeField]
        private AnimationCurve m_todSunIntensity;
        [SerializeField]
        private AnimationCurve m_todSunShadowStrength;
        [SerializeField]
        private AnimationCurve m_todCloudHeightLevelDensity;
        [SerializeField]
        private AnimationCurve m_todCloudHeightLevelThickness;
        [SerializeField]
        private AnimationCurve m_todCloudHeightLevelSpeed;
        [SerializeField]
        private AnimationCurve m_todCloudOpacity;
        [SerializeField]
        private AnimationCurve m_todAmbientIntensity;
        [SerializeField]
        private AnimationCurve m_todAtmosphereThickness;
        [SerializeField]
        private AnimationCurve m_todFogDensity;
        [SerializeField]
        private AnimationCurve m_todFogStartDistance;
        [SerializeField]
        private AnimationCurve m_todFogEndDistance;
        [SerializeField]
        private AnimationCurve m_todHDRPFogBaseHeight;
        [SerializeField]
        private AnimationCurve m_todHDRPFogAnisotropy;
        [SerializeField]
        private AnimationCurve m_todHDRPFogLightProbeDimmer;
        [SerializeField]
        private AnimationCurve m_todHDRPFogDepthExtent;
        [SerializeField]
        private Gradient m_todHDRPGroundTint;
        [SerializeField]
        private Gradient m_todHDRPFogAlbedo;

        //Time Of Day
        [SerializeField]
        private float m_todHorizonalAngle;
        [SerializeField]
        private bool m_todEnabled;
        [SerializeField]
        private AnimationCurve m_todSunSize;
        [SerializeField]
        private AnimationCurve m_todSunSizeConvergence;
        [SerializeField]
        public AnimationCurve m_todPostProcessExposure;

        //Wind
        [SerializeField]
        private WindZone m_windZone;
        [SerializeField]
        private float m_windSpeed;
        [SerializeField]
        private float m_windTurbulence;
        [SerializeField]
        private float m_windFrequency;
        [SerializeField]
        private float m_windDirection = 0f;
        [SerializeField]
        private float m_windMultiplier = 1f;
        [SerializeField]
        private float m_minTerrainWind = 0.1f;
        [SerializeField]
        private float m_maxTerrainWind = 1.0f;
        //Snow
        [SerializeField]
        private bool m_enableSnow = true;
        [SerializeField]
        private bool m_snowCoverAlwaysEnabled = true;
        [SerializeField]
        private float m_snowIntensity = 0f;
        [SerializeField]
        private float m_snowHeight = 300f;
        [SerializeField]
        private float m_permanentSnowHeight = 300f;
        [SerializeField]
        private float m_snowingHeight = 50f;
        [SerializeField]
        private float m_snowFadeHeight = 50f;
        [SerializeField]
        private float m_snowDuration;
        public float m_snowWaitTime;
        public float m_snowSampleStrength;
        private float m_snowActualChance;
        private float m_snowRandomValue;
        private float m_newSnowIntensity = 0f;
        private float m_savedSnowingHeight;
        //private float m_newSnowVolume = 0f;
        //Rain
        [SerializeField]
        private bool m_enableRain = true;
        [SerializeField]
        private float m_rainIntensity = 0f;
        public float m_rainSampleStrength;
        [SerializeField]
        private float m_rainDuration;
        public float m_rainWaitTime;
        private float m_actualChance;
        private float m_randomValue;
        private float m_newRainIntensity = 0f;
        //private float m_newRainVolume = 0f;

        //Thunder
        private float m_thunderWaitDuration = 30f;
        [SerializeField]
        private Color m_thunderLightColor;
        [SerializeField]
        private float m_thunderLightIntensity;
        [SerializeField]
        private List<AudioClip> m_thunderAudioSources;
        [SerializeField]
        public float m_thunderLightRadius = 300f;
        private float m_thunderActualChance;
        private float m_thunderRandomValue;

        //Cloud Particles
        [SerializeField]
        private bool m_enableClouds = true;
        [SerializeField]
        private int m_cloudHeight = 50;
        [SerializeField]
        private Color m_cloudAmbientColor = Color.white;
        [SerializeField]
        private float m_cloudScale = 1f;
        [SerializeField]
        private float m_cloudOffset = 50f;
        [SerializeField]
        private AnimationCurve m_cloudDomeBrightness;
        [SerializeField]
        private float m_cloudBrightness = 1.4f;
        [SerializeField]
        private float m_cloudFade = 300f;
        [SerializeField]
        private Vector4 m_cloudTilingAndWind = new Vector4(2f, 0.6f, 1f, -16f);
        [SerializeField]
        private Vector4 m_cloudOpacity = new Vector4(0.6f, 1f, 0.4f, 1f);
        [SerializeField]
        private float m_cloudRotationSpeedLow = 0.225f;
        [SerializeField]
        private float m_cloudRotationSpeedMiddle = 0.12f;
        [SerializeField]
        private float m_cloudRotationSpeedFar = 0.08f;

        //Season
        [SerializeField]
        private bool m_enableSeasons;
        [SerializeField]
        private SeasonMode m_seasonMode = SeasonMode.Summer;
        [SerializeField]
        private float m_season = 0f;
        [SerializeField]
        private Color m_seasonWinterTint = Color.white;
        [SerializeField]
        private Color m_seasonSpringTint = Color.white;
        [SerializeField]
        private Color m_seasonSummerTint = Color.white;
        [SerializeField]
        private Color m_seasonAutumnTint = Color.white;

        //Water
        [SerializeField]
        private Material m_waterMaterial;
        [SerializeField]
        private GaiaAudioManager GaiaAudioManager;

        //Weather
        private float m_newSkyboxExposure = 0f;
        private float m_newAmbientIntensity = 0f;
        private float m_newSunIntensity = 0f;
        private float m_newWindSpeed = 0f;
        private float m_newWindTurbulence = 0f;
        private float m_newWindFrequency = 0f;
        private float m_newWindMultiplier = 0f;
        [SerializeField]
        private GameObject m_thunderObject;
        [SerializeField]
        private ThunderStrike m_thunderComponent;

        /// <summary>
        /// Shader strings
        /// </summary>
        //Vegetation Wind
        private const string m_windZoneObjectName = "PW Wind Zone";
        private const string m_globalWindSpeed = "_PW_Global_WindSpeed";
        private const string m_globalWindFrequency = "_PW_Global_WindFrequency";
        private const string m_globalWindGustDistance = "_PW_Global_WindGustDistance";
        private const string m_globalBillboardWind = "PWSF_ BillboardWindStrength";
        private const string m_globalWindDirection = "_PW_Global_WindDirection";
        //Vegetation Season
        public const string m_globalSeasonTint = "_PW_Global_SeasonalTint";
        //Water Wind
        private const string m_globalWaveSpeed = "PWSF_GlobalWindIntensityWater";
        //Rain
        private const string m_shaderRainIntensity = "Dynamic Rain Intensity";
        //Snow
        public const string m_globalSnowIntensity = "_PW_Global_CoverLayer1Progress";
        public const string m_globalCoverLayer1FadeStart = "_PW_Global_CoverLayer1FadeStart";
        public const string m_globalCoverLayer1FadeDist = "_PW_Global_CoverLayer1FadeDist";
        //Season
        private const string m_shaderSeasonColorShift = "_PW_SeasonalTintAmount";
        //Particles
        private const string m_builtInParticleShader = "Particles/Standard Surface";
        private const string m_lightweightParticleShader = "Legacy Shaders/Particles/Additive";
        private const string m_universalParticleShader = "Legacy Shaders/Particles/Additive";
        private const string m_highDefinitionParticleShader = "Legacy Shaders/Particles/Additive";

        /// <summary>
        /// Bools
        /// </summary>
        private bool m_updateWind;
        private bool m_updateSnow;
        private bool m_updateRain;
        private bool m_updateSeason;
        private bool m_updateClouds;
        private bool IsCurrentSettingsSet;
        public bool IsSnowingFinished;
        public bool IsRainingFinished;
        public bool SkyboxShaderValid;
        private bool IsThundering;
        public bool CloudSystemValid;
        public bool WaterSystemValid;
        public bool AtmosphereSystemValid;
        private bool GaiaSceneProfileValid;

        //Audio Sources
        [SerializeField]
        private AudioSource m_snowAudioSource;
        [SerializeField]
        private AudioSource m_rainAudioSource;

        //Terrains
        [SerializeField]
        private Terrain[] m_terrains;

        //Ground Particles
        [SerializeField]
        private GroundParticlesCulling[] m_groundParticlesCullings;

        //Cameras
        private static Camera[] m_mainCameras;

        //Skybox
        [SerializeField]
        private Material m_skyboxMaterial;

        //Sunlight
        public Light m_sunLight;
        //Moonlight
        public Light m_moonLight;

        //Post Processing
#if UNITY_POST_PROCESSING_STACK_V2
        private PostProcessProfile m_processProfile;
        private PostProcessVolume[] m_volumes;
#endif

        //CTS
#if CTS_PRESENT
        [SerializeField]
        private CTSWeatherManager m_cTSWeatherManager;
#endif

        //Volume Profiles
#if HDPipeline
        [SerializeField]
        private VolumeProfile m_hdrpVolumeProfile;
        [SerializeField]
        private VolumeProfile m_hdrpFXVolumeProfile;
        public bool m_updateHDRPShadows = false;
#endif
#if UPPipeline
        [SerializeField]
        private VolumeProfile m_urpFXVolumeProfile;
#endif

        #endregion
        #region Unity Functions

        private void Start()
        {
            if (m_instance == null)
            {
                m_instance = this;
            }
            Initialize();
            InitializeShaderSetup();
        }
        /// <summary>
        /// Loads on enable and disable
        /// </summary>
        private void OnEnable()
        {
            if (m_instance == null)
            {
                m_instance = this;
            }
            //If is not starting application
            if (!Application.isPlaying)
            {
                Initialize();
                InitializeShaderSetup();
                DeInitialize();
            }
        }
        private void OnDisable()
        {
            DeinitializePostFX();
        }
        private void OnValidate()
        {
            ForceUpdateSystem = true;
        }
        /// <summary>
        /// Update every frame
        /// </summary>
        private void LateUpdate()
        {
            m_currentTime = GaiaGlobal.GetTimeOfDayMainValue();
            SetAutoExposure();

            if (!Application.isPlaying)
            {
                if (ForceUpdateSystem)
                {
                    UpdateAllSystems(false);
                    ForceUpdateSystem = false;
                }
                return;
            }

            m_renderPipeline = GaiaUtils.GetActivePipeline();

            //Update last know location
            LastKnownLocation = m_player.transform.position;

            if (m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
                if (m_snowMode == SnowMode.RandomChance)
                {
                    //Check Snow Conditions
                    if (EnableSnow)
                    {
                        CheckSnowConditions();
                    }
                }
                else
                {
                    //Is player higher than the snow set height check?
                    if (m_player.transform.position.y >= SnowHeight)
                    {
                        //Check Snow Conditions
                        if (EnableSnow)
                        {
                            CheckSnowConditions();
                        }
                    }
                }

                if (m_rainMode == RainMode.RandomChance)
                {
                    //Check Rain Conditions
                    if (EnableRain)
                    {
                        CheckRainConditions();
                        if (IsRaining)
                        {
                            //Check Thunder Conditions
                            if (m_enableThunder)
                            {
                                CheckThunderConditions();
                            }
                        }
                    }
                }
                else
                {
                    if (m_player.transform.position.y < SnowHeight)
                    {
                        //Check Rain Conditions
                        if (EnableRain)
                        {
                            CheckRainConditions();
                            if (IsRaining)
                            {
                                //Check Thunder Conditions
                                if (m_enableThunder)
                                {
                                    CheckThunderConditions();
                                }
                            }
                        }
                    }
                }

                if (IsSnowing || IsRaining)
                {
                    m_weatherSystemActive = true;
                }

                //Start processing weather changes
                if (m_weatherSystemActive)
                {
                    UpdateWeatherSystem();
                }
            }

            DoesAtmosphereNeedUpdate = DoesAtmosphereNeedToUpdate();

            GetVolume();

            SetAutoDOF();
        }
        private void Update()
        {
            if (Application.isPlaying)
            {
                if (EnableSeasons)
                {
                    SetSeasons();
                }
            }
        }

        #endregion
        #region Private Functions

        #region Initialization

        /// <summary>
        /// Setup on enable or start
        /// </summary>
        public void Initialize()
        {
            try
            {
                GaiaSceneProfileValid = GaiaUtils.CheckIfSceneProfileExists();
                if (m_thunderComponent == null)
                {
                    m_thunderComponent = FindObjectOfType<ThunderStrike>();
                }

                m_savedSnowingHeight = SnowHeight;
                m_renderPipeline = GaiaUtils.GetActivePipeline();
#if UNITY_POST_PROCESSING_STACK_V2
                CheckPWSkyPostFXManager();
#endif
                CheckValidSystems();

                //Gets the wind zone if it's empty
                if (WindZone == null)
                {
                    WindZone = GetOrCreateWindZone();
                }

                //Get the player
                if (m_player == null)
                {
                    m_player = GetPlayer();
                }

                if (GaiaGlobal.Instance == null)
                {
                    GaiaGlobal.Instance = FindObjectOfType<GaiaGlobal>();
                }

                RainIntensity = 0f;
                SnowIntensity = 0f;
                IsThundering = false;
                m_instantVFX = false;
                m_disableWeatherFX = false;

                if (m_player != null)
                {
                    if (AudioReverbFilter == null)
                    {
                        AudioReverbFilter = m_player.gameObject.GetComponent<AudioReverbFilter>();
                        if (AudioReverbFilter == null)
                        {
                            AudioReverbFilter = m_player.gameObject.AddComponent<AudioReverbFilter>();
                        }

                        AudioReverbFilter.reverbPreset = AudioReverbPreset.Forest;
                        AudioReverbPreset = AudioReverbFilter.reverbPreset;
                    }
                    else
                    {
                        AudioReverbPreset = AudioReverbFilter.reverbPreset;
                    }
                }

                if (m_sunLight == null)
                {
                    GaiaUtils.GetMainDirectionalLight();
                }

                if (m_moonLight == null)
                {
                    m_moonLight = GaiaUtils.GetMainMoonLight();
                }

                if (m_rainAudioSource != null)
                {
                    m_rainAudioSource.volume = 0f;
                }

                if (m_snowAudioSource != null)
                {
                    m_snowAudioSource.volume = 0f;
                }

                //Get the water material
                if (m_waterMaterial == null)
                {
                    m_waterMaterial = GaiaUtils.GetWaterMaterial(GaiaConstants.waterSurfaceObject);
                }

                if (m_snowParticles != null)
                {
                    m_snowParticles.Stop();
                    m_snowParticleEmission = m_snowParticles.emission;
                    m_snowParticleEmission.rateOverTime = 0;
                }

                if (m_snowVFX != null)
                {
                    m_snowVFX.Stop();
                    m_snowVFXEmission = m_snowVFX.emission;
                    m_snowVFXEmission.rateOverTime = 0;
                }

                if (m_rainParticles != null)
                {
                    m_rainParticles.sharedMaterial.SetFloat("_PW_VFX_Weather_Intensity", 0f);
                    m_rainParticles.enabled = false;
                }

                if (m_rainVFX != null)
                {
                    m_rainVFX.Stop();
                }

                if (gameObject.name != GaiaConstants.gaiaWeatherObject)
                {
                    gameObject.name = GaiaConstants.gaiaWeatherObject;
                }

                if (m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    if (m_skyboxMaterial == null || m_skyboxMaterial.shader != Shader.Find(GaiaShaderID.m_pwSkySkyboxShader))
                    {
                        m_skyboxMaterial = RenderSettings.skybox;
                    }

                    GameObject cloudObject = GameObject.Find("PW_VFX_SkyDome");
                    if (cloudObject != null)
                    {
                        MeshRenderer clouds = cloudObject.GetComponent<MeshRenderer>();
                        if (clouds != null)
                        {
                            Material cloudMaterialInstance = new Material(clouds.sharedMaterial);
                            clouds.sharedMaterial = cloudMaterialInstance;
                        }
                    }

                    if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.Universal)
                    {
#if UPPipeline
                        if (m_urpFXVolumeProfile == null)
                        {
                            m_urpFXVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Post Processing", "Null");
                        }
#endif
                    }
                    else
                    {
#if UNITY_POST_PROCESSING_STACK_V2
                        if (m_processProfile == null)
                        {
                            m_processProfile = GetPostProcessingProfile();
                        }
#endif
                    }
                }
                else
                {
                    HDRPUpdateSkyMode(true);
#if HDPipeline
                    if (m_hdrpFXVolumeProfile == null)
                    {
                        m_hdrpFXVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Post Processing", "Null");
                    }

                    if (m_hdrpVolumeProfile == null)
                    {
                        m_hdrpVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Environment", "Processing");
                    }

                    HDRPUpdateSkyMode();
#endif
                }

                IsRaining = false;
                IsRainingFinished = true;
                IsSnowing = false;
                IsSnowingFinished = true;
                m_weatherSystemActive = false;
                GetScenePostProcessingSettings();
                GetCurrentSettings();
                IsCurrentSettingsSet = false;
                m_rainWaitTime = GenerateRandomRangeValue(m_rainWeatherSettings.m_minWaitTime, m_rainWeatherSettings.m_maxWaitTime);
                m_snowWaitTime = GenerateRandomRangeValue(m_snowWeatherSettings.m_minWaitTime, m_snowWeatherSettings.m_maxWaitTime);
                m_thunderWaitDuration = ThunderWaitDuration;

                if (PW_VFX_Clouds.Instance != null)
                {
                    PW_VFX_Clouds.Instance.CloudsUpdate();
                }

                UpdateWindValues(GaiaConstants.GaiaGlobalWindType.Custom);
                UpdateAllSystems(false);
                UpdateWeatherSystem();

                if (PW_VFX_Atmosphere.Instance != null)
                {
                    CurrentSettings.m_startCloudDomeBrightness = CloudDomeBrightness.Evaluate(m_currentTime);
                    PW_VFX_Atmosphere.Instance.UpdateSystem();
                }

                if (GaiaGlobal.Instance != null)
                {
                    GaiaGlobal.Instance.UpdateNightMode();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Initialize had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }

        /// <summary>
        /// Setup used to configure the shaders ont he weather system materials
        /// </summary>
        public void InitializeShaderSetup()
        {
            try
            {
#if UNITY_EDITOR
                if (m_snowParticles == null)
                {
                    GameObject snowParticles = GameObject.Find("PW_VFX_Snow");
                    if (snowParticles == null)
                    {
                        GameObject prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(GaiaUtils.GetAssetPath("PW_VFX_Snow.prefab"));
                        if (prefabObject != null)
                        {
                            snowParticles = PrefabUtility.InstantiatePrefab(prefabObject) as GameObject;
                        }
                    }

                    if (snowParticles != null)
                    {
                        snowParticles.transform.SetParent(gameObject.transform);
                        m_snowParticles = snowParticles.GetComponent<ParticleSystem>();
                    }
                    else
                    {
                        Debug.LogError("(Gaia Snow Particles) could not be found or Instantiated from the project. It could be missing?");
                    }
                }
                if (m_rainParticles == null)
                {
                    GameObject rainParticles = GameObject.Find("PW_VFX_Rain");
                    if (rainParticles == null)
                    {
                        GameObject prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(GaiaUtils.GetAssetPath("PW_VFX_Rain.prefab"));
                        if (prefabObject != null)
                        {
                            rainParticles = PrefabUtility.InstantiatePrefab(prefabObject) as GameObject;
                        }
                    }

                    if (rainParticles != null)
                    {
                        rainParticles.transform.SetParent(gameObject.transform);
                        m_rainParticles = rainParticles.GetComponent<MeshRenderer>();
                    }
                    else
                    {
                        Debug.LogError("(Gaia Rain Particles) could not be found or Instantiated from the project. It could be missing?");
                    }
                }
#endif

                if (m_snowParticles != null)
                {
                    GameObject snowvVFXObj = GameObject.Find("SnowFall_Far");
                    if (snowvVFXObj != null)
                    {
                        m_snowVFX = snowvVFXObj.GetComponent<ParticleSystem>();
                        if (m_snowVFX != null)
                        {
                            m_snowVFX.Stop();
                        }
                    }

                    m_snowAudioSource = m_snowParticles.gameObject.GetComponent<AudioSource>();
                    if (m_snowAudioSource == null)
                    {
                        m_snowAudioSource = m_snowParticles.gameObject.AddComponent<AudioSource>();
                        m_snowAudioSource.volume = 0f;
                        m_snowAudioSource.loop = true;
                    }
                }
                if (m_rainParticles != null)
                {
                    GameObject rainvVFXObj = GameObject.Find("Particle System_Rain");
                    if (rainvVFXObj != null)
                    {
                        m_rainVFX = rainvVFXObj.GetComponent<ParticleSystem>();
                        if (m_rainVFX != null)
                        {
                            m_rainVFX.Stop();
                        }
                    }

                    m_rainAudioSource = m_rainParticles.gameObject.GetComponent<AudioSource>();
                    if (m_rainAudioSource == null)
                    {
                        m_rainAudioSource = m_rainParticles.gameObject.AddComponent<AudioSource>();
                        m_rainAudioSource.volume = 0f;
                        m_rainAudioSource.loop = true;
                    }
                }

                m_renderPipeline = GaiaUtils.GetActivePipeline();
                if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.BuiltIn)
                {
                    //Built-In
                    //Rain
                    if (m_rainParticles != null)
                    {
                        m_rainParticles.sharedMaterial.shader = Shader.Find("PW_VFX_Rain");
                    }
                    if (m_rainVFX != null)
                    {
                        ParticleSystemRenderer systemRenderer = m_rainVFX.GetComponent<ParticleSystemRenderer>();
                        if (systemRenderer != null)
                        {
                            Texture mainTexture = systemRenderer.sharedMaterial.GetTexture("_MainTex");
                            if (mainTexture == null)
                            {
                                mainTexture = systemRenderer.sharedMaterial.GetTexture("_BaseMap");
                            }
                            if (mainTexture != null)
                            {
                                systemRenderer.sharedMaterial.shader = Shader.Find("Particles/Standard Unlit");
                                systemRenderer.sharedMaterial.SetTexture("_MainTex", mainTexture);
                            }
                        }
                    }

                    //Snow
                    if (m_snowParticles != null)
                    {
                        ParticleSystemRenderer systemRenderer = m_snowParticles.GetComponent<ParticleSystemRenderer>();
                        if (systemRenderer != null)
                        {
                            systemRenderer.sharedMaterial.shader = Shader.Find("PWS/VFX/PW_VFX_Snowfall_Close");
                        }
                    }
                    if (m_snowVFX != null)
                    {
                        ParticleSystemRenderer systemRenderer = m_snowVFX.GetComponent<ParticleSystemRenderer>();
                        if (systemRenderer != null)
                        {
                            systemRenderer.sharedMaterial.shader = Shader.Find("PWS/VFX/PW_VFX_SnowFall_Middle");
                        }
                    }

                    //Clouds
                    GameObject cloudObject = GameObject.Find("PW_VFX_SkyDome");
                    if (cloudObject != null)
                    {
                        MeshRenderer[] meshRenderers = cloudObject.GetComponentsInChildren<MeshRenderer>();
                        if (meshRenderers.Length > 0)
                        {
                            foreach (MeshRenderer renderer in meshRenderers)
                            {
                                if (renderer.name.Contains("Space"))
                                {
                                    renderer.sharedMaterial.shader = Shader.Find("PWS/VFX/PW_VFX_SkyDome_Space");
                                }
                                else
                                {
                                    renderer.sharedMaterial.shader = Shader.Find("PWS/VFX/PW_SkydomeCloudsHight");
                                }
                            }
                        }
                    }
                }
                else if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.Universal)
                {
                    //URP
                    //Rain
                    if (m_rainParticles != null)
                    {
                        m_rainParticles.sharedMaterial.shader = Shader.Find("PW_VFX_Rain");
                    }
                    if (m_rainVFX != null)
                    {
                        ParticleSystemRenderer systemRenderer = m_rainVFX.GetComponent<ParticleSystemRenderer>();
                        if (systemRenderer != null)
                        {
                            Texture mainTexture = systemRenderer.sharedMaterial.GetTexture("_MainTex");
                            if (mainTexture == null)
                            {
                                mainTexture = systemRenderer.sharedMaterial.GetTexture("_BaseMap");
                            }
                            if (mainTexture != null)
                            {
                                systemRenderer.sharedMaterial.shader = Shader.Find("Legacy Shaders/Particles/Additive");
                                systemRenderer.sharedMaterial.SetTexture("_MainTex", mainTexture);
                            }
                        }
                    }

                    //Snow
                    if (m_snowParticles != null)
                    {
                        ParticleSystemRenderer systemRenderer = m_snowParticles.GetComponent<ParticleSystemRenderer>();
                        if (systemRenderer != null)
                        {
                            systemRenderer.sharedMaterial.shader = Shader.Find("PWS/VFX/PW_VFX_Snowfall_Close");
                        }
                    }
                    if (m_snowVFX != null)
                    {
                        ParticleSystemRenderer systemRenderer = m_snowVFX.GetComponent<ParticleSystemRenderer>();
                        if (systemRenderer != null)
                        {
                            systemRenderer.sharedMaterial.shader = Shader.Find("PWS/VFX/PW_VFX_SnowFall_Middle");
                        }
                    }

                    //Clouds
                    GameObject cloudObject = GameObject.Find("PW_VFX_SkyDome");
                    if (cloudObject != null)
                    {
                        MeshRenderer[] meshRenderers = cloudObject.GetComponentsInChildren<MeshRenderer>();
                        if (meshRenderers.Length > 0)
                        {
                            foreach (MeshRenderer renderer in meshRenderers)
                            {
                                if (renderer.name.Contains("Space"))
                                {
                                    renderer.sharedMaterial.shader = Shader.Find("PWS/VFX/PW_VFX_SkyDome_Space");
                                }
                                else
                                {
                                    renderer.sharedMaterial.shader = Shader.Find("PWS/VFX/PW_SkydomeCloudsHight");
                                }
                            }
                        }
                    }
                }
                else
                {
                    //HDRP
                    //Rain
                    if (m_rainParticles != null)
                    {
                        m_rainParticles.sharedMaterial.shader = Shader.Find("PW_VFX_Rain");
                    }
                    if (m_rainVFX != null)
                    {
                        ParticleSystemRenderer systemRenderer = m_rainVFX.GetComponent<ParticleSystemRenderer>();
                        if (systemRenderer != null)
                        {
                            Texture mainTexture = systemRenderer.sharedMaterial.GetTexture("_MainTex");
                            if (mainTexture == null)
                            {
                                mainTexture = systemRenderer.sharedMaterial.GetTexture("_BaseMap");
                            }
                            if (mainTexture != null)
                            {
                                systemRenderer.sharedMaterial.shader = Shader.Find("Legacy Shaders/Particles/Additive");
                                systemRenderer.sharedMaterial.SetTexture("_MainTex", mainTexture);
                            }
                        }
                    }

                    //Snow
                    if (m_snowParticles != null)
                    {
                        ParticleSystemRenderer systemRenderer = m_snowParticles.GetComponent<ParticleSystemRenderer>();
                        if (systemRenderer != null)
                        {
                            systemRenderer.sharedMaterial.shader = Shader.Find("PWS/VFX/PW_VFX_Snowfall_Close");
                        }
                    }
                    if (m_snowVFX != null)
                    {
                        ParticleSystemRenderer systemRenderer = m_snowVFX.GetComponent<ParticleSystemRenderer>();
                        if (systemRenderer != null)
                        {
                            systemRenderer.sharedMaterial.shader = Shader.Find("PWS/VFX/PW_VFX_SnowFall_Middle");
                        }
                    }

                    //Clouds
                    GameObject cloudObject = GameObject.Find("PW_VFX_SkyDome");
                    if (cloudObject != null)
                    {
                        MeshRenderer[] meshRenderers = cloudObject.GetComponentsInChildren<MeshRenderer>();
                        if (meshRenderers.Length > 0)
                        {
                            foreach (MeshRenderer renderer in meshRenderers)
                            {
                                if (renderer.name.Contains("Space"))
                                {
                                    renderer.sharedMaterial.shader = Shader.Find("PWS/VFX/PW_VFX_SkyDome_Space");
                                }
                                else
                                {
                                    renderer.sharedMaterial.shader = Shader.Find("PWS/VFX/PW_SkydomeCloudsHight");
                                }
                            }
                        }
                    }

                    HDRPUpdateSkyMode();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Initialize Shaders had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }

        /// <summary>
        /// Setup used on disable
        /// </summary>
        private void DeInitialize()
        {
            try
            {
                m_renderPipeline = GaiaUtils.GetActivePipeline();

                Shader.SetGlobalFloat(m_shaderRainIntensity, 0f);
                Shader.SetGlobalFloat(m_globalSnowIntensity, 0f);
                Shader.SetGlobalFloat(m_shaderSeasonColorShift, 0f);
                Shader.SetGlobalFloat(m_globalWindSpeed, 0f);
                Shader.SetGlobalFloat(m_globalWindFrequency, 0f);
                Shader.SetGlobalFloat(m_globalWindGustDistance, 0f);

                if (m_snowParticles != null)
                {
                    m_snowParticles.Stop();
                    m_snowParticleEmission = m_snowParticles.emission;
                    m_snowParticleEmission.rateOverTime = 0;
                }

                if (m_snowVFX != null)
                {
                    m_snowVFX.Stop();
                    m_snowVFXEmission = m_snowVFX.emission;
                    m_snowVFXEmission.rateOverTime = 0;
                }

                if (m_rainParticles != null)
                {
                    m_rainParticles.sharedMaterial.SetFloat("_PW_VFX_Weather_Intensity", 0f);
                }

                if (m_rainVFX != null)
                {
                    m_rainVFX.Stop();
                }

                if (m_sunLight == null)
                {
                    m_sunLight = GaiaUtils.GetMainDirectionalLight();
                }

                if (m_moonLight == null)
                {
                    m_moonLight = GaiaUtils.GetMainMoonLight();
                }

                if (GaiaGlobal.Instance != null)
                {
                    GaiaGlobal.Instance.UpdateGaiaTimeOfDay(false);
                }

                IsSnowingFinished = false;
                IsRainingFinished = false;

                UpdateAllSystems(false);
            }
            catch (Exception e)
            {
                Debug.LogError("DeInitialize had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }

        /// <summary>
        /// Setup used on disable for post fx
        /// </summary>
        private void DeinitializePostFX()
        {
            try
            {
                if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.BuiltIn)
                {
#if UNITY_POST_PROCESSING_STACK_V2

                    UnityEngine.Rendering.PostProcessing.Bloom bloom;
                    UnityEngine.Rendering.PostProcessing.ColorGrading colorGrading;
                    UnityEngine.Rendering.PostProcessing.Vignette vignette;
                    if (m_processProfile != null)
                    {
                        if (m_processProfile.TryGetSettings(out bloom))
                        {
                            bloom.intensity.overrideState = true;
                            bloom.intensity.value = CurrentScenePostFXSettings.m_fxBloomIntensity;
                        }

                        if (m_processProfile.TryGetSettings(out colorGrading))
                        {
                            colorGrading.temperature.overrideState = true;
                            colorGrading.temperature.value = CurrentScenePostFXSettings.m_fxTemperature;

                            colorGrading.tint.overrideState = true;
                            colorGrading.tint.value = CurrentScenePostFXSettings.m_fxTint;

                            colorGrading.colorFilter.overrideState = true;
                            colorGrading.colorFilter.value = CurrentScenePostFXSettings.m_fxColorFilter;
                        }

                        if (m_processProfile.TryGetSettings(out vignette))
                        {
                            vignette.intensity.overrideState = true;
                            vignette.intensity.value = CurrentScenePostFXSettings.m_fxVignetteIntensity;
                        }
                    }

#endif
                }
                else if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.Universal)
                {
#if UPPipeline
                    if (m_urpFXVolumeProfile == null)
                    {
                        m_urpFXVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Post Processing", "Null");
                    }

                    UnityEngine.Rendering.Universal.Bloom bloom;
                    UnityEngine.Rendering.Universal.ColorAdjustments colorGrading;
                    UnityEngine.Rendering.Universal.WhiteBalance whiteBalance;
                    UnityEngine.Rendering.Universal.Vignette vignette;
                    if (m_urpFXVolumeProfile != null)
                    {
                        if (m_urpFXVolumeProfile.TryGet(out bloom))
                        {
                            bloom.intensity.overrideState = true;
                            bloom.intensity.value = CurrentScenePostFXSettings.m_fxBloomIntensity;
                        }

                        if (m_urpFXVolumeProfile.TryGet(out colorGrading))
                        {
                            colorGrading.colorFilter.overrideState = true;
                            colorGrading.colorFilter.value = CurrentScenePostFXSettings.m_fxColorFilter;
                        }

                        if (m_urpFXVolumeProfile.TryGet(out whiteBalance))
                        {
                            whiteBalance.temperature.overrideState = true;
                            whiteBalance.temperature.value = CurrentScenePostFXSettings.m_fxTemperature;

                            whiteBalance.tint.overrideState = true;
                            whiteBalance.tint.value = CurrentScenePostFXSettings.m_fxTint;
                        }

                        if (m_urpFXVolumeProfile.TryGet(out vignette))
                        {
                            vignette.intensity.overrideState = true;
                            vignette.intensity.value = CurrentScenePostFXSettings.m_fxVignetteIntensity;
                        }
                    }
#endif
                }
                else
                {
#if HDPipeline
                    if (m_hdrpFXVolumeProfile == null)
                    {
                        m_hdrpFXVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Post Processing", "Null");
                    }

                    UnityEngine.Rendering.HighDefinition.Bloom bloom;
                    UnityEngine.Rendering.HighDefinition.ColorAdjustments colorGrading;
                    UnityEngine.Rendering.HighDefinition.WhiteBalance whiteBalance;
                    UnityEngine.Rendering.HighDefinition.Vignette vignette;
                    if (m_hdrpFXVolumeProfile != null)
                    {
                        if (m_hdrpFXVolumeProfile.TryGet(out bloom))
                        {
                            bloom.intensity.overrideState = true;
                            bloom.intensity.value = CurrentScenePostFXSettings.m_fxBloomIntensity;
                        }

                        if (m_hdrpFXVolumeProfile.TryGet(out colorGrading))
                        {
                            colorGrading.colorFilter.overrideState = true;
                            colorGrading.colorFilter.value = CurrentScenePostFXSettings.m_fxColorFilter;
                        }

                        if (m_hdrpFXVolumeProfile.TryGet(out whiteBalance))
                        {
                            whiteBalance.temperature.overrideState = true;
                            whiteBalance.temperature.value = CurrentScenePostFXSettings.m_fxTemperature;

                            whiteBalance.tint.overrideState = true;
                            whiteBalance.tint.value = CurrentScenePostFXSettings.m_fxTint;
                        }

                        if (m_hdrpFXVolumeProfile.TryGet(out vignette))
                        {
                            vignette.intensity.overrideState = true;
                            vignette.intensity.value = CurrentScenePostFXSettings.m_fxVignetteIntensity;
                        }
                    }
#endif
                }
            }
            catch (Exception e)
            {
                Debug.LogError("DeInitialize Post FX had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }

        #endregion

        #region Update Check Functions

        /// <summary>
        /// Checks if it's raining or not
        /// </summary>
        private void CheckRainConditions()
        {
            try
            {
                if (!IsSnowing)
                {
                    if (!IsRaining)
                    {
                        m_rainWaitTime -= Time.deltaTime;
                        if (m_rainWaitTime < 0)
                        {
                            m_actualChance = m_rainSampleStrength * m_rainWeatherSettings.m_chance;
                            m_randomValue = Random.value;
                            m_rainDuration = GenerateRandomRangeValue(m_rainWeatherSettings.m_durationMinWaitTime, m_rainWeatherSettings.m_durationMaxWaitTime);
                            if (m_randomValue < m_actualChance)
                            {
                                SkyboxShaderValid = ValidateSkyboxShader();
                                if (m_snowStormChance < m_rainSampleStrength * m_snowStormChance)
                                {
                                    if (IsNetworkSynced)
                                    {
                                        IsCurrentSettingsSet = false;
                                        m_snowAudioSource.volume = 0f;
                                        m_weatherSystemActive = true;
                                        IsSnowing = true;
                                        IsSnowingFinished = false;
                                        TransationTime = 0;
                                    }
                                }
                                else
                                {
                                    if (IsNetworkSynced)
                                    {
                                        IsCurrentSettingsSet = false;
                                        m_rainAudioSource.volume = 0f;
                                        m_weatherSystemActive = true;
                                        IsRaining = true;
                                        IsRainingFinished = false;
                                        m_thunderActualChance = m_rainSampleStrength * m_thunderChance;
                                        m_thunderRandomValue = Random.value;
                                        m_thunderWaitDuration = ThunderWaitDuration;
                                        if (m_thunderRandomValue < m_thunderActualChance)
                                        {
                                            IsThundering = true;
                                        }
                                        TransationTime = 0;
                                    }
                                }
                            }
                            else
                            {
                                m_rainWaitTime = GenerateRandomRangeValue(m_rainWeatherSettings.m_minWaitTime, m_rainWeatherSettings.m_maxWaitTime);
                            }
                        }
                    }
                    else
                    {
                        if (IsNetworkSynced)
                        {
                            m_rainDuration -= Time.deltaTime;
                            if (m_rainDuration < 0)
                            {
                                SkyboxShaderValid = ValidateSkyboxShader();
                                IsRaining = false;
                                IsRainingFinished = false;
                                m_rainWaitTime = GenerateRandomRangeValue(m_rainWeatherSettings.m_minWaitTime, m_rainWeatherSettings.m_maxWaitTime);
                                TransationTime = 0;
                                GetCurrentSettings();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Check Rain had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Checks if it's snowing or not
        /// </summary>
        private void CheckSnowConditions()
        {
            try
            {
                if (!IsSnowing)
                {
                    if (IsRaining)
                    {
                        return;
                    }

                    m_snowWaitTime -= Time.deltaTime;
                    if (m_snowWaitTime < 0)
                    {
                        m_snowActualChance = m_snowSampleStrength * m_snowWeatherSettings.m_chance;
                        m_snowRandomValue = Random.value;
                        m_snowDuration = GenerateRandomRangeValue(m_snowWeatherSettings.m_durationMinWaitTime, m_snowWeatherSettings.m_durationMaxWaitTime);
                        if (m_snowRandomValue < m_snowActualChance)
                        {
                            SkyboxShaderValid = ValidateSkyboxShader();
                            if (IsNetworkSynced)
                            {
                                IsCurrentSettingsSet = false;
                                m_snowAudioSource.volume = 0f;
                                m_weatherSystemActive = true;
                                IsSnowing = true;
                                IsSnowingFinished = false;
                                TransationTime = 0;
                            }
                        }
                        else
                        {
                            m_snowWaitTime = GenerateRandomRangeValue(m_snowWeatherSettings.m_minWaitTime, m_snowWeatherSettings.m_maxWaitTime);
                        }
                    }
                }
                else
                {
                    if (IsNetworkSynced)
                    {
                        m_snowDuration -= Time.deltaTime;
                        if (m_snowDuration < 0)
                        {
                            SkyboxShaderValid = ValidateSkyboxShader();
                            IsSnowing = false;
                            IsSnowingFinished = false;
                            m_snowWaitTime = GenerateRandomRangeValue(m_snowWeatherSettings.m_minWaitTime, m_snowWeatherSettings.m_maxWaitTime);
                            TransationTime = 0;
                            GetCurrentSettings();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Check Snow had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Checks if it's thundering or not
        /// </summary>
        private void CheckThunderConditions()
        {
            try
            {
                if (IsThundering)
                {
                    m_thunderWaitDuration -= Time.deltaTime;
                    if (m_thunderWaitDuration < 0)
                    {
                        UpdateThunderSystem();
                        m_thunderWaitDuration = ThunderWaitDuration;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Check Thunder had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }

        #endregion

        #region System Events

        /// <summary>
        /// Updates all the systems
        /// </summary>
        public void UpdateAllSystems(bool revertDefault)
        {
            try
            {
                if (revertDefault)
                {
                    UpdateWindSettings(WindZone);
                    UpdateCloudSettings();
                    UpdateTerrainWindSettings();
                }
                else
                {
                    m_updateWind = true;
                    m_updateSnow = true;
                    m_updateSeason = true;
                    m_updateRain = true;
                    m_updateClouds = true;

                    UpdateWeatherEvents();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Update all systems had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Forces the updates the sky shaders
        /// </summary>
        public void ForceUpdateSkyShaders()
        {
            try
            {
                if (PW_VFX_Atmosphere.Instance != null)
                {
                    PW_VFX_Atmosphere.Instance.UpdateSunPosition();
                    PW_VFX_Atmosphere.Instance.UpdateSystem();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Fource sky shader updates had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Updates the master volume
        /// </summary>
        private void UpdateAudioVolume()
        {
            try
            {
                //Only checks while playing
                if (Application.isPlaying)
                {
                    if (m_rainAudioSource != null)
                    {
                        if (IsRaining)
                        {
                            if (m_rainAudioSource.volume > GlobalVolume || m_rainAudioSource.volume > RainVolume)
                            {
                                m_rainAudioSource.volume = RainVolume * GlobalVolume;
                            }
                            else if (m_rainAudioSource.volume < GlobalVolume || m_rainAudioSource.volume < RainVolume)
                            {
                                m_rainAudioSource.volume = RainVolume * GlobalVolume;
                            }
                        }
                    }

                    if (m_snowAudioSource != null)
                    {
                        if (IsSnowing)
                        {
                            if (m_snowAudioSource.volume > GlobalVolume || m_snowAudioSource.volume > SnowVolume)
                            {
                                m_snowAudioSource.volume = SnowVolume * GlobalVolume;
                            }
                            else if (m_snowAudioSource.volume < GlobalVolume || m_snowAudioSource.volume < SnowVolume)
                            {
                                m_snowAudioSource.volume = SnowVolume * GlobalVolume;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Updating audio volumes had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Weather update event refreshes the weather settings
        /// </summary>
        private void UpdateWeatherEvents()
        {
            try
            {
                if (!Application.isPlaying)
                {
                    CheckValidSystems();
                }
                if (m_updateWind)
                {
                    //Gets the wind zone if it's empty
                    if (WindZone == null)
                    {
                        WindZone = GetOrCreateWindZone();
                    }

                    //Set the global wind settings
                    UpdateWindSettings(WindZone);

                    //Sets the terrain grass wind settings
                    UpdateTerrainWindSettings();
                    m_updateWind = false;
                }

                if (m_updateRain)
                {
                    //Sets the global rain settings
                    UpdateRainSettings();
                    m_updateRain = false;
                }

                if (m_updateSnow)
                {
                    //Sets the global snow settings
                    UpdateSnowSettings();
                    m_updateSnow = false;
                }

                if (m_updateClouds)
                {
                    //Sets the cloud settings
                    UpdateCloudSettings();
                    m_updateClouds = false;
                }

                if (m_updateSeason)
                {
                    //Sets the season settings
                    UpdateSeasonMode();
                    m_updateSeason = false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Updating weather events had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Updates the wind settings
        /// </summary>
        /// <param name="windZone"></param>
        private void UpdateWindSettings(WindZone windZone)
        {
            try
            {
                float windSpeed = WindSpeed * WindMultiplier;
                float windFrequency = WindFrequency * WindMultiplier;
                float windTurbulence = WindTurbulence * WindMultiplier;
                Shader.SetGlobalFloat(m_globalWindSpeed, windSpeed);
                Shader.SetGlobalFloat(m_globalWindFrequency, windFrequency);
                Shader.SetGlobalFloat(m_globalWindGustDistance, windTurbulence);

                windZone.windMain = windSpeed;
                windZone.windTurbulence = windTurbulence;
                windZone.windPulseFrequency = windFrequency;

                WindZone.gameObject.transform.rotation = Quaternion.Euler(25f, WindDirection * 360f, 0f);

                if (PWS_WaterSystem.Instance != null)
                {
                    PWS_WaterSystem.Instance.directionAngle = WindDirection * 360f;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Updating wind settings had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Updates the wind values
        /// </summary>
        /// <param name="windType"></param>
        public void UpdateWindValues(GaiaConstants.GaiaGlobalWindType windType)
        {
            try
            {
                switch (windType)
                {
                    case GaiaConstants.GaiaGlobalWindType.Calm:
                        WindSpeed = 0.2f;
                        WindTurbulence = 0.2f;
                        WindFrequency = 0.05f;
                        break;
                    case GaiaConstants.GaiaGlobalWindType.Moderate:
                        WindSpeed = 0.45f;
                        WindTurbulence = 0.4f;
                        WindFrequency = 0.2f;
                        break;
                    case GaiaConstants.GaiaGlobalWindType.Strong:
                        WindSpeed = 1f;
                        WindTurbulence = 0.8f;
                        WindFrequency = 0.5f;
                        break;
                    case GaiaConstants.GaiaGlobalWindType.None:
                        WindSpeed = 0f;
                        WindTurbulence = 0f;
                        WindFrequency = 0f;
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Updating wind values had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Updates the rain settings
        /// </summary>
        private void UpdateRainSettings()
        {
            try
            {
                if (!Application.isPlaying && !RunInEditor)
                {
                    return;
                }

                if (EnableRain)
                {
                    Shader.SetGlobalFloat(m_shaderRainIntensity, RainIntensity);
                }
                else
                {
                    Shader.SetGlobalFloat(m_shaderRainIntensity, 0f);
                }

#if CTS_PRESENT
                if (m_cTSWeatherManager == null)
                {
                    m_cTSWeatherManager = GetOrCreateWeatherManager();
                }

                SetCTSRain(m_cTSWeatherManager);
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Updating rain settings had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Updates the snow settings if they have changed
        /// </summary>
        private void UpdateSnowSettings()
        {
            try
            {
                if (EnableSnow)
                {
                    if (SnowCoverAlwaysEnabled)
                    {
                        Shader.SetGlobalFloat(m_globalSnowIntensity, 1f);
                    }
                    else
                    {
                        Shader.SetGlobalFloat(m_globalSnowIntensity, SnowIntensity);
                    }

                    if (IsSnowing && !IsSnowingFinished)
                    {
                        Shader.SetGlobalFloat(m_globalCoverLayer1FadeStart, m_savedSnowingHeight);
                    }
                    else if (!IsSnowing && IsSnowingFinished)
                    {
                        Shader.SetGlobalFloat(m_globalCoverLayer1FadeStart, PermanentSnowHeight);
                    }
                    else
                    {
                        Shader.SetGlobalFloat(m_globalCoverLayer1FadeStart, m_savedSnowingHeight);
                    }

                    Shader.SetGlobalFloat(m_globalCoverLayer1FadeDist, SnowFadeHeight);
                }
                else
                {
                    Shader.SetGlobalFloat(m_globalSnowIntensity, 0f);
                    Shader.SetGlobalFloat(m_globalCoverLayer1FadeStart, 0f);
                    Shader.SetGlobalFloat(m_globalCoverLayer1FadeDist, 0f);
                }

#if CTS_PRESENT
                if (m_cTSWeatherManager == null)
                {
                    m_cTSWeatherManager = GetOrCreateWeatherManager();
                }

                SetCTSSnow(m_cTSWeatherManager);
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Updating snow settings had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Updates the season settings
        /// </summary>
        private void UpdateSeasonMode()
        {
            try
            {
                if (EnableSeasons)
                {
                    Color color = GetActualSeasonalTintColor();
                    if (color == Color.black)
                    {
                        color = Color.white;
                    }
                    Shader.SetGlobalColor(m_globalSeasonTint, GaiaUtils.ColorInvert(color));

                    if (GaiaSceneProfileValid)
                    {
                        GaiaGlobal.Instance.SceneProfile.m_gaiaWeather.m_season = Season;
                    }
                }
                else
                {
                    Shader.SetGlobalColor(m_globalSeasonTint, GaiaUtils.ColorInvert(Color.white));
                }

#if CTS_PRESENT
            if (m_cTSWeatherManager == null)
            {
                m_cTSWeatherManager = GetOrCreateWeatherManager();
            }

            if (m_cTSWeatherManager != null)
            {
                m_cTSWeatherManager.SeasonalTintActive = EnableSeasons;
                m_cTSWeatherManager.Season = Season;
                m_cTSWeatherManager.WinterTint = SeasonWinterTint;
                m_cTSWeatherManager.SpringTint = SeasonSpringTint;
                m_cTSWeatherManager.SummerTint = SeasonSummerTint;
                m_cTSWeatherManager.AutumnTint = SeasonAutumnTint;
            }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Updating seasons settings had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Updates the terrain wind settings
        /// </summary>
        private void UpdateTerrainWindSettings()
        {
            try
            {
                if (Terrain.activeTerrains.Length > 0)
                {
                    foreach (Terrain terrain in Terrain.activeTerrains)
                    {
                        //"Speed" in terrain inspector
                        terrain.terrainData.wavingGrassStrength = Mathf.Clamp(WindSpeed * 2f, MinTerrainWind, MaxTerrainWind);
                        //"Size" (Wind Wave Size) in  terrain inspector
                        terrain.terrainData.wavingGrassSpeed = Mathf.Clamp(WindTurbulence * 2f, MinTerrainWind, MaxTerrainWind);
                        //"Bending" in terrain inspector
                        terrain.terrainData.wavingGrassAmount = Mathf.Clamp(WindFrequency / 3f, MinTerrainWind, MaxTerrainWind);

                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Updating terrain wind settings had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Updates the weather system
        /// </summary>
        private bool UpdateWeatherSystem()
        {
            try
            {
                bool finishedProcessing = false;
                if (!Application.isPlaying && !RunInEditor)
                {
                    return finishedProcessing;
                }

                if (!IsCurrentSettingsSet)
                {
                    GetCurrentSettings();
                    IsCurrentSettingsSet = true;
                }

                if (m_snowParticles != null)
                {
                    if (m_snowAudioSource == null)
                    {
                        m_snowAudioSource = m_snowParticles.GetComponent<AudioSource>();
                        if (m_snowAudioSource == null)
                        {
                            m_snowAudioSource = m_snowParticles.gameObject.AddComponent<AudioSource>();
                            m_snowAudioSource.volume = 0f;
                            m_snowAudioSource.loop = true;
                        }
                    }
                    m_snowAudioSource.clip = m_snowWeatherSettings.m_ambientAudio;

                    if (EnableSnow)
                    {
                        if (IsSnowing)
                        {
                            EnableSnowSystem();
                            DisableRainSystem();
                        }
                        else
                        {
                            DisableSnowSystem();
                        }

                        UpdateCloudSettings();
                    }
                    else
                    {
                        m_snowAudioSource.Stop();
                        m_snowAudioSource.volume = 0;

                        IsSnowing = false;
                    }
                }

                if (m_rainParticles != null)
                {
                    if (m_rainAudioSource == null)
                    {
                        m_rainAudioSource = m_rainParticles.GetComponent<AudioSource>();
                        if (m_rainAudioSource == null)
                        {
                            m_rainAudioSource = m_rainParticles.gameObject.AddComponent<AudioSource>();
                            m_rainAudioSource.volume = 0f;
                            m_rainAudioSource.loop = true;
                        }
                    }
                    m_rainAudioSource.clip = m_rainWeatherSettings.m_ambientAudio;

                    if (EnableRain)
                    {
                        if (!IsSnowing)
                        {
                            if (IsRaining)
                            {
                                EnableRainSystem();
                                DisableSnowSystem();
                            }
                            else
                            {
                                DisableRainSystem();
                            }
                        }

                        UpdateCloudSettings();
                    }
                    else
                    {
                        m_rainAudioSource.Stop();
                        m_rainAudioSource.volume = 0;
                        RainIntensity = 0;
                    }
                }

                return finishedProcessing;
            }
            catch (Exception e)
            {
                Debug.LogError("Updating weather systems had a issue " + e.Message + " This came from " + e.StackTrace);
                return false;
            }
        }
        /// <summary>
        /// Updates the clouds system
        /// </summary>
        private void UpdateCloudSettings()
        {
            try
            {
                if (CloudSystemValid)
                {
                    if (EnableClouds)
                    {
                        PW_VFX_Clouds.Instance.SetCloudRenderer(true);
                    }
                    else
                    {
                        PW_VFX_Clouds.Instance.SetCloudRenderer(false);
                    }

                    PW_VFX_Clouds.Instance.SunLight = m_sunLight;
                    PW_VFX_Clouds.Instance.AmbColor = CloudAmbientColor;
                    PW_VFX_Clouds.Instance.m_scale = CloudScale;
                    if (WaterSystemValid)
                    {
                        PW_VFX_Clouds.Instance.m_seaLevel = PWS_WaterSystem.Instance.SeaLevel;
                    }
                    else
                    {
                        PW_VFX_Clouds.Instance.m_seaLevel = 50f;
                    }
                    PW_VFX_Clouds.Instance.m_yOffset = CloudOffset;
                    PW_VFX_Clouds.Instance.PW_SkyDome_Opacity = CloudOpacity;
                    PW_VFX_Clouds.Instance.PW_Wind_Cloud_Dir = CloudTilingAndWind;
                    PW_VFX_Clouds.Instance.PW_Cloud_Brightness = CloudBrightness;
                    PW_VFX_Clouds.Instance.PW_SkyDome_Brightness = CloudDomeBrightness;

                    PW_VFX_Clouds.Instance.PW_Clouds_Fade = CloudFade;
                    PW_VFX_Clouds.Instance.PW_Wind_Cloud_Dir = new Vector4(CloudTilingAndWind.x, CloudTilingAndWind.y, WindSpeed, CloudTilingAndWind.w);
                    PW_VFX_Clouds.Instance.m_cloudRotationSpeedLow = CloudRotationSpeedLow;
                    PW_VFX_Clouds.Instance.m_cloudRotationSpeedMiddle = CloudRotationSpeedMiddle;
                    PW_VFX_Clouds.Instance.m_cloudRotationSpeedFar = CloudRotationSpeedFar;
                    if (!Application.isPlaying)
                    {
                        PW_VFX_Clouds.Instance.CloudsUpdate();
                    }
                }
                if (AtmosphereSystemValid)
                {
                    PW_VFX_Atmosphere.Instance.SkyboxMaterial = m_skyboxMaterial;
                    PW_VFX_Atmosphere.Instance.TimeOfDaySunColor = TODSunColor;
                    PW_VFX_Atmosphere.Instance.TimeOfDayFogColor = TODFogColor;
                    PW_VFX_Atmosphere.Instance.TimeOfDaySkyColor = TODAmbientSkyColor;
                    PW_VFX_Atmosphere.Instance.TimeOfDayEqutorColor = TODAmbientEquatorColor;
                    PW_VFX_Atmosphere.Instance.TimeOfDayGroundColor = TODAmbientGroundColor;
                    PW_VFX_Atmosphere.Instance.AtmosphereThickness = TODAtmosphereThickness;
                    PW_VFX_Atmosphere.Instance.AmbientIntensity = TODAmbientIntensity;
                    PW_VFX_Atmosphere.Instance.SunBrightnessIntensity = TODSunIntensity;
                    PW_VFX_Atmosphere.Instance.TimeOfDayShadowStrength = TODSunShadowStrength;
                    PW_VFX_Atmosphere.Instance.CloudsHightLevelDensity = TODCloudHeightLevelDensity;
                    PW_VFX_Atmosphere.Instance.CloudsHightLevelThickness = TODCloudHeightLevelThickness;
                    PW_VFX_Atmosphere.Instance.CloudsHightLevelSpeed = TODCloudHeightLevelSpeed;
                    PW_VFX_Atmosphere.Instance.CloudOpacity = TODCloudOpacity;
                    PW_VFX_Atmosphere.Instance.FogDensityColor = TODFogDensity;
                    PW_VFX_Atmosphere.Instance.FogStartDistance = TODFogStartDistance;
                    PW_VFX_Atmosphere.Instance.FogEndDistance = TODFogEndDistance;
                    PW_VFX_Atmosphere.Instance.SunSize = TODSunSize;
                    PW_VFX_Atmosphere.Instance.SunSizeConvergence = TODSunSizeConvergence;
                    PW_VFX_Atmosphere.Instance.SkyboxTint = TODSkyboxTint;
                    PW_VFX_Atmosphere.Instance.SkyboxExposure = TODSkyboxExposure;
                    PW_VFX_Atmosphere.Instance.TODSkyboxFogHeight = TODSkyboxFogHeight;
                    PW_VFX_Atmosphere.Instance.TODSkyboxFogGradient = TODSkyboxFogGradient;
#if HDPipeline
                    PW_VFX_Atmosphere.Instance.HDRPFogAnisotropy = TODHDRPFogAnisotropy;
                    PW_VFX_Atmosphere.Instance.HDRPFogBaseHeight = TODHDRPFogBaseHeight;
                    PW_VFX_Atmosphere.Instance.HDRPFogDepthExtent = TODHDRPFogDepthExtent;
                    PW_VFX_Atmosphere.Instance.HDRPFogLightProbeDimmer = TODHDRPFogLightProbeDimmer;
                    PW_VFX_Atmosphere.Instance.HDRPGroundColor = TODHDRPGroundTint;
                    PW_VFX_Atmosphere.Instance.HDRPFogAlbedo = TODHDRPFogAlbedo;
#endif
                    if (!Application.isPlaying)
                    {
                        PW_VFX_Atmosphere.Instance.UpdateSystem();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Updating Cloud settings had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Updates the thunder systen
        /// </summary>
        private void UpdateThunderSystem()
        {
            try
            {
                if (!IsThundering || !Application.isPlaying)
                {
                    return;
                }

                if (m_thunderComponent != null)
                {
                    m_thunderComponent.GetAllVariables();
                    m_thunderComponent.PlayThunderSystem();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Playing thunder had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }

        #endregion

        #region Weather Setup Functions

        /// <summary>
        /// Enable Rain
        /// </summary>
        /// <returns></returns>
        private bool EnableRainSystem()
        {
            try
            {
                IsRaining = true;

                if (m_rainAudioSource == null)
                {
                    m_rainAudioSource = m_rainParticles.GetComponent<AudioSource>();
                    if (m_rainAudioSource == null)
                    {
                        m_rainAudioSource = m_rainParticles.gameObject.AddComponent<AudioSource>();
                        m_rainAudioSource.volume = 0f;
                        m_rainAudioSource.loop = true;
                    }
                }
                m_rainAudioSource.clip = m_rainWeatherSettings.m_ambientAudio;

                if (!m_rainAudioSource.isPlaying)
                {
                    m_rainAudioSource.Play();
                }

                m_rainParticles.transform.position = new Vector3(m_player.transform.position.x, m_player.transform.position.y - 10, m_player.transform.position.z);
                if (!IsRainingFinished)
                {
                    if (!m_weatherSystemActive)
                    {
                        return true;
                    }

                    if (!m_getCurrentRainValue)
                    {
                        m_currentRainIntensity = RainIntensity;
                        m_currentRainVolume = m_rainAudioSource.volume;
                        if (m_rainParticles != null)
                        {
                            m_currentRainParticleAlpha = m_rainParticles.sharedMaterial.GetFloat(GaiaShaderID.m_rainIntensity);
                        }
                        else
                        {
                            m_currentRainParticleAlpha = 0f;
                        }
                        m_getCurrentRainValue = true;
                    }

                    m_rainAudioSource.volume = Mathf.Lerp(m_currentRainVolume, RainVolume * GlobalVolume, GetFadeTime());
                    RainIntensity = Mathf.Lerp(m_currentRainIntensity, 1f, GetFadeTime());

                    if (GaiaUnderwaterEffects.Instance != null)
                    {
                        if (!GaiaUnderwaterEffects.Instance.IsUnderwater)
                        {
                            if (m_modifyFog)
                            {
                                if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                                {
                                    SetHDRPFogToWeather(true, m_rainWeatherSettings);
                                }
                                else
                                {
                                    SetFogToWeather(true, m_rainWeatherSettings);
                                }
                            }
                        }
                    }

                    SetRainVFX(true, m_rainParticles, m_rainVFX);

                    if (m_modifyClouds)
                    {
                        SetCloudsToWeather(true, m_rainWeatherSettings);
                    }

                    if (m_modifySkybox)
                    {
                        if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                        {
                            SetHDRPSkyboxToWeather(true, m_rainWeatherSettings);
                        }
                        else
                        {
                            SetSkyboxToWeather(true, m_rainWeatherSettings);
                        }
                    }

                    if (m_modifyPostProcessing)
                    {
                        if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                        {
                            SetHDRPFXToWeather(true, m_rainWeatherSettings);
                        }
                        else if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.Universal)
                        {
                            SetURPFXToWeather(true, m_rainWeatherSettings);
                        }
                        else
                        {
                            SetFXToWeather(true, m_rainWeatherSettings);
                        }
                    }

                    if (m_modifyWind)
                    {
                        SetWindToWeather(true, m_rainWeatherSettings);
                    }

                    if (m_modifySun)
                    {
                        if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                        {
                            SetHDRPSunToWeather(true, m_rainWeatherSettings);
                        }
                        else
                        {
                            SetSunToWeather(true, m_weatherSystemActive, m_rainWeatherSettings);
                        }
                    }

                    if (m_modifyAmbient)
                    {
                        if (m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
                        {
                            SetAmbientToWeather(true, m_rainWeatherSettings);
                        }
                    }

                    SetParticlesToWeather(m_rainParticles, m_rainVFX, m_rainWeatherSettings);
                    IsRainingFinished = FinishedWeatherCheck(RainIntensity, RainVolume);
                }
                else
                {
                    if (IsRaining)
                    {
                        SetAllAfterFinishedLerping(m_rainWeatherSettings);
                        SetParticlesToWeather(m_rainParticles, m_rainVFX, m_rainWeatherSettings);
                    }
                }

                return IsRainingFinished;
            }
            catch (Exception e)
            {
                Debug.LogError("Enable Rain had a issue " + e.Message + " This came from " + e.StackTrace);
                return false;
            }
        }
        /// <summary>
        /// Disable Rain
        /// </summary>
        /// <returns></returns>
        private bool DisableRainSystem()
        {
            try
            {
                if (m_rainAudioSource == null)
                {
                    m_rainAudioSource = m_rainParticles.GetComponent<AudioSource>();
                    if (m_rainAudioSource == null)
                    {
                        m_rainAudioSource = m_rainParticles.gameObject.AddComponent<AudioSource>();
                        m_rainAudioSource.volume = 0f;
                        m_rainAudioSource.loop = true;
                    }
                }
                m_rainAudioSource.clip = m_rainWeatherSettings.m_ambientAudio;

                if (!IsRainingFinished)
                {
                    if (!m_weatherSystemActive)
                    {
                        return true;
                    }

                    if (m_getCurrentRainValue)
                    {
                        m_currentRainIntensity = RainIntensity;
                        m_currentRainVolume = m_rainAudioSource.volume;
                        if (m_rainParticles != null)
                        {
                            m_currentRainParticleAlpha = m_rainParticles.sharedMaterial.GetFloat(GaiaShaderID.m_rainIntensity);
                        }
                        else
                        {
                            m_currentRainParticleAlpha = 0.95f;
                        }
                        m_getCurrentRainValue = false;
                    }

                    m_rainParticles.transform.position = new Vector3(m_player.transform.position.x, m_player.transform.position.y - 10, m_player.transform.position.z);
                    m_rainAudioSource.volume = Mathf.Lerp(m_currentRainVolume, 0f, GetFadeTime());
                    RainIntensity = Mathf.Lerp(m_currentRainIntensity, 0f, GetFadeTime());

                    if (GaiaUnderwaterEffects.Instance != null)
                    {
                        if (!GaiaUnderwaterEffects.Instance.IsUnderwater)
                        {
                            if (m_modifyFog)
                            {
                                if (!IsSnowing)
                                {
                                    if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                                    {
                                        SetHDRPFogToWeather(false, m_rainWeatherSettings);
                                    }
                                    else
                                    {
                                        SetFogToWeather(false, m_rainWeatherSettings);
                                    }
                                }
                            }
                        }
                    }

                    SetRainVFX(false, m_rainParticles, m_rainVFX);

                    if (m_modifyClouds)
                    {
                        if (!IsSnowing)
                        {
                            SetCloudsToWeather(false, m_rainWeatherSettings);
                        }
                    }

                    if (m_modifySkybox)
                    {
                        if (!IsSnowing)
                        {
                            if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                            {
                                SetHDRPSkyboxToWeather(false, m_rainWeatherSettings);
                            }
                            else
                            {
                                SetSkyboxToWeather(false, m_rainWeatherSettings);
                            }
                        }
                    }

                    if (m_modifyPostProcessing)
                    {
                        if (!IsSnowing)
                        {
                            if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                            {
                                SetHDRPFXToWeather(false, m_rainWeatherSettings);
                            }
                            else if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.Universal)
                            {
                                SetURPFXToWeather(false, m_rainWeatherSettings);
                            }
                            else
                            {
                                SetFXToWeather(false, m_rainWeatherSettings);
                            }
                        }
                    }

                    if (m_modifyWind)
                    {
                        if (!IsSnowing)
                        {
                            SetWindToWeather(false, m_rainWeatherSettings);
                        }
                    }

                    if (m_modifySun)
                    {
                        if (!IsSnowing)
                        {
                            if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                            {
                                SetHDRPSunToWeather(false, m_rainWeatherSettings);
                            }
                            else
                            {
                                SetSunToWeather(false, m_weatherSystemActive, m_rainWeatherSettings);
                            }
                        }
                    }

                    if (m_modifyAmbient)
                    {
                        if (!IsSnowing)
                        {
                            if (m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
                            {
                                SetAmbientToWeather(false, m_rainWeatherSettings);
                            }
                        }
                    }

                    SetParticlesToWeather(m_rainParticles, m_rainVFX, m_rainWeatherSettings);
                    IsRainingFinished = FinishedWeatherCheck(RainIntensity, RainVolume);

                    if (IsRainingFinished)
                    {
                        if (m_rainAudioSource.isPlaying)
                        {
                            m_rainAudioSource.Stop();
                        }

                        if (m_disableWeatherFX)
                        {
                            EnableRain = false;
                            EnableSnow = false;
                            m_disableWeatherFX = false;
                        }

                        IsRaining = false;
                        IsThundering = false;
                        m_weatherSystemActive = false;
                        m_instantVFX = false;
                    }
                }

                return IsRainingFinished;
            }
            catch (Exception e)
            {
                Debug.LogError("Disable Rain had a issue " + e.Message + " This came from " + e.StackTrace);
                return false;
            }
        }
        /// <summary>
        /// Enables Snow
        /// </summary>
        /// <returns></returns>
        private bool EnableSnowSystem()
        {
            try
            {
                IsSnowing = true;

                if (m_snowAudioSource == null)
                {
                    m_snowAudioSource = m_snowParticles.GetComponent<AudioSource>();
                    if (m_snowAudioSource == null)
                    {
                        m_snowAudioSource = m_snowParticles.gameObject.AddComponent<AudioSource>();
                        m_snowAudioSource.volume = 0f;
                        m_snowAudioSource.loop = true;
                    }
                }
                m_snowAudioSource.clip = m_snowWeatherSettings.m_ambientAudio;

                if (!m_snowAudioSource.isPlaying)
                {
                    m_snowAudioSource.Play();
                }

                m_snowParticles.transform.position = new Vector3(m_player.transform.position.x, m_player.transform.position.y + 5f, m_player.transform.position.z);
                if (!IsSnowingFinished)
                {
                    if (!m_weatherSystemActive)
                    {
                        return true;
                    }

                    if (!m_getCurrentSnowValue)
                    {
                        m_currentSnowIntensity = SnowIntensity;
                        m_currentSnowVolume = m_snowAudioSource.volume;
                        m_getCurrentSnowValue = true;
                    }

                    m_snowAudioSource.volume = Mathf.Lerp(m_currentSnowVolume, SnowVolume * GlobalVolume, GetFadeTime());
                    SnowIntensity = Mathf.Lerp(m_currentSnowIntensity, 1f, GetFadeTime());

                    if (GaiaUnderwaterEffects.Instance != null)
                    {
                        if (!GaiaUnderwaterEffects.Instance.IsUnderwater)
                        {
                            if (m_modifyFog)
                            {
                                if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                                {
                                    SetHDRPFogToWeather(true, m_snowWeatherSettings);
                                }
                                else
                                {
                                    SetFogToWeather(true, m_snowWeatherSettings);
                                }
                            }
                        }
                    }

                    SetSnowVFX(true, m_snowParticleEmission, m_snowVFXEmission, m_instantVFX);
                    SetParticlesToSnow(m_snowParticles, m_snowVFX, m_snowWeatherSettings);
                    SetSnowHeightToWeather(true, SnowingHeight);

                    if (m_modifyClouds)
                    {
                        SetCloudsToWeather(true, m_snowWeatherSettings);
                    }

                    if (m_modifySkybox)
                    {
                        if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                        {
                            SetHDRPSkyboxToWeather(true, m_snowWeatherSettings);
                        }
                        else
                        {
                            SetSkyboxToWeather(true, m_snowWeatherSettings);
                        }
                    }

                    if (m_modifyPostProcessing)
                    {
                        if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                        {
                            SetHDRPFXToWeather(true, m_snowWeatherSettings);
                        }
                        else if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.Universal)
                        {
                            SetURPFXToWeather(true, m_snowWeatherSettings);
                        }
                        else
                        {
                            SetFXToWeather(true, m_snowWeatherSettings);
                        }
                    }

                    if (m_modifyWind)
                    {
                        SetWindToWeather(true, m_snowWeatherSettings);
                    }

                    if (m_modifySun)
                    {
                        if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                        {
                            SetHDRPSunToWeather(true, m_snowWeatherSettings);
                        }
                        else
                        {
                            SetSunToWeather(true, m_weatherSystemActive, m_snowWeatherSettings);
                        }
                    }

                    if (m_modifyAmbient)
                    {
                        if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                        {
                            SetAmbientToWeather(true, m_snowWeatherSettings);
                        }
                    }

                    IsSnowingFinished = FinishedWeatherCheck(SnowIntensity, SnowVolume);
                }
                else
                {
                    if (IsSnowing)
                    {
                        SetAllAfterFinishedLerping(m_snowWeatherSettings);
                        SetParticlesToSnow(m_snowParticles, m_snowVFX, m_snowWeatherSettings);
                    }
                }

                return IsSnowingFinished;
            }
            catch (Exception e)
            {
                Debug.LogError("Enable Snow had a issue " + e.Message + " This came from " + e.StackTrace);
                return false;
            }
        }
        /// <summary>
        /// Disables snow
        /// </summary>
        /// <returns></returns>
        private bool DisableSnowSystem()
        {
            try
            {
                if (m_snowAudioSource == null)
                {
                    m_snowAudioSource = m_snowParticles.GetComponent<AudioSource>();
                    if (m_snowAudioSource == null)
                    {
                        m_snowAudioSource = m_snowParticles.gameObject.AddComponent<AudioSource>();
                        m_snowAudioSource.volume = 0f;
                        m_snowAudioSource.loop = true;
                    }
                }
                m_snowAudioSource.clip = m_snowWeatherSettings.m_ambientAudio;

                if (!IsSnowingFinished)
                {
                    if (!m_weatherSystemActive)
                    {
                        return true;
                    }

                    if (m_getCurrentSnowValue)
                    {
                        m_currentSnowIntensity = SnowIntensity;
                        m_currentSnowVolume = m_snowAudioSource.volume;
                        m_getCurrentSnowValue = false;
                    }

                    m_snowParticles.transform.position = new Vector3(m_player.transform.position.x, m_player.transform.position.y + 5f, m_player.transform.position.z);
                    m_snowAudioSource.volume = Mathf.Lerp(m_currentSnowVolume, 0f, GetFadeTime());
                    SnowIntensity = Mathf.Lerp(m_currentSnowIntensity, 0f, GetFadeTime());

                    if (GaiaUnderwaterEffects.Instance != null)
                    {
                        if (!GaiaUnderwaterEffects.Instance.IsUnderwater)
                        {
                            if (m_modifyFog)
                            {
                                if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                                {
                                    SetHDRPFogToWeather(false, m_snowWeatherSettings);
                                }
                                else
                                {
                                    SetFogToWeather(false, m_snowWeatherSettings);
                                }
                            }
                        }
                    }

                    SetSnowVFX(false, m_snowParticleEmission, m_snowVFXEmission, m_instantVFX);
                    SetParticlesToSnow(m_snowParticles, m_snowVFX, m_snowWeatherSettings);
                    SetSnowHeightToWeather(false, SnowingHeight);

                    if (m_modifyClouds)
                    {
                        if (!IsRaining)
                        {
                            SetCloudsToWeather(false, m_snowWeatherSettings);
                        }
                    }

                    if (m_modifySkybox)
                    {
                        if (!IsRaining)
                        {
                            if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                            {
                                SetHDRPSkyboxToWeather(false, m_snowWeatherSettings);
                            }
                            else
                            {
                                SetSkyboxToWeather(false, m_snowWeatherSettings);
                            }
                        }
                    }

                    if (m_modifyPostProcessing)
                    {
                        if (!IsRaining)
                        {
                            if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                            {
                                SetHDRPFXToWeather(false, m_snowWeatherSettings);
                            }
                            else if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.Universal)
                            {
                                SetURPFXToWeather(false, m_snowWeatherSettings);
                            }
                            else
                            {
                                SetFXToWeather(false, m_snowWeatherSettings);
                            }
                        }
                    }

                    if (m_modifyWind)
                    {
                        if (!IsRaining)
                        {
                            SetWindToWeather(false, m_snowWeatherSettings);
                        }
                    }

                    if (m_modifySun)
                    {
                        if (!IsRaining)
                        {
                            if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                            {
                                SetHDRPSunToWeather(false, m_snowWeatherSettings);
                            }
                            else
                            {
                                SetSunToWeather(false, m_weatherSystemActive, m_snowWeatherSettings);
                            }
                        }
                    }

                    if (m_modifyAmbient)
                    {
                        if (!IsRaining)
                        {
                            if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                            {
                                SetAmbientToWeather(false, m_snowWeatherSettings);
                            }
                        }
                    }

                    IsSnowingFinished = FinishedWeatherCheck(SnowIntensity, SnowVolume);

                    if (IsSnowingFinished)
                    {
                        if (m_snowAudioSource.isPlaying)
                        {
                            m_snowAudioSource.Stop();
                        }

                        if (m_disableWeatherFX)
                        {
                            EnableSnow = false;
                            EnableRain = false;
                            m_disableWeatherFX = false;
                        }

                        IsSnowing = false;
                        m_weatherSystemActive = false;
                        m_instantVFX = false;
                    }
                }
                else
                {
                    return true;
                }

                return IsSnowingFinished;
            }
            catch (Exception e)
            {
                Debug.LogError("Disable Snow had a issue " + e.Message + " This came from " + e.StackTrace);
                return false;
            }
        }

        #endregion

        #region CTS Functions

#if CTS_PRESENT
        /// <summary>
        /// Gets or creates the weather manager for CTS
        /// </summary>
        /// <returns></returns>
        private CTSWeatherManager GetOrCreateWeatherManager()
        {
            if (FindObjectOfType<CompleteTerrainShader>() == null)
            {
                return null;
            }

            CTSWeatherManager cTSWeatherManager = FindObjectOfType<CTSWeatherManager>();
            if (cTSWeatherManager == null)
            {
                //Add a weather manager
                GameObject ctsWeatherManager = GameObject.Find("CTS Weather Manager");
                if (ctsWeatherManager == null)
                {
                    ctsWeatherManager = new GameObject();
                    ctsWeatherManager.name = "CTS Weather Manager";
                    cTSWeatherManager = ctsWeatherManager.AddComponent<CTSWeatherManager>();
                    CompleteTerrainShader.SetDirty(ctsWeatherManager, false, false);
                }
#if UNITY_EDITOR
                EditorGUIUtility.PingObject(ctsWeatherManager);
#endif

                //And now add weather controllers
                foreach (var terrain in Terrain.activeTerrains)
                {
                    CompleteTerrainShader shader = terrain.gameObject.GetComponent<CompleteTerrainShader>();
                    if (shader != null)
                    {
                        CTSWeatherController controller = terrain.gameObject.GetComponent<CTSWeatherController>();
                        if (controller == null)
                        {
                            controller = terrain.gameObject.AddComponent<CTSWeatherController>();
                            CompleteTerrainShader.SetDirty(terrain, false, false);
                            CompleteTerrainShader.SetDirty(controller, false, false);
                        }
                    }
                }
            }

            return cTSWeatherManager;
        }

        /// <summary>
        /// Sets rain in CTS
        /// </summary>
        /// <param name="manager"></param>
        private void SetCTSRain(CTSWeatherManager manager)
        {
            if (manager == null)
            {
                return;
            }
            manager.RainPower = RainIntensity;
            manager.MaxRainSmoothness = 2f;
        }

        /// <summary>
        /// Sets snow in CTS
        /// </summary>
        /// <param name="manager"></param>
        private void SetCTSSnow(CTSWeatherManager manager)
        {
            if (manager == null)
            {
                return;
            }
            if (SnowCoverAlwaysEnabled)
            {
                manager.SnowPower = 1f;
            }
            else
            {
                manager.SnowPower = SnowIntensity;
            }

            manager.SnowMinHeight = PermanentSnowHeight;
        }
#endif

        #endregion

        #region Weather Check Functions

        /// <summary>
        /// Checks if the settings are finished
        /// </summary>
        /// <param name="weatherIntensity"></param>
        /// <returns></returns>
        private bool FinishedWeatherCheck(float weatherIntensity, float volume)
        {
            try
            {
                bool finished = false;
                if (IsSnowing)
                {
                    if (!SnowCoverAlwaysEnabled)
                    {
                        if (weatherIntensity == 1f)
                        {
                            finished = true;
                        }
                        else
                        {
                            finished = false;
                        }
                    }
                    else
                    {
                        finished = FinishedVolumeCheck(m_snowAudioSource, volume * GlobalVolume);
                    }
                }
                else
                {
                    if (!IsRaining)
                    {
                        if (!SnowCoverAlwaysEnabled)
                        {
                            if (weatherIntensity == 0f)
                            {
                                finished = true;
                            }
                            else
                            {
                                finished = false;
                            }
                        }
                        else
                        {
                            FinishedVolumeCheck(m_snowAudioSource, 0f);
                        }
                    }
                }

                if (IsRaining)
                {
                    if (weatherIntensity == 1)
                    {
                        finished = true;
                    }
                    else
                    {
                        finished = false;
                    }
                }
                else
                {
                    if (!IsSnowing)
                    {
                        if (weatherIntensity == 0)
                        {
                            finished = true;
                        }
                        else
                        {
                            finished = false;
                        }
                    }
                }

                return finished;
            }
            catch (Exception e)
            {
                Debug.LogError("Finish weather check had a issue " + e.Message + " This came from " + e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Checks if the settings are finished
        /// </summary>
        /// <param name="currentSource"></param>
        /// <param name="volumeCheck"></param>
        /// <returns></returns>
        private bool FinishedVolumeCheck(AudioSource currentSource, float volumeCheck)
        {
            try
            {
                bool finished = false;
                if (volumeCheck == 0)
                {
                    if (currentSource.volume == volumeCheck)
                    {
                        finished = true;
                    }
                }
                else if (currentSource.volume == volumeCheck)
                {
                    finished = true;
                }
                else
                {
                    finished = false;
                }
                return finished;
            }
            catch (Exception e)
            {
                Debug.LogError("Finish volume check had a issue " + e.Message + " This came from " + e.StackTrace);
                return false;
            }
        }

        #region Get

        /// <summary>
        /// Gets the scene post processing fx settings
        /// </summary>
        private void GetScenePostProcessingSettings()
        {
            try
            {
                if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.BuiltIn)
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_processProfile == null)
                    {
                        m_processProfile = GetPostProcessingProfile();
                    }

                    //Post FX
                    if (m_processProfile != null)
                    {
                        UnityEngine.Rendering.PostProcessing.Bloom bloom;
                        if (m_processProfile.TryGetSettings(out bloom))
                        {
                            CurrentScenePostFXSettings.m_fxBloomIntensity = bloom.intensity.value;
                        }

                        UnityEngine.Rendering.PostProcessing.ColorGrading colorGrading;
                        if (m_processProfile.TryGetSettings(out colorGrading))
                        {
                            CurrentScenePostFXSettings.m_fxTemperature = colorGrading.temperature.value;
                            CurrentScenePostFXSettings.m_fxTint = colorGrading.tint.value;
                            CurrentScenePostFXSettings.m_fxColorFilter = colorGrading.colorFilter.value;
                        }

                        UnityEngine.Rendering.PostProcessing.Vignette vignette;
                        if (m_processProfile.TryGetSettings(out vignette))
                        {
                            CurrentScenePostFXSettings.m_fxVignetteIntensity = vignette.intensity.value;
                        }
                    }
#endif
                }
                else if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.Universal)
                {
#if UPPipeline
                    UnityEngine.Rendering.Universal.Bloom bloom;
                    UnityEngine.Rendering.Universal.ColorAdjustments colorGrading;
                    UnityEngine.Rendering.Universal.WhiteBalance whiteBalance;
                    UnityEngine.Rendering.Universal.Vignette vignette;
                    if (m_urpFXVolumeProfile == null)
                    {
                        return;
                    }

                    if (m_urpFXVolumeProfile.TryGet(out bloom))
                    {
                        CurrentScenePostFXSettings.m_fxBloomIntensity = bloom.intensity.value;
                    }

                    if (m_urpFXVolumeProfile.TryGet(out colorGrading))
                    {
                        CurrentScenePostFXSettings.m_fxColorFilter = colorGrading.colorFilter.value;
                    }

                    if (m_urpFXVolumeProfile.TryGet(out whiteBalance))
                    {
                        CurrentScenePostFXSettings.m_fxTemperature = whiteBalance.temperature.value;
                        CurrentScenePostFXSettings.m_fxTint = whiteBalance.tint.value;
                    }

                    if (m_urpFXVolumeProfile.TryGet(out vignette))
                    {
                        CurrentScenePostFXSettings.m_fxVignetteIntensity = vignette.intensity.value;
                    }
#endif
                }
                else
                {
#if HDPipeline
                    if (m_hdrpFXVolumeProfile == null)
                    {
                        m_hdrpFXVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Post Processing", "Null");
                    }

                    if (m_hdrpFXVolumeProfile == null)
                    {
                        return;
                    }

                    UnityEngine.Rendering.HighDefinition.Bloom bloom;
                    UnityEngine.Rendering.HighDefinition.ColorAdjustments colorGrading;
                    UnityEngine.Rendering.HighDefinition.WhiteBalance whiteBalance;
                    UnityEngine.Rendering.HighDefinition.Vignette vignette;
                    if (m_hdrpFXVolumeProfile.TryGet(out bloom))
                    {
                        CurrentScenePostFXSettings.m_fxBloomIntensity = bloom.intensity.value;
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out colorGrading))
                    {
                        CurrentScenePostFXSettings.m_fxColorFilter = colorGrading.colorFilter.value;
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out whiteBalance))
                    {
                        CurrentScenePostFXSettings.m_fxTemperature = whiteBalance.temperature.value;
                        CurrentScenePostFXSettings.m_fxTint = whiteBalance.tint.value;
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out vignette))
                    {
                        CurrentScenePostFXSettings.m_fxVignetteIntensity = vignette.intensity.value;
                    }
#endif
                }
                //Wind
                CurrentScenePostFXSettings.m_windSpeed = WindSpeed;
                CurrentScenePostFXSettings.m_windTurbulence = WindTurbulence;
                CurrentScenePostFXSettings.m_windFrequency = WindFrequency;
                CurrentScenePostFXSettings.m_windMultiplier = WindMultiplier;
            }
            catch (Exception e)
            {
                Debug.LogError("Get scene post fx had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Gets the volumes from the Gaia Audio Manager
        /// </summary>
        private void GetVolume()
        {
            try
            {
                if (GaiaAudioManager == null)
                {
                    GaiaAudioManager = GaiaAudioManager.Instance;
                }

                if (GaiaAudioManager != null)
                {
                    GlobalVolume = GaiaAudioManager.m_masterVolume;
                    RainVolume = GaiaAudioManager.m_rainVolume;
                    SnowVolume = GaiaAudioManager.m_snowVolume;
                    m_thunderVolume = GaiaAudioManager.m_thunderVolume;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Get Volume had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }

        #endregion

        #region Set

        private bool ValidateSkyboxShader()
        {
            try
            {
                if (m_skyboxMaterial == null)
                {
                    return false;
                }

                string shader = m_skyboxMaterial.shader.name;
                if (shader == GaiaShaderID.m_pwSkySkyboxShader)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Debug.LogError("An issue occoured while checking the skybox shader " + e.Message + " This happened at " + e.StackTrace);
                return false;
            }
        }
        /// <summary>
        /// Sets all values when weather is active to time of day if the transition has finished
        /// </summary>
        /// <param name="weatherSettings"></param>
        private void SetAllAfterFinishedLerping(WeatherSettings weatherSettings)
        {
            try
            {
                if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
#if HDPipeline
                    #region Clouds

                    weatherSettings.m_cloudDomeBrightness.Evaluate(m_currentTime);

                    if (PW_VFX_Atmosphere.Instance != null)
                    {
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightDensity, weatherSettings.m_cloudDensity.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightThickness, weatherSettings.m_cloudThickness.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudSpeed, weatherSettings.m_cloudDensity.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudOpacity, weatherSettings.m_newCloudOpacity.Evaluate(m_currentTime));
                    }

                    #endregion

                    #region Skybox

                    if (m_hdrpVolumeProfile != null)
                    {
                        VisualEnvironment visualEnvironment;
                        if (m_hdrpVolumeProfile.TryGet(out visualEnvironment))
                        {
                            PhysicallyBasedSky physicallyBasedSky;
                            SkyType currentSky = (SkyType)visualEnvironment.skyType.value;
                            if (currentSky == SkyType.PhysicallyBased)
                            {
                                if (m_hdrpVolumeProfile.TryGet(out physicallyBasedSky))
                                {
                                    if (weatherSettings.m_skyboxTint != null)
                                    {
                                        physicallyBasedSky.groundTint.value = weatherSettings.m_skyboxTint.Evaluate(m_currentTime);
                                    }
                                    if (weatherSettings.m_skyboxExposure != null)
                                    {
                                        physicallyBasedSky.exposure.value = weatherSettings.m_skyboxExposure.Evaluate(m_currentTime);
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region Sun

                    HDAdditionalLightData lightData = null;
                    if (CheckIsNight())
                    {
                        lightData = m_moonLight.GetComponent<HDAdditionalLightData>();
                    }
                    else
                    {
                        lightData = m_sunLight.GetComponent<HDAdditionalLightData>();
                    }

                    if (lightData != null)
                    {
                        if (weatherSettings.m_sunIntensity != null)
                        {
                            lightData.SetIntensity(GaiaUtils.SetHDRPFloat(weatherSettings.m_sunIntensity.Evaluate(m_currentTime), 2.14f));
                        }
                        if (weatherSettings.m_sunColor != null)
                        {
                            lightData.SetColor(weatherSettings.m_sunColor.Evaluate(m_currentTime));
                        }
                    }

                    #endregion

                    #region Fog

                    if (m_hdrpVolumeProfile != null)
                    {
                        if (GaiaUnderwaterEffects.Instance != null)
                        {
                            if (!GaiaUnderwaterEffects.Instance.IsUnderwater)
                            {
                                UnityEngine.Rendering.HighDefinition.Fog volumetricFog;
                                if (m_hdrpVolumeProfile.TryGet(out volumetricFog))
                                {
                                    volumetricFog.enableVolumetricFog.value = true;
                                    if (weatherSettings.m_fogColor != null)
                                    {
                                        volumetricFog.color.value = weatherSettings.m_fogColor.Evaluate(m_currentTime);
                                    }
                                    if (weatherSettings.m_fogHeight != null)
                                    {
                                        volumetricFog.baseHeight.value = weatherSettings.m_fogHeight.Evaluate(m_currentTime);
                                    }
                                    if (weatherSettings.m_volumetricGlobalAnisotropy != null)
                                    {
                                        volumetricFog.anisotropy.value = weatherSettings.m_volumetricGlobalAnisotropy.Evaluate(m_currentTime);
                                    }
                                    if (weatherSettings.m_fogEndDistance != null)
                                    {
                                        volumetricFog.meanFreePath.value = weatherSettings.m_fogEndDistance.Evaluate(m_currentTime);
                                    }
                                    if (weatherSettings.m_volumetricGlobalProbeDimmer != null)
                                    {
                                        volumetricFog.globalLightProbeDimmer.value = weatherSettings.m_volumetricGlobalProbeDimmer.Evaluate(m_currentTime);
                                    }
                                    if (weatherSettings.m_volumetricDepthExtent != null)
                                    {
                                        volumetricFog.depthExtent.value = weatherSettings.m_volumetricDepthExtent.Evaluate(m_currentTime);
                                    }
                                }
                            }
                        }

                    }

                    #endregion

                    #region Post FX

                    UnityEngine.Rendering.HighDefinition.Bloom bloom;
                    UnityEngine.Rendering.HighDefinition.ColorAdjustments colorGrading;
                    UnityEngine.Rendering.HighDefinition.WhiteBalance whiteBalance;
                    UnityEngine.Rendering.HighDefinition.Vignette vignette;
                    if (m_hdrpFXVolumeProfile.TryGet(out bloom))
                    {
                        bloom.intensity.overrideState = true;
                        if (weatherSettings.m_fXBloomIntensity != null)
                        {
                            //bloom.intensity.value = weatherSettings.m_fXBloomIntensity.Evaluate(m_currentTime);
                        }
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out colorGrading))
                    {
                        colorGrading.colorFilter.overrideState = true;
                        if (weatherSettings.m_fXColorFilter != null)
                        {
                            colorGrading.colorFilter.value = weatherSettings.m_fXColorFilter.Evaluate(m_currentTime);
                        }
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out whiteBalance))
                    {
                        whiteBalance.temperature.overrideState = true;
                        if (weatherSettings.m_fXTemerature != null)
                        {
                            whiteBalance.temperature.value = weatherSettings.m_fXTemerature.Evaluate(m_currentTime);
                        }

                        whiteBalance.tint.overrideState = true;
                        if (weatherSettings.m_fXTint != null)
                        {
                            whiteBalance.tint.value = weatherSettings.m_fXTint.Evaluate(m_currentTime);
                        }
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out vignette))
                    {
                        vignette.intensity.overrideState = true;
                        if (weatherSettings.m_fXVignetteIntensity != null)
                        {
                            vignette.intensity.value = weatherSettings.m_fXVignetteIntensity.Evaluate(m_currentTime);
                        }
                    }

                    #endregion
#endif
                }
                else
                {
                    #region Clouds

                    if (PW_VFX_Atmosphere.Instance != null)
                    {
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudDomeBrightness, weatherSettings.m_cloudDomeBrightness.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightDensity, weatherSettings.m_cloudDensity.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightThickness, weatherSettings.m_cloudThickness.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudSpeed, weatherSettings.m_cloudDensity.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudOpacity, weatherSettings.m_newCloudOpacity.Evaluate(m_currentTime));
                    }

                    #endregion

                    #region Skybox

                    string shader = null;
                    if (m_skyboxMaterial != null)
                    {
                        shader = m_skyboxMaterial.shader.name;
                    }

                    if (shader == GaiaShaderID.m_pwSkySkyboxShader)
                    {
                        if (weatherSettings.m_skyboxTint != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxTint))
                            {
                                m_skyboxMaterial.SetColor(GaiaShaderID.m_unitySkyboxTint, weatherSettings.m_skyboxTint.Evaluate(m_currentTime));
                            }
                        }
                        if (weatherSettings.m_skyboxExposure != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxExposure))
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxExposure, weatherSettings.m_skyboxExposure.Evaluate(m_currentTime));
                            }
                        }

                        if (weatherSettings.m_skyboxAtmosphereThickness != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxAtmosphereThickness))
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxAtmosphereThickness, Mathf.Lerp(CurrentSettings.m_skyboxAtmosphereThickness, weatherSettings.m_skyboxAtmosphereThickness.Evaluate(m_currentTime), GetFadeTime()));
                            }
                        }

                        if (weatherSettings.m_skyboxSkyboxFogHeight != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_pwSkyFogHeight))
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_pwSkyFogHeight, Mathf.Lerp(CurrentSettings.m_skyboxFogHeight, Mathf.Clamp(weatherSettings.m_skyboxSkyboxFogHeight.Evaluate(m_currentTime), -1000f, 8000f), GetFadeTime()));
                            }
                        }

                        if (weatherSettings.m_skyboxSkyboxFogGradient != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_pwSkyFogGradient))
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_pwSkyFogGradient, Mathf.Lerp(CurrentSettings.m_skyboxFogGradient, Mathf.Clamp01(weatherSettings.m_skyboxSkyboxFogGradient.Evaluate(m_currentTime)), GetFadeTime()));
                            }
                        }

                        if (TODSunSize != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxSunSize))
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxSunSize, TODSunSize.Evaluate(m_currentTime));
                            }
                        }

                        if (TODSunSizeConvergence != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxSunSizeConvergence))
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxSunSizeConvergence, TODSunSizeConvergence.Evaluate(m_currentTime));
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Shader not supported");
                    }

                    #endregion

                    #region Sun

                    if (CheckIsNight())
                    {
                        if (weatherSettings.m_sunIntensity != null)
                        {
                            m_moonLight.intensity = weatherSettings.m_sunIntensity.Evaluate(m_currentTime);
                        }
                        if (weatherSettings.m_sunColor != null)
                        {
                            m_moonLight.color = weatherSettings.m_sunColor.Evaluate(m_currentTime);
                        }
                    }
                    else
                    {
                        if (weatherSettings.m_sunIntensity != null)
                        {
                            m_sunLight.intensity = weatherSettings.m_sunIntensity.Evaluate(m_currentTime);
                        }
                        if (weatherSettings.m_sunColor != null)
                        {
                            m_sunLight.color = weatherSettings.m_sunColor.Evaluate(m_currentTime);
                        }
                    }

                    #endregion

                    #region Fog

                    RenderSettings.fog = true;
                    if (GaiaUnderwaterEffects.Instance != null)
                    {
                        if (!GaiaUnderwaterEffects.Instance.IsUnderwater)
                        {
                            if (weatherSettings.m_fogColor != null)
                            {
                                RenderSettings.fogColor = weatherSettings.m_fogColor.Evaluate(m_currentTime);
                            }
                            if (weatherSettings.m_fogDensity != null)
                            {
                                RenderSettings.fogDensity = weatherSettings.m_fogDensity.Evaluate(m_currentTime);
                            }
                            if (weatherSettings.m_fogStartDistance != null)
                            {
                                RenderSettings.fogStartDistance = weatherSettings.m_fogStartDistance.Evaluate(m_currentTime);
                            }
                            if (weatherSettings.m_fogEndDistance != null)
                            {
                                RenderSettings.fogEndDistance = weatherSettings.m_fogEndDistance.Evaluate(m_currentTime);
                            }
                        }
                    }

                    #endregion

                    #region Ambient

                    if (weatherSettings.m_ambientIntensity != null)
                    {
                        RenderSettings.ambientIntensity = weatherSettings.m_ambientIntensity.Evaluate(m_currentTime);
                    }
                    if (weatherSettings.m_ambientSkyColor != null)
                    {
                        RenderSettings.ambientSkyColor = weatherSettings.m_ambientSkyColor.Evaluate(m_currentTime);
                    }
                    if (weatherSettings.m_ambientEquatorColor != null)
                    {
                        RenderSettings.ambientEquatorColor = weatherSettings.m_ambientEquatorColor.Evaluate(m_currentTime);
                    }
                    if (weatherSettings.m_ambientGroundColor != null)
                    {
                        RenderSettings.ambientGroundColor = weatherSettings.m_ambientGroundColor.Evaluate(m_currentTime);
                    }

                    #endregion

                    #region Post FX

                    if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.BuiltIn)
                    {
#if UNITY_POST_PROCESSING_STACK_V2
                        if (m_processProfile != null)
                        {
                            UnityEngine.Rendering.PostProcessing.Bloom bloom;
                            if (m_processProfile.TryGetSettings(out bloom))
                            {
                                bloom.intensity.value = weatherSettings.m_fXBloomIntensity.Evaluate(m_currentTime);
                            }

                            UnityEngine.Rendering.PostProcessing.ColorGrading colorGrading;
                            if (m_processProfile.TryGetSettings(out colorGrading))
                            {
                                colorGrading.temperature.value = weatherSettings.m_fXTemerature.Evaluate(m_currentTime);
                                colorGrading.tint.value = weatherSettings.m_fXTint.Evaluate(m_currentTime);
                                colorGrading.colorFilter.value = weatherSettings.m_fXColorFilter.Evaluate(m_currentTime);
                            }

                            UnityEngine.Rendering.PostProcessing.Vignette vignette;
                            if (m_processProfile.TryGetSettings(out vignette))
                            {
                                vignette.intensity.value = weatherSettings.m_fXVignetteIntensity.Evaluate(m_currentTime);
                            }
                        }
#endif
                    }
                    else
                    {
#if UPPipeline
                        if (m_urpFXVolumeProfile == null)
                        {
                            m_urpFXVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Post Processing", "Null");
                        }

                        if (m_urpFXVolumeProfile == null)
                        {
                            Debug.LogError("Unable to find the post processing volume profile... Exiting...");
                            return;
                        }

                        UnityEngine.Rendering.Universal.Bloom bloom;
                        UnityEngine.Rendering.Universal.ColorAdjustments colorGrading;
                        UnityEngine.Rendering.Universal.WhiteBalance whiteBalance;
                        UnityEngine.Rendering.Universal.Vignette vignette;
                        if (m_urpFXVolumeProfile.TryGet(out bloom))
                        {
                            bloom.intensity.overrideState = true;
                            if (weatherSettings.m_fXBloomIntensity != null)
                            {
                                bloom.intensity.value = weatherSettings.m_fXBloomIntensity.Evaluate(m_currentTime);
                            }
                        }
                        if (m_urpFXVolumeProfile.TryGet(out colorGrading))
                        {
                            colorGrading.colorFilter.overrideState = true;
                            if (weatherSettings.m_fXColorFilter != null)
                            {
                                colorGrading.colorFilter.value = weatherSettings.m_fXColorFilter.Evaluate(m_currentTime);
                            }
                        }
                        if (m_urpFXVolumeProfile.TryGet(out whiteBalance))
                        {
                            whiteBalance.temperature.overrideState = true;
                            if (weatherSettings.m_fXTemerature != null)
                            {
                                whiteBalance.temperature.value = weatherSettings.m_fXTemerature.Evaluate(m_currentTime);
                            }

                            whiteBalance.tint.overrideState = true;
                            if (weatherSettings.m_fXTint != null)
                            {
                                whiteBalance.tint.value = weatherSettings.m_fXTint.Evaluate(m_currentTime);
                            }
                        }
                        if (m_urpFXVolumeProfile.TryGet(out vignette))
                        {
                            vignette.intensity.overrideState = true;
                            if (weatherSettings.m_fXVignetteIntensity != null)
                            {
                                vignette.intensity.value = weatherSettings.m_fXVignetteIntensity.Evaluate(m_currentTime);
                            }
                        }
#endif
                    }

                    #endregion
                }

                if (IsSnowing)
                {
                    if (SnowIntensity != 1f)
                    {
                        SnowIntensity = 1f;
                    }
                }
                else
                {
                    if (SnowIntensity > 0f)
                    {
                        SnowIntensity = 0f;
                    }
                }

                if (IsRaining)
                {
                    if (RainIntensity != 1f)
                    {
                        RainIntensity = 1f;
                    }
                }
                else
                {
                    if (RainIntensity > 0f)
                    {
                        RainIntensity = 0f;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Set all settings had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the weather particles
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="renderer"></param>
        /// <param name="particleSystem"></param>
        private void SetRainVFX(bool enabling, MeshRenderer renderer, ParticleSystem particleSystem)
        {
            try
            {
                if (renderer == null)
                {
                    Debug.LogError("Missing mesh renderer to apply weather rain/snow fx too.");
                    return;
                }

                if (particleSystem == null)
                {
                    Debug.LogError("Missing particle system to apply weather rain/snow fx too.");
                    return;
                }

                renderer.enabled = true;
                if (enabling)
                {
                    if (renderer.enabled)
                    {
                        if (GaiaUtils.ValidateShaderProperty(renderer.sharedMaterial, GaiaShaderID.m_rainIntensity))
                        {
                            renderer.sharedMaterial.SetFloat(GaiaShaderID.m_rainIntensity, Mathf.Lerp(m_currentRainParticleAlpha, 0.95f, GetFadeTime()));
                            if (renderer.sharedMaterial.GetFloat(GaiaShaderID.m_rainIntensity) > 0.4f)
                            {
                                particleSystem.Play();
                            }
                        }
                    }
                }
                else
                {
                    if (renderer.enabled)
                    {
                        if (GaiaUtils.ValidateShaderProperty(renderer.sharedMaterial, GaiaShaderID.m_rainIntensity))
                        {
                            renderer.sharedMaterial.SetFloat(GaiaShaderID.m_rainIntensity, Mathf.Lerp(m_currentRainParticleAlpha, 0f, GetFadeTime()));
                            if (renderer.sharedMaterial.GetFloat(GaiaShaderID.m_rainIntensity) < 0.15f)
                            {
                                particleSystem.Stop();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Set rain vfx had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the weather particles
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="renderer"></param>
        /// <param name="particleSystem"></param>
        private void SetSnowVFX(bool enabling, ParticleSystem.EmissionModule particleSystem1, ParticleSystem.EmissionModule particleSystemEmission1, bool instantStartStop = false)
        {
            try
            {
                if (enabling)
                {
                    if (instantStartStop)
                    {
                        if (particleSystem1.rateOverTime.constant < 512)
                        {
                            ParticleSystem.MinMaxCurve rateOverTime1 = particleSystem1.rateOverTime;
                            rateOverTime1.mode = ParticleSystemCurveMode.Constant;
                            rateOverTime1.constant = 512f;
                            particleSystem1.rateOverTime = rateOverTime1;
                        }
                        if (!m_snowParticles.isPlaying)
                        {
                            m_snowParticles.Play();
                        }

                        if (particleSystemEmission1.rateOverTime.constant < 256)
                        {
                            ParticleSystem.MinMaxCurve rateOverTime2 = particleSystemEmission1.rateOverTime;
                            rateOverTime2.mode = ParticleSystemCurveMode.Constant;
                            rateOverTime2.constant = 256f;
                            particleSystemEmission1.rateOverTime = rateOverTime2;
                        }
                        if (!m_snowVFX.isPlaying)
                        {
                            m_snowVFX.Play();
                        }
                    }
                    else
                    {
                        if (particleSystem1.rateOverTime.constant < 512)
                        {
                            ParticleSystem.MinMaxCurve rateOverTime1 = particleSystem1.rateOverTime;
                            rateOverTime1.mode = ParticleSystemCurveMode.Constant;
                            rateOverTime1.constant += 1f;
                            particleSystem1.rateOverTime = rateOverTime1;
                        }
                        if (!m_snowParticles.isPlaying)
                        {
                            m_snowParticles.Play();
                        }

                        if (particleSystemEmission1.rateOverTime.constant < 256)
                        {
                            ParticleSystem.MinMaxCurve rateOverTime2 = particleSystemEmission1.rateOverTime;
                            rateOverTime2.mode = ParticleSystemCurveMode.Constant;
                            rateOverTime2.constant += 0.5f;
                            particleSystemEmission1.rateOverTime = rateOverTime2;
                        }
                        if (!m_snowVFX.isPlaying)
                        {
                            m_snowVFX.Play();
                        }
                    }
                }
                else
                {
                    if (instantStartStop)
                    {
                        if (particleSystem1.rateOverTime.constant > 0)
                        {
                            ParticleSystem.MinMaxCurve rateOverTime1 = particleSystem1.rateOverTime;
                            rateOverTime1.mode = ParticleSystemCurveMode.Constant;
                            rateOverTime1.constant = 0f;
                            particleSystem1.rateOverTime = rateOverTime1;
                        }
                        if (m_snowParticles.isPlaying)
                        {
                            m_snowParticles.Stop();
                        }

                        if (particleSystemEmission1.rateOverTime.constant > 0)
                        {
                            ParticleSystem.MinMaxCurve rateOverTime2 = particleSystemEmission1.rateOverTime;
                            rateOverTime2.mode = ParticleSystemCurveMode.Constant;
                            rateOverTime2.constant = 0f;
                            particleSystemEmission1.rateOverTime = rateOverTime2;
                        }
                        if (m_snowVFX.isPlaying)
                        {
                            m_snowVFX.Stop();
                        }
                    }
                    else
                    {
                        if (particleSystem1.rateOverTime.constant > 0)
                        {
                            ParticleSystem.MinMaxCurve rateOverTime1 = particleSystem1.rateOverTime;
                            rateOverTime1.mode = ParticleSystemCurveMode.Constant;
                            rateOverTime1.constant -= 2.5f;
                            particleSystem1.rateOverTime = rateOverTime1;
                        }
                        else
                        {
                            if (m_snowParticles.isPlaying)
                            {
                                m_snowParticles.Stop();
                            }
                        }

                        if (particleSystemEmission1.rateOverTime.constant > 0)
                        {
                            ParticleSystem.MinMaxCurve rateOverTime2 = particleSystemEmission1.rateOverTime;
                            rateOverTime2.mode = ParticleSystemCurveMode.Constant;
                            rateOverTime2.constant -= 1f;
                            particleSystemEmission1.rateOverTime = rateOverTime2;
                        }
                        else
                        {
                            if (m_snowVFX.isPlaying)
                            {
                                m_snowVFX.Stop();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Set snow vfx had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the particles to the time of day
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="weatherSettings"></param>
        private void SetParticlesToWeather(MeshRenderer renderer, ParticleSystem particleSystem, WeatherSettings weatherSettings)
        {
            try
            {
                if (GaiaUtils.ValidateShaderProperty(renderer.sharedMaterial, GaiaShaderID.m_weatherMainColor))
                {
                    Color color = renderer.sharedMaterial.GetColor(GaiaShaderID.m_weatherMainColor);
                    renderer.sharedMaterial.SetColor(GaiaShaderID.m_weatherMainColor, new Color(color.r, color.g, color.b, Mathf.Clamp01(weatherSettings.m_weatherParticleAlpha.Evaluate(m_currentTime))));
                }

                //Take the angle of the wind direction to make the weather effect "go with the wind" a little.
                renderer.transform.rotation = Quaternion.Euler(WindZone.transform.rotation.eulerAngles.x * -0.75f, WindZone.transform.rotation.eulerAngles.y, 0f);



                if (particleSystem == null)
                {
                    return;
                }
                Color color1 = particleSystem.main.startColor.color;
                if (color1.r == 0)
                {
                    color1 = Color.white;
                }

                ParticleSystem.MainModule mainModule1 = particleSystem.main;
                mainModule1.startColor = new ParticleSystem.MinMaxGradient(new Color(color1.r, color1.g, color1.b, weatherSettings.m_weatherParticleAlpha.Evaluate(m_currentTime)));
            }
            catch (Exception e)
            {
                Debug.LogError("Set particles to weather had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the snow particles to weather conditions/settings
        /// </summary>
        /// <param name="particleSystem1"></param>
        /// <param name="particleSystem2"></param>
        /// <param name="weatherSettings"></param>
        private void SetParticlesToSnow(ParticleSystem particleSystem1, ParticleSystem particleSystem2, WeatherSettings weatherSettings)
        {
            try
            {
                if (particleSystem1 == null)
                {
                    return;
                }

                Color color1 = particleSystem1.main.startColor.color;
                if (color1.r == 0)
                {
                    color1 = Color.white;
                }
                Color color2 = particleSystem2.main.startColor.color;
                if (color2.r == 0)
                {
                    color2 = Color.white;
                }

                ParticleSystem.MainModule mainModule1 = particleSystem1.main;
                mainModule1.startColor = new ParticleSystem.MinMaxGradient(new Color(color1.r, color1.g, color1.b, weatherSettings.m_weatherParticleAlpha.Evaluate(m_currentTime)));

                if (particleSystem2 == null)
                {
                    return;
                }

                ParticleSystem.MainModule mainModule2 = particleSystem2.main;
                mainModule2.startColor = new ParticleSystem.MinMaxGradient(new Color(color2.r, color2.g, color2.b, weatherSettings.m_weatherParticleAlpha.Evaluate(m_currentTime)));
            }
            catch (Exception e)
            {
                Debug.LogError("Set particles to snow had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Updates the seasons
        /// </summary>
        private void SetSeasons()
        {
            Season = Mathf.Lerp(Season, 3.9999f, Time.deltaTime / m_seasonTransitionDuration);
            if (Season > 3.96)
            {
                Season = 0f;
            }
        }
        /// <summary>
        /// Sets the cloud settings
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="weatherSettings"></param>
        private void SetCloudsToWeather(bool enabling, WeatherSettings weatherSettings)
        {
            try
            {
                if (enabling)
                {
                    if (PW_VFX_Atmosphere.Instance != null)
                    {
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudDomeBrightness, Mathf.Lerp(CurrentSettings.m_cloudDomeBrightness, weatherSettings.m_cloudDomeBrightness.Evaluate(m_currentTime), GetFadeTime()));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightDensity, Mathf.Lerp(CurrentSettings.m_cloudDensity, weatherSettings.m_cloudDensity.Evaluate(m_currentTime), GetFadeTime()));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightThickness, Mathf.Lerp(CurrentSettings.m_cloudThickness, weatherSettings.m_cloudThickness.Evaluate(m_currentTime), GetFadeTime()));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudSpeed, Mathf.Lerp(CurrentSettings.m_cloudSpeed, weatherSettings.m_cloudDensity.Evaluate(m_currentTime), GetFadeTime()));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudOpacity, Mathf.Lerp(CurrentSettings.m_cloudOpacity, weatherSettings.m_newCloudOpacity.Evaluate(m_currentTime), GetFadeTime()));
                    }
                }
                else
                {
                    if (PW_VFX_Clouds.Instance != null)
                    {
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudDomeBrightness, Mathf.Lerp(CurrentSettings.m_cloudDomeBrightness, PW_VFX_Clouds.Instance.PW_SkyDome_Brightness.Evaluate(m_currentTime) * 1.35f, GetFadeTime()));
                    }

                    if (PW_VFX_Atmosphere.Instance != null)
                    {
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightDensity, Mathf.Lerp(CurrentSettings.m_cloudDensity, PW_VFX_Atmosphere.Instance.CloudsHightLevelDensity.Evaluate(m_currentTime), GetFadeTime()));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightThickness, Mathf.Lerp(CurrentSettings.m_cloudThickness, PW_VFX_Atmosphere.Instance.CloudsHightLevelThickness.Evaluate(m_currentTime), GetFadeTime()));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudSpeed, Mathf.Lerp(CurrentSettings.m_cloudSpeed, PW_VFX_Atmosphere.Instance.CloudsHightLevelSpeed.Evaluate(m_currentTime), GetFadeTime()));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudOpacity, Mathf.Lerp(CurrentSettings.m_cloudOpacity, PW_VFX_Atmosphere.Instance.CloudOpacity.Evaluate(m_currentTime), GetFadeTime()));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Set clouds to weather had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the skybox shader in RenderSettings.skybox
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="weatherSettings"></param>
        private void SetSkyboxToWeather(bool enabling, WeatherSettings weatherSettings)
        {
            try
            {
                if (enabling)
                {
                    if (SkyboxShaderValid)
                    {
                        if (weatherSettings.m_skyboxTint != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxTint))
                            {
                                m_skyboxMaterial.SetColor(GaiaShaderID.m_unitySkyboxTint, Color.Lerp(CurrentSettings.m_skyboxTint, weatherSettings.m_skyboxTint.Evaluate(m_currentTime), GetFadeTime()));
                            }
                        }
                        if (weatherSettings.m_skyboxExposure != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxExposure))
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxExposure, Mathf.Lerp(CurrentSettings.m_skyboxExposure, weatherSettings.m_skyboxExposure.Evaluate(m_currentTime), GetFadeTime()));
                            }
                        }
                        if (weatherSettings.m_skyboxAtmosphereThickness != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxAtmosphereThickness))
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxAtmosphereThickness, Mathf.Lerp(CurrentSettings.m_skyboxAtmosphereThickness, weatherSettings.m_skyboxAtmosphereThickness.Evaluate(m_currentTime), GetFadeTime()));
                            }
                        }
                        if (weatherSettings.m_skyboxSkyboxFogHeight != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_pwSkyFogHeight))
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_pwSkyFogHeight, Mathf.Lerp(CurrentSettings.m_skyboxFogHeight, Mathf.Clamp(weatherSettings.m_skyboxSkyboxFogHeight.Evaluate(m_currentTime), -1000f, 8000f), GetFadeTime()));
                            }
                        }

                        if (weatherSettings.m_skyboxSkyboxFogGradient != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_pwSkyFogGradient))
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_pwSkyFogGradient, Mathf.Lerp(CurrentSettings.m_skyboxFogGradient, Mathf.Clamp01(weatherSettings.m_skyboxSkyboxFogGradient.Evaluate(m_currentTime)), GetFadeTime()));
                            }
                        }

                        if (TODSunSize != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxSunSize))
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxSunSize, TODSunSize.Evaluate(m_currentTime));
                            }
                        }

                        if (TODSunSizeConvergence != null)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxSunSizeConvergence))
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxSunSizeConvergence, TODSunSizeConvergence.Evaluate(m_currentTime));
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Shader not supported, please use Skybox/Procedural shader");
                    }
                }
                else
                {
                    if (SkyboxShaderValid)
                    {
                        if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxTint))
                        {
                            m_skyboxMaterial.SetColor(GaiaShaderID.m_unitySkyboxTint, Color.Lerp(CurrentSettings.m_skyboxTint, PW_VFX_Atmosphere.Instance.SkyboxTint.Evaluate(m_currentTime), GetFadeTime()));
                        }
                        if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxExposure))
                        {
                            m_skyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxExposure, Mathf.Lerp(CurrentSettings.m_skyboxExposure, PW_VFX_Atmosphere.Instance.SkyboxExposure.Evaluate(m_currentTime), GetFadeTime()));
                        }
                        if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxAtmosphereThickness))
                        {
                            m_skyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxAtmosphereThickness, Mathf.Lerp(CurrentSettings.m_skyboxAtmosphereThickness, PW_VFX_Atmosphere.Instance.AtmosphereThickness.Evaluate(m_currentTime), GetFadeTime()));
                        }
                        if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_pwSkyFogHeight))
                        {
                            m_skyboxMaterial.SetFloat(GaiaShaderID.m_pwSkyFogHeight, Mathf.Lerp(CurrentSettings.m_skyboxFogHeight, Mathf.Clamp(PW_VFX_Atmosphere.Instance.TODSkyboxFogHeight.Evaluate(m_currentTime), -1000f, 8000f), GetFadeTime()));
                        }
                        if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_pwSkyFogGradient))
                        {
                            m_skyboxMaterial.SetFloat(GaiaShaderID.m_pwSkyFogGradient, Mathf.Lerp(CurrentSettings.m_skyboxFogGradient, Mathf.Clamp01(PW_VFX_Atmosphere.Instance.TODSkyboxFogGradient.Evaluate(m_currentTime)), GetFadeTime()));
                        }
                        if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxSunSize))
                        {
                            if (TODSunSize != null)
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxSunSize, TODSunSize.Evaluate(m_currentTime));
                            }
                        }
                        if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxSunSizeConvergence))
                        {
                            if (TODSunSizeConvergence != null)
                            {
                                m_skyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxSunSizeConvergence, TODSunSizeConvergence.Evaluate(m_currentTime));
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Shader not supported");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Set skybox to weather had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the snow height
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="snowHeight"></param>
        private void SetSnowHeightToWeather(bool enabling, float snowHeight)
        {
            if (enabling)
            {
                m_savedSnowingHeight = Mathf.Lerp(CurrentSettings.m_snowHeight, snowHeight, GetFadeTime());
            }
            else
            {
                m_savedSnowingHeight = Mathf.Lerp(CurrentSettings.m_snowHeight, SnowHeight, GetFadeTime());
            }
        }
        /// <summary>
        /// Sets the skybox shader in HDRP
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="weatherSettings"></param>
        private void SetHDRPSkyboxToWeather(bool enabling, WeatherSettings weatherSettings)
        {
            try
            {
#if HDPipeline
                if (m_hdrpVolumeProfile == null)
                {
                    m_hdrpVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Environment", "Processing");
                }

                if (m_hdrpVolumeProfile == null)
                {
                    Debug.LogError("Unable to find the sky volume profile... Exiting...");
                    return;
                }

                if (m_hdrpVolumeProfile != null)
                {
                    VisualEnvironment visualEnvironment;
                    if (m_hdrpVolumeProfile.TryGet(out visualEnvironment))
                    {
                        PhysicallyBasedSky physicallyBasedSky;
                        SkyType currentSky = (SkyType)visualEnvironment.skyType.value;
                        if (enabling)
                        {
                            if (currentSky == SkyType.PhysicallyBased)
                            {
                                if (m_hdrpVolumeProfile.TryGet(out physicallyBasedSky))
                                {
                                    if (weatherSettings.m_skyboxTint != null)
                                    {
                                        physicallyBasedSky.groundTint.value = Color.Lerp(CurrentSettings.m_skyboxTint, weatherSettings.m_skyboxTint.Evaluate(m_currentTime), GetFadeTime());
                                    }
                                    if (weatherSettings.m_skyboxExposure != null)
                                    {
                                        physicallyBasedSky.exposure.value = Mathf.Lerp(CurrentSettings.m_skyboxExposure, weatherSettings.m_skyboxExposure.Evaluate(m_currentTime), GetFadeTime());
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (currentSky == SkyType.PhysicallyBased)
                            {
                                if (m_hdrpVolumeProfile.TryGet(out physicallyBasedSky))
                                {
                                    physicallyBasedSky.groundTint.value = Color.Lerp(CurrentSettings.m_skyboxTint, PW_VFX_Atmosphere.Instance.SkyboxTint.Evaluate(m_currentTime), GetFadeTime());
                                    physicallyBasedSky.exposure.value = Mathf.Lerp(CurrentSettings.m_skyboxExposure, PW_VFX_Atmosphere.Instance.SkyboxExposure.Evaluate(m_currentTime), GetFadeTime());
                                }
                            }
                        }
                    }
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Set HDRP Skybox to weather had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the sun to the weather
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="weatherActive"></param>
        /// <param name="weatherSettings"></param>
        private void SetSunToWeather(bool enabling, bool weatherActive, WeatherSettings weatherSettings)
        {
            try
            {
                if (enabling)
                {
                    if (CheckIsNight())
                    {
                        if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.BuiltIn)
                        {
                            if (weatherSettings.m_sunIntensity != null)
                            {
                                m_moonLight.intensity = Mathf.Lerp(CurrentSettings.m_sunIntensity, weatherSettings.m_sunIntensity.Evaluate(m_currentTime), GetFadeTime());
                            }
                            if (weatherSettings.m_sunColor != null)
                            {
                                m_moonLight.color = Color.Lerp(CurrentSettings.m_sunColor, weatherSettings.m_sunColor.Evaluate(m_currentTime), GetFadeTime());
                            }
                        }
                        else
                        {
                            if (weatherSettings.m_sunIntensity != null)
                            {
                                m_moonLight.intensity = Mathf.Lerp(CurrentSettings.m_sunIntensity, weatherSettings.m_sunIntensity.Evaluate(m_currentTime), GetFadeTime());
                            }
                            if (weatherSettings.m_sunColor != null)
                            {
                                m_moonLight.color = Color.Lerp(CurrentSettings.m_sunColor, weatherSettings.m_sunColor.Evaluate(m_currentTime), GetFadeTime());
                            }
                        }
                    }
                    else
                    {
                        if (weatherSettings.m_sunIntensity != null)
                        {
                            m_sunLight.intensity = Mathf.Lerp(CurrentSettings.m_sunIntensity, weatherSettings.m_sunIntensity.Evaluate(m_currentTime), GetFadeTime());
                        }
                        if (weatherSettings.m_sunColor != null)
                        {
                            m_sunLight.color = Color.Lerp(CurrentSettings.m_sunColor, weatherSettings.m_sunColor.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }
                }
                else
                {
                    if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.BuiltIn)
                    {
                        if (CheckIsNight())
                        {
                            m_moonLight.intensity = Mathf.Lerp(CurrentSettings.m_sunIntensity, PW_VFX_Atmosphere.Instance.SunBrightnessIntensity.Evaluate(m_currentTime), GetFadeTime());
                            m_moonLight.color = Color.Lerp(CurrentSettings.m_sunColor, PW_VFX_Atmosphere.Instance.TimeOfDaySunColor.Evaluate(m_currentTime), GetFadeTime());
                        }
                        else
                        {
                            m_sunLight.intensity = Mathf.Lerp(CurrentSettings.m_sunIntensity, PW_VFX_Atmosphere.Instance.SunBrightnessIntensity.Evaluate(m_currentTime), GetFadeTime());
                            m_sunLight.color = Color.Lerp(CurrentSettings.m_sunColor, PW_VFX_Atmosphere.Instance.TimeOfDaySunColor.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }
                    else
                    {
                        if (CheckIsNight())
                        {
                            m_moonLight.intensity = Mathf.Lerp(CurrentSettings.m_sunIntensity, PW_VFX_Atmosphere.Instance.SunBrightnessIntensity.Evaluate(m_currentTime), GetFadeTime());
                            m_moonLight.color = Color.Lerp(CurrentSettings.m_sunColor, PW_VFX_Atmosphere.Instance.TimeOfDaySunColor.Evaluate(m_currentTime), GetFadeTime());
                        }
                        else
                        {
                            m_sunLight.intensity = Mathf.Lerp(CurrentSettings.m_sunIntensity, PW_VFX_Atmosphere.Instance.SunBrightnessIntensity.Evaluate(m_currentTime), GetFadeTime());
                            m_sunLight.color = Color.Lerp(CurrentSettings.m_sunColor, PW_VFX_Atmosphere.Instance.TimeOfDaySunColor.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Set sun to weather had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the sun to the weather
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="intensity"></param>
        /// <param name="color"></param>
        /// <param name="oldIntensity"></param>
        /// <param name="oldColor"></param>
        private void SetHDRPSunToWeather(bool enabling, WeatherSettings weatherSettings)
        {
            try
            {
#if HDPipeline
                HDAdditionalLightData lightData = null;
                if (CheckIsNight())
                {
                    lightData = m_moonLight.GetComponent<HDAdditionalLightData>();
                }
                else
                {
                    lightData = m_sunLight.GetComponent<HDAdditionalLightData>();
                }

                if (lightData != null)
                {
                    if (!m_updateHDRPShadows)
                    {
                        lightData.EnableShadows(true);
                        lightData.SetShadowResolution(1024);
                        lightData.useContactShadow.useOverride = true;
                        m_updateHDRPShadows = true;
                    }

                    if (enabling)
                    {
                        if (weatherSettings.m_sunIntensity != null)
                        {
                            lightData.SetIntensity(Mathf.Lerp(CurrentSettings.m_sunIntensity, GaiaUtils.SetHDRPFloat(weatherSettings.m_sunIntensity.Evaluate(m_currentTime), 2.14f), GetFadeTime()));
                        }
                        if (weatherSettings.m_sunColor != null)
                        {
                            lightData.SetColor(Color.Lerp(CurrentSettings.m_sunColor, weatherSettings.m_sunColor.Evaluate(m_currentTime), GetFadeTime()));
                        }
                    }
                    else
                    {
                        lightData.SetIntensity(Mathf.Lerp(CurrentSettings.m_sunIntensity, GaiaUtils.SetHDRPFloat(PW_VFX_Atmosphere.Instance.SunBrightnessIntensity.Evaluate(m_currentTime), 2.14f), GetFadeTime()));
                        lightData.SetColor(Color.Lerp(CurrentSettings.m_sunColor, PW_VFX_Atmosphere.Instance.TimeOfDaySunColor.Evaluate(m_currentTime), GetFadeTime()));
                    }
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Set HDRP Sun to weather had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the wind to the weather
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="windType"></param>
        /// <param name="oldWindType"></param>
        private void SetWindToWeather(bool enabling, WeatherSettings weatherSettings)
        {
            try
            {
                float stepAmount = 0.005f;
                if (enabling)
                {
                    m_newWindSpeed = Mathf.Lerp(CurrentSettings.m_windSpeed, weatherSettings.m_windSpeed, GetFadeTime());
                    if (Mathf.Abs(m_newWindSpeed - WindSpeed) > stepAmount)
                    {
                        WindSpeed = m_newWindSpeed;
                    }

                    m_newWindTurbulence = Mathf.Lerp(CurrentSettings.m_windTurbulence, weatherSettings.m_windTurbulence, GetFadeTime());
                    if (Mathf.Abs(m_newWindTurbulence - WindTurbulence) > stepAmount)
                    {
                        WindTurbulence = m_newWindTurbulence;
                    }

                    m_newWindFrequency = Mathf.Lerp(CurrentSettings.m_windFrequency, weatherSettings.m_windFrequency, GetFadeTime());
                    if (Mathf.Abs(m_newWindFrequency - WindFrequency) > stepAmount)
                    {
                        WindFrequency = m_newWindFrequency;
                    }

                    m_newWindMultiplier = Mathf.Lerp(CurrentSettings.m_windMultiplier, weatherSettings.m_windMultiplier, GetFadeTime());
                    if (Mathf.Abs(m_newWindMultiplier - WindMultiplier) > stepAmount)
                    {
                        WindMultiplier = m_newWindMultiplier;
                    }
                }
                else
                {
                    WindSpeed = Mathf.Lerp(CurrentSettings.m_windSpeed, CurrentScenePostFXSettings.m_windSpeed, GetFadeTime());
                    WindTurbulence = Mathf.Lerp(CurrentSettings.m_windTurbulence, CurrentScenePostFXSettings.m_windTurbulence, GetFadeTime());
                    WindFrequency = Mathf.Lerp(CurrentSettings.m_windFrequency, CurrentScenePostFXSettings.m_windFrequency, GetFadeTime());
                    WindMultiplier = Mathf.Lerp(CurrentSettings.m_windFrequency, CurrentScenePostFXSettings.m_windMultiplier, GetFadeTime());
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Set wind to weather had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the post fx to the weather
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="weatherSettings"></param>
        private void SetFXToWeather(bool enabling, WeatherSettings weatherSettings)
        {
            try
            {
                if (!Application.isPlaying)
                {
                    return;
                }

#if UNITY_POST_PROCESSING_STACK_V2
                if (m_processProfile == null)
                {
                    m_processProfile = GetPostProcessingProfile();
                }

                UnityEngine.Rendering.PostProcessing.Bloom bloom;
                UnityEngine.Rendering.PostProcessing.ColorGrading colorGrading;
                UnityEngine.Rendering.PostProcessing.Vignette vignette;
                if (enabling)
                {
                    if (m_processProfile.TryGetSettings(out bloom))
                    {
                        bloom.intensity.overrideState = true;
                        if (weatherSettings.m_fXBloomIntensity != null)
                        {
                            bloom.intensity.value = Mathf.Lerp(CurrentSettings.m_fxBloomIntensity, weatherSettings.m_fXBloomIntensity.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }

                    if (m_processProfile.TryGetSettings(out colorGrading))
                    {
                        colorGrading.temperature.overrideState = true;
                        if (weatherSettings.m_fXTemerature != null)
                        {
                            colorGrading.temperature.value = Mathf.Lerp(CurrentSettings.m_fxTemperature, weatherSettings.m_fXTemerature.Evaluate(m_currentTime), GetFadeTime());
                        }

                        colorGrading.tint.overrideState = true;
                        if (weatherSettings.m_fXTint != null)
                        {
                            colorGrading.tint.value = Mathf.Lerp(CurrentSettings.m_fxTint, weatherSettings.m_fXTint.Evaluate(m_currentTime), GetFadeTime());
                        }

                        colorGrading.colorFilter.overrideState = true;
                        if (weatherSettings.m_fXColorFilter != null)
                        {
                            colorGrading.colorFilter.value = Color.Lerp(CurrentSettings.m_fxColorFilter, weatherSettings.m_fXColorFilter.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }

                    if (m_processProfile.TryGetSettings(out vignette))
                    {
                        vignette.intensity.overrideState = true;
                        if (weatherSettings.m_fXVignetteIntensity != null)
                        {
                            vignette.intensity.value = Mathf.Lerp(CurrentSettings.m_fxVignetteIntensity, weatherSettings.m_fXVignetteIntensity.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }
                }
                else
                {
                    if (m_processProfile.TryGetSettings(out bloom))
                    {
                        bloom.intensity.overrideState = true;
                        bloom.intensity.value = Mathf.Lerp(CurrentSettings.m_fxBloomIntensity, CurrentScenePostFXSettings.m_fxBloomIntensity, GetFadeTime());
                    }

                    if (m_processProfile.TryGetSettings(out colorGrading))
                    {
                        colorGrading.temperature.overrideState = true;
                        colorGrading.temperature.value = Mathf.Lerp(CurrentSettings.m_fxTemperature, CurrentScenePostFXSettings.m_fxTemperature, GetFadeTime());

                        colorGrading.tint.overrideState = true;
                        colorGrading.tint.value = Mathf.Lerp(CurrentSettings.m_fxTint, CurrentScenePostFXSettings.m_fxTint, GetFadeTime());

                        colorGrading.colorFilter.overrideState = true;
                        colorGrading.colorFilter.value = Color.Lerp(CurrentSettings.m_fxColorFilter, CurrentScenePostFXSettings.m_fxColorFilter, GetFadeTime());
                    }

                    if (m_processProfile.TryGetSettings(out vignette))
                    {
                        vignette.intensity.value = Mathf.Lerp(CurrentSettings.m_fxVignetteIntensity, CurrentScenePostFXSettings.m_fxVignetteIntensity, GetFadeTime());
                    }
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Set post fx to weather had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the post fx to the weather
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="weatherSettings"></param>
        private void SetHDRPFXToWeather(bool enabling, WeatherSettings weatherSettings)
        {
            try
            {
                if (!Application.isPlaying)
                {
                    return;
                }

#if HDPipeline
                if (m_hdrpFXVolumeProfile == null)
                {
                    m_hdrpFXVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Post Processing", "Null");
                }

                if (m_hdrpFXVolumeProfile == null)
                {
                    Debug.LogError("Unable to find the post processing volume profile... Exiting...");
                    return;
                }

                UnityEngine.Rendering.HighDefinition.Bloom bloom;
                UnityEngine.Rendering.HighDefinition.ColorAdjustments colorGrading;
                UnityEngine.Rendering.HighDefinition.WhiteBalance whiteBalance;
                UnityEngine.Rendering.HighDefinition.Vignette vignette;
                if (enabling)
                {
                    if (m_hdrpFXVolumeProfile.TryGet(out bloom))
                    {
                        bloom.intensity.overrideState = true;
                        if (weatherSettings.m_fXBloomIntensity != null)
                        {
                            //bloom.intensity.value = Mathf.Lerp(CurrentSettings.m_fxBloomIntensity, weatherSettings.m_fXBloomIntensity.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out colorGrading))
                    {
                        colorGrading.colorFilter.overrideState = true;
                        if (weatherSettings.m_fXColorFilter != null)
                        {
                            colorGrading.colorFilter.value = Color.Lerp(CurrentSettings.m_fxColorFilter, weatherSettings.m_fXColorFilter.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out whiteBalance))
                    {
                        whiteBalance.temperature.overrideState = true;
                        if (weatherSettings.m_fXTemerature != null)
                        {
                            whiteBalance.temperature.value = Mathf.Lerp(CurrentSettings.m_fxTemperature, weatherSettings.m_fXTemerature.Evaluate(m_currentTime), GetFadeTime());
                        }

                        whiteBalance.tint.overrideState = true;
                        if (weatherSettings.m_fXTint != null)
                        {
                            whiteBalance.tint.value = Mathf.Lerp(CurrentSettings.m_fxTint, weatherSettings.m_fXTint.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out vignette))
                    {
                        vignette.intensity.overrideState = true;
                        if (weatherSettings.m_fXVignetteIntensity != null)
                        {
                            vignette.intensity.value = Mathf.Lerp(CurrentSettings.m_fxVignetteIntensity, weatherSettings.m_fXVignetteIntensity.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }
                }
                else
                {
                    if (m_hdrpFXVolumeProfile.TryGet(out bloom))
                    {
                        bloom.intensity.overrideState = true;
                        bloom.intensity.value = Mathf.Lerp(CurrentSettings.m_fxBloomIntensity, CurrentScenePostFXSettings.m_fxBloomIntensity, GetFadeTime());
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out colorGrading))
                    {
                        colorGrading.colorFilter.overrideState = true;
                        colorGrading.colorFilter.value = Color.Lerp(CurrentSettings.m_fxColorFilter, CurrentScenePostFXSettings.m_fxColorFilter, GetFadeTime());
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out whiteBalance))
                    {
                        whiteBalance.temperature.overrideState = true;
                        whiteBalance.temperature.value = Mathf.Lerp(CurrentSettings.m_fxTemperature, CurrentScenePostFXSettings.m_fxTemperature, GetFadeTime());

                        whiteBalance.tint.overrideState = true;
                        whiteBalance.tint.value = Mathf.Lerp(CurrentSettings.m_fxTint, CurrentScenePostFXSettings.m_fxTint, GetFadeTime());
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out vignette))
                    {
                        vignette.intensity.overrideState = true;
                        vignette.intensity.value = Mathf.Lerp(CurrentSettings.m_fxVignetteIntensity, CurrentScenePostFXSettings.m_fxVignetteIntensity, GetFadeTime());
                    }
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Set HDRP Post fx to weather had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the post fx to the weather
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="weatherSettings"></param>
        private void SetURPFXToWeather(bool enabling, WeatherSettings weatherSettings)
        {
            try
            {
                if (!Application.isPlaying)
                {
                    return;
                }

#if UPPipeline
                if (m_urpFXVolumeProfile == null)
                {
                    m_urpFXVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Post Processing", "Null");
                }

                if (m_urpFXVolumeProfile == null)
                {
                    Debug.LogError("Unable to find the post processing volume profile... Exiting...");
                    return;
                }

                UnityEngine.Rendering.Universal.Bloom bloom;
                UnityEngine.Rendering.Universal.ColorAdjustments colorGrading;
                UnityEngine.Rendering.Universal.WhiteBalance whiteBalance;
                UnityEngine.Rendering.Universal.Vignette vignette;
                if (enabling)
                {
                    if (m_urpFXVolumeProfile.TryGet(out bloom))
                    {
                        bloom.intensity.overrideState = true;
                        if (weatherSettings.m_fXBloomIntensity != null)
                        {
                            bloom.intensity.value = Mathf.Lerp(CurrentSettings.m_fxBloomIntensity, weatherSettings.m_fXBloomIntensity.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }

                    if (m_urpFXVolumeProfile.TryGet(out colorGrading))
                    {
                        colorGrading.colorFilter.overrideState = true;
                        if (weatherSettings.m_fXColorFilter != null)
                        {
                            colorGrading.colorFilter.value = Color.Lerp(CurrentSettings.m_fxColorFilter, weatherSettings.m_fXColorFilter.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }

                    if (m_urpFXVolumeProfile.TryGet(out whiteBalance))
                    {
                        whiteBalance.temperature.overrideState = true;
                        if (weatherSettings.m_fXTemerature != null)
                        {
                            whiteBalance.temperature.value = Mathf.Lerp(CurrentSettings.m_fxTemperature, weatherSettings.m_fXTemerature.Evaluate(m_currentTime), GetFadeTime());
                        }

                        whiteBalance.tint.overrideState = true;
                        if (weatherSettings.m_fXTint != null)
                        {
                            whiteBalance.tint.value = Mathf.Lerp(CurrentSettings.m_fxTint, weatherSettings.m_fXTint.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }

                    if (m_urpFXVolumeProfile.TryGet(out vignette))
                    {
                        vignette.intensity.overrideState = true;
                        if (weatherSettings.m_fXVignetteIntensity != null)
                        {
                            vignette.intensity.value = Mathf.Lerp(CurrentSettings.m_fxVignetteIntensity, weatherSettings.m_fXVignetteIntensity.Evaluate(m_currentTime), GetFadeTime());
                        }
                    }
                }
                else
                {
                    if (m_urpFXVolumeProfile.TryGet(out bloom))
                    {
                        bloom.intensity.overrideState = true;
                        bloom.intensity.value = Mathf.Lerp(CurrentSettings.m_fxBloomIntensity, CurrentScenePostFXSettings.m_fxBloomIntensity, GetFadeTime());
                    }

                    if (m_urpFXVolumeProfile.TryGet(out colorGrading))
                    {
                        colorGrading.colorFilter.overrideState = true;
                        colorGrading.colorFilter.value = Color.Lerp(CurrentSettings.m_fxColorFilter, CurrentScenePostFXSettings.m_fxColorFilter, GetFadeTime());
                    }

                    if (m_urpFXVolumeProfile.TryGet(out whiteBalance))
                    {
                        whiteBalance.temperature.overrideState = true;
                        whiteBalance.temperature.value = Mathf.Lerp(CurrentSettings.m_fxTemperature, CurrentScenePostFXSettings.m_fxTemperature, GetFadeTime());

                        whiteBalance.tint.overrideState = true;
                        whiteBalance.tint.value = Mathf.Lerp(CurrentSettings.m_fxTint, CurrentScenePostFXSettings.m_fxTint, GetFadeTime());
                    }

                    if (m_urpFXVolumeProfile.TryGet(out vignette))
                    {
                        vignette.intensity.overrideState = true;
                        vignette.intensity.value = Mathf.Lerp(CurrentSettings.m_fxVignetteIntensity, CurrentScenePostFXSettings.m_fxVignetteIntensity, GetFadeTime());
                    }
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Set URP Post fx to weather had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the fog to the weather
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="fogColor"></param>
        /// <param name="fogDensity"></param>
        /// <param name="fogStart"></param>
        /// <param name="fogEnd"></param>
        /// <param name="oldFogColor"></param>
        /// <param name="oldFogDensity"></param>
        /// <param name="oldFogStart"></param>
        /// <param name="oldFogEnd"></param>
        private void SetFogToWeather(bool enabling, WeatherSettings weatherSettings)
        {
            try
            {
                if (enabling)
                {
                    if (weatherSettings.m_fogColor != null)
                    {
                        RenderSettings.fogColor = Color.Lerp(CurrentSettings.m_fogColor, weatherSettings.m_fogColor.Evaluate(m_currentTime), GetFadeTime());
                    }
                    if (weatherSettings.m_fogDensity != null)
                    {
                        RenderSettings.fogDensity = Mathf.Lerp(CurrentSettings.m_fogDensity, weatherSettings.m_fogDensity.Evaluate(m_currentTime), GetFadeTime());
                    }
                    if (weatherSettings.m_fogStartDistance != null)
                    {
                        RenderSettings.fogStartDistance = Mathf.Lerp(CurrentSettings.m_fogStart, weatherSettings.m_fogStartDistance.Evaluate(m_currentTime), GetFadeTime());
                    }
                    if (weatherSettings.m_fogEndDistance != null)
                    {
                        RenderSettings.fogEndDistance = Mathf.Lerp(CurrentSettings.m_fogEnd, weatherSettings.m_fogEndDistance.Evaluate(m_currentTime), GetFadeTime());
                    }
                }
                else
                {
                    RenderSettings.fogColor = Color.Lerp(CurrentSettings.m_fogColor, PW_VFX_Atmosphere.Instance.TimeOfDayFogColor.Evaluate(m_currentTime), GetFadeTime());
                    RenderSettings.fogDensity = Mathf.Lerp(CurrentSettings.m_fogDensity, PW_VFX_Atmosphere.Instance.FogDensityColor.Evaluate(m_currentTime), GetFadeTime());
                    RenderSettings.fogStartDistance = Mathf.Lerp(CurrentSettings.m_fogStart, PW_VFX_Atmosphere.Instance.FogStartDistance.Evaluate(m_currentTime), GetFadeTime());
                    RenderSettings.fogEndDistance = Mathf.Lerp(CurrentSettings.m_fogEnd, PW_VFX_Atmosphere.Instance.FogEndDistance.Evaluate(m_currentTime), GetFadeTime());
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Set fog to weather had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the fog to the weather
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="fogColor"></param>
        /// <param name="fogDensity"></param>
        /// <param name="fogStart"></param>
        /// <param name="fogEnd"></param>
        /// <param name="oldFogColor"></param>
        /// <param name="oldFogDensity"></param>
        /// <param name="oldFogStart"></param>
        /// <param name="oldFogEnd"></param>
        private void SetHDRPFogToWeather(bool enabling, WeatherSettings weatherSettings)
        {
            try
            {
#if HDPipeline
                if (m_hdrpVolumeProfile == null)
                {
                    m_hdrpVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Environment", "Processing");
                }

                if (m_hdrpVolumeProfile == null)
                {
                    Debug.LogError("Unable to find the sky volume profile... Exiting...");
                    return;
                }

                VisualEnvironment visualEnvironment;
                if (m_hdrpVolumeProfile.TryGet(out visualEnvironment))
                {
                    UnityEngine.Rendering.HighDefinition.Fog volumetricFog;
                    if (enabling)
                    {
                        if (m_hdrpVolumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.enableVolumetricFog.value = true;
                            if (weatherSettings.m_fogColor != null)
                            {
                                volumetricFog.color.value = Color.Lerp(CurrentSettings.m_fogColor, weatherSettings.m_fogColor.Evaluate(m_currentTime), GetFadeTime());
                            }
                            if (weatherSettings.m_fogHeight != null)
                            {
                                volumetricFog.baseHeight.value = Mathf.Lerp(CurrentSettings.m_hdrpFogHeight, weatherSettings.m_fogHeight.Evaluate(m_currentTime), GetFadeTime());
                            }
                            if (weatherSettings.m_volumetricGlobalAnisotropy != null)
                            {
                                volumetricFog.anisotropy.value = Mathf.Lerp(CurrentSettings.m_hdrpFogAnisotropy, weatherSettings.m_volumetricGlobalAnisotropy.Evaluate(m_currentTime), GetFadeTime());
                            }
                            if (weatherSettings.m_fogEndDistance != null)
                            {
                                volumetricFog.meanFreePath.value = Mathf.Lerp(CurrentSettings.m_fogEnd, weatherSettings.m_fogEndDistance.Evaluate(m_currentTime), GetFadeTime());
                            }
                            if (weatherSettings.m_volumetricGlobalProbeDimmer != null)
                            {
                                volumetricFog.globalLightProbeDimmer.value = Mathf.Lerp(CurrentSettings.m_fogEnd, weatherSettings.m_volumetricGlobalProbeDimmer.Evaluate(m_currentTime), GetFadeTime());
                            }
                            if (weatherSettings.m_volumetricDepthExtent != null)
                            {
                                volumetricFog.depthExtent.value = Mathf.Lerp(CurrentSettings.m_fogEnd, weatherSettings.m_volumetricDepthExtent.Evaluate(m_currentTime), GetFadeTime());
                            }
                        }
                    }
                    else
                    {
                        if (m_hdrpVolumeProfile.TryGet(out volumetricFog))
                        {
                            volumetricFog.enableVolumetricFog.value = true;
                            if (weatherSettings.m_fogColor != null)
                            {
                                volumetricFog.color.value = Color.Lerp(CurrentSettings.m_fogColor, PW_VFX_Atmosphere.Instance.TimeOfDayFogColor.Evaluate(m_currentTime), GetFadeTime());
                            }
                            if (weatherSettings.m_fogHeight != null)
                            {
                                volumetricFog.baseHeight.value = Mathf.Lerp(CurrentSettings.m_hdrpFogHeight, PW_VFX_Atmosphere.Instance.HDRPFogBaseHeight.Evaluate(m_currentTime), GetFadeTime());
                            }
                            if (weatherSettings.m_volumetricGlobalAnisotropy != null)
                            {
                                volumetricFog.anisotropy.value = Mathf.Lerp(CurrentSettings.m_hdrpFogAnisotropy, PW_VFX_Atmosphere.Instance.HDRPFogAnisotropy.Evaluate(m_currentTime), GetFadeTime());
                            }
                            if (weatherSettings.m_fogEndDistance != null)
                            {
                                volumetricFog.meanFreePath.value = Mathf.Lerp(CurrentSettings.m_fogEnd, PW_VFX_Atmosphere.Instance.FogEndDistance.Evaluate(m_currentTime), GetFadeTime());
                            }
                            if (weatherSettings.m_volumetricGlobalProbeDimmer != null)
                            {
                                volumetricFog.globalLightProbeDimmer.value = Mathf.Lerp(CurrentSettings.m_fogEnd, PW_VFX_Atmosphere.Instance.HDRPFogLightProbeDimmer.Evaluate(m_currentTime), GetFadeTime());
                            }
                            if (weatherSettings.m_volumetricDepthExtent != null)
                            {
                                volumetricFog.depthExtent.value = Mathf.Lerp(CurrentSettings.m_fogEnd, PW_VFX_Atmosphere.Instance.HDRPFogDepthExtent.Evaluate(m_currentTime), GetFadeTime());
                            }
                        }
                    }
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Set HDRP Fog to weather had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the ambient to the weather
        /// </summary>
        /// <param name="enabling"></param>
        /// <param name="ambientIntensity"></param>
        /// <param name="skyColor"></param>
        /// <param name="equatorColor"></param>
        /// <param name="groundColor"></param>
        /// <param name="oldAmbientIntensity"></param>
        /// <param name="oldSkyColor"></param>
        /// <param name="oldEquatorColor"></param>
        /// <param name="oldGroundColor"></param>
        private void SetAmbientToWeather(bool enabling, WeatherSettings weatherSettings)
        {
            try
            {
                if (enabling)
                {
                    if (weatherSettings.m_ambientIntensity != null)
                    {
                        RenderSettings.ambientIntensity = Mathf.Lerp(CurrentSettings.m_intensityAmbient, weatherSettings.m_ambientIntensity.Evaluate(m_currentTime), GetFadeTime());
                    }
                    if (weatherSettings.m_ambientSkyColor != null)
                    {
                        RenderSettings.ambientSkyColor = Color.Lerp(CurrentSettings.m_skyAmbient, weatherSettings.m_ambientSkyColor.Evaluate(m_currentTime), GetFadeTime());
                    }
                    if (weatherSettings.m_ambientEquatorColor != null)
                    {
                        RenderSettings.ambientEquatorColor = Color.Lerp(CurrentSettings.m_equatorAmbient, weatherSettings.m_ambientEquatorColor.Evaluate(m_currentTime), GetFadeTime());
                    }
                    if (weatherSettings.m_ambientGroundColor != null)
                    {
                        RenderSettings.ambientGroundColor = Color.Lerp(CurrentSettings.m_groundAmbient, weatherSettings.m_ambientGroundColor.Evaluate(m_currentTime), GetFadeTime());
                    }
                }
                else
                {
                    RenderSettings.ambientIntensity = Mathf.Lerp(CurrentSettings.m_intensityAmbient, PW_VFX_Atmosphere.Instance.AmbientIntensity.Evaluate(m_currentTime), GetFadeTime());
                    RenderSettings.ambientSkyColor = Color.Lerp(CurrentSettings.m_skyAmbient, PW_VFX_Atmosphere.Instance.TimeOfDaySkyColor.Evaluate(m_currentTime), GetFadeTime());
                    RenderSettings.ambientEquatorColor = Color.Lerp(CurrentSettings.m_equatorAmbient, PW_VFX_Atmosphere.Instance.TimeOfDayEqutorColor.Evaluate(m_currentTime), GetFadeTime());
                    RenderSettings.ambientGroundColor = Color.Lerp(CurrentSettings.m_groundAmbient, PW_VFX_Atmosphere.Instance.TimeOfDayGroundColor.Evaluate(m_currentTime), GetFadeTime());
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Set Ambient to weather had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets the auto DOF based on if the sun can see the camera
        /// </summary>
        private void SetAutoDOF()
        {
            try
            {
                if (!Application.isPlaying)
                {
                    return;
                }

                if (m_enableAutoDOF)
                {
                    return;
                }

#if UNITY_POST_PROCESSING_STACK_V2
                if (m_processProfile != null)
                {
                    UnityEngine.Rendering.PostProcessing.DepthOfField depthOfField;
                    if (m_processProfile.TryGetSettings(out depthOfField))
                    {
                        if (m_camera == null)
                        {
                            m_camera = GaiaUtils.GetCamera();
                        }
                        else
                        {
                            Ray ray = m_camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                            if (CheckIsNight())
                            {
                                if (Physics.Raycast(ray.origin, -m_moonLight.transform.forward, m_camera.farClipPlane, m_depthOfFieldDetectionLayers))
                                {
                                    depthOfField.focusDistance.value = 1.1f;
                                }
                                else
                                {
                                    depthOfField.focusDistance.value = 0.9f;
                                }
                            }
                            else
                            {
                                if (Physics.Raycast(ray.origin, -m_sunLight.transform.forward, m_camera.farClipPlane, m_depthOfFieldDetectionLayers))
                                {
                                    depthOfField.focusDistance.value = 1.1f;
                                }
                                else
                                {
                                    depthOfField.focusDistance.value = 0.9f;
                                }
                            }
                        }
                    }
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Set depth of field had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets auto exposure only built-in supported at this moment in time
        /// </summary>
        private void SetAutoExposure()
        {
            try
            {
#if UNITY_POST_PROCESSING_STACK_V2
                if (m_processProfile != null)
                {
                    if (m_processProfile != null)
                    {
                        UnityEngine.Rendering.PostProcessing.AutoExposure autoExposure;
                        if (m_processProfile.TryGetSettings(out autoExposure))
                        {
                            if (m_todPostProcessExposure != null)
                            {
                                autoExposure.keyValue.value = m_todPostProcessExposure.Evaluate(m_currentTime);
                            }
                        }
                    }
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Set auto exposure had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }

        #endregion

        #endregion

        #region Utils

        /// <summary>
        /// Checks if systems are valid
        /// </summary>
        private void CheckValidSystems()
        {
            if (PW_VFX_Clouds.Instance != null)
            {
                CloudSystemValid = true;
            }
            else
            {
                CloudSystemValid = false;
            }
            if (PW_VFX_Atmosphere.Instance != null)
            {
                AtmosphereSystemValid = true;
            }
            else
            {
                AtmosphereSystemValid = false;
            }

            if (PWS_WaterSystem.Instance != null)
            {
                WaterSystemValid = true;
            }
            else
            {
                WaterSystemValid = false;
            }
        }
        /// <summary>
        /// Sets the HDRP sky type to physically based sky
        /// </summary>
        private void HDRPUpdateSkyMode(bool setAllDefaults = false)
        {
            try
            {
#if HDPipeline
                if (m_hdrpVolumeProfile == null)
                {
                    m_hdrpVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Environment", "Processing");
                }

                else if (m_hdrpVolumeProfile != null)
                {
                    VisualEnvironment visualEnvironment;
                    PhysicallyBasedSky physicallyBasedSky;
                    HDRISky hDRISky;
                    GradientSky gradientSky;

                    if (m_hdrpVolumeProfile.TryGet(out visualEnvironment))
                    {
                        if (m_hdrpVolumeProfile.TryGet(out physicallyBasedSky))
                        {
                            if (m_hdrpVolumeProfile.TryGet(out hDRISky))
                            {
                                if (m_hdrpVolumeProfile.TryGet(out gradientSky))
                                {
                                    visualEnvironment.skyType.value = (int) SkyType.PhysicallyBased;

                                    physicallyBasedSky.active = true;                 
                                    //Planet
                                    if (setAllDefaults)
                                    {
#if !UNITY_2020_2_OR_NEWER
                                        physicallyBasedSky.earthPreset.value = false;
#endif
                                        physicallyBasedSky.planetaryRadius.value = 6378100f;
                                        physicallyBasedSky.sphericalMode.value = true;
                                        physicallyBasedSky.planetCenterPosition.value = new Vector3(0f, -6378100f, 0f);
                                        physicallyBasedSky.planetRotation.value = new Vector3(0f, 0f, 0f);
                                        physicallyBasedSky.groundColorTexture.value = null;
                                        physicallyBasedSky.groundTint.value = GaiaUtils.GetColorFromHTML("B1B1B1");
                                        physicallyBasedSky.groundEmissionTexture.value = null;
                                        physicallyBasedSky.groundEmissionMultiplier.value = 1f;
                                        //Space
                                        physicallyBasedSky.spaceRotation.value = new Vector3(0f, 0f, 0f);
                                        physicallyBasedSky.spaceEmissionTexture.value = null;
                                        physicallyBasedSky.spaceEmissionMultiplier.value = 1f;
                                        //Air
                                        physicallyBasedSky.airMaximumAltitude.value = 55261.97f;
                                        physicallyBasedSky.airDensityR.value = 0.04534f;
                                        physicallyBasedSky.airDensityG.value = 0.1023724f;
                                        physicallyBasedSky.airDensityB.value = 0.2326406f;
                                        physicallyBasedSky.airTint.value = GaiaUtils.GetColorFromHTML("E6E6FF");
                                        //Aerosolos
                                        physicallyBasedSky.aerosolMaximumAltitude.value = 8289.296f;
                                        physicallyBasedSky.aerosolDensity.value = 0.01f;
                                        physicallyBasedSky.aerosolTint.value = GaiaUtils.GetColorFromHTML("E6E6E6");
                                        physicallyBasedSky.aerosolAnisotropy.value = 0f;
                                        //Artistic Overrides
                                        physicallyBasedSky.colorSaturation.value = 1f;
                                        physicallyBasedSky.alphaSaturation.value = 1f;
                                        physicallyBasedSky.alphaMultiplier.value = 1f;
                                        physicallyBasedSky.horizonTint.value = GaiaUtils.GetColorFromHTML("FFFFFF");
                                        physicallyBasedSky.horizonZenithShift.value = 0f;
                                        physicallyBasedSky.zenithTint.value = GaiaUtils.GetColorFromHTML("FFFFFF");
                                    }

                                    hDRISky.active = false;
                                    gradientSky.active = false;

#if UNITY_EDITOR
                                    EditorUtility.SetDirty(m_hdrpVolumeProfile);
#endif
                                }
                            }
                        }
                    }
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("HDRP Update sky mode had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Gets the actual seasonal color
        /// </summary>
        /// <returns></returns>
        private Color GetActualSeasonalTintColor()
        {
            try
            {
                Color tint = Color.white;
                if (Season < 1f)
                {
                    if (tint != SeasonWinterTint)
                    {
                        tint = Color.Lerp(SeasonWinterTint, SeasonSpringTint, Season);
                    }
                }
                else if (Season < 2f)
                {
                    if (tint != SeasonSpringTint)
                    {
                        tint = Color.Lerp(SeasonSpringTint, SeasonSummerTint, Season - 1f);
                    }
                }
                else if (Season < 3f)
                {
                    if (tint != SeasonSummerTint)
                    {
                        tint = Color.Lerp(SeasonSummerTint, SeasonAutumnTint, Season - 2f);
                    }
                }
                else
                {
                    if (tint != SeasonAutumnTint)
                    {
                        tint = Color.Lerp(SeasonAutumnTint, SeasonWinterTint, Season - 3f);
                    }
                }

                return tint;
            }
            catch (Exception e)
            {
                Debug.LogError("Setting real tint color had a issue " + e.Message + " This came from " + e.StackTrace);
                return Color.white;
            }
        }
        /// <summary>
        /// Checks to see if atmosphere needs to be processing updates
        /// </summary>
        /// <param name="checkColor"></param>
        /// <param name="checkFloat"></param>
        /// <returns></returns>
        public bool DoesAtmosphereNeedToUpdate(bool overwrite = false)
        {
            try
            {
                bool needsUpdating = false;
                if (GaiaGlobal.Instance != null)
                {
                    if (GaiaGlobal.Instance.GaiaTimeOfDayValue.m_todEnabled)
                    {
                        needsUpdating = true;
                    }
                    else
                    {
                        needsUpdating = true;
                    }
                }

                return needsUpdating;
            }
            catch (Exception e)
            {
                Debug.LogError("Does atmosphere need updating had a issue " + e.Message + " This came from " + e.StackTrace);
                return false;
            }
        }
        /// <summary>
        /// Generates a random number based on min and max values given
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private float GenerateRandomRangeValue(float min, float max)
        {
            float value = Random.Range(min, max);
            return value;
        }
        /// <summary>
        /// Create a wind zone fi it doesn't exist
        /// </summary>
        private WindZone GetOrCreateWindZone()
        {
            try
            {
                GameObject windZone = GameObject.Find(m_windZoneObjectName);
                WindZone windSettings = FindObjectOfType<WindZone>();
                if (windSettings != null)
                {
                    return windSettings;
                }
                if (windZone == null)
                {
#if UNITY_EDITOR
                    windZone = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(GaiaUtils.GetAssetPath(m_windZoneObjectName + ".prefab"))) as GameObject;
#endif
                    Vector3 windRotation = new Vector3(25f, 0f, 0f);
                    if (windZone != null)
                    {
                        windZone.transform.Rotate(windRotation, Space.World);

                        windSettings = windZone.GetComponent<WindZone>();
                        if (windSettings == null)
                        {
                            windSettings = windZone.AddComponent<WindZone>();
                        }
                    }
                }

                windSettings.mode = WindZoneMode.Directional;
                windSettings.windMain = 0.35f;
                windSettings.windTurbulence = 0.35f;
                windSettings.windPulseMagnitude = 0.2f;
                windSettings.windPulseFrequency = 0.05f;
                m_windSpeed = windSettings.windMain;
                m_windTurbulence = windSettings.windTurbulence;

                m_windZone = windSettings;

                GameObject parentObject = GameObject.Find("Gaia Weather");
                if (parentObject != null)
                {
                    if (windZone != null)
                    {
                        windZone.transform.SetParent(parentObject.transform);
                    }
                }

                return windSettings;
            }
            catch (Exception e)
            {
                Debug.LogError("Creating a wind zone had a issue " + e.Message + " This came from " + e.StackTrace);
                return null;
            }
        }
        /// <summary>
        /// Gets the post processing profile fromt he global volume
        /// </summary>
        /// <returns></returns>
#if UNITY_POST_PROCESSING_STACK_V2
        public PostProcessProfile GetPostProcessingProfile()
        {
            try
            {
                m_volumes = FindObjectsOfType<PostProcessVolume>();
                if (m_volumes.Length > 0)
                {
                    foreach (PostProcessVolume volume in m_volumes)
                    {
                        if (volume.isGlobal)
                        {
                            if (volume.sharedProfile != null)
                            {
                                return volume.sharedProfile;
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Debug.LogError("Getting the scene post processing profile had a issue " + e.Message + " This came from " + e.StackTrace);
                return null;
            }
        }
        /// <summary>
        /// Sets the post processing profile
        /// </summary>
        /// <param name="profile"></param>
        public void SetPostProcessingProfile(PostProcessProfile profile, bool directToCamera)
        {
            try
            {
                if (profile == null)
                {
                    return;
                }

                m_processProfile = profile;
                CheckPWSkyPostFXManager();

                PostProcessLayer processLayer = FindObjectOfType<PostProcessLayer>();
                if (processLayer != null)
                {
                    processLayer.finalBlitToCameraTarget = directToCamera;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Settings post processing profile had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Checks to see if the post fx manager script is attached to the volume object
        /// </summary>
        public void CheckPWSkyPostFXManager()
        {
            m_volumes = FindObjectsOfType<PostProcessVolume>();
            if (m_volumes.Length > 0)
            {
                foreach (var volume in m_volumes)
                {
                    if (volume.isGlobal)
                    {
                        PWSkyPostProcessingManager manager = volume.gameObject.GetComponent<PWSkyPostProcessingManager>();
                        if (manager == null)
                        {
                            manager = volume.gameObject.AddComponent<PWSkyPostProcessingManager>();
                        }
                        break;
                    }
                }
            }

            if (m_player != null)
            {
                PostProcessLayer layer = m_player.GetComponent<PostProcessLayer>();
                if (layer != null)
                {
                    layer.fog.enabled = true;
                }
            }
        }
#endif
        /// <summary>
        /// Samples the texture based on position at active terrain
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private float SampleLocationWithMask(Texture2D texture, Vector3 location, ChannelSelection channelSelection)
        {
            try
            {
                if (texture == null)
                {
                    return 1f;
                }

                Terrain[] terrains = Terrain.activeTerrains;
                if (terrains.Length < 1)
                {
                    return 1f;
                }

                Terrain terrain = TerrainHelper.GetTerrain(location);
                if (terrain != null)
                {
                    float scalerX = (float)(location.x - terrain.transform.position.x) / (float)terrain.terrainData.size.x;
                    float scalerZ = (float)(location.z - terrain.transform.position.z) / (float)terrain.terrainData.size.z;
                    Color pixelColor = texture.GetPixel(Mathf.RoundToInt(scalerX * texture.width), Mathf.RoundToInt(scalerZ * texture.height));

                    switch (channelSelection)
                    {
                        case ChannelSelection.R:
                            return pixelColor.r;
                        case ChannelSelection.G:
                            return pixelColor.g;
                        case ChannelSelection.B:
                            return pixelColor.b;
                        case ChannelSelection.A:
                            return pixelColor.a;
                    }
                }

                return 0f;
            }
            catch (Exception e)
            {
                Debug.LogError("Sampling location with mask texture had a issue " + e.Message + " This came from " + e.StackTrace);
                return 1f;
            }
        }
        /// <summary>
        /// Removes warnings of privtae fields not being used
        /// </summary>
        private void RemoveWarnings()
        {
            if (m_newSkyboxExposure < 0)
            {
                m_newSkyboxExposure = 0;
            }
            if (m_newAmbientIntensity < 0)
            {
                m_newAmbientIntensity = 0;
            }
            if (m_newSunIntensity < 0)
            {
                m_newSunIntensity = 0;
            }
        }

        #endregion

        #endregion
        #region Public Functions

        /// <summary>
        /// Sets the sun rotation
        /// </summary>
        /// <param name="value"></param>
        public void SetSunRotation(float value)
        {
            if (PW_VFX_Atmosphere.Instance != null)
            {
                PW_VFX_Atmosphere.Instance.UpdateSunRotationY(value);
            }
        }
        /// <summary>
        /// Gets the sun rotation
        /// </summary>
        /// <param name="value"></param>
        public float GetSunRotation()
        {
            if (PW_VFX_Atmosphere.Instance != null)
            {
                return PW_VFX_Atmosphere.Instance.m_sunRotation;
            }

            return 0f;
        }
        /// <summary>
        /// Updates the sun and moon shadow setup
        /// </summary>
        /// <param name="profile"></param>
        public void UpdateLightShadows(GaiaLightingProfileValues profile)
        {
            if (profile == null)
            {
                return;
            }

            if (m_sunLight == null)
            {
                return;
            }

            m_sunLight.shadows = profile.m_shadowCastingMode;
            m_sunLight.shadowResolution = profile.m_sunShadowResolution;

            if (m_moonLight == null)
            {
                return;
            }

            m_moonLight.shadows = profile.m_shadowCastingMode;
            m_moonLight.shadowResolution = profile.m_sunShadowResolution;
        }
        /// <summary>
        /// Updates the fog mode
        /// </summary>
        /// <param name="fogMode"></param>
        public void UpdateFogMode(FogMode fogMode)
        {
            RenderSettings.fogMode = fogMode;
        }
        /// <summary>
        /// Set if weather is enabled or disabled
        /// </summary>
        /// <param name="enabled"></param>
        public void SetWeatherStatus(bool enabled)
        {
            if (!enabled)
            {
                m_disableWeatherFX = true;
                if (IsSnowing && !IsRaining)
                {
                    StopSnow();
                    return;
                }

                if (IsRaining && !IsSnowing)
                {
                    StopRain();
                    return;
                }

                if (!IsRaining && !IsSnowing)
                {
                    EnableRain = enabled;
                    EnableSnow = enabled;
                }
            }
            else
            {
                EnableRain = enabled;
                EnableSnow = enabled;
            }
        }
        /// <summary>
        /// Checks if the weather particles need to be disabled while underwater
        /// </summary>
        public void CheckUnderwaterParticlesVFX(bool isUnderwater)
        {
            try
            {
                if (isUnderwater)
                {
                    if (IsRaining)
                    {
                        if (m_rainParticles != null)
                        {
                            m_rainParticles.enabled = false;
                        }

                        if (m_rainVFX != null)
                        {
                            m_rainVFX.Stop();
                        }

                        if (m_rainAudioSource != null)
                        {
                            m_rainAudioSource.spatialBlend = 1f;
                        }
                    }

                    if (IsSnowing)
                    {
                        if (m_snowParticles != null)
                        {
                            m_snowParticles.Stop();
                        }

                        if (m_snowVFX != null)
                        {
                            m_snowVFX.Stop();
                        }

                        if (m_snowAudioSource != null)
                        {
                            m_snowAudioSource.spatialBlend = 1f;
                        }
                    }

                    if (AudioReverbFilter != null)
                    {
                        AudioReverbFilter.reverbPreset = AudioReverbPreset.Underwater;
                    }
                }
                else
                {
                    if (IsRaining)
                    {
                        if (m_rainParticles != null)
                        {
                            m_rainParticles.enabled = true;
                        }

                        if (m_rainVFX != null)
                        {
                            m_rainVFX.Play();
                        }

                        if (m_rainAudioSource != null)
                        {
                            m_rainAudioSource.spatialBlend = 0f;
                        }
                    }

                    if (IsSnowing)
                    {
                        if (m_snowParticles != null)
                        {
                            m_snowParticles.Play();
                        }

                        if (m_snowVFX != null)
                        {
                            m_snowVFX.Play();
                        }

                        if (m_snowAudioSource != null)
                        {
                            m_snowAudioSource.spatialBlend = 0f;
                        }
                    }

                    if (AudioReverbFilter != null)
                    {
                        AudioReverbFilter.reverbPreset = AudioReverbPreset;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Check underwater particles had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Gets all the current settings from lighting/fog/clouds etc...
        /// </summary>
        public void GetCurrentSettings()
        {
            try
            {
                if (m_sunLight == null)
                {
                    m_sunLight = GaiaUtils.GetMainDirectionalLight();
                }

                if (m_moonLight == null)
                {
                    m_moonLight = GaiaUtils.GetMainMoonLight();
                }

                //Skybox,Fog,Ambient
                if (m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    //Fog
                    CurrentSettings.m_fogColor = RenderSettings.fogColor;
                    CurrentSettings.m_fogDensity = RenderSettings.fogDensity;
                    CurrentSettings.m_fogStart = RenderSettings.fogStartDistance;
                    CurrentSettings.m_fogEnd = RenderSettings.fogEndDistance;

                    //Ambient
                    CurrentSettings.m_skyAmbient = RenderSettings.ambientSkyColor;
                    CurrentSettings.m_equatorAmbient = RenderSettings.ambientEquatorColor;
                    CurrentSettings.m_groundAmbient = RenderSettings.ambientGroundColor;
                    CurrentSettings.m_intensityAmbient = RenderSettings.ambientIntensity;

                    //Skybox
                    if (m_skyboxMaterial != null)
                    {
                        if (m_skyboxMaterial.shader.name == GaiaShaderID.m_pwSkySkyboxShader)
                        {
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxTint))
                            {
                                CurrentSettings.m_skyboxTint = m_skyboxMaterial.GetColor(GaiaShaderID.m_unitySkyboxTint);
                            }
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxExposure))
                            {
                                CurrentSettings.m_skyboxExposure = m_skyboxMaterial.GetFloat(GaiaShaderID.m_unitySkyboxExposure);
                            }
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_unitySkyboxAtmosphereThickness))
                            {
                                CurrentSettings.m_skyboxAtmosphereThickness = m_skyboxMaterial.GetFloat(GaiaShaderID.m_unitySkyboxAtmosphereThickness);
                            }
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_pwSkyFogHeight))
                            {
                                CurrentSettings.m_skyboxFogHeight = m_skyboxMaterial.GetFloat(GaiaShaderID.m_pwSkyFogHeight);
                            }
                            if (GaiaUtils.ValidateShaderProperty(m_skyboxMaterial, GaiaShaderID.m_pwSkyFogGradient))
                            {
                                CurrentSettings.m_skyboxFogGradient = m_skyboxMaterial.GetFloat(GaiaShaderID.m_pwSkyFogGradient);
                            }
                        }
                    }
                }
                else
                {
#if HDPipeline
                    if (m_hdrpVolumeProfile == null)
                    {
                        m_hdrpVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Environment", "Processing");
                    }

                    if (m_hdrpVolumeProfile == null)
                    {
                        return;
                    }

                    PhysicallyBasedSky physicallyBasedSky;
                    UnityEngine.Rendering.HighDefinition.Fog volumetricFog;
                    if (m_hdrpVolumeProfile.TryGet(out physicallyBasedSky))
                    {
                        if (m_hdrpVolumeProfile.TryGet(out volumetricFog))
                        {
                            //Fog
                            CurrentSettings.m_fogColor = volumetricFog.color.value;
                            CurrentSettings.m_hdrpFogAnisotropy = volumetricFog.anisotropy.value;
                            CurrentSettings.m_hdrpFogDepthExtent = volumetricFog.depthExtent.value;
                            CurrentSettings.m_hdrpFogHeight = volumetricFog.baseHeight.value;
                            CurrentSettings.m_hdrpFogLightProbeDimmer = volumetricFog.globalLightProbeDimmer.value;

                            //Sky
                            CurrentSettings.m_skyboxTint = physicallyBasedSky.groundTint.value;
                            CurrentSettings.m_skyboxExposure = physicallyBasedSky.exposure.value;
                        }
                    }
#endif
                }

                //Post FX
                if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.Universal)
                {
#if UPPipeline
                    if (m_urpFXVolumeProfile == null)
                    {
                        m_urpFXVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Post Processing", "Null");
                    }

                    UnityEngine.Rendering.Universal.Bloom bloom;
                    UnityEngine.Rendering.Universal.ColorAdjustments colorGrading;
                    UnityEngine.Rendering.Universal.WhiteBalance whiteBalance;
                    UnityEngine.Rendering.Universal.Vignette vignette;
                    if (m_urpFXVolumeProfile == null)
                    {
                        return;
                    }

                    if (m_urpFXVolumeProfile.TryGet(out bloom))
                    {
                        CurrentSettings.m_fxBloomIntensity = bloom.intensity.value;
                    }

                    if (m_urpFXVolumeProfile.TryGet(out colorGrading))
                    {
                        CurrentSettings.m_fxColorFilter = colorGrading.colorFilter.value;
                    }

                    if (m_urpFXVolumeProfile.TryGet(out whiteBalance))
                    {
                        CurrentSettings.m_fxTemperature = whiteBalance.temperature.value;
                        CurrentSettings.m_fxTint = whiteBalance.tint.value;
                    }

                    if (m_urpFXVolumeProfile.TryGet(out vignette))
                    {
                        CurrentSettings.m_fxVignetteIntensity = vignette.intensity.value;
                    }
#endif
                }
                else if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
#if HDPipeline
                    UnityEngine.Rendering.HighDefinition.Bloom bloom;
                    UnityEngine.Rendering.HighDefinition.ColorAdjustments colorGrading;
                    UnityEngine.Rendering.HighDefinition.WhiteBalance whiteBalance;
                    UnityEngine.Rendering.HighDefinition.Vignette vignette;

                    if (m_hdrpFXVolumeProfile == null)
                    {
                        m_hdrpFXVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Post Processing", "Null");
                    }

                    if (m_hdrpFXVolumeProfile == null)
                    {
                        return;
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out bloom))
                    {
                        CurrentSettings.m_fxBloomIntensity = bloom.intensity.value;
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out colorGrading))
                    {
                        CurrentSettings.m_fxColorFilter = colorGrading.colorFilter.value;
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out whiteBalance))
                    {
                        CurrentSettings.m_fxTemperature = whiteBalance.temperature.value;
                        CurrentSettings.m_fxTint = whiteBalance.tint.value;
                    }

                    if (m_hdrpFXVolumeProfile.TryGet(out vignette))
                    {
                        CurrentSettings.m_fxVignetteIntensity = vignette.intensity.value;
                    }
#endif
                }
                else
                {
                    //Post FX
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_processProfile != null)
                    {
                        UnityEngine.Rendering.PostProcessing.Bloom bloom;
                        if (m_processProfile.TryGetSettings(out bloom))
                        {
                            CurrentSettings.m_fxBloomIntensity = bloom.intensity.value;
                        }

                        UnityEngine.Rendering.PostProcessing.ColorGrading colorGrading;
                        if (m_processProfile.TryGetSettings(out colorGrading))
                        {
                            CurrentSettings.m_fxTemperature = colorGrading.temperature.value;
                            CurrentSettings.m_fxTint = colorGrading.tint.value;
                            CurrentSettings.m_fxColorFilter = colorGrading.colorFilter.value;
                        }

                        UnityEngine.Rendering.PostProcessing.Vignette vignette;
                        if (m_processProfile.TryGetSettings(out vignette))
                        {
                            CurrentSettings.m_fxVignetteIntensity = vignette.intensity.value;
                        }
                    }
#endif
                }

                //Snow
                if (IsSnowing)
                {
                    CurrentSettings.m_snowHeight = SnowHeight;
                }
                else
                {
                    CurrentSettings.m_snowHeight = m_savedSnowingHeight;
                }

                //Sun
                if (m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    if (CheckIsNight())
                    {
                        CurrentSettings.m_sunColor = m_moonLight.color;
                        CurrentSettings.m_sunIntensity = m_moonLight.intensity;
                    }
                    else
                    {
                        CurrentSettings.m_sunColor = m_sunLight.color;
                        CurrentSettings.m_sunIntensity = m_sunLight.intensity;
                    }
                }
                else
                {
#if HDPipeline
                    HDAdditionalLightData HDLightData = null;
                    if (CheckIsNight())
                    {
                        HDLightData = GaiaUtils.GetOrAddHDRPLightData(m_moonLight);
                    }
                    else
                    {
                        HDLightData = GaiaUtils.GetOrAddHDRPLightData(m_sunLight);
                    }

                    if (HDLightData != null)
                    {
                        CurrentSettings.m_sunColor = HDLightData.color;
                        CurrentSettings.m_sunIntensity = HDLightData.intensity;
                    }
#endif
                }

                //Clouds
                if (PW_VFX_Atmosphere.Instance != null)
                {
                    CurrentSettings.m_cloudDensity = Shader.GetGlobalFloat(GaiaShaderID.m_cloudHeightDensity);
                    CurrentSettings.m_cloudThickness = Shader.GetGlobalFloat(GaiaShaderID.m_cloudHeightThickness);
                    CurrentSettings.m_cloudSpeed = Shader.GetGlobalFloat(GaiaShaderID.m_cloudSpeed);
                    CurrentSettings.m_cloudOpacity = Shader.GetGlobalFloat(GaiaShaderID.m_cloudOpacity);
                    CurrentSettings.m_cloudDomeBrightness = Shader.GetGlobalFloat(GaiaShaderID.m_cloudDomeBrightness);
                }

                //Volume
                if (m_rainAudioSource != null)
                {
                    CurrentSettings.m_rainAudioVolume = m_rainAudioSource.volume;
                }

                if (m_snowAudioSource != null)
                {
                    CurrentSettings.m_snowAudioVolume = m_snowAudioSource.volume;
                }

                //Wind
                CurrentSettings.m_windSpeed = WindSpeed;
                m_newWindSpeed = WindSpeed;
                CurrentSettings.m_windTurbulence = WindTurbulence;
                m_newWindTurbulence = WindTurbulence;
                CurrentSettings.m_windFrequency = WindFrequency;
                m_newWindFrequency = WindFrequency;
                CurrentSettings.m_windMultiplier = WindMultiplier;
                m_newWindMultiplier = WindMultiplier;
            }
            catch (Exception e)
            {
                Debug.LogError("Get all current settings had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Gets the fade duration time for weather effects
        /// </summary>
        /// <returns></returns>
        public float GetFadeTime()
        {
            if (m_disableWeatherFX || m_instantVFX)
            {
                return 1f;
            }
            else
            {
                TransationTime += 0.05f * Time.deltaTime;
                return TransationTime / m_weatherFadeDuration;
            }
        }
        /// <summary>
        /// Play Snow
        /// </summary>
        public void PlaySnow()
        {
            try
            {
                SkyboxShaderValid = ValidateSkyboxShader();
                if (EnableSnow)
                {
                    m_snowWaitTime = 0;
                    if (IsRaining)
                    {
                        IsRainingFinished = false;
                    }
                    else
                    {
                        IsRainingFinished = true;
                    }
                    IsRaining = false;
                    IsSnowing = true;
                    IsSnowingFinished = false;
                    m_weatherSystemActive = true;
                    m_snowDuration = GenerateRandomRangeValue(m_snowWeatherSettings.m_durationMinWaitTime, m_snowWeatherSettings.m_durationMaxWaitTime);
                    if (m_enableThunder)
                    {
                        IsThundering = false;
                    }
                    TransationTime = 0;

                    if (m_rainAudioSource != null)
                    {
                        m_rainAudioSource.volume = 0f;
                    }

                    IsCurrentSettingsSet = false;
                    if (m_instantStartStop)
                    {
                        m_instantVFX = true;
                    }
                    else
                    {
                        m_instantVFX = false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Play snow had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Stops the snow
        /// </summary>
        public void StopSnow()
        {
            try
            {
                SkyboxShaderValid = ValidateSkyboxShader();
                if (EnableSnow)
                {
                    m_snowWaitTime = GenerateRandomRangeValue(m_snowWeatherSettings.m_minWaitTime, m_snowWeatherSettings.m_maxWaitTime);
                    IsSnowing = false;
                    IsSnowingFinished = false;
                    TransationTime = 0;
                    GetCurrentSettings();
                    if (m_instantStartStop)
                    {
                        m_instantVFX = true;
                    }
                    else
                    {
                        m_instantVFX = false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Stop snow had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Play Rain
        /// </summary>
        public void PlayRain()
        {
            try
            {
                SkyboxShaderValid = ValidateSkyboxShader();
                if (EnableRain)
                {
                    m_rainWaitTime = 0;
                    if (IsSnowing)
                    {
                        IsSnowingFinished = false;
                    }
                    else
                    {
                        IsSnowingFinished = true;
                    }
                    IsSnowing = false;
                    IsRaining = true;
                    IsRainingFinished = false;
                    m_rainDuration = GenerateRandomRangeValue(m_rainWeatherSettings.m_durationMinWaitTime, m_rainWeatherSettings.m_durationMaxWaitTime);
                    m_weatherSystemActive = true;
                    if (m_enableThunder)
                    {
                        IsThundering = true;
                        m_thunderWaitDuration = 5f;
                    }
                    TransationTime = 0;

                    IsCurrentSettingsSet = false;
                    if (m_instantStartStop)
                    {
                        m_instantVFX = true;
                    }
                    else
                    {
                        m_instantVFX = false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Play rain had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Stops the rain
        /// </summary>
        public void StopRain()
        {
            try
            {
                SkyboxShaderValid = ValidateSkyboxShader();
                if (EnableRain)
                {
                    m_rainWaitTime = GenerateRandomRangeValue(m_rainWeatherSettings.m_minWaitTime, m_rainWeatherSettings.m_maxWaitTime);
                    IsRaining = false;
                    IsRainingFinished = false;
                    if (m_enableThunder)
                    {
                        IsThundering = false;
                    }
                    TransationTime = 0;
                    GetCurrentSettings();
                    if (m_instantStartStop)
                    {
                        m_instantVFX = true;
                    }
                    else
                    {
                        m_instantVFX = false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Stop rain had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Check is it is night time
        /// </summary>
        /// <returns></returns>
        public bool CheckIsNight()
        {
            try
            {
                bool isNight = false;
                if (m_moonLight != null)
                {
                    m_moonLight.transform.localRotation = new Quaternion(0.0f, -1.0f, 0.0f, 0.0f);
                }

                m_currentTime = GaiaGlobal.GetTimeOfDayMainValue();
                if (m_currentTime > 0.2475f && m_currentTime < 0.7562f)
                {
                    isNight = false;
                }
                else
                {
                    isNight = true;
                }

                if (m_modifyPostProcessing)
                {
                    UpdateHDRPTODPostFX(isNight);
                }

                return isNight;
            }
            catch (Exception e)
            {
                Debug.LogError("Check is night had a issue " + e.Message + " This came from " + e.StackTrace);
                return false;
            }
        }
        /// <summary>
        /// Updates HDRP post fx during day and night to get best result
        /// </summary>
        /// <param name="isNight"></param>
        public void UpdateHDRPTODPostFX(bool isNight)
        {
            try
            {
#if HDPipeline
                UnityEngine.Rendering.HighDefinition.Exposure exposure;
                UnityEngine.Rendering.HighDefinition.SplitToning splitToning;
                UnityEngine.Rendering.HighDefinition.ColorAdjustments colorAdjustments;
                UnityEngine.Rendering.HighDefinition.Vignette vignette;
                if (m_hdrpFXVolumeProfile == null)
                {
                    m_hdrpFXVolumeProfile = GaiaUtils.GetVolumeProfile(true, "Post Processing", "Null");
                }

                if (!IsRaining || !IsSnowing)
                {
                    if (m_hdrpFXVolumeProfile != null)
                    {
                        if (isNight)
                        {
                            if (m_hdrpFXVolumeProfile.TryGet(out exposure))
                            {
                                exposure.mode.value = ExposureMode.Automatic;
                                exposure.limitMin.value = -0.5f;
                                exposure.limitMax.value = 0f;
                            }

                            if (m_hdrpFXVolumeProfile.TryGet(out splitToning))
                            {
                                splitToning.balance.value = -20f;
                            }

                            if (m_hdrpFXVolumeProfile.TryGet(out colorAdjustments))
                            {
                                colorAdjustments.hueShift.value = -3f;
                                colorAdjustments.postExposure.value = 0f;
                            }

                            if (m_hdrpFXVolumeProfile.TryGet(out vignette))
                            {
                                vignette.smoothness.value = 0.75f;
                            }
                        }
                        else
                        {
                            if (m_hdrpFXVolumeProfile.TryGet(out exposure))
                            {
                                exposure.mode.value = ExposureMode.Automatic;
                                exposure.limitMin.value = -2f;
                                exposure.limitMax.value = -1f;
                            }

                            if (m_hdrpFXVolumeProfile.TryGet(out splitToning))
                            {
                                splitToning.balance.value = -10f;
                            }

                            if (m_hdrpFXVolumeProfile.TryGet(out colorAdjustments))
                            {
                                colorAdjustments.hueShift.value = 0f;
                                colorAdjustments.postExposure.value = -0.75f;
                            }

                            if (m_hdrpFXVolumeProfile.TryGet(out vignette))
                            {
                                vignette.smoothness.value = 0.625f;
                            }
                        }
                    }
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Update HDRP time of day post fx had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Loads up the particle systems
        /// </summary>
        public void LoadUpContent()
        {
            try
            {
#if UNITY_EDITOR
                if (RenderSettings.skybox == null)
                {
                    RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(GaiaUtils.GetAssetPath("Gaia Sky.mat"));
                }
                m_skyboxMaterial = RenderSettings.skybox;

                if (m_sunLight == null)
                {
                    m_sunLight = GaiaUtils.GetMainDirectionalLight();
                }

                if (m_moonLight == null)
                {
                    m_moonLight = GaiaUtils.GetMainMoonLight();
                }

                if (m_moonLight != null)
                {
                    m_moonLight.transform.localRotation = new Quaternion(0.0f, -1.0f, 0.0f, 0.0f);
                }

                if (m_sunLight != null && m_moonLight != null)
                {
                    m_moonLight.transform.SetParent(m_sunLight.transform);
                }

                if (m_snowParticles == null)
                {
                    GameObject snowParticles = GameObject.Find("PW_VFX_Snow");
                    if (snowParticles == null)
                    {
                        GameObject prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(GaiaUtils.GetAssetPath("PW_VFX_Snow.prefab"));
                        if (prefabObject != null)
                        {
                            snowParticles = PrefabUtility.InstantiatePrefab(prefabObject) as GameObject;
                        }
                    }

                    if (snowParticles != null)
                    {
                        snowParticles.transform.SetParent(gameObject.transform);
                        m_snowParticles = snowParticles.GetComponent<ParticleSystem>();
                    }
                    else
                    {
                        Debug.LogError("(Gaia Snow Particles) could not be found or Instantiated from the project. It could be missing?");
                    }
                }

                if (m_snowParticles != null)
                {
                    GameObject snowvVFXObj = GameObject.Find("SnowFall_Far");
                    if (snowvVFXObj != null)
                    {
                        m_snowVFX = snowvVFXObj.GetComponent<ParticleSystem>();
                        if (m_snowVFX != null)
                        {
                            m_snowVFX.Stop();
                        }
                    }

                    m_snowAudioSource = m_snowParticles.gameObject.GetComponent<AudioSource>();
                    if (m_snowAudioSource == null)
                    {
                        m_snowAudioSource = m_snowParticles.gameObject.AddComponent<AudioSource>();
                        m_snowAudioSource.volume = 0f;
                        m_snowAudioSource.loop = true;
                    }
                }

                if (m_rainParticles == null)
                {
                    GameObject rainParticles = GameObject.Find("PW_VFX_Rain");
                    if (rainParticles == null)
                    {
                        GameObject prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(GaiaUtils.GetAssetPath("PW_VFX_Rain.prefab"));
                        if (prefabObject != null)
                        {
                            rainParticles = PrefabUtility.InstantiatePrefab(prefabObject) as GameObject;
                        }
                    }

                    if (rainParticles != null)
                    {
                        rainParticles.transform.SetParent(gameObject.transform);
                        m_rainParticles = rainParticles.GetComponent<MeshRenderer>();
                    }
                    else
                    {
                        Debug.LogError("(Gaia Rain Particles) could not be found or Instantiated from the project. It could be missing?");
                    }
                }

                if (m_rainParticles != null)
                {
                    GameObject rainvVFXObj = GameObject.Find("Particle System_Rain");
                    if (rainvVFXObj != null)
                    {
                        m_rainVFX = rainvVFXObj.GetComponent<ParticleSystem>();
                        if (m_rainVFX != null)
                        {
                            m_rainVFX.Stop();
                        }
                    }

                    m_rainAudioSource = m_rainParticles.gameObject.GetComponent<AudioSource>();
                    if (m_rainAudioSource == null)
                    {
                        m_rainAudioSource = m_rainParticles.gameObject.AddComponent<AudioSource>();
                        m_rainAudioSource.volume = 0f;
                        m_rainAudioSource.loop = true;
                    }
                }

                if (m_cloudsMeshRenderer == null)
                {
                    GameObject cloudsParticles = GameObject.Find("Gaia Clouds");
                    if (cloudsParticles == null)
                    {
                        GameObject prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(GaiaUtils.GetAssetPath("Gaia Clouds.prefab"));
                        if (prefabObject != null)
                        {
                            cloudsParticles = PrefabUtility.InstantiatePrefab(prefabObject) as GameObject;
                        }
                    }

                    if (cloudsParticles != null)
                    {
                        m_cloudsMeshRenderer = cloudsParticles;
                        cloudsParticles.transform.SetParent(gameObject.transform);
                        if (PW_VFX_Clouds.Instance != null)
                        {
                            PW_VFX_Clouds.Instance.Initialize();
                        }
#if GAIA_PRO_PRESENT
                        if (GaiaUtils.UsesFloatingPointFix())
                        {
                            if (cloudsParticles.GetComponent<FloatingPointFixParticleSystem>() == null)
                            {
                                cloudsParticles.AddComponent<FloatingPointFixParticleSystem>();
                            }
                        }
#endif
                    }
                    else
                    {
                        Debug.LogError("(Gaia Clouds) could not be found or Instantiated from the project. It could be missing?");
                    }
                }

                if (m_thunderObject == null || m_thunderComponent == null)
                {
                    m_thunderObject = GameObject.Find(GaiaConstants.GaiaThunderObjectName);
                    if (m_thunderObject == null)
                    {
                        if (m_thunderObject == null)
                        {
#if UNITY_EDITOR
                            m_thunderObject = AssetDatabase.LoadAssetAtPath<GameObject>(GaiaUtils.GetAssetPath(GaiaConstants.GaiaThunderPrefabName));
                            if (m_thunderObject != null)
                            {
                                m_thunderObject = PrefabUtility.InstantiatePrefab(m_thunderObject) as GameObject;
                                if (m_thunderObject != null)
                                {
                                    m_thunderObject.transform.SetParent(gameObject.transform);
                                }
                            }
#endif
                        }
                    }

                    if (m_thunderObject != null)
                    {
                        m_thunderComponent = m_thunderObject.GetComponent<ThunderStrike>();
                    }
                }

                if (WindZone == null)
                {
                    WindZone = GetOrCreateWindZone();
                }

                if (m_rainWeatherSettings.m_ambientAudio == null)
                {
                    m_rainWeatherSettings.m_ambientAudio = AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Rain Storm.wav"));
                }

                if (m_snowWeatherSettings.m_ambientAudio == null)
                {
                    m_snowWeatherSettings.m_ambientAudio = AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Winter Blizzard.wav"));
                }

                if (ThunderAudioSources == null)
                {
                    ThunderAudioSources = new List<AudioClip>
                    {
                        AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Distant Thunder Rumble.mp3")),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Long Thunder Rumble.mp3")),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Thunder Crack and Rumble.mp3")),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Thunder Crack to Fade.mp3")),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Thunder Rumble.mp3")),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Thunder Rumble and Crack.mp3"))
                    };
                }
                else
                {
                    ThunderAudioSources = new List<AudioClip>
                    {
                        AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Distant Thunder Rumble.mp3")),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Long Thunder Rumble.mp3")),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Thunder Crack and Rumble.mp3")),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Thunder Crack to Fade.mp3")),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Thunder Rumble.mp3")),
                        AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Thunder Rumble and Crack.mp3"))
                    };
                }

                //Get the player
                if (m_player == null)
                {
                    m_player = GetPlayer();
                }

                //Get the water material
                if (m_waterMaterial == null)
                {
                    m_waterMaterial = GaiaUtils.GetWaterMaterial(GaiaConstants.waterSurfaceObject);
                }

                if (!Application.isPlaying)
                {
                    if (m_rainParticles != null)
                    {
                        m_rainParticles.sharedMaterial.SetFloat("_PW_VFX_Weather_Intensity", 0f);
                    }

                    if (m_snowParticles != null)
                    {
                        m_snowParticles.Stop();
                        m_snowParticleEmission = m_snowParticles.emission;
                        m_snowParticleEmission.rateOverTime = 0;
                    }

                    if (m_snowVFX != null)
                    {
                        m_snowVFX.Stop();
                        m_snowVFXEmission = m_snowVFX.emission;
                        m_snowVFXEmission.rateOverTime = 0;
                    }
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Loading up content had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Sets up the default values for the system
        /// </summary>
        public void SetupDefaults()
        {
            try
            {
                IsNetworkSynced = true;
                WindSpeed = 0.35f;
                WindTurbulence = 0.35f;
                WindFrequency = 0.2f;
                WindMultiplier = 1f;
                MinTerrainWind = 0.1f;
                MaxTerrainWind = 1.0f;
                RunInEditor = false;
                if (GaiaAudioManager.Instance != null)
                {
                    GaiaAudioManager = GaiaAudioManager.Instance;
                    GlobalVolume = GaiaAudioManager.Instance.m_masterVolume;
                    RainVolume = GaiaAudioManager.Instance.m_rainVolume;
                    SnowVolume = GaiaAudioManager.Instance.m_snowVolume;
                    m_thunderVolume = GaiaAudioManager.Instance.m_thunderVolume;
                }
                else
                {
                    GlobalVolume = 0.5f;
                    RainVolume = 1f;
                    SnowVolume = 1f;
                    m_thunderVolume = 1f;
                }

                SystemStepSize = 0.05f;

                m_weatherFadeDuration = 30f;
                m_modifyFog = true;
                m_modifySkybox = true;
                m_modifyPostProcessing = true;
                m_modifyWind = true;
                m_modifySun = true;

                EnableSeasons = true;
                Season = 1.5f;
                m_seasonTransitionDuration = 2000f;
                SeasonWinterTint = GaiaUtils.GetColorFromHTML("D3EFFF");
                SeasonSpringTint = GaiaUtils.GetColorFromHTML("BCFF96");
                SeasonSummerTint = GaiaUtils.GetColorFromHTML("FFB960");
                SeasonAutumnTint = GaiaUtils.GetColorFromHTML("FFE8E6");

                EnableClouds = true;
                CloudOffset = 50;
                CloudAmbientColor = GaiaUtils.GetColorFromHTML("5DC8FF");
                if (m_player != null)
                {
                    Camera camera = m_player.GetComponent<Camera>();
                    if (camera != null)
                    {
                        float distance = camera.farClipPlane;
                        if (distance > 1000 && distance < 2000)
                        {
                            CloudScale = 2f;
                        }
                        else if (distance > 2000 && distance < 3000)
                        {
                            CloudScale = 3f;
                        }
                        else if (distance > 3000 && distance < 4000)
                        {
                            CloudScale = 4f;
                        }
                        else
                        {
                            CloudScale = 5f;
                        }
                    }
                }
                else
                {
                    CloudScale = 5f;
                }
                CloudBrightness = 1f;
                CloudFade = 250f;
                CloudTilingAndWind = new Vector4(1.5f, 1f, 1f, -16f);
                CloudOpacity = new Vector4(0.2f, 1f, 0.45f, 0.75f);
                CloudRotationSpeedLow = 0.3f;
                CloudRotationSpeedMiddle = 0.2f;
                CloudRotationSpeedFar = 0.1f;

                if (GaiaGlobal.Instance != null)
                {
                    GaiaGlobal.Instance.UpdateTimeOfDayMode(GaiaConstants.TimeOfDayStartingMode.Day, false);
                    GaiaGlobal.Instance.UpdateGaiaTimeOfDay(true);
#if UNITY_EDITOR
                    EditorUtility.SetDirty(GaiaGlobal.Instance);
#endif
                }

                EnableSnow = true;
                SnowCoverAlwaysEnabled = false;
                Terrain terrain = Terrain.activeTerrain;
                if (terrain != null)
                {
                    int numberTerrains = Terrain.activeTerrains.Length;
                    int searchQuality = 32;
                    float maxHeight = 0f;
                    for (int terrainIdx = 0; terrainIdx < numberTerrains; terrainIdx++)
                    {
                        terrain = Terrain.activeTerrains[terrainIdx];
                        Vector3 terrainSize = terrain.terrainData.size;

                        for (int row = 0; row < searchQuality; ++row)
                        {
                            for (int columns = 0; columns < searchQuality; ++columns)
                            {
                                Vector3 newPosition = Vector3.zero;
                                newPosition.x = ((columns + 1) * terrainSize.x / searchQuality) - terrainSize.x / searchQuality / 2f + terrain.transform.position.x;
                                newPosition.z = ((row + 1) * terrainSize.z / searchQuality) - terrainSize.z / searchQuality / 2f + terrain.transform.position.z;
                                float sampledHeight = terrain.SampleHeight(newPosition);
                                if (sampledHeight > maxHeight)
                                {
                                    maxHeight = sampledHeight;
                                }
                            }
                        }
                    }

                    if (PWS_WaterSystem.Instance != null)
                    {
                        SnowingHeight = PWS_WaterSystem.Instance.SeaLevel + 10f;
                    }
                    else
                    {
                        SnowingHeight = 60f;
                    }

                    SnowHeight = Mathf.RoundToInt(maxHeight) / 1.7f;
                    PermanentSnowHeight = Mathf.RoundToInt(maxHeight) / 1.7f;
                }
                else
                {
                    SnowHeight = 650f;
                    PermanentSnowHeight = 650f;
                }
                SnowFadeHeight = 50f;
                m_snowMode = SnowMode.RandomChance;
                m_snowWeatherSettings.m_chance = 0.65f;
                m_snowWeatherSettings.m_channelSelection = ChannelSelection.R;
                m_snowWeatherSettings.m_durationMinWaitTime = 120f;
                m_snowWeatherSettings.m_durationMaxWaitTime = 700f;
                m_snowWeatherSettings.m_minWaitTime = 120f;
                m_snowWeatherSettings.m_maxWaitTime = 500f;

                EnableRain = true;
                m_rainHeight = 400f;
                m_rainMode = RainMode.RandomChance;
                m_rainWeatherSettings.m_chance = 0.7f;
                m_rainWeatherSettings.m_channelSelection = ChannelSelection.G;
                m_rainWeatherSettings.m_durationMinWaitTime = 120f;
                m_rainWeatherSettings.m_durationMaxWaitTime = 700f;
                m_rainWeatherSettings.m_minWaitTime = 120f;
                m_rainWeatherSettings.m_maxWaitTime = 500f;

                m_enableThunder = true;
                m_thunderChance = 0.5f;
                ThunderLightColor = GaiaUtils.GetColorFromHTML("00A1FF");
                ThunderLightIntensity = 2f;
                ThunderLightRadius = 500f;

                m_sunLight.shadowStrength = 0.8f;
                m_moonLight.shadowStrength = 0.8f;

                RenderSettings.fog = true;
                if (GaiaGlobal.Instance != null)
                {
                    GaiaGlobal.Instance.UpdateTimeOfDayMode(GaiaConstants.TimeOfDayStartingMode.Day, false);
                    GaiaGlobal.Instance.GaiaTimeOfDayValue.m_todDayTimeScale = 1;
                    GaiaGlobal.Instance.GaiaTimeOfDayValue.m_todEnabled = false;
                }

                if (m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    GameObject staticLighting = GameObject.Find("StaticLightingSky");
                    if (staticLighting != null)
                    {
                        DestroyImmediate(staticLighting);
                    }
                }

                Initialize();
                DeInitialize();
            }
            catch (Exception e)
            {
                Debug.LogError("Setup defaults had a issue " + e.Message + " This came from " + e.StackTrace);
            }
        }
        /// <summary>
        /// Reverts back to defaults
        /// </summary>
        public void RevertToDefaults()
        {
            m_renderPipeline = GaiaUtils.GetActivePipeline();
            LoadUpContent();
            SetupDefaults();
            Initialize();
            DeInitialize();
            InitializeShaderSetup();
        }
        /// <summary>
        /// If editor isn't set to run
        /// </summary>
        /// <returns></returns>
        public bool CheckEditorRuntimeValues()
        {
            try
            {
                bool noChanges = false;
                if (RunInEditor)
                {
                    UpdateAllSystems(false);
                }
                else
                {
                    if (Shader.GetGlobalFloat(m_globalSnowIntensity) > 0)
                    {
                        Shader.SetGlobalFloat(m_globalSnowIntensity, 0f);
                        noChanges = false;
                    }
                    else
                    {
                        noChanges = true;
                    }

                    if (Shader.GetGlobalColor(m_globalSeasonTint) != Color.black)
                    {
                        Shader.SetGlobalColor(m_globalSeasonTint, Color.black);
                        noChanges = false;
                    }
                    else
                    {
                        noChanges = true;
                    }

                    if (Shader.GetGlobalFloat(m_globalCoverLayer1FadeStart) > 0)
                    {
                        Shader.SetGlobalFloat(m_globalCoverLayer1FadeStart, 0f);
                        noChanges = false;
                    }
                    else
                    {
                        noChanges = true;
                    }

                    if (Shader.GetGlobalFloat(m_globalCoverLayer1FadeDist) > 0)
                    {
                        Shader.SetGlobalFloat(m_globalCoverLayer1FadeDist, 0f);
                        noChanges = false;
                    }
                    else
                    {
                        noChanges = true;
                    }

#if CTS_PRESENT
                    if (m_cTSWeatherManager == null)
                    {
                        m_cTSWeatherManager = GetOrCreateWeatherManager();
                    }
                    if (m_cTSWeatherManager != null)
                    {

                        if (m_cTSWeatherManager.SnowPower > 0)
                        {
                            m_cTSWeatherManager.SnowPower = 0f;
                        }

                        if (m_cTSWeatherManager.SnowMinHeight > 0)
                        {
                            m_cTSWeatherManager.SnowMinHeight = 0f;
                        }
                    }
#endif
                }

                return noChanges;
            }
            catch (Exception e)
            {
                Debug.LogError("Checking editor runtime values had a issue " + e.Message + " This came from " + e.StackTrace);
                return false;
            }
        }
        /// <summary>
        /// Get the active season (Returns names: Winter, Spring, Autumn, Summer)
        /// </summary>
        /// <returns></returns>
        public string GetSeason()
        {
            string season = "";

            if (Season > 0 && Season < 1)
            {
                season = "Winter";
            }
            else if (Season > 1 && Season < 2)
            {
                season = "Spring";
            }
            else if (Season > 2 && Season < 3)
            {
                season = "Summer";
            }
            else
            {
                season = "Autumn";
            }

            return season;
        }

        #endregion
        #region Public Static Functions

        /// <summary>
        /// Adds global wind shader script to the scene
        /// </summary>
        public static ProceduralWorldsGlobalWeather AddGlobalWeather(GaiaConstants.GaiaGlobalWindType windType, GaiaLightingProfileValues profileValues)
        {
            try
            {
                GameObject windShderObject = GameObject.Find(GaiaConstants.gaiaWeatherObject);
                if (windShderObject == null)
                {
                    windShderObject = new GameObject(GaiaConstants.gaiaWeatherObject);
                }

                ProceduralWorldsGlobalWeather globalWeather = windShderObject.GetComponent<ProceduralWorldsGlobalWeather>();
                if (globalWeather == null)
                {
                    globalWeather = windShderObject.AddComponent<ProceduralWorldsGlobalWeather>();
                }

                if (GaiaUtils.CheckIfSceneProfileExists())
                {
                    SceneProfile profile = GaiaGlobal.Instance.SceneProfile;
                    if (profile != null)
                    {
                        GaiaLightingProfileValues values = profile.m_lightingProfiles[profile.m_selectedLightingProfileValuesIndex];
                        if (values != null && values.m_typeOfLighting == profileValues.m_typeOfLighting)
                        {
                            values.m_pwSkyWindData.WindDirection = 0f;
                            values.m_pwSkyWindData.WindSpeed = 0.35f;
                            values.m_pwSkyWindData.WindTurbulence = 0.35f;
                            values.m_pwSkyWindData.WindFrequency = 0.2f;
                            values.m_pwSkyWindData.WindMultiplier = 1f;
                        }
                    }
                }

#if GAIA_PRO_PRESENT
                profileValues.m_pwSkyAtmosphereData.Load(globalWeather);
                profileValues.m_pwSkyCloudData.Load(globalWeather);
                profileValues.m_pwSkySeasonData.Load(globalWeather);
                profileValues.m_pwSkyWeatherData.Load(globalWeather);
                profileValues.m_pwSkyWindData.Load(globalWeather);
#endif
                globalWeather.RevertToDefaults();
                globalWeather.CheckEditorRuntimeValues();

                GameObject parent = GameObject.Find(GaiaConstants.gaiaRuntimeObject);
                if (parent != null)
                {
                    windShderObject.transform.SetParent(parent.transform);
                }
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (!UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().isDirty)
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                    }
                }
#endif
                return globalWeather;
            }
            catch (Exception e)
            {
                Debug.LogError("Creating/Updating glboal weather system had a issue " + e.Message + " This came from " + e.StackTrace);
                return null;
            }
        }
        /// <summary>
        /// Saves all the changes to the profile
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="weather"></param>
        public static void SaveValuesToLightProfile(GaiaLightingProfileValues profile, ProceduralWorldsGlobalWeather weather)
        {
            if (profile == null || weather == null)
            {
                return;
            }

#if GAIA_PRO_PRESENT
            profile.m_pwSkyWindData.Save(weather);
            profile.m_pwSkySeasonData.Save(weather);
            profile.m_pwSkyCloudData.Save(weather);
            profile.m_pwSkyWeatherData.Save(weather);
            profile.m_pwSkyAtmosphereData.Save(weather);
#endif
        }
        /// <summary>
        /// Removes the global wind shader from the scene
        /// </summary>
        public static void RemoveGlobalWindShader()
        {
            GameObject windShaderObject = GameObject.Find(GaiaConstants.gaiaWeatherObject);
            if (windShaderObject != null)
            {
                Object.DestroyImmediate(windShaderObject);
            }

            Light moonLight = GaiaUtils.GetMainMoonLight();
            if (moonLight != null)
            {
                Object.DestroyImmediate(moonLight.gameObject);
            }
        }
        /// <summary>
        /// Used to enable network synced. This should be enable for the host. If you are a client DisableNetworkSynced() should be called on new clients joining
        /// </summary>
        public static void EnableNetworkSynced()
        {
            if (Instance != null)
            {
                Instance.IsNetworkSynced = true;
            }
        }
        /// <summary>
        /// Used to disabled network sync. This should be called if you are a client connecting to a host. If you're the host EnableNetworkSynced() should be called on host connecting/creating room
        /// </summary>
        public static void DisableNetworkSynced()
        {
            if (Instance != null)
            {
                Instance.IsNetworkSynced = false;
                if (Instance.IsRaining)
                {
                    Instance.StopRain();
                }
                if (Instance.IsSnowing)
                {
                    Instance.StopSnow();
                }
            }
        }
        /// <summary>
        /// Get player transform
        /// </summary>
        /// <returns></returns>
        public static Transform GetPlayer()
        {
            try
            {
                Transform player = null;
                m_mainCameras = FindObjectsOfType<Camera>();
                if (m_mainCameras.Length > 0)
                {
                    foreach (Camera camera in m_mainCameras)
                    {
                        if (camera.CompareTag("MainCamera"))
                        {
                            player = camera.transform;
                            break;
                        }
                        else if (camera.CompareTag("Player"))
                        {
                            player = camera.transform;
                            break;
                        }
                        else if (camera.CompareTag("Camera"))
                        {
                            player = camera.transform;
                            break;
                        }
                        else
                        {
                            player = camera.transform;
                            break;
                        }
                    }
                }

                m_mainCameras = new Camera[0];

                return player;
            }
            catch (Exception e)
            {
                Debug.LogError("Getting the player had a issue " + e.Message + " This came from " + e.StackTrace);
                return null;
            }
        }

        #endregion
    }
}
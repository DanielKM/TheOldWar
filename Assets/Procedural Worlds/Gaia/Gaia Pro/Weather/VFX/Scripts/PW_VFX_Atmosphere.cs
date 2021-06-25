using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Gaia
{
    [ExecuteAlways]
    public class PW_VFX_Atmosphere : MonoBehaviour
    {
        public static PW_VFX_Atmosphere Instance
        {
            get { return m_instance; }
        }
        [SerializeField]
        private static PW_VFX_Atmosphere m_instance;

        #region Public Variables

        public float AdditionalFogDistanceLinear = 0f;
        public float AdditionalFogDistanceExponential = 0f;
        public float m_sunRotation = 0f;
        public Material SkyboxMaterial;
        public Light SunLight;
        public Transform SunAxisTransform;
        public Transform EarthAxisTransform;
        public Gradient TimeOfDaySunColor;
        public AnimationCurve SunBrightnessIntensity;
        public Gradient TimeOfDayFogColor;
        public AnimationCurve TimeOfDayShadowStrength;
        public Gradient TimeOfDaySkyColor;
        public Gradient TimeOfDayEqutorColor;
        public Gradient TimeOfDayGroundColor;
        public AnimationCurve CloudsHightLevelDensity;
        public AnimationCurve CloudsHightLevelThickness;
        public AnimationCurve CloudsHightLevelSpeed;
        public AnimationCurve CloudOpacity;
        public AnimationCurve AmbientIntensity;
        public AnimationCurve AtmosphereThickness;
        public AnimationCurve FogDensityColor;
        public AnimationCurve FogStartDistance;
        public AnimationCurve FogEndDistance;
        public AnimationCurve SunSize;
        public AnimationCurve SunSizeConvergence;
        public Gradient SkyboxTint;
        public AnimationCurve SkyboxExposure;
        public AnimationCurve TODSkyboxFogHeight;
        public AnimationCurve TODSkyboxFogGradient;

        //HDRP
        public Gradient HDRPGroundColor;
        public Gradient HDRPFogAlbedo;
        public AnimationCurve HDRPFogBaseHeight;
        public AnimationCurve HDRPFogAnisotropy;
        public AnimationCurve HDRPFogLightProbeDimmer;
        public AnimationCurve HDRPFogDepthExtent;
#if HDPipeline
        public VolumeProfile HDRPVolumeProfile;
        public HDAdditionalLightData HDRPLightData;
#endif

#endregion

        #region Private Variables

        private Vector3 FinalSunRotation;
        [HideInInspector]
        public List<Material> CloudLayerMaterials = new List<Material>();

        [SerializeField]
        [HideInInspector]
        private GameObject SeasonAxis;
        public bool m_weatherManagerExists;
        private bool m_skyboxMaterialExists;
        private bool m_underwaterExists;
        private bool m_gradientOrCurvesExists;
        private bool m_sunLightExists;
        [SerializeField]
        private float m_currentTime;
        [SerializeField]
        private ProceduralWorldsGlobalWeather WeatherSystem;

        #endregion

        #region Unity Functions

        private void Start()
        {
            m_instance = this;
            Initialize();
            UpdateSystem();
        }
        private void OnEnable()
        {
            m_instance = this;
            Initialize();
            if (CloudLayerMaterials.Count == 0)
            {
                CloudLayerMaterials = GaiaUtils.GetCloudLayerMaterials("PW_VFX_SkyDome", GaiaShaderID.m_checkNameSpace);
            }
            
        }
        private void LateUpdate()
        {
            if (!m_weatherManagerExists)
            {
                Debug.LogError("Gaia Weather System could not be found in the scene.");
                return;
            }

            m_currentTime = GaiaGlobal.GetTimeOfDayMainValue();
            if (WeatherSystem.m_sunLight != null)
            {
                WeatherSystem.m_sunLight.transform.eulerAngles = new Vector3(270f + Mathf.Lerp(0f, 360f, m_currentTime), m_sunRotation, 0f);
            }

            if (WeatherSystem.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
#if HDPipeline
                if (HDRPVolumeProfile == null)
                {
                    HDRPVolumeProfile = GaiaUtils.GetVolumeProfile(false, "Environment", "Processing");
                }
                if (HDRPVolumeProfile == null)
                {
                    return;
                }
#endif
            }
            else
            {
                if (SkyboxMaterial == null)
                {
                    return;
                }
            }

            //Update lighting
            if (Application.isPlaying)
            {
                AmbientCalculation();
            }
            else
            {
                UpdateSystem();
            }
        }

        #endregion

        #region Functions Private

        /// <summary>
        /// Initialze the systems
        /// </summary>
        private void Initialize()
        {
            if (SunLight == null)
            {
                SunLight = GaiaUtils.GetMainDirectionalLight();
            }

            if (SkyboxMaterial == null)
            {
                SkyboxMaterial = RenderSettings.skybox;
            }
            if (SkyboxMaterial != null)
            {
                m_skyboxMaterialExists = true;
                if (SkyboxMaterial.shader != Shader.Find(GaiaShaderID.m_pwSkySkyboxShader))
                {
                    SkyboxMaterial = RenderSettings.skybox;
                }

                SkyboxMaterial.SetInt(GaiaShaderID.m_unitySkyboxSunDisk, (int)GaiaConstants.ProceduralSkySunTypes.HighQuality);
                SkyboxMaterial.EnableKeyword(GaiaShaderID.m_unitySunQualityKeyword);
                List<string> keywords = new List<string>
                {
                    GaiaShaderID.m_unitySunQualityKeyword
                };
                SkyboxMaterial.shaderKeywords = keywords.ToArray();
            }
            else
            {
                m_skyboxMaterialExists = false;
            }

            if (EarthAxisTransform == null)
            {
                EarthAxisTransform = GameObject.Find("EarthAxis").transform;
            }
            if (SunAxisTransform == null)
            {
                SunAxisTransform = GameObject.Find("SunAxis").transform;
            }

            if (SeasonAxis == null)
            {
                SeasonAxis = GameObject.Find("SeasonAxis");
            }
            if (SeasonAxis != null)
            {
                SeasonAxis.hideFlags = HideFlags.HideInHierarchy;
            }

            if (GaiaGlobal.Instance != null)
            {
                SceneProfile sp = GaiaGlobal.Instance.SceneProfile;
                if (sp != null)
                {
                    if (sp.m_selectedLightingProfileValuesIndex < sp.m_lightingProfiles.Count - 1)
                    {
                        m_sunRotation = sp.m_lightingProfiles[sp.m_selectedLightingProfileValuesIndex].m_pwSkySunRotation;
                    }
                }
            }

            WeatherSystem = ProceduralWorldsGlobalWeather.Instance;
            m_weatherManagerExists = ProceduralWorldsGlobalWeather.Instance;
            m_underwaterExists = GaiaUnderwaterEffects.Instance;

            m_gradientOrCurvesExists = CheckIfGradientsAndAnimationCruvesExist();
        }
        /// <summary>
        /// Updates all the curves and gradients in the scene
        /// </summary>
        private void AmbientCalculation()
        {
            bool update = !WeatherSystem.IsRaining;
            if (WeatherSystem.IsSnowing)
            {
                update = false;
            }

            if (update)
            {
                m_sunLightExists = SetSunLight(WeatherSystem.CheckIsNight());
                if (WeatherSystem.DoesAtmosphereNeedUpdate)
                {
                    if (WeatherSystem.m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
                    {
                        if (m_gradientOrCurvesExists)
                        {
                            if (m_skyboxMaterialExists)
                            {
                                if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_unitySkyboxGroundColor))
                                {
                                    SkyboxMaterial.SetColor(GaiaShaderID.m_unitySkyboxGroundColor, TimeOfDayFogColor.Evaluate(m_currentTime));
                                }
                                if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_unitySkyboxAtmosphereThickness))
                                {
                                    SkyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxAtmosphereThickness, AtmosphereThickness.Evaluate(m_currentTime));
                                }
                                if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_unitySkyboxSunSize))
                                {
                                    SkyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxSunSize, SunSize.Evaluate(m_currentTime));
                                }
                                if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_unitySkyboxSunSizeConvergence))
                                {
                                    SkyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxSunSizeConvergence, SunSizeConvergence.Evaluate(m_currentTime));
                                }
                                if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_unitySkyboxTint))
                                {
                                    SkyboxMaterial.SetColor(GaiaShaderID.m_unitySkyboxTint, SkyboxTint.Evaluate(m_currentTime));
                                }
                                if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_unitySkyboxExposure))
                                {
                                    SkyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxExposure, SkyboxExposure.Evaluate(m_currentTime));
                                }
                                if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_pwSkyFogHeight))
                                {
                                    SkyboxMaterial.SetFloat(GaiaShaderID.m_pwSkyFogHeight, Mathf.Clamp(TODSkyboxFogHeight.Evaluate(m_currentTime), -1000f, 8000f));
                                }
                                if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_pwSkyFogGradient))
                                {
                                    SkyboxMaterial.SetFloat(GaiaShaderID.m_pwSkyFogGradient, Mathf.Clamp01(TODSkyboxFogGradient.Evaluate(m_currentTime)));
                                }
                            }
                            if (m_sunLightExists)
                            {
                                if (m_weatherManagerExists)
                                {
                                    SunLight.color = TimeOfDaySunColor.Evaluate(m_currentTime);
                                    SunLight.shadowStrength = Mathf.Clamp01(TimeOfDayShadowStrength.Evaluate(m_currentTime));
                                    SunLight.intensity = SunBrightnessIntensity.Evaluate(m_currentTime);
                                }
                            }
                            if (m_underwaterExists)
                            {
                                if (!GaiaUnderwaterEffects.Instance.IsUnderwater)
                                {
                                    RenderSettings.fogColor = TimeOfDayFogColor.Evaluate(m_currentTime);
                                    if (AdditionalFogDistanceExponential < 0)
                                    {
                                        RenderSettings.fogDensity = FogDensityColor.Evaluate(m_currentTime) - Mathf.Abs(AdditionalFogDistanceExponential);
                                    }
                                    else
                                    {
                                        RenderSettings.fogDensity = FogDensityColor.Evaluate(m_currentTime) + AdditionalFogDistanceExponential;
                                    }
                                    RenderSettings.fogStartDistance = FogStartDistance.Evaluate(m_currentTime);
                                    if (AdditionalFogDistanceLinear < 0)
                                    {
                                        RenderSettings.fogEndDistance = FogEndDistance.Evaluate(m_currentTime) - Mathf.Abs(AdditionalFogDistanceLinear);
                                    }
                                    else
                                    {
                                        RenderSettings.fogEndDistance = FogEndDistance.Evaluate(m_currentTime) + AdditionalFogDistanceLinear;
                                    }
                                }
                            }

                            RenderSettings.ambientMode = AmbientMode.Trilight;
                            switch (RenderSettings.ambientMode)
                            {
                                case AmbientMode.Trilight:
                                {
                                    RenderSettings.ambientSkyColor = TimeOfDaySkyColor.Evaluate(m_currentTime) * AmbientIntensity.Evaluate(m_currentTime);
                                    RenderSettings.ambientEquatorColor = TimeOfDayEqutorColor.Evaluate(m_currentTime) * AmbientIntensity.Evaluate(m_currentTime);
                                    RenderSettings.ambientGroundColor = TimeOfDayGroundColor.Evaluate(m_currentTime) * AmbientIntensity.Evaluate(m_currentTime);
                                    break;
                                }
                                case AmbientMode.Flat:
                                {
                                    RenderSettings.ambientSkyColor = TimeOfDaySkyColor.Evaluate(m_currentTime) * AmbientIntensity.Evaluate(m_currentTime);
                                    break;
                                }
                                case AmbientMode.Skybox:
                                    RenderSettings.ambientIntensity = AmbientIntensity.Evaluate(m_currentTime);
                                    break;
                            }

                            if (CloudLayerMaterials.Count > 0)
                            {
                                Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeFogColor, TimeOfDayFogColor.Evaluate(m_currentTime));
                                Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeSunColor, TimeOfDaySunColor.Evaluate(m_currentTime));
                                Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeFinalCloudColor, TimeOfDayEqutorColor.Evaluate(m_currentTime));
                                Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeFinalSkyColor, TimeOfDaySkyColor.Evaluate(m_currentTime) * TimeOfDayFogColor.Evaluate(m_currentTime));
                                Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightDensity, CloudsHightLevelDensity.Evaluate(m_currentTime));
                                Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightThickness, CloudsHightLevelThickness.Evaluate(m_currentTime));
                                Shader.SetGlobalFloat(GaiaShaderID.m_cloudOpacity, CloudOpacity.Evaluate(m_currentTime));
                                Shader.SetGlobalFloat(GaiaShaderID.m_cloudSpeed, CloudsHightLevelSpeed.Evaluate(m_currentTime));
                            }
                        }
                    }
                    else
                    {
#if HDPipeline
                        if (m_gradientOrCurvesExists)
                        {
                            if (HDRPLightData != null)
                            {
                                HDRPLightData.SetColor(TimeOfDaySunColor.Evaluate(m_currentTime));
                                HDRPLightData.SetIntensity(GaiaUtils.SetHDRPFloat(SunBrightnessIntensity.Evaluate(m_currentTime), 2.14f));
                                HDRPLightData.color = TimeOfDaySunColor.Evaluate(m_currentTime);
                            }
                            if (HDRPVolumeProfile.TryGet(out PhysicallyBasedSky physicallyBasedSky))
                            {
                                if (HDRPVolumeProfile.TryGet(out Fog volumetricFog))
                                {
                                    volumetricFog.tint.value = TimeOfDayFogColor.Evaluate(m_currentTime);  
                                    volumetricFog.albedo.value = HDRPFogAlbedo.Evaluate(m_currentTime);
                                    physicallyBasedSky.groundTint.value = HDRPGroundColor.Evaluate(m_currentTime);
                                    physicallyBasedSky.exposure.value = SkyboxExposure.Evaluate(m_currentTime);
                                    volumetricFog.anisotropy.value = HDRPFogAnisotropy.Evaluate(m_currentTime);
                                    volumetricFog.baseHeight.value = HDRPFogBaseHeight.Evaluate(m_currentTime);
                                    volumetricFog.depthExtent.value = HDRPFogDepthExtent.Evaluate(m_currentTime);
                                    volumetricFog.globalLightProbeDimmer.value = HDRPFogLightProbeDimmer.Evaluate(m_currentTime);
                                }
                            }
                            if (CloudLayerMaterials.Count > 0)
                            {
                                Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeFogColor, TimeOfDayFogColor.Evaluate(m_currentTime));
                                Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeSunColor, TimeOfDaySunColor.Evaluate(m_currentTime));
                                Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeFinalCloudColor, TimeOfDayEqutorColor.Evaluate(m_currentTime));
                                Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeFinalSkyColor, TimeOfDaySkyColor.Evaluate(m_currentTime) * TimeOfDayFogColor.Evaluate(m_currentTime));
                                Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightDensity, CloudsHightLevelDensity.Evaluate(m_currentTime));
                                Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightThickness, CloudsHightLevelThickness.Evaluate(m_currentTime));
                                Shader.SetGlobalFloat(GaiaShaderID.m_cloudOpacity, CloudOpacity.Evaluate(m_currentTime));
                                Shader.SetGlobalFloat(GaiaShaderID.m_cloudSpeed, CloudsHightLevelSpeed.Evaluate(m_currentTime));
                            }
                        }
#endif
                    }

                    WeatherSystem.DoesAtmosphereNeedUpdate = false;
                }
            }
        }
        /// <summary>
        /// Sets the correct light if it's day or night
        /// </summary>
        /// <param name="isNight"></param>
        private bool SetSunLight(bool isNight)
        {
            bool exists = false;
            if (m_weatherManagerExists)
            {
                if (isNight)
                {
                    SunLight = WeatherSystem.m_moonLight;
                }
                else
                {
                    SunLight = WeatherSystem.m_sunLight;
                }

#if HDPipeline
                HDRPLightData = GaiaUtils.GetOrAddHDRPLightData(SunLight);
                if (HDRPLightData != null)
                {
                    GaiaUtils.CheckHDRPLightRenderMode(HDRPLightData, HDRPLightData.intensity);
                }
#endif
                if (SunLight != null)
                {
                    exists = true;
                }
            }

            return exists;
        }
        /// <summary>
        /// Checks to make sure all gradients and animation curves are present
        /// </summary>
        /// <returns></returns>
        private bool CheckIfGradientsAndAnimationCruvesExist()
        {
            if (TimeOfDaySunColor == null)
            {
                return false;
            }
            if (SunBrightnessIntensity == null)
            {
                return false;
            }
            if (TimeOfDayFogColor == null)
            {
                return false;
            }
            if (TimeOfDayShadowStrength == null)
            {
                return false;
            }
            if (TimeOfDaySkyColor == null)
            {
                return false;
            }
            if (TimeOfDayEqutorColor == null)
            {
                return false;
            }
            if (TimeOfDayGroundColor == null)
            {
                return false;
            }
            if (CloudsHightLevelDensity == null)
            {
                return false;
            }
            if (CloudsHightLevelThickness == null)
            {
                return false;
            }
            if (CloudsHightLevelSpeed == null)
            {
                return false;
            }
            if (CloudOpacity == null)
            {
                return false;
            }
            if (AmbientIntensity == null)
            {
                return false;
            }
            if (AtmosphereThickness == null)
            {
                return false;
            }
            if (FogDensityColor == null)
            {
                return false;
            }
            if (FogStartDistance == null)
            {
                return false;
            }
            if (FogEndDistance == null)
            {
                return false;
            }
            if (SunSize == null)
            {
                return false;
            }
            if (SunSizeConvergence == null)
            {
                return false;
            }
            if (SkyboxTint == null)
            {
                return false;
            }
            if (SkyboxExposure == null)
            {
                return false;
            }
            if (TODSkyboxFogHeight == null)
            {
                return false;
            }
            if (TODSkyboxFogGradient == null)
            {
                return false;
            }
            if (HDRPGroundColor == null)
            {
                return false;
            }
            if (HDRPFogAlbedo == null)
            {
                return false;
            }
            if (HDRPFogBaseHeight == null)
            {
                return false;
            }
            if (HDRPFogAnisotropy == null)
            {
                return false;
            }
            if (HDRPFogLightProbeDimmer == null)
            {
                return false;
            }
            if (HDRPFogDepthExtent == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Updates the sun rotation on the Y axis
        /// </summary>
        /// <param name="value"></param>
        public void UpdateSunRotationY(float value)
        {
            m_sunRotation = value;
        }
        /// <summary>
        /// Applies an update to all the systems
        /// </summary>
        public void UpdateSystem()
        {
            if (!Application.isPlaying)
            {
                m_gradientOrCurvesExists = CheckIfGradientsAndAnimationCruvesExist();
            }

            if (!m_weatherManagerExists)
            {
                return;
            }

            if (WeatherSystem.IsRaining || WeatherSystem.IsSnowing)
            {
                return;
            }

            m_sunLightExists = SetSunLight(WeatherSystem.CheckIsNight());
            if (WeatherSystem.m_renderPipeline == GaiaConstants.EnvironmentRenderer.BuiltIn || WeatherSystem.m_renderPipeline == GaiaConstants.EnvironmentRenderer.Universal)
            {
                if (SkyboxMaterial == null)
                {
                    return;
                }

                if (m_gradientOrCurvesExists)
                {
                    if (m_sunLightExists)
                    {
                        SunLight.intensity = SunBrightnessIntensity.Evaluate(m_currentTime);
                        SunLight.color = TimeOfDaySunColor.Evaluate(m_currentTime);
                        SunLight.shadowStrength = Mathf.Clamp01(TimeOfDayShadowStrength.Evaluate(m_currentTime));
                    }

                    RenderSettings.fogColor = TimeOfDayFogColor.Evaluate(m_currentTime);
                    if (AdditionalFogDistanceExponential < 0)
                    {
                        RenderSettings.fogDensity = FogDensityColor.Evaluate(m_currentTime) - Mathf.Abs(AdditionalFogDistanceExponential);
                    }
                    else
                    {
                        RenderSettings.fogDensity = FogDensityColor.Evaluate(m_currentTime) + AdditionalFogDistanceExponential;
                    }
                    RenderSettings.fogStartDistance = FogStartDistance.Evaluate(m_currentTime);
                    if (AdditionalFogDistanceLinear < 0)
                    {
                        RenderSettings.fogEndDistance = FogEndDistance.Evaluate(m_currentTime) - Mathf.Abs(AdditionalFogDistanceLinear);
                    }
                    else
                    {
                        RenderSettings.fogEndDistance = FogEndDistance.Evaluate(m_currentTime) + AdditionalFogDistanceLinear;
                    }
                }
                RenderSettings.ambientMode = AmbientMode.Trilight;
                switch (RenderSettings.ambientMode)
                {
                    case AmbientMode.Trilight:
                    {
                        if (m_gradientOrCurvesExists)
                        {
                            RenderSettings.ambientSkyColor = TimeOfDaySkyColor.Evaluate(m_currentTime) * AmbientIntensity.Evaluate(m_currentTime);
                            RenderSettings.ambientEquatorColor = TimeOfDayEqutorColor.Evaluate(m_currentTime) * AmbientIntensity.Evaluate(m_currentTime);
                            RenderSettings.ambientGroundColor = TimeOfDayGroundColor.Evaluate(m_currentTime) * AmbientIntensity.Evaluate(m_currentTime);
                        }
                        break;
                    }
                    case AmbientMode.Flat:
                    {
                        if (m_gradientOrCurvesExists)
                        {
                            RenderSettings.ambientSkyColor = TimeOfDaySkyColor.Evaluate(m_currentTime) * AmbientIntensity.Evaluate(m_currentTime);
                        }
                        break;
                    }
                    default:
                        RenderSettings.ambientIntensity = AmbientIntensity.Evaluate(m_currentTime);
                        break;
                }
                if (m_skyboxMaterialExists)
                {
                    if (m_gradientOrCurvesExists)
                    {
                        if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_unitySkyboxGroundColor))
                        {
                            SkyboxMaterial.SetColor(GaiaShaderID.m_unitySkyboxGroundColor, TimeOfDayFogColor.Evaluate(m_currentTime));
                        }
                        if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_unitySkyboxAtmosphereThickness))
                        {
                            SkyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxAtmosphereThickness, AtmosphereThickness.Evaluate(m_currentTime));
                        }
                        if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_unitySkyboxSunSize))
                        {
                            SkyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxSunSize, SunSize.Evaluate(m_currentTime));
                        }
                        if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_unitySkyboxSunSizeConvergence))
                        {
                            SkyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxSunSizeConvergence, SunSizeConvergence.Evaluate(m_currentTime));
                        }
                        if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_unitySkyboxExposure))
                        {
                            SkyboxMaterial.SetFloat(GaiaShaderID.m_unitySkyboxExposure, SkyboxExposure.Evaluate(m_currentTime));
                        }
                        if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_unitySkyboxTint))
                        {
                            SkyboxMaterial.SetColor(GaiaShaderID.m_unitySkyboxTint, SkyboxTint.Evaluate(m_currentTime));
                        }
                        if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_pwSkyFogHeight))
                        {
                            SkyboxMaterial.SetFloat(GaiaShaderID.m_pwSkyFogHeight, Mathf.Clamp(TODSkyboxFogHeight.Evaluate(m_currentTime), -1000f, 8000f));
                        }
                        if (GaiaUtils.ValidateShaderProperty(SkyboxMaterial, GaiaShaderID.m_pwSkyFogGradient))
                        {
                            SkyboxMaterial.SetFloat(GaiaShaderID.m_pwSkyFogGradient, Mathf.Clamp01(TODSkyboxFogGradient.Evaluate(m_currentTime)));
                        }
                    }
                }
                if (CloudLayerMaterials.Count > 0)
                {
                    if (m_gradientOrCurvesExists)
                    { 
                        Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeFogColor, TimeOfDayFogColor.Evaluate(m_currentTime));
                        Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeSunColor, TimeOfDaySunColor.Evaluate(m_currentTime));
                        Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeFinalCloudColor, TimeOfDayEqutorColor.Evaluate(m_currentTime));
                        Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeFinalSkyColor, TimeOfDaySkyColor.Evaluate(m_currentTime) * TimeOfDayFogColor.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightDensity, CloudsHightLevelDensity.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightThickness, CloudsHightLevelThickness.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudOpacity, CloudOpacity.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudSpeed, CloudsHightLevelSpeed.Evaluate(m_currentTime));
                    }
                }
            }
            else
            {
#if HDPipeline
                if (m_gradientOrCurvesExists)
                {
                    if (HDRPLightData != null)
                    {
                        HDRPLightData.SetColor(TimeOfDaySunColor.Evaluate(m_currentTime));
                        HDRPLightData.SetIntensity(GaiaUtils.SetHDRPFloat(SunBrightnessIntensity.Evaluate(m_currentTime), 2.14f));
                        if (!WeatherSystem.m_updateHDRPShadows)
                        {
                            HDRPLightData.EnableShadows(true);
                            HDRPLightData.SetShadowResolution(1024);
                            HDRPLightData.useContactShadow.useOverride = true;
                            WeatherSystem.m_updateHDRPShadows = true;
                        }
                    }
                    if (HDRPVolumeProfile != null)
                    {
                        if (HDRPVolumeProfile.TryGet(out PhysicallyBasedSky physicallyBasedSky))
                        {
                            if (HDRPVolumeProfile.TryGet(out Fog volumetricFog))
                            {
                                volumetricFog.tint.value = TimeOfDayFogColor.Evaluate(m_currentTime);
                                volumetricFog.albedo.value = HDRPFogAlbedo.Evaluate(m_currentTime);
                                physicallyBasedSky.groundTint.value = HDRPGroundColor.Evaluate(m_currentTime);
                                physicallyBasedSky.exposure.value = SkyboxExposure.Evaluate(m_currentTime);
                                volumetricFog.meanFreePath.value = FogEndDistance.Evaluate(m_currentTime);
                                volumetricFog.anisotropy.value = HDRPFogAnisotropy.Evaluate(m_currentTime);
                                volumetricFog.baseHeight.value = HDRPFogBaseHeight.Evaluate(m_currentTime);
                                volumetricFog.depthExtent.value = HDRPFogDepthExtent.Evaluate(m_currentTime);
                                volumetricFog.globalLightProbeDimmer.value = HDRPFogLightProbeDimmer.Evaluate(m_currentTime);
                            }
                        }
                    }
                    if (CloudLayerMaterials.Count > 0)
                    {
                        Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeFogColor, TimeOfDayFogColor.Evaluate(m_currentTime));
                        Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeSunColor, TimeOfDaySunColor.Evaluate(m_currentTime));
                        Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeFinalCloudColor, TimeOfDayEqutorColor.Evaluate(m_currentTime));
                        Shader.SetGlobalColor(GaiaShaderID.m_cloudDomeFinalSkyColor, TimeOfDaySkyColor.Evaluate(m_currentTime) * TimeOfDayFogColor.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightDensity, CloudsHightLevelDensity.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudHeightThickness, CloudsHightLevelThickness.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudOpacity, CloudOpacity.Evaluate(m_currentTime));
                        Shader.SetGlobalFloat(GaiaShaderID.m_cloudSpeed, CloudsHightLevelSpeed.Evaluate(m_currentTime));
                    }
                }
#endif
            }
        }
        /// <summary>
        /// Updates the sun position/rotation
        /// </summary>
        public void UpdateSunPosition()
        {
            m_currentTime = GaiaGlobal.GetTimeOfDayMainValue();
            if (SunLight != null)
            {
                WeatherSystem.m_sunLight.transform.eulerAngles = new Vector3(270f + Mathf.Lerp(0f, 360f, m_currentTime), m_sunRotation, 0f);
            }
        }
        /// <summary>
        /// Sets the render queue and gpu instancing for the clouds
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="shaderName"></param>
        public void SetCloudShaderSettings(GaiaLightingProfileValues profile, string shaderName, GaiaConstants.EnvironmentRenderer renderPipeline)
        {
            try
            {
                if (profile == null || CloudLayerMaterials.Count < 1)
                {
                    return;
                }

                if (renderPipeline != GaiaConstants.EnvironmentRenderer.BuiltIn)
                {
                    if (profile.m_pwSkyAtmosphereData.CloudRenderQueue != GaiaConstants.CloudRenderQueue.Transparent3000)
                    {
                        profile.m_pwSkyAtmosphereData.CloudRenderQueue = GaiaConstants.CloudRenderQueue.Transparent3000;
                        Debug.Log("Cloud render queue has been set to 'Transpartent 3000'. Other render queue do not work in SRP.");
                    }
                }

                for (int i = 0; i < CloudLayerMaterials.Count; i++)
                {
                    if (CloudLayerMaterials[i].shader == Shader.Find(shaderName))
                    {
                        CloudLayerMaterials[i].enableInstancing = profile.m_pwSkyAtmosphereData.CloudGPUInstanced;
                        switch (profile.m_pwSkyAtmosphereData.CloudRenderQueue)
                        {
                            case GaiaConstants.CloudRenderQueue.Background1000:
                                CloudLayerMaterials[i].renderQueue = 1000;
                                break;
                            case GaiaConstants.CloudRenderQueue.Geometry2000:
                                CloudLayerMaterials[i].renderQueue = 2000;
                                break;
                            case GaiaConstants.CloudRenderQueue.AlphaTest2450:
                                CloudLayerMaterials[i].renderQueue = 2450;
                                break;
                            case GaiaConstants.CloudRenderQueue.Transparent3000:
                                CloudLayerMaterials[i].renderQueue = 3000;
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
    }
}
using System.Collections;
using UnityEngine;
#if GAIA_PRO_PRESENT
using Gaia;
#endif
namespace GeNa.Core
{
    public enum RiverWeatherType
    {
        None,
        Raining,
        Snowing
    }
    public class GeNaRiverWeatherController : MonoBehaviour
    {
        #region Variables
        #region Public
        public GeNaRiverProfile m_riverProfile;
        public float m_transitionTime = 10f;
        public RiverWeatherType m_activeWeatherType = RiverWeatherType.None;
        public float SeaLevel;
        #endregion
        #region Private
#if GAIA_PRO_PRESENT
        private ProceduralWorldsGlobalWeather m_gaiaWeather;
#endif
        private GeNaRiverParamaters m_currentProfileValues = new GeNaRiverParamaters();
        private RiverWeatherType m_currentWeatherType = RiverWeatherType.None;
        private MeshRenderer m_meshRenderer;
        private float m_time = 0f;
        private bool m_isProcessing = false;
        private bool m_weatherPresent = false;
        private bool m_meshRendererPresent = false;
        #endregion
        #endregion
        #region Unity Functions
        private void OnEnable()
        {
            if (m_meshRenderer == null)
            {
                m_meshRenderer = GetComponent<MeshRenderer>();
            }
            if (m_meshRenderer != null)
            {
                m_meshRendererPresent = true;
            }
            else
            {
                m_meshRendererPresent = false;
            }
#if GAIA_PRO_PRESENT
            m_gaiaWeather = ProceduralWorldsGlobalWeather.Instance;
            if (m_gaiaWeather != null)
            {
                m_weatherPresent = true;
            }
            else
            {
                m_weatherPresent = false;
            }
#else
            m_weatherPresent = false;
#endif
        }
        private void LateUpdate()
        {
            if (m_weatherPresent && m_meshRendererPresent)
            {
                if (!m_isProcessing)
                {
#if GAIA_PRO_PRESENT
                    if (m_gaiaWeather.IsRaining)
                    {
                        m_activeWeatherType = RiverWeatherType.Raining;
                    }
                    else if (m_gaiaWeather.IsSnowing)
                    {
                        m_activeWeatherType = RiverWeatherType.Snowing;
                    }
                    else
                    {
                        m_activeWeatherType = RiverWeatherType.None;
                    }
#endif
                    if (m_activeWeatherType != m_currentWeatherType)
                    {
                        GetCurrentValues(m_meshRenderer.sharedMaterial);
                        m_isProcessing = true;
                        StartCoroutine(ProcessRiverWeather(m_activeWeatherType, m_meshRenderer.sharedMaterial));
                    }
                }
                else
                {
                    if (HasFinishedProcessing())
                    {
                        m_isProcessing = false;
                        StopCoroutine(ProcessRiverWeather(m_activeWeatherType, m_meshRenderer.sharedMaterial));
                    }
                }
            }
        }
        #endregion
        #region Private Functions
        /// <summary>
        /// Apply the changes to the material
        /// </summary>
        /// <param name="type"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        private IEnumerator ProcessRiverWeather(RiverWeatherType type, Material material)
        {
            while (!HasFinishedProcessing())
            {
                switch (type)
                {
                    case RiverWeatherType.None:
                        //Water Color
                        material.SetColor(GeNaRiverShaderID.m_mainColor, Color.Lerp(m_currentProfileValues.m_mainColor, m_riverProfile.RiverParamaters.m_mainColor, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_mainColorDepthStrength, Mathf.Lerp(m_currentProfileValues.m_mainColorDepthStrength, m_riverProfile.RiverParamaters.m_mainColorDepthStrength, m_time));
                        material.SetColor(GeNaRiverShaderID.m_tintColor, Color.Lerp(m_currentProfileValues.m_tintColor, m_riverProfile.RiverParamaters.m_tintColor, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_tintStrength, Mathf.Lerp(m_currentProfileValues.m_tintStrength, m_riverProfile.RiverParamaters.m_tintStrength, m_time));
                        //Water PBR
                        material.SetFloat(GeNaRiverShaderID.m_smoothness, Mathf.Lerp(m_currentProfileValues.m_smoothness, m_riverProfile.RiverParamaters.m_smoothness, m_time));
                        material.SetColor(GeNaRiverShaderID.m_specularColor, Color.Lerp(m_currentProfileValues.m_tintColor, m_riverProfile.RiverParamaters.m_specularColor, m_time));
                        //Flow
                        material.SetFloat(GeNaRiverShaderID.m_speed, Mathf.Lerp(m_currentProfileValues.m_speed, m_riverProfile.RiverParamaters.m_speed, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_normalShift, Mathf.Lerp(m_currentProfileValues.m_normalShift, m_riverProfile.RiverParamaters.m_normalShift, m_time));
                        //Blends
                        material.SetFloat(GeNaRiverShaderID.m_speed, Mathf.Lerp(m_currentProfileValues.m_speed, m_riverProfile.RiverParamaters.m_speed, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_normalShift, Mathf.Lerp(m_currentProfileValues.m_normalShift, m_riverProfile.RiverParamaters.m_normalShift, m_time));
                        //Normal and Height
                        material.SetFloat(GeNaRiverShaderID.m_normalStrength, Mathf.Lerp(m_currentProfileValues.m_normalStrength, m_riverProfile.RiverParamaters.m_normalStrength, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_shoreRippleHeight, Mathf.Lerp(m_currentProfileValues.m_shoreRippleHeight, m_riverProfile.RiverParamaters.m_shoreRippleHeight, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_refractionStrength, Mathf.Lerp(m_currentProfileValues.m_refractionStrength, m_riverProfile.RiverParamaters.m_refractionStrength, m_time));
                        //Foam
                        material.SetColor(GeNaRiverShaderID.m_foamColor, Color.Lerp(m_currentProfileValues.m_foamColor, m_riverProfile.RiverParamaters.m_foamColor, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamNormalStrength, Mathf.Lerp(m_currentProfileValues.m_normalStrength, m_riverProfile.RiverParamaters.m_normalStrength, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamShoreBlend, Mathf.Lerp(m_currentProfileValues.m_foamShoreBlend, m_riverProfile.RiverParamaters.m_foamShoreBlend, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamHeight, Mathf.Lerp(m_currentProfileValues.m_foamHeight, m_riverProfile.RiverParamaters.m_foamHeight, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamRipple, Mathf.Lerp(m_currentProfileValues.m_foamRipple, m_riverProfile.RiverParamaters.m_foamRipple, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamSpeed, Mathf.Lerp(m_currentProfileValues.m_foamSpeed, m_riverProfile.RiverParamaters.m_foamSpeed, m_time));
                        //Sea Level
                        material.SetFloat(GeNaRiverShaderID.m_seaLevel, Mathf.Lerp(m_currentProfileValues.m_foamRipple, SeaLevel, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_seaLevelBlend, Mathf.Lerp(m_currentProfileValues.m_foamSpeed, m_riverProfile.RiverParamaters.m_seaLevelBlend, m_time));
                        material.SetColor(GeNaRiverShaderID.m_seaLevelFoamColor, Color.Lerp(m_currentProfileValues.m_foamColor, m_riverProfile.RiverParamaters.m_seaLevelFoamColor, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_seaLevelFoamNormalStrength, Mathf.Lerp(m_currentProfileValues.m_foamSpeed, m_riverProfile.RiverParamaters.m_seaLevelFoamNormalStrength, m_time));
                        material.SetColor(GeNaRiverShaderID.m_pBRColor, Color.Lerp(m_currentProfileValues.m_foamColor, m_riverProfile.RiverParamaters.m_pBRColor, m_time));
                        break;
                    case RiverWeatherType.Raining:
                        //Water Color
                        material.SetColor(GeNaRiverShaderID.m_mainColor, Color.Lerp(m_currentProfileValues.m_mainColor, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_mainColor, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_mainColorDepthStrength, Mathf.Lerp(m_currentProfileValues.m_mainColorDepthStrength, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_mainColorDepthStrength, m_time));
                        material.SetColor(GeNaRiverShaderID.m_tintColor, Color.Lerp(m_currentProfileValues.m_tintColor, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_tintColor, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_tintStrength, Mathf.Lerp(m_currentProfileValues.m_tintStrength, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_tintStrength, m_time));
                        //Water PBR
                        material.SetFloat(GeNaRiverShaderID.m_smoothness, Mathf.Lerp(m_currentProfileValues.m_smoothness, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_smoothness, m_time));
                        material.SetColor(GeNaRiverShaderID.m_specularColor, Color.Lerp(m_currentProfileValues.m_tintColor, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_specularColor, m_time));
                        //Flow
                        material.SetFloat(GeNaRiverShaderID.m_speed, Mathf.Lerp(m_currentProfileValues.m_speed, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_speed, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_normalShift, Mathf.Lerp(m_currentProfileValues.m_normalShift, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_normalShift, m_time));
                        //Blends
                        material.SetFloat(GeNaRiverShaderID.m_speed, Mathf.Lerp(m_currentProfileValues.m_speed, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_speed, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_normalShift, Mathf.Lerp(m_currentProfileValues.m_normalShift, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_normalShift, m_time));
                        //Normal and Height
                        material.SetFloat(GeNaRiverShaderID.m_normalStrength, Mathf.Lerp(m_currentProfileValues.m_normalStrength, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_normalStrength, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_shoreRippleHeight, Mathf.Lerp(m_currentProfileValues.m_shoreRippleHeight, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_shoreRippleHeight, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_refractionStrength, Mathf.Lerp(m_currentProfileValues.m_refractionStrength, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_refractionStrength, m_time));
                        //Foam
                        material.SetColor(GeNaRiverShaderID.m_foamColor, Color.Lerp(m_currentProfileValues.m_foamColor, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_foamColor, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamNormalStrength, Mathf.Lerp(m_currentProfileValues.m_normalStrength, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_normalStrength, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamShoreBlend, Mathf.Lerp(m_currentProfileValues.m_foamShoreBlend, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_foamShoreBlend, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamHeight, Mathf.Lerp(m_currentProfileValues.m_foamHeight, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_foamHeight, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamRipple, Mathf.Lerp(m_currentProfileValues.m_foamRipple, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_foamRipple, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamSpeed, Mathf.Lerp(m_currentProfileValues.m_foamSpeed, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_foamSpeed, m_time));
                        //Sea Level
                        material.SetFloat(GeNaRiverShaderID.m_seaLevel, Mathf.Lerp(m_currentProfileValues.m_foamRipple, SeaLevel, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_seaLevelBlend, Mathf.Lerp(m_currentProfileValues.m_foamSpeed, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_seaLevelBlend, m_time));
                        material.SetColor(GeNaRiverShaderID.m_seaLevelFoamColor, Color.Lerp(m_currentProfileValues.m_foamColor, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_seaLevelFoamColor, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_seaLevelFoamNormalStrength, Mathf.Lerp(m_currentProfileValues.m_foamSpeed, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_seaLevelFoamNormalStrength, m_time));
                        material.SetColor(GeNaRiverShaderID.m_pBRColor, Color.Lerp(m_currentProfileValues.m_foamColor, m_riverProfile.WeatherParamaters.m_rainRiverProfile.RiverParamaters.m_pBRColor, m_time));
                        break;
                    case RiverWeatherType.Snowing:
                        //Water Color
                        material.SetColor(GeNaRiverShaderID.m_mainColor, Color.Lerp(m_currentProfileValues.m_mainColor, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_mainColor, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_mainColorDepthStrength, Mathf.Lerp(m_currentProfileValues.m_mainColorDepthStrength, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_mainColorDepthStrength, m_time));
                        material.SetColor(GeNaRiverShaderID.m_tintColor, Color.Lerp(m_currentProfileValues.m_tintColor, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_tintColor, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_tintStrength, Mathf.Lerp(m_currentProfileValues.m_tintStrength, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_tintStrength, m_time));
                        //Water PBR
                        material.SetFloat(GeNaRiverShaderID.m_smoothness, Mathf.Lerp(m_currentProfileValues.m_smoothness, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_smoothness, m_time));
                        material.SetColor(GeNaRiverShaderID.m_specularColor, Color.Lerp(m_currentProfileValues.m_tintColor, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_specularColor, m_time));
                        //Flow
                        material.SetFloat(GeNaRiverShaderID.m_speed, Mathf.Lerp(m_currentProfileValues.m_speed, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_speed, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_normalShift, Mathf.Lerp(m_currentProfileValues.m_normalShift, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_normalShift, m_time));
                        //Blends
                        material.SetFloat(GeNaRiverShaderID.m_speed, Mathf.Lerp(m_currentProfileValues.m_speed, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_speed, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_normalShift, Mathf.Lerp(m_currentProfileValues.m_normalShift, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_normalShift, m_time));
                        //Normal and Height
                        material.SetFloat(GeNaRiverShaderID.m_normalStrength, Mathf.Lerp(m_currentProfileValues.m_normalStrength, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_normalStrength, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_shoreRippleHeight, Mathf.Lerp(m_currentProfileValues.m_shoreRippleHeight, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_shoreRippleHeight, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_refractionStrength, Mathf.Lerp(m_currentProfileValues.m_refractionStrength, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_refractionStrength, m_time));
                        //Foam
                        material.SetColor(GeNaRiverShaderID.m_foamColor, Color.Lerp(m_currentProfileValues.m_foamColor, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_foamColor, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamNormalStrength, Mathf.Lerp(m_currentProfileValues.m_normalStrength, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_normalStrength, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamShoreBlend, Mathf.Lerp(m_currentProfileValues.m_foamShoreBlend, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_foamShoreBlend, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamHeight, Mathf.Lerp(m_currentProfileValues.m_foamHeight, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_foamHeight, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamRipple, Mathf.Lerp(m_currentProfileValues.m_foamRipple, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_foamRipple, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_foamSpeed, Mathf.Lerp(m_currentProfileValues.m_foamSpeed, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_foamSpeed, m_time));
                        //Sea Level
                        material.SetFloat(GeNaRiverShaderID.m_seaLevel, Mathf.Lerp(m_currentProfileValues.m_foamRipple, SeaLevel, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_seaLevelBlend, Mathf.Lerp(m_currentProfileValues.m_foamSpeed, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_seaLevelBlend, m_time));
                        material.SetColor(GeNaRiverShaderID.m_seaLevelFoamColor, Color.Lerp(m_currentProfileValues.m_foamColor, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_seaLevelFoamColor, m_time));
                        material.SetFloat(GeNaRiverShaderID.m_seaLevelFoamNormalStrength, Mathf.Lerp(m_currentProfileValues.m_foamSpeed, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_seaLevelFoamNormalStrength, m_time));
                        material.SetColor(GeNaRiverShaderID.m_pBRColor, Color.Lerp(m_currentProfileValues.m_foamColor, m_riverProfile.WeatherParamaters.m_snowRiverProfile.RiverParamaters.m_pBRColor, m_time));
                        break;
                }
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
        }
        /// <summary>
        /// Gets the current amterial values and sets up the process step
        /// </summary>
        /// <param name="material"></param>
        private void GetCurrentValues(Material material)
        {
            m_time = 0f;
            m_currentWeatherType = m_activeWeatherType;
            if (material == null)
            {
                return;
            }

            //Water Color
            m_currentProfileValues.m_mainColor = material.GetColor(GeNaRiverShaderID.m_mainColor);
            m_currentProfileValues.m_mainColorDepthStrength = material.GetFloat(GeNaRiverShaderID.m_mainColorDepthStrength);
            m_currentProfileValues.m_tintColor = material.GetColor(GeNaRiverShaderID.m_tintColor);
            m_currentProfileValues.m_tintStrength = material.GetFloat(GeNaRiverShaderID.m_tintStrength);
            //Water PBR
            m_currentProfileValues.m_smoothness = material.GetFloat(GeNaRiverShaderID.m_smoothness);
            m_currentProfileValues.m_specularColor = material.GetColor(GeNaRiverShaderID.m_specularColor);
            //Flow
            m_currentProfileValues.m_speed = material.GetFloat(GeNaRiverShaderID.m_speed);
            m_currentProfileValues.m_normalShift = material.GetFloat(GeNaRiverShaderID.m_normalShift);
            //Blends
            m_currentProfileValues.m_shoreBlend = material.GetFloat(GeNaRiverShaderID.m_speed);
            m_currentProfileValues.m_shoreNormalBlend = material.GetFloat(GeNaRiverShaderID.m_shoreNormalBlend);
            //Normal and Height
            m_currentProfileValues.m_normalStrength = material.GetFloat(GeNaRiverShaderID.m_normalStrength);
            m_currentProfileValues.m_shoreRippleHeight = material.GetFloat(GeNaRiverShaderID.m_shoreRippleHeight);
            m_currentProfileValues.m_refractionStrength = material.GetFloat(GeNaRiverShaderID.m_refractionStrength);
            //Foam
            m_currentProfileValues.m_foamColor = material.GetColor(GeNaRiverShaderID.m_foamColor);
            m_currentProfileValues.m_foamNormalStrength = material.GetFloat(GeNaRiverShaderID.m_foamNormalStrength);
            m_currentProfileValues.m_foamShoreBlend = material.GetFloat(GeNaRiverShaderID.m_foamShoreBlend);
            m_currentProfileValues.m_foamHeight = material.GetFloat(GeNaRiverShaderID.m_foamHeight);
            m_currentProfileValues.m_foamRipple = material.GetFloat(GeNaRiverShaderID.m_foamRipple);
            m_currentProfileValues.m_foamSpeed = material.GetFloat(GeNaRiverShaderID.m_foamSpeed);
            //Sea Level
            SeaLevel = material.GetFloat(GeNaRiverShaderID.m_foamRipple);
            m_currentProfileValues.m_seaLevelBlend = material.GetFloat(GeNaRiverShaderID.m_seaLevelBlend);
            m_currentProfileValues.m_seaLevelFoamColor = material.GetColor(GeNaRiverShaderID.m_seaLevelFoamColor);
            m_currentProfileValues.m_seaLevelFoamNormalStrength = material.GetFloat(GeNaRiverShaderID.m_seaLevelFoamNormalStrength);
            m_currentProfileValues.m_pBRColor = material.GetColor(GeNaRiverShaderID.m_pBRColor);
        }
        /// <summary>
        /// Checked to see if processing has finished
        /// </summary>
        /// <returns></returns>
        private bool HasFinishedProcessing()
        {
            m_time += Time.deltaTime / m_transitionTime;
            if (m_time >= 1f)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
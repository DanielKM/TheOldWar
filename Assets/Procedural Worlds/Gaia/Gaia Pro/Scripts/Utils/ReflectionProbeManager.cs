using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gaia
{
    [Serializable]
    public class GaiaLightCheckData
    {
        //Sun
        public Color m_sunColor;
        public float m_sunIntensity;
        public Vector3 m_sunRotation;

        //Ambient
        public Color m_skyAmbientColor;
        public Color m_equatorAmbientColor;
        public Color m_groundAmbientColor;
        public float m_ambientIntensity;

        //Skybox
        public Color m_skyTintColor;
        public float m_skyExposure;
        public float m_skyRotation;
        public Cubemap m_skyHDRITexture;
        public float m_skyAtmosphereThickness;
        public Color m_skyGroundColor;

        //Time of day
        public float m_minute;
        public float m_checkMinuteTime = 30f;
        public int m_hour;
        public int m_checkHourTime = 1;
        public bool m_snowEnabled;
        public bool m_rainEnabled;
    }

    [ExecuteAlways]
    public class ReflectionProbeManager : MonoBehaviour
    {
        public static ReflectionProbeManager Instance
        {
            get { return m_instance; }
        }
        [SerializeField]
        private static ReflectionProbeManager m_instance;

        #region Variables

        public bool EnableSystem
        {
            get { return m_enableSystem; }
            set
            {
                if (m_enableSystem != value)
                {
                    m_enableSystem = value;
                    if (!m_enableSystem)
                    {
                        CleanUp();
                    }
                }
            }
        }
        [SerializeField]
        private bool m_enableSystem = true;

        public bool UseReflectionProbeCuller
        {
            get { return m_useReflectionProbeCuller; }
            set
            {
                if (m_useReflectionProbeCuller != value)
                {
                    m_useReflectionProbeCuller = value;
                    if (!GaiaUtils.HasDynamicLoadedTerrains())
                    {
                        SetReflectionProbeCuller();
                    }
                }
            }
        }
        [SerializeField]
        private bool m_useReflectionProbeCuller = true;

        public float ReflectionProbeCullingDistance
        {
            get { return m_reflectionProbeCullingDistance; }
            set
            {
                if (m_reflectionProbeCullingDistance != value)
                {
                    m_reflectionProbeCullingDistance = value;
                    if (!GaiaUtils.HasDynamicLoadedTerrains())
                    {
                        SetReflectionProbeCuller();
                    }
                }
            }
        }
        [SerializeField]
        private float m_reflectionProbeCullingDistance = 650f;

        public LayerMask ProbeLayerMask
        {
            get { return m_probeLayerMask; }
            set
            {
                if (m_probeLayerMask != value)
                {
                    m_probeLayerMask = value;
                    SyncProbeLayerMask();
                }
            }
        }
        [SerializeField] 
        private LayerMask m_probeLayerMask = -1;

        public bool m_checkTODEveryFrame = true;
        public bool m_probeCullingNeedsUpdating = false;
        public bool m_isProcessing = false;

        public List<ReflectionProbe> m_sceneReflectionProbes = new List<ReflectionProbe>();
        public List<ReflectionProbe> m_priorityReflectionProbes = new List<ReflectionProbe>();
        public List<ReflectionProbe> m_nonPriorityReflectionProbes = new List<ReflectionProbe>();
        public GaiaLightCheckData m_lightCheckData = new GaiaLightCheckData();
        public Vector3 m_playerBoundsSize = new Vector3(600f, 600f, 600f);
        public Bounds m_playerBounds = new Bounds(Vector3.zero, Vector3.one);
        public Transform m_player;
        public float m_probePriorityCheck = 40f;
        public float m_probeProcessTime = 0.5f;

        [SerializeField]
        private Light m_sunLight;
        [SerializeField]
        private Material m_skyMaterial;
        [SerializeField]
        private ProceduralWorldsGlobalWeather WeatherSystem;

        private float m_currentProbePriorityCheck;
        private int m_priorityProbesCount;
        private int m_currentPriorityProbesCount;
        private int m_nonPriorityProbesCount;
        private int m_currentNonPriorityProbesCount;

        #endregion

        #region Unity Functions

        private void Start()
        {
            m_skyMaterial = RenderSettings.skybox;
            m_sceneReflectionProbes = GetAllSceneProbes(true);
            for (int i = 0; i < m_sceneReflectionProbes.Count; i++)
            {
                m_sceneReflectionProbes[i].RenderProbe();
            }
            SetReflectionProbeCuller();
            SetAllLightCheckData();
        }
        private void OnEnable()
        {
            m_instance = this;
            WeatherSystem = ProceduralWorldsGlobalWeather.Instance;
            if (m_player == null)
            {
                m_player = GaiaUtils.GetPlayerTransform();
            }

            m_currentProbePriorityCheck = m_probePriorityCheck;
            CleanUp();
        }
        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (EnableSystem)
            {
                #if GAIA_PRO_PRESENT
                if (m_checkTODEveryFrame)
                {
                    if (!m_isProcessing)
                    {
                        if (CheckTimeOfDay())
                        {
                            m_currentProbePriorityCheck = -1;
                            StopCoroutine(ProcessPriorityProbes());
                        }
                    }
                }
                #endif

                if (!m_isProcessing)
                {
                    m_currentProbePriorityCheck -= Time.deltaTime;
                    if (m_currentProbePriorityCheck < 0)
                    {
                        m_sceneReflectionProbes = GetAllSceneProbes();
                        m_currentProbePriorityCheck = m_probePriorityCheck;
                        m_priorityReflectionProbes.Clear();
                        m_nonPriorityReflectionProbes.Clear();
                        UpdateBounds();
                        CleanUp();
                        m_sunLight = GetSunOrMoonLight();

                        for (int i = 0; i < m_sceneReflectionProbes.Count; i++)
                        {
                            CheckProbeDistance(m_playerBounds, m_sceneReflectionProbes[i]);
                        }
                        m_currentNonPriorityProbesCount = m_nonPriorityReflectionProbes.Count;
                        m_currentPriorityProbesCount = m_priorityReflectionProbes.Count;

                        if (m_currentNonPriorityProbesCount < 1 || m_currentPriorityProbesCount < 1)
                        {
                            SetAllLightCheckData();
                            CleanUp();
                        }
                        else
                        {
                            StartCoroutine(ProcessPriorityProbes());
                        }
                    }
                }
            }
        }
        private IEnumerator ProcessPriorityProbes()
        {
            while (m_priorityProbesCount != m_currentPriorityProbesCount || m_currentPriorityProbesCount == 0)
            {
                m_isProcessing = true;
                yield return new WaitForEndOfFrame();
                if (CheckIfHasChanged())
                {
                    for (int i = 0; i < m_priorityReflectionProbes.Count; i++)
                    {
                        m_priorityProbesCount++;
                        if (m_priorityReflectionProbes[i] != null)
                        {
                            if (m_priorityReflectionProbes[i].isActiveAndEnabled)
                            {
                                m_priorityReflectionProbes[i].RenderProbe();
                                yield return new WaitForSecondsRealtime(m_probeProcessTime);
                            }
                        }

                        if (m_priorityProbesCount == m_currentPriorityProbesCount)
                        {
                            StartCoroutine(ProcessNonPriorityProbes());
                        }
                    }
                }
                else
                {
                    SetAllLightCheckData();
                    CleanUp();
                }
            }
        }
        private IEnumerator ProcessNonPriorityProbes()
        {
            while (m_nonPriorityProbesCount != m_currentNonPriorityProbesCount || m_currentNonPriorityProbesCount == 0)
            {
                StopCoroutine(ProcessPriorityProbes());
                yield return new WaitForEndOfFrame();
                for (int i = 0; i < m_nonPriorityReflectionProbes.Count; i++)
                {
                    m_nonPriorityProbesCount++;
                    if (m_nonPriorityReflectionProbes[i] != null)
                    {
                        if (m_nonPriorityReflectionProbes[i].isActiveAndEnabled)
                        {
                            m_nonPriorityReflectionProbes[i].RenderProbe();
                            yield return new WaitForSecondsRealtime(m_probeProcessTime);
                        }
                    }

                    if (m_nonPriorityProbesCount == m_currentNonPriorityProbesCount)
                    {
                        SetAllLightCheckData();
                        CleanUp();
                    }
                }
            }
        }

        #endregion

        #region Private Funtions

        private Light GetSunOrMoonLight()
        {
            Light light = m_sunLight;
            if (WeatherSystem != null)
            {
                if (WeatherSystem.CheckIsNight())
                {
                    light = WeatherSystem.m_moonLight;
                }
                else
                {
                    light = WeatherSystem.m_sunLight;
                }
            }

            return light;
        }
        /// <summary>
        /// Stops and resets IEnumerators
        /// </summary>
        private void CleanUp()
        {
            m_isProcessing = false;
            StopAllCoroutines();
            m_priorityProbesCount = 0;
            m_nonPriorityProbesCount = 0;
        }
        /// <summary>
        /// Updates the bounds check for the player
        /// </summary>
        private void UpdateBounds()
        {
            if (m_playerBounds == null)
            {
                m_playerBounds = new Bounds(Vector3.zero, Vector3.one);
            }

            m_playerBounds.size = m_playerBoundsSize;
            if (m_player != null)
            {
                m_playerBounds.center = m_player.position;
            }
        }
        /// <summary>
        /// Gets all the reflection probes in the scene
        /// </summary>
        /// <returns></returns>
        private List<ReflectionProbe> GetAllSceneProbes(bool renderProbes = false)
        {
            List<ReflectionProbe> reflectionProbes = new List<ReflectionProbe>();
            ReflectionProbe[] probes = FindObjectsOfType<ReflectionProbe>();
            for (int i = 0; i < probes.Length; i++)
            {
                if (probes[i].isActiveAndEnabled)
                {
                    probes[i].mode = ReflectionProbeMode.Realtime;
                    probes[i].refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                    reflectionProbes.Add(probes[i]);
                }
            }

            return reflectionProbes;
        }
        /// <summary>
        /// Checks if the probe is near the player
        /// </summary>
        private void CheckProbeDistance(Bounds playerBounds, ReflectionProbe probe)
        {
            if (playerBounds == null || probe == null)
            {
                return;
            }
            if (playerBounds.Contains(probe.transform.position))
            {
                m_priorityReflectionProbes.Add(probe);
            }
            else
            {
                m_nonPriorityReflectionProbes.Add(probe);
            }
        }
        /// <summary>
        /// Sets all the lighting check data
        /// </summary>
        private void SetAllLightCheckData()
        {
            if (m_lightCheckData == null)
            {
                return;
            }

            //Set sun settings
            if (m_sunLight == null)
            {
                m_sunLight = GaiaUtils.GetMainDirectionalLight();
            }
            if (m_sunLight != null)
            {
                m_lightCheckData.m_sunColor = m_sunLight.color;
                m_lightCheckData.m_sunIntensity = m_sunLight.intensity;
                m_lightCheckData.m_sunRotation = m_sunLight.transform.eulerAngles;
            }

            //Set ambient settings
            m_lightCheckData.m_ambientIntensity = RenderSettings.ambientIntensity;
            m_lightCheckData.m_skyAmbientColor = RenderSettings.ambientSkyColor;
            m_lightCheckData.m_equatorAmbientColor = RenderSettings.ambientEquatorColor;
            m_lightCheckData.m_groundAmbientColor = RenderSettings.ambientGroundColor;

            //Set skybox settings
            if (m_skyMaterial == null)
            {
                m_skyMaterial = RenderSettings.skybox;
            }
            if (m_skyMaterial != null)
            {
                if (m_skyMaterial.shader == Shader.Find(GaiaShaderID.m_unitySkyboxShaderHDRI))
                {
                    m_lightCheckData.m_skyTintColor = m_skyMaterial.GetColor(GaiaShaderID.m_unitySkyboxTintHDRI);
                    m_lightCheckData.m_skyExposure = m_skyMaterial.GetFloat(GaiaShaderID.m_unitySkyboxExposure);
                    m_lightCheckData.m_skyRotation = m_skyMaterial.GetFloat(GaiaShaderID.m_unitySkyboxRotation);
                    m_lightCheckData.m_skyHDRITexture = (Cubemap)m_skyMaterial.GetTexture(GaiaShaderID.m_unitySkyboxCubemap);
                }
                else if (m_skyMaterial.shader == Shader.Find(GaiaShaderID.m_unitySkyboxShader))
                {
                    m_lightCheckData.m_skyTintColor = m_skyMaterial.GetColor(GaiaShaderID.m_unitySkyboxTint);
                    m_lightCheckData.m_skyGroundColor = m_skyMaterial.GetColor(GaiaShaderID.m_unitySkyboxGroundColor);
                    m_lightCheckData.m_skyExposure = m_skyMaterial.GetFloat(GaiaShaderID.m_unitySkyboxExposure);
                    m_lightCheckData.m_skyAtmosphereThickness = m_skyMaterial.GetFloat(GaiaShaderID.m_unitySkyboxAtmosphereThickness);
                }
            }

            //Set time of day settings
            #if GAIA_PRO_PRESENT

            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                m_lightCheckData.m_minute = GaiaGlobal.Instance.SceneProfile.m_gaiaTimeOfDay.m_todMinutes;
                m_lightCheckData.m_hour = GaiaGlobal.Instance.SceneProfile.m_gaiaTimeOfDay.m_todHour;

                if (WeatherSystem != null)
                {
                    GaiaGlobal.GaiaGlobalNetworkSyncGetWeather(out m_lightCheckData.m_rainEnabled, out m_lightCheckData.m_snowEnabled);
                }
            }

            #endif
        }
        /// <summary>
        /// Checks if time of day has changed
        /// </summary>
        /// <returns></returns>
        private bool CheckTimeOfDay()
        {
#if GAIA_PRO_PRESENT

            //Time of day check
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                if (GaiaGlobal.Instance.SceneProfile.m_gaiaTimeOfDay.m_todMinutes >= m_lightCheckData.m_minute + m_lightCheckData.m_checkMinuteTime)
                {
                    return true;
                }
                if (GaiaGlobal.Instance.SceneProfile.m_gaiaTimeOfDay.m_todHour >= m_lightCheckData.m_hour + m_lightCheckData.m_checkHourTime)
                {
                    return true;
                }

                if (WeatherSystem != null)
                {
                    if (m_lightCheckData.m_rainEnabled != WeatherSystem.IsRaining)
                    {
                        return true;
                    }

                    if (m_lightCheckData.m_snowEnabled != WeatherSystem.IsSnowing)
                    {
                        return true;
                    }
                }
            }
#endif
            return false;
        }
        /// <summary>
        /// Syncs the probes layer mask
        /// </summary>
        private void SyncProbeLayerMask()
        {
            if (!Application.isPlaying)
            {
                m_sceneReflectionProbes = GetAllSceneProbes();
            }
            else
            {
                if (m_sceneReflectionProbes.Count < 1)
                {
                    m_sceneReflectionProbes = GetAllSceneProbes();
                }
            }

            for (int i = 0; i < m_sceneReflectionProbes.Count; i++)
            {
                if (m_sceneReflectionProbes[i] != null)
                {
                    m_sceneReflectionProbes[i].cullingMask = ProbeLayerMask;
                }
            }
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Sets up the reflection probe culling
        /// </summary>
        public void SetReflectionProbeCuller()
        {
            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                if (!Application.isPlaying)
                {
                    //Action<Terrain> terrAction = (t) => ProcessProbeCuller(t); 
                    //GaiaUtils.CallFunctionOnDynamicLoadedTerrains(terrAction, true, null, "Updating Reflection Probe Culling");
                    //if (TerrainLoaderManager.Instance != null)
                    //{
                    //    TerrainLoaderManager.Instance.Refresh();
                    //}
                }
            }
            else
            {
                for (int i = 0; i < Terrain.activeTerrains.Length; i++)
                {
                    ProcessProbeCuller(Terrain.activeTerrains[i]);
                }
            }
        }
        /// <summary>
        /// Updates the probe culling distance
        /// </summary>
        /// <param name="terrain"></param>
        private void ProcessProbeCuller(Terrain terrain)
        {
            try
            {
                GameObject probeParent = GameObject.Find(terrain.name + " Reflection Probes");
                if (probeParent != null)
                {
                    ReflectionProbe[] allProbes = probeParent.GetComponentsInChildren<ReflectionProbe>();
                    if (allProbes.Length > 0)
                    {
                        foreach (var probe in allProbes)
                        {
                            ReflectionProbeCuller culler = probe.GetComponent<ReflectionProbeCuller>();
                            if (UseReflectionProbeCuller)
                            {
                                if (culler == null)
                                {
                                    culler = probe.gameObject.AddComponent<ReflectionProbeCuller>();
                                    culler.m_probe = probe;
                                    culler.Initialize();
                                }

                                culler.m_maxRenderDistance = ReflectionProbeCullingDistance;
                            }
                            else
                            {
                                if (culler != null)
                                {
                                    DestroyImmediate(culler);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        /// <summary>
        /// Checks if any lighting changes have been made
        /// </summary>
        /// <returns></returns>
        public bool CheckIfHasChanged()
        {
            if (m_lightCheckData == null)
            {
                Debug.LogError("Light Check Data was empty, pleae make sure you have created one and assigned it in the script before executing this function.");
                return false;
            }

            //Sun check
            if (m_sunLight != null)
            {
                if (m_sunLight.intensity != m_lightCheckData.m_sunIntensity)
                {
                    return true;
                }

                if (m_sunLight.color != m_lightCheckData.m_sunColor)
                {
                    return true;
                }

                if (m_sunLight.transform.eulerAngles != m_lightCheckData.m_sunRotation)
                {
                    return true;
                }
            }

            //Ambient check
            if (RenderSettings.ambientIntensity != m_lightCheckData.m_ambientIntensity)
            {
                return true;
            }
            if (RenderSettings.ambientSkyColor != m_lightCheckData.m_skyAmbientColor)
            {
                return true;
            }
            if (RenderSettings.ambientEquatorColor != m_lightCheckData.m_equatorAmbientColor)
            {
                return true;
            }
            if (RenderSettings.ambientGroundColor != m_lightCheckData.m_groundAmbientColor)
            {
                return true;
            }

            //Skybox check
            if (m_skyMaterial != null)
            {
                if (m_skyMaterial.shader == Shader.Find(GaiaShaderID.m_unitySkyboxShaderHDRI))
                {
                    if (m_lightCheckData.m_skyTintColor != m_skyMaterial.GetColor(GaiaShaderID.m_unitySkyboxTintHDRI))
                    {
                        return true;
                    }
                    if (m_lightCheckData.m_skyExposure != m_skyMaterial.GetFloat(GaiaShaderID.m_unitySkyboxExposure))
                    {
                        return true;
                    }
                    if (m_lightCheckData.m_skyRotation != m_skyMaterial.GetFloat(GaiaShaderID.m_unitySkyboxRotation))
                    {
                        return true;
                    }
                    if (m_lightCheckData.m_skyHDRITexture != m_skyMaterial.GetTexture(GaiaShaderID.m_unitySkyboxCubemap))
                    {
                        return true;
                    }
                }
                else if (m_skyMaterial.shader == Shader.Find(GaiaShaderID.m_unitySkyboxShader))
                {
                    if (m_lightCheckData.m_skyTintColor != m_skyMaterial.GetColor(GaiaShaderID.m_unitySkyboxTint))
                    {
                        return true;
                    }
                    if (m_lightCheckData.m_skyGroundColor != m_skyMaterial.GetColor(GaiaShaderID.m_unitySkyboxGroundColor))
                    {
                        return true;
                    }
                    if (m_lightCheckData.m_skyExposure != m_skyMaterial.GetFloat(GaiaShaderID.m_unitySkyboxExposure))
                    {
                        return true;
                    }
                    if (m_lightCheckData.m_skyAtmosphereThickness != m_skyMaterial.GetFloat(GaiaShaderID.m_unitySkyboxAtmosphereThickness))
                    {
                        return true;
                    }
                }
            }

            #if GAIA_PRO_PRESENT

            //Time of day check
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                if (m_lightCheckData.m_minute != GaiaGlobal.Instance.SceneProfile.m_gaiaTimeOfDay.m_todMinutes)
                {
                    return true;
                }
                if (m_lightCheckData.m_hour != GaiaGlobal.Instance.SceneProfile.m_gaiaTimeOfDay.m_todHour)
                {
                    return true;
                }
            }

            #endif

            return false;
        }
        /// <summary>
        /// Gets or creates the reflection probe manager
        /// </summary>
        /// <returns></returns>
        public static ReflectionProbeManager GetOrCreateProbeManager()
        {
            ReflectionProbeManager manager = FindObjectOfType<ReflectionProbeManager>();
            GameObject parent = GameObject.Find(GaiaConstants.gaiaLightingObject);
            if (manager != null)
            {
                if (parent != null)
                {
                    manager.gameObject.transform.SetParent(parent.transform);
                }

                if (!GaiaUtils.HasDynamicLoadedTerrains())
                {
                    manager.SetReflectionProbeCuller();
                }

                return manager;
            }
            else
            {
                GameObject probeGameObject = new GameObject("Reflection Probe Manager");
                manager = probeGameObject.AddComponent<ReflectionProbeManager>();
                if (parent != null)
                {
                    probeGameObject.transform.SetParent(parent.transform);
                }

                manager.SetReflectionProbeCuller();

                return manager;
            }
        }
        /// <summary>
        /// Removes the reflection probe manager
        /// </summary>
        public static void RemoveReflectionProbeManager()
        {
            GameObject probeManager = GameObject.Find("Reflection Probe Manager");
            if (probeManager != null)
            {
                DestroyImmediate(probeManager);
            }
        }

        #endregion
    }
}
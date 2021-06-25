using UnityEngine;
using UnityEngine.Rendering;

namespace GeNa.Core
{
    public enum ReflectionProbeRenderResolution
    {
        Resolution16,
        Resolution32,
        Resolution64,
        Resolution128,
        Resolution256,
        Resolution512,
        Resolution1024,
        Resolution2048,
    }
    
    [ExecuteAlways]
    public class RiverReflectionProbeManager : MonoBehaviour
    {
        #region Varaibles

        #region Public Varaibles

        public LayerMask LayerMask
        {
            get => m_layerMask;
            set
            {
                if (m_layerMask != value)
                {
                    m_layerMask = value;
                    m_layerMaskValue = m_layerMask.value;
                    ApplyReflectionProbe();
                }
            }
        }
        public ReflectionProbeMode ProbeMode
        {
            get => m_probeMode;
            set
            {
                if (m_probeMode != value)
                {
                    m_probeMode = value;
                    ApplyReflectionProbe();
                }
            }
        }
        public ReflectionProbeRefreshMode ProbeRefreshMode
        {
            get => m_probeRefreshMode;
            set
            {
                if (m_probeRefreshMode != value)
                {
                    m_probeRefreshMode = value;
                    ApplyReflectionProbe();
                }
            }
        }
        public ReflectionProbeTimeSlicingMode ProbeTimeSlicingMode
        {
            get => m_probeTimeSlicingMode;
            set
            {
                if (m_probeTimeSlicingMode != value)
                {
                    m_probeTimeSlicingMode = value;
                    ApplyReflectionProbe();
                }
            }
        }
        public ReflectionProbeRenderResolution ProbeRenderResolution
        {
            get => m_probeRenderResolution;
            set
            {
                if (m_probeRenderResolution != value)
                {
                    m_probeRenderResolution = value;
                    ApplyReflectionProbe();
                }
            }
        }
        public float RenderDistance
        {
            get => m_renderDistance;
            set
            {
                if (m_renderDistance != value)
                {
                    m_renderDistance = value;
                    ApplyReflectionProbe();
                }
            }
        }
        public ReflectionProbe m_reflectionProbe;

        #endregion
        #region Private Varaibles

        [SerializeField] private LayerMask m_layerMask = -1;
        [SerializeField] private ReflectionProbeMode m_probeMode = ReflectionProbeMode.Realtime;
        [SerializeField] private ReflectionProbeRefreshMode m_probeRefreshMode = ReflectionProbeRefreshMode.OnAwake;
        [SerializeField] private ReflectionProbeTimeSlicingMode m_probeTimeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
        [SerializeField] private ReflectionProbeRenderResolution m_probeRenderResolution = ReflectionProbeRenderResolution.Resolution64;
        [SerializeField] private float m_renderDistance = 2000f;
        [SerializeField] private int m_layerMaskValue = -1;

        #endregion

        #endregion
        #region Unity Functions

        /// <summary>
        /// Execute on enable
        /// </summary>
        private void OnEnable()
        {
            ApplyReflectionProbe();
        }

        #endregion
        #region Private Functions

        /// <summary>
        /// Applies the changes to the reflection probe
        /// Also executes this function OnEnable
        /// </summary>
        private void ApplyReflectionProbe()
        {
            if (m_reflectionProbe == null)
            {
                m_reflectionProbe = GetComponent<ReflectionProbe>();
                if (m_reflectionProbe == null)
                {
                    m_reflectionProbe = gameObject.AddComponent<ReflectionProbe>();
                }
            }

            m_reflectionProbe.mode = ProbeMode;
            m_reflectionProbe.refreshMode = ProbeRefreshMode;
            m_reflectionProbe.timeSlicingMode = ProbeTimeSlicingMode;
            m_reflectionProbe.cullingMask = m_layerMaskValue;
            m_reflectionProbe.farClipPlane = RenderDistance;
            m_reflectionProbe.resolution = SetResolution();

            if (IsInSceneAndActive())
            {
                m_reflectionProbe.RenderProbe();
            }
        }
        /// <summary>
        /// Checks to see if the object is active and in an active scene
        /// </summary>
        /// <returns></returns>
        private bool IsInSceneAndActive()
        {
            if (gameObject.activeInHierarchy)
            {
                if (gameObject.scene.isLoaded)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Sets the render resolution of the probe
        /// </summary>
        /// <returns></returns>
        private int SetResolution()
        {
            switch (ProbeRenderResolution)
            {
                case ReflectionProbeRenderResolution.Resolution16:
                    return 16;
                case ReflectionProbeRenderResolution.Resolution32:
                    return 32;
                case ReflectionProbeRenderResolution.Resolution64:
                    return 64;
                case ReflectionProbeRenderResolution.Resolution128:
                    return 128;
                case ReflectionProbeRenderResolution.Resolution256:
                    return 256;
                case ReflectionProbeRenderResolution.Resolution512:
                    return 512;
                case ReflectionProbeRenderResolution.Resolution1024:
                    return 1024;
                case ReflectionProbeRenderResolution.Resolution2048:
                    return 2048;
                default:
                    return 32;
            }
        }

        #endregion
    }
}
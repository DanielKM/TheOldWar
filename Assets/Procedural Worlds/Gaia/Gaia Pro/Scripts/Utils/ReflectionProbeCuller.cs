using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gaia
{
    [ExecuteAlways]
    [RequireComponent(typeof(ReflectionProbe))]
    public class ReflectionProbeCuller : MonoBehaviour
    {
        [Header("Global Settings")] 
        public bool m_systemActive = true;

        public bool m_syncToManager = true;
        public ReflectionProbe m_probe;
        public float m_maxRenderDistance = 650f;
        public int CheckPeriodMin = 5;
        public int checkPeriodMax = 10;

        private float currentDistance = 256f;
        private int checkPeriod;
        private int currentCheckPriod = 0;
        private bool m_probeExists = false;
        private bool m_gaiaGlobalCameraExists = false;
        private bool m_managerExists = false;

        #region Unity Functions

        /// <summary>
        /// Initialize when it's not playing application
        /// </summary>
        private void OnEnable()
        {
            if (ReflectionProbeManager.Instance != null)
            {
                m_managerExists = true;
            }
            else
            {
                m_managerExists = false;
            }

            Initialize();
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Function used to initialize the systems on this script
        /// </summary>
        public void Initialize()
        {
            currentCheckPriod = 0;
            checkPeriod = UnityEngine.Random.Range(CheckPeriodMin, checkPeriodMax);
            if (m_probe == null)
            {
                m_probe = gameObject.GetComponent<ReflectionProbe>();
            }

            if (m_probe != null)
            {
                m_probeExists = true;
                m_probe.enabled = true;
                m_probe.RenderProbe();
            }

            CancelInvoke("DistanceCheck");

            if (Application.isPlaying)
            {
                if (GaiaGlobal.Instance != null)
                {
                    if (GaiaGlobal.Instance.m_mainCamera != null)
                    {
                        m_gaiaGlobalCameraExists = true;
                    }
                    else
                    {
                        m_gaiaGlobalCameraExists = false;
                    }

                    if (m_gaiaGlobalCameraExists)
                    {
                        currentDistance = Vector3.Distance(GaiaGlobal.Instance.m_mainCamera.transform.position, gameObject.transform.position);
                        DistanceApprove();
                    }
                }

                if (m_managerExists)
                {
                    if (ReflectionProbeManager.Instance.m_probeCullingNeedsUpdating)
                    {
                        if (m_syncToManager)
                        {
                            m_systemActive = ReflectionProbeManager.Instance.UseReflectionProbeCuller;
                            m_maxRenderDistance = ReflectionProbeManager.Instance.ReflectionProbeCullingDistance;
                        }
                    }
                }

                InvokeRepeating("DistanceCheck", 0.05f, 0.15f);
            }
        }
        /// <summary>
        /// Does distance need to be checked?
        /// </summary>
        private void DistanceCheck()
        {
            if (!m_systemActive)
            {
                return;
            }

            currentCheckPriod++;
            if (currentCheckPriod > checkPeriod)
            {
                if (m_gaiaGlobalCameraExists && m_probeExists)
                {
                    DistanceApprove();
                }
            }
        }
        /// <summary>
        /// Used to recalculate the distance
        /// </summary>
        private void DistanceCalculate()
        {
            currentDistance = Vector3.Distance(GaiaGlobal.Instance.m_mainCamera.transform.position, gameObject.transform.position);
        }
        /// <summary>
        /// Recalculate the distance check to check if particle system need to be enabled or not
        /// </summary>
        private void DistanceApprove()
        {
            currentCheckPriod = 0;
            DistanceCalculate();
            if (currentDistance < m_maxRenderDistance)
            {
                if (m_probeExists && m_probe.isActiveAndEnabled)
                {
                    m_probe.enabled = true;
                    m_probe.RenderProbe();
                }
            }
            else
            {
                if (m_probeExists)
                {
                    m_probe.enabled = false;
                }
            }

            checkPeriod = UnityEngine.Random.Range(CheckPeriodMin, checkPeriodMax);
        }

        #endregion
    }
}
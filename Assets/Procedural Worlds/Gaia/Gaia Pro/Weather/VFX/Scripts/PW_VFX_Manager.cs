using UnityEngine;

namespace Gaia
{
    [ExecuteInEditMode]
    public class PW_VFX_Manager : MonoBehaviour
    {
        #region Public Variables

        [Header("VFX Manager Setup")]
        public ParticleSystem TargetParticleSystem;
        public float RenderDistance = 128f;
        public int CheckPeriodMin = 15;
        public int checkPeriodMax = 30;

        private float checkPeriod;
        private bool m_particlesExist;
        private bool m_gaiaGlobalCameraExists;

        #endregion

        #region Private Variables

        [SerializeField]
        [HideInInspector]
        private ParticleSystem.EmissionModule EmissionModule;
        private float currentDistance = 256f;
        private int currentCheckPriod = 0;

        #endregion

        #region Unity Functions

        /// <summary>
        /// Used to initialize the system
        /// </summary>
        private void Start()
        {
            if (Application.isPlaying)
            {
                checkPeriod = UnityEngine.Random.Range(CheckPeriodMin, checkPeriodMax);
                Initialize();
                InvokeRepeating("DistanceCheck", 0.05f, 0.15f);
            }
        }
        /// <summary>
        /// Initialize when it's not playing application
        /// </summary>
        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                Initialize();
                CancelInvoke("DistanceCheck");
            }
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Function used to initialize the systems on this script
        /// </summary>
        private void Initialize()
        {
            currentCheckPriod = 0;
            if (TargetParticleSystem == null)
            {
                TargetParticleSystem = gameObject.GetComponent<ParticleSystem>();
            }

            if (TargetParticleSystem != null)
            {
                m_particlesExist = true;
                EmissionModule = TargetParticleSystem.emission;
                if (Application.isPlaying)
                {
                    EmissionModule.enabled = false;
                }
                else
                {
                    EmissionModule.enabled = true;
                }

            }

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
        }
        /// <summary>
        /// Does distance need to be checked?
        /// </summary>
        private void DistanceCheck()
        {
            currentCheckPriod++;
            if (currentCheckPriod > checkPeriod)
            {
                if (m_gaiaGlobalCameraExists && m_particlesExist)
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
            if (currentDistance < RenderDistance)
            {
                if (m_particlesExist)
                {
                    if (!TargetParticleSystem.isPlaying)
                    {
                        TargetParticleSystem.Play();
                    }

                    EmissionModule.enabled = true;
                }
            }
            else
            {
                if (m_particlesExist)
                {
                    if (TargetParticleSystem.isPlaying)
                    {
                        TargetParticleSystem.Stop();
                    }

                    EmissionModule.enabled = false;
                }
            }

            checkPeriod = UnityEngine.Random.Range(CheckPeriodMin, checkPeriodMax);
        }

        #endregion
    }
}
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    public class InteriorWeatherController : MonoBehaviour
    {
        public CollisionDetectionType TriggerType
        {
            get { return m_triggerType; }
            set
            {
                if (m_triggerType != value)
                {
                    m_triggerType = value;
                    UpdateColliderType();
                }
            }
        }
        public Vector3 TriggerSize
        {
            get { return m_triggerSize; }
            set
            {
                if (m_triggerSize != value)
                {
                    m_triggerSize = value;
                    UpdateColliderType();
                }
            }
        }
        public float TriggerRadius
        {
            get { return m_triggerRadius; }
            set
            {
                if (m_triggerRadius != value)
                {
                    m_triggerRadius = value;
                    UpdateColliderType();
                }
            }
        }

        public AudioReverbPreset m_interiorAudioRevertPreset = AudioReverbPreset.Room;
        public AudioReverbPreset m_exteriorAudioRevertPreset = AudioReverbPreset.Forest;
        public bool m_enableWeatherParticleColliders = true;
        public ParticleSystemCollisionQuality m_colliderQuality = ParticleSystemCollisionQuality.High;
        public LayerMask m_collideLayers = 1;

        [SerializeField]
        private CollisionDetectionType m_triggerType = CollisionDetectionType.Box;
        [SerializeField]
        private Vector3 m_triggerSize = new Vector3(30f, 30f, 30f);
        [SerializeField]
        private float m_triggerRadius = 30f;

        [SerializeField]
        private Camera m_playerCamera;
        [SerializeField]
        private AudioReverbFilter m_reverbFilter;
        [SerializeField]
        private GameObject m_rainParticles;
        [SerializeField]
        private GameObject m_snowParticles;


        private const string m_createVolumeMenuItem = "GameObject/Procedural Worlds/Gaia/Interior Weather Volume";

        #region Unity Functions

        /// <summary>
        /// Draw gizmo when object is selected
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Gizmos.color = new Color(1f, 0.137112f, 0f, 0.4f);
                Gizmos.matrix = gameObject.transform.localToWorldMatrix;
                Gizmos.DrawCube(Vector3.zero, boxCollider.size);
            }

            SphereCollider sphereCollider = gameObject.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                Gizmos.color = new Color(1f, 0.137112f, 0f, 0.4f);
                Gizmos.matrix = gameObject.transform.localToWorldMatrix;
                Gizmos.DrawSphere(Vector3.zero, sphereCollider.radius);
            }
        }

        /// <summary>
        /// Load on enable
        /// </summary>
        private void OnEnable()
        {
            SetupReverbFilter();
        }

        /// <summary>
        /// Sets interior reverb on enter
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (m_playerCamera == null)
            {
                Debug.LogError("Player was not found");
                return;
            }

            if (other.tag == m_playerCamera.tag)
            {
                m_reverbFilter.reverbPreset = m_interiorAudioRevertPreset;
                UpdateParticleColliders(true);
            }
        }

        /// <summary>
        /// Sets exterior reverb on exit
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            if (m_playerCamera == null)
            {
                Debug.LogError("Player was not found");
                return;
            }

            if (other.tag == m_playerCamera.tag)
            {
                m_reverbFilter.reverbPreset = m_exteriorAudioRevertPreset;
                UpdateParticleColliders(false);
            }
        }

        #endregion

        #region Utils

        /// <summary>
        /// Sets up the player and the audio reverb
        /// </summary>
        public void SetupReverbFilter()
        {
            if (m_playerCamera == null)
            {
                m_playerCamera = ProceduralWorldsGlobalWeather.GetPlayer().GetComponent<Camera>();
            }

            if (m_playerCamera != null)
            {
                m_reverbFilter = m_playerCamera.gameObject.GetComponent<AudioReverbFilter>();
                if (m_reverbFilter == null)
                {
                    m_reverbFilter = m_playerCamera.gameObject.AddComponent<AudioReverbFilter>();
                    m_reverbFilter.reverbPreset = AudioReverbPreset.Forest;
                }
            }

            if (m_rainParticles == null)
            {
                m_rainParticles = GameObject.Find("Gaia Rain Particles");
            }

            if (m_snowParticles == null)
            {
                m_snowParticles = GameObject.Find("Gaia Snow Particles");
            }
        }

        /// <summary>
        /// Updates the collider settings
        /// </summary>
        private void UpdateColliderType()
        {
            switch (TriggerType)
            {
                case CollisionDetectionType.Box:
                    AddBoxCollider();
                    break;
                case CollisionDetectionType.Spherical:
                    AddSphericalCollider();
                    break;
            }
        }

        /// <summary>
        /// Adds and configures the box collider
        /// </summary>
        private void AddBoxCollider()
        {
            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider>();
            }

            boxCollider.isTrigger = true;
            boxCollider.size = TriggerSize;

            SphereCollider sphereCollider = gameObject.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(sphereCollider);
                }
                else
                {
                    DestroyImmediate(sphereCollider);
                }
            }
        }

        /// <summary>
        /// Adds and configures the spherical collider
        /// </summary>
        private void AddSphericalCollider()
        {
            SphereCollider sphereCollider = gameObject.GetComponent<SphereCollider>();
            if (sphereCollider == null)
            {
                sphereCollider = gameObject.AddComponent<SphereCollider>();
            }

            sphereCollider.isTrigger = true;
            sphereCollider.radius = TriggerRadius;

            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(boxCollider);
                }
                else
                {
                    DestroyImmediate(boxCollider);
                }
            }
        }

        /// <summary>
        /// Used to update the particles collision conditions
        /// </summary>
        /// <param name="enabled"></param>
        private void UpdateParticleColliders(bool enabled)
        {
            if (m_enableWeatherParticleColliders)
            {
                if (m_rainParticles == null)
                {
                    m_rainParticles = GameObject.Find("Gaia Rain Particles");
                }

                if (m_snowParticles == null)
                {
                    m_snowParticles = GameObject.Find("Gaia Snow Particles");
                }

                if (m_rainParticles != null)
                {
                    ParticleSystem rainSystem = m_rainParticles.GetComponent<ParticleSystem>();
                    if (rainSystem != null)
                    {
                        ParticleSystem.CollisionModule collision = rainSystem.collision;
                        if (enabled)
                        {
                            collision.enabled = true;
                            collision.type = ParticleSystemCollisionType.World;
                            collision.quality = m_colliderQuality;
                            collision.collidesWith = m_collideLayers;
                        }
                        else
                        {
                            collision.enabled = false;
                        }
                    }
                }

                if (m_snowParticles != null)
                {
                    ParticleSystem rainSystem = m_snowParticles.GetComponent<ParticleSystem>();
                    if (rainSystem != null)
                    {
                        ParticleSystem.CollisionModule collision = rainSystem.collision;
                        if (enabled)
                        {
                            collision.enabled = true;
                            collision.type = ParticleSystemCollisionType.World;
                            collision.quality = m_colliderQuality;
                            collision.collidesWith = m_collideLayers;
                        }
                        else
                        {
                            collision.enabled = false;
                        }
                    }
                }
            }
        }

        #endregion

        #region Public Static Functions

#if UNITY_EDITOR
        /// <summary>
        /// Function used to create a new controller volume
        /// </summary>
        [MenuItem(m_createVolumeMenuItem)]
        public static void CreateInteriorWeatherControllerVolume()
        {
            GameObject newVolumeObject = new GameObject("New Gaia Interior Weather Volume");
            newVolumeObject.transform.position = SceneView.lastActiveSceneView.camera.transform.position;

            InteriorWeatherController interiorController = newVolumeObject.AddComponent<InteriorWeatherController>();
            interiorController.SetupReverbFilter();

            BoxCollider boxCollider = newVolumeObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(30f, 30f, 30f);
            boxCollider.isTrigger = true;

            Selection.activeObject = newVolumeObject;
            EditorGUIUtility.PingObject(newVolumeObject);
            SceneView.lastActiveSceneView.FrameSelected();
        }
#endif

        #endregion
    }
}
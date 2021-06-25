using System.Collections.Generic;
using UnityEngine;

namespace Gaia
{
    public class ThunderStrike : MonoBehaviour
    {
        #region Public Variables

        [HideInInspector]
        public bool m_playThunder
        {
            get { return PlayThunder; }
            set
            {
                if (PlayThunder != value)
                {
                    PlayThunder = value;
                    if (m_player != null)
                    {
                        CreateLocation(m_player.position);
                    }
                }
            }
        }
        [HideInInspector]
        public Transform m_player;
        [HideInInspector]
        public Color m_strikeColor = Color.blue;
        [HideInInspector]
        public float m_strikeIntenisty = 1f;
        [HideInInspector]
        public List<AudioClip> m_thunderStrikeAudios = new List<AudioClip>();
        [HideInInspector]
        public float m_volume = 1f;
        [HideInInspector]
        public float m_thunderLightRadius = 300f;

        #endregion

        #region Private Variables

        [HideInInspector]
        [SerializeField]
        private ProceduralWorldsGlobalWeather GlobalWeather;
        [HideInInspector]
        [SerializeField]
        private AudioSource m_thunderAudioSource;
        [HideInInspector]
        [SerializeField]
        private bool PlayThunder;
        [HideInInspector]
        [SerializeField]
        private Light ThunderLight;
        [HideInInspector]
        [SerializeField]
        private Material ThunderMaterial;
        [HideInInspector]
        [SerializeField]
        private bool IsPlaying;

        #endregion

        #region Unity Functions

        private void Update()
        {
            if (IsPlaying)
            {
                if (ThunderLight != null)
                {
                    ThunderLight.intensity = Mathf.Lerp(ThunderLight.intensity, 0f, Time.deltaTime * 2);
                    if (ThunderLight.intensity < 0.15f)
                    {
                        ThunderLight.intensity = 0f;
                        IsPlaying = false;
                    }
                }
            }
        }

        private void Start()
        {
            GetAllVariables();
        }

        #endregion

        #region Public Functions

        public void GetAllVariables()
        {
            if (m_thunderAudioSource == null)
            {
                m_thunderAudioSource = GetOrCreateAudioSource();
            }

            if (ThunderLight == null)
            {
                ThunderLight = GetOrCreateLightSource();
            }

            //Loads from the weather manager system
            if (GlobalWeather == null)
            {
                GlobalWeather = FindObjectOfType<ProceduralWorldsGlobalWeather>();
            }

            if (GlobalWeather != null)
            {
                m_player = GlobalWeather.m_player;
                m_volume = GlobalWeather.m_thunderVolume * GlobalWeather.GlobalVolume;
                m_strikeColor = GlobalWeather.ThunderLightColor;
                m_strikeIntenisty = GlobalWeather.ThunderLightIntensity;
                m_thunderStrikeAudios = GlobalWeather.ThunderAudioSources;
                m_thunderLightRadius = GlobalWeather.ThunderLightRadius;
            }
        }

        public void PlayThunderSystem()
        {
            GetAllVariables();
            Vector3 location = Vector3.zero;
            if (m_player != null)
            {
                location = CreateLocation(m_player.position);
            }

            if (!IsPlaying)
            {
                gameObject.transform.position = location;
                if (ThunderLight != null)
                {
                    ThunderLight.color = m_strikeColor;
                    ThunderLight.intensity = m_strikeIntenisty;
                    ThunderLight.range = m_thunderLightRadius;
                    PlaySound();
                    IsPlaying = true;
                }
            }
        }

        #endregion

        #region Private Functions

        private void OnEnable()
        {
            GetAllVariables();
        }

        private void PlaySound()
        {
            if (m_thunderStrikeAudios == null)
            {
                return;
            }

            if (m_thunderStrikeAudios.Count > 0)
            {
                int random = Random.Range(1, m_thunderStrikeAudios.Count);
                if (m_thunderAudioSource != null)
                {
                    m_thunderAudioSource.PlayOneShot(m_thunderStrikeAudios[random], m_volume);
                }
            }
        }

        private Vector3 CreateLocation(Vector3 playerLosition)
        {
            float newPositionX = 0f;
            float newPositionY = 0f;
            float newPositionZ = 0f;

            float randomDirectionRange = Random.value;
            if (randomDirectionRange > 0.5f)
            {
                newPositionX = Random.Range(playerLosition.x, playerLosition.x + 700f);
                newPositionY = Random.Range(playerLosition.y + 50f, playerLosition.y + 250f);
                newPositionZ = Random.Range(playerLosition.z, playerLosition.z + 700f);
            }
            else
            {
                newPositionX = Random.Range(playerLosition.x, playerLosition.x - 700f);
                newPositionY = Random.Range(playerLosition.y + 50f, playerLosition.y + 250f);
                newPositionZ = Random.Range(playerLosition.z, playerLosition.z - 700f);
            }

            Vector3 newPosition = new Vector3(newPositionX, newPositionY, newPositionZ);
            return newPosition;
        }

        private AudioSource GetOrCreateAudioSource()
        {
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            return audioSource;
        }

        private Light GetOrCreateLightSource()
        {
            Light light = gameObject.GetComponent<Light>();
            if (light == null)
            {
                light = gameObject.AddComponent<Light>();
            }

            return light;
        }

        #endregion
    }
}
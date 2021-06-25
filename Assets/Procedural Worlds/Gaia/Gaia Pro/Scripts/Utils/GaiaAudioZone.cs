using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Gaia
{
	[System.Serializable]
	public class GaiaAudioZoneItem
	{
		public AudioClip m_audioClip;
		public float m_volume = 1f;
		public float m_fadeInTime = 5f;
		public float m_fadeOutTime = 5f;
	}

	/// <summary>
	/// This class will play back a set of audio clip in a random order over time.  Clips will never play twice in a row, unless there is only 1 clip.
	/// Audio zones are assumed to be non movable objects. If you move it then you will need to update its m_position variable.
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
    [ExecuteInEditMode]
    [Serializable]
	public class GaiaAudioZone : MonoBehaviour		
	{
        /// <summary>
        /// Audiozone states
        /// </summary>
        public enum  AudioZoneState
        {
            Active,
            BecomingInactive,
            Inactive
        }

		/// <summary>
		/// Current zone volume taking into account fade in and out
		/// </summary>
		public float m_currentZoneVolume = 1;

		/// <summary>
		/// This is the list of audio clips we can play
		/// </summary>
		public List<GaiaAudioZoneItem> m_audioList = new List<GaiaAudioZoneItem>();

        /// <summary>
        /// Currently playing audio item
        /// </summary>
        [NonSerialized]
        public GaiaAudioZoneItem m_currentAudioItem = null;

		/// <summary>
		/// Global audio source, or trigger based
		/// </summary>
		public bool m_isGlobalAudioSource = false;

		/// <summary>
		/// Minimum time in seconds to take a break between tracks
		/// </summary>
		public float m_minimumBreakTime = 5f;

		/// <summary>
		/// Maximum time in seconds to take a break between tracks
		/// </summary>
		public float m_maximumBreakTime = 10f;

        /// <summary>
        /// Set when the audio zone is active - in range of player
        /// </summary>
        [NonSerialized]
        public AudioZoneState m_audioZoneState = AudioZoneState.Inactive;

        /// <summary>
        /// Audio source for this component
        /// </summary>
        [NonSerialized]
		private AudioSource m_audioSource = null;

        /// <summary>
        /// Whether or not audio is currently playing
        /// </summary>
        [NonSerialized]
        public bool m_audioIsPlaying = false;

        /// <summary>
        /// Currently selected track index
        /// </summary>
        [NonSerialized]
        public int m_selectedTrackIndex = -1;

        /// <summary>
        /// Whether or not to draw gizmos
        /// </summary>
        [NonSerialized]
        public bool m_showGizmos = false;

        /// <summary>
        /// Gizmo color
        /// </summary>
        public Color m_gizmoColor = new Color(1f, 0.137112f, 0f, 0.4f);

        /// <summary>
        /// What the zone radius is for this audio zone (if its not a global audio zone)
        /// </summary>
        public float m_zoneRadius = 30f;

        /// <summary>
        /// What the zone radius squared is for this audio zone (if its not a global audio zone)
        /// </summary>
        private float m_zoneRadiusSqr = 30f * 30f;

        /// <summary>
        /// The time at which a play state change happened
        /// </summary>
        private float m_timeWhenTrackStarted = 0f;
        private float m_timeWhenFadeInEnds = 0f;
        private float m_timeWhenFadeOutBegins = 0f;
        private float m_timeWhenFadeOutEnds = 0f;
        private float m_timeWhenNextTrackStarts = 0f;
        private float m_timeWhenDeactivating = 0f;

        /// <summary>
        /// Set to true if viewing debug messages
        /// </summary>
        private bool m_showDebug = false;

        /// <summary>
        /// Amount of time it takes to deactivate a zone in seconds
        /// </summary>
        private float m_deactivationTime = 10f;

        #region Unity Methods

        /// <summary>
        /// Draw gizmo when object is selected
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (m_showGizmos)
            {
                if (Application.isPlaying)
                {
                    if (m_audioZoneState == AudioZoneState.Active)
                    {
                        DrawGizmos();
                    }
                }
                else
                {
                    DrawGizmos();
                }
            }
        }

        /// <summary>
        /// Draw gizmo when object is selected
        /// </summary>
        private void OnDrawGizmos()
        {
            OnDrawGizmosSelected();
        }

        /// <summary>
        /// Draw gizmos
        /// </summary>
        private void DrawGizmos()
        {
            Color oldColor = Gizmos.color;
            Gizmos.color = m_gizmoColor;
            if (m_isGlobalAudioSource)
            {
                Gizmos.DrawSphere(gameObject.transform.position, 50f);
            }
            else
            {
                Gizmos.DrawSphere(gameObject.transform.position, m_zoneRadius);
            }
            Gizmos.color = oldColor;
        }

        /// <summary>
        /// On enable
        /// </summary>
        private void OnEnable()
        {
            //Setup audio source
            m_audioSource = GetComponent<AudioSource>();

            //Remove sphere collider if present - this is a hangover from the original implementation, added for backwards compatibility.
            SphereCollider sc = GetComponent<SphereCollider>();
            if (sc != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(sc);
                }
                else
                {
                    GameObject.DestroyImmediate(sc);
                }
            }
        }

        /// <summary>
        /// When the audio zone is enabled
        /// </summary>
        private void Start()
        {
            //Initialize settings to stopped / inactive
            Initialize();

            //Register this audiozone for potential activation
            if (GaiaAudioManager.Instance != null)
            {
                GaiaAudioManager.Instance.RegisterAudioZone(this);
            }
        }
        
        /// <summary>
        /// Called when audiozone is destroyed
        /// </summary>
        private void OnDestroy()
        {
            if (GaiaAudioManager.Instance != null)
            {
                GaiaAudioManager.Instance.DeRegisterAudioZone(this);
            }
        }

        #endregion

        #region Audio Zone Methods

        /// <summary>
        /// Initialize the audio zone
        /// </summary>
        public void Initialize()
        {
            //Set sqr for sqr distance check
            m_zoneRadiusSqr = m_zoneRadius * m_zoneRadius;

            //Set state to inactive
            m_audioZoneState = AudioZoneState.Inactive;

            //Make sure everything is shut down
            if (m_audioSource == null)
            {
                m_audioSource = GetComponent<AudioSource>();
            }
            m_audioSource.Stop();
            m_audioSource.volume = 0f;
            m_audioIsPlaying = false;

            //Set up audio source settings
            if (!m_isGlobalAudioSource)
            {
                m_audioSource.spatialBlend = 1f;
                m_audioSource.maxDistance = m_zoneRadius;
                m_audioSource.rolloffMode = AudioRolloffMode.Linear;
            }
            else
            {
                m_audioSource.spatialBlend = 0f;
            }

            //Set up time until next track
            m_timeWhenNextTrackStarts = Time.time;
        }

        /// <summary>
        /// Called by GaiaAudioManager to handle zone updates - only called when its active
        /// </summary>
        public void ProcessActiveUpdate(float masterVolume, bool showGizmos)
        {
            //Set gizmo state
            m_showGizmos = showGizmos;

            float currentTime = Time.time;
            if (currentTime > m_timeWhenNextTrackStarts)
            {
                //Otherwise choose a new track
                PlayNextRandomTrack();
            }

            //Now update the volume
            if (currentTime < m_timeWhenFadeInEnds)
            {
                float deltaTime = m_timeWhenFadeInEnds - m_timeWhenTrackStarted;
                if (deltaTime > 0f)
                {
                    m_currentZoneVolume = Mathf.Lerp(m_currentAudioItem.m_volume, 0f, (m_timeWhenFadeInEnds - currentTime) / deltaTime);
                }
                else
                {
                    m_currentZoneVolume = m_currentAudioItem.m_volume;
                }
                if (m_currentZoneVolume > masterVolume)
                {
                    m_currentZoneVolume = masterVolume;
                }
                m_audioSource.volume = m_currentZoneVolume;
            }
            else if (currentTime < m_timeWhenFadeOutBegins)
            {
                m_currentZoneVolume = m_currentAudioItem.m_volume;
                if (m_currentZoneVolume > masterVolume)
                {
                    m_currentZoneVolume = masterVolume;
                }
                m_audioSource.volume = m_currentZoneVolume;
            }
            else if (currentTime < m_timeWhenFadeOutEnds)
            {
                float deltaTime = m_timeWhenFadeOutEnds - m_timeWhenFadeOutBegins;
                if (deltaTime > 0f)
                {
                    m_currentZoneVolume = Mathf.Lerp(0f, m_currentAudioItem.m_volume, (m_timeWhenFadeOutEnds - currentTime) / deltaTime);
                }
                else
                {
                    m_currentZoneVolume = 0f;
                }
                if (m_currentZoneVolume > masterVolume)
                {
                    m_currentZoneVolume = masterVolume;
                }
                m_audioSource.volume = m_currentZoneVolume;
            }
            //Or stop
            else if (m_audioIsPlaying)
            {
                Stop();
            }
        }

        /// <summary>
        /// Plays the next random track from the list, but make sure the same track doesn't play twice in a row
        /// if at all possible.
        /// </summary>
        public void PlayNextRandomTrack()
        {
            // This will pick a random sound from the list, but make sure the same track doesnt play twice in a row
            int newTrackIdx = UnityEngine.Random.Range(0, m_audioList.Count);
            if (newTrackIdx == m_selectedTrackIndex)
            {
                newTrackIdx = m_selectedTrackIndex + 1;
                if (newTrackIdx >= m_audioList.Count)
                {
                    newTrackIdx = 0;
                }
            }
            PlayTrack(newTrackIdx);
        }

        /// <summary>
        /// Plays the selected track
        /// </summary>
        public void PlayTrack(int trackIdx)
        {
            if (m_audioList.Count == 0)
            {
                Debug.LogWarning("GaiaAudioZone : Cannot play selected track, no tracks have been added!");
                return;
            }

            m_selectedTrackIndex = trackIdx;
            if (m_selectedTrackIndex >= m_audioList.Count)
            {
                Debug.LogWarning("GaiaAudioZone : Invalid track index selected, selecting first track instead!");
                m_selectedTrackIndex = 0;
            }

            //Set current clip
            m_currentAudioItem = m_audioList[m_selectedTrackIndex];
            m_audioSource.clip = m_currentAudioItem.m_audioClip;

            //Setup times for fades, tracks etc
            m_timeWhenTrackStarted = Time.time;
            m_timeWhenFadeInEnds = m_timeWhenTrackStarted + m_currentAudioItem.m_fadeInTime;
            m_timeWhenFadeOutBegins = m_timeWhenTrackStarted + m_currentAudioItem.m_audioClip.length - m_currentAudioItem.m_fadeOutTime;
            m_timeWhenFadeOutEnds = m_timeWhenTrackStarted + m_currentAudioItem.m_audioClip.length;
            m_timeWhenNextTrackStarts = m_timeWhenFadeOutEnds + Random.Range(m_minimumBreakTime, m_maximumBreakTime);

            //Set initial volume
            if (m_currentAudioItem.m_fadeInTime == 0)
            {
                m_currentZoneVolume = m_currentAudioItem.m_volume;
            }
            else
            {
                m_currentZoneVolume = 0f;
            }

            //Start playing
            m_audioSource.Play();
            m_audioIsPlaying = true;

            //Tell world
            if (m_showDebug)
            {
                Debug.Log("Playing " + m_audioSource.clip + " playstate " + m_audioSource.isPlaying);
            }
        }

        /// <summary>
        /// Check to see if the zone is in range of the position given - if in range then it should become active.
        /// Uses sqr magnitude for efficiency.
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>True if the zone is deemed to be in range.</returns>
        public bool InRange(Vector3 position)
        {
            if (m_isGlobalAudioSource)
            {
                return true;
            }

            if ((transform.position - position).sqrMagnitude <= m_zoneRadiusSqr)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Stop the currently playing track
        /// </summary>
        public void Stop()
        {
            if (m_audioSource != null)
            {
                m_audioSource.Stop();
                m_audioSource.volume = 0f;
            }

            m_currentZoneVolume = 0f;
            m_audioIsPlaying = false;
        }

        public void Activate()
        {
            if (m_audioZoneState == AudioZoneState.Inactive)
            {
                m_audioZoneState = AudioZoneState.Active;
                PlayNextRandomTrack();
            }
            else if (m_audioZoneState == AudioZoneState.BecomingInactive)
            {
                m_audioZoneState = AudioZoneState.Active;
                m_timeWhenDeactivating = 0f;
            }
        }

        /// <summary>
        /// Flag the audiozone for deactivation in 10 seconds - we use a delay for when people are on the border of a zone
        /// to stop weird sound crossovers
        /// </summary>
        public void FlagForDeactivation()
        {
            m_audioZoneState = AudioZoneState.BecomingInactive;
            m_timeWhenDeactivating = Time.time + m_deactivationTime;
        }

        /// <summary>
        /// Whether or not it can be deactivated now
        /// </summary>
        /// <returns></returns>
        public bool CanDeactivateNow()
        {
            if (Time.time > m_timeWhenDeactivating)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deactivate this audio zone
        /// </summary>
        public void Deactivate()
        {
            m_audioZoneState = AudioZoneState.Inactive;
            Stop();
        }

        #endregion
    }
}	
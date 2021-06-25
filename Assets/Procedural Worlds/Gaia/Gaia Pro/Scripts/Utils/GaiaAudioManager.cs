using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gaia
{        
	[ExecuteInEditMode]
	[Serializable]
	public class GaiaAudioManager : MonoBehaviour
	{
        /// <summary>
        /// Master volume of everything
        /// </summary>
        public float m_masterVolume = 1f;

		/// <summary>
		/// Volume for weather system.
		/// </summary>
		public float m_rainVolume = .5f;
		public float m_snowVolume = .5f;
		public float m_thunderVolume = .5f; 

		/// <summary>
		/// Target player
		/// </summary>
		public GameObject m_player;

		/// <summary>
		/// Active audio zones that should be in process of playing something
		/// </summary>
		[NonSerialized]
		public List<GaiaAudioZone> m_activeAudioZones = new List<GaiaAudioZone>();

		/// <summary>
		/// Audiozones slated for deactivation
		/// </summary>
		[NonSerialized]
        public List<GaiaAudioZone> m_becomingInActiveAudioZones = new List<GaiaAudioZone>();

        /// <summary>
        /// Currently inactive audiozones
        /// </summary>
        [NonSerialized]
        public List<GaiaAudioZone> m_inactiveAudioZones = new List<GaiaAudioZone>();

        /// <summary>
        /// Show zone gizmos
        /// </summary>
        public bool m_showGizmos = false;

		/// <summary>
		/// Single instance of audio manager in the scene
		/// </summary>
		private static GaiaAudioManager m_instance = null;

		/// <summary>
		/// Current audio manager instance
		/// </summary>
		public static GaiaAudioManager Instance
		{
			get
			{
				return m_instance;
			}
		}

		/// <summary>
		/// When scene loads set up our singleton instance
		/// </summary>
		private void Awake()
        {
            if (m_instance == null)
            {
                m_instance = this;
            }
        }

        /// <summary>
        /// Set up our singleton instance
        /// </summary>
        private void OnEnable()
        {
            if (m_instance == null)
            {
                m_instance = this;
            }
        }

        /// <summary>
        /// Start up our audio zone state change management system when scene starts playing
        /// </summary>
        private void Start()
        {
            StartCoroutine("UpdateAudioZoneStates");
        }

		/// <summary>
		/// Process active audio zone updates
		/// </summary>
		private void Update()
		{
            //Only valid in play mode
            if (!Application.isPlaying)
            {
                return;
            }

            //Only valid if we have a player
            if (m_player == null)
            {
                return;
            }

			// Process active zones
            int zonesUpdated = 0;
			for (int currentZone = 0; currentZone < m_activeAudioZones.Count; currentZone++)
			{
				GaiaAudioZone az = m_activeAudioZones[currentZone];
				if (az != null)
				{
					az.ProcessActiveUpdate(m_masterVolume, m_showGizmos);
                    zonesUpdated++;
				}
            }

            #if UNITY_EDITOR
            if (zonesUpdated > 0)
            {
                if (Selection.activeGameObject == this.gameObject)
                {
                    EditorUtility.SetDirty(this);
				}
			}
            #endif
		}

		/// <summary>
		/// Set the gizmo state on the audio zones - would only ever be called in editor so it can be less efficient
		/// </summary>
		public void UpdateSceneGizmos()
        {
            GaiaAudioZone[] audioZones = GameObject.FindObjectsOfType<GaiaAudioZone>();
            foreach(GaiaAudioZone audioZone in audioZones)
            {
                if (audioZone != null)
                {
                    audioZone.m_showGizmos = m_showGizmos;
				}
			}
        }

        /// <summary>
        /// Register an audiozone. Always added to inactive queue first.
        /// </summary>
        /// <param name="az">Audiozone to register</param>
        public void RegisterAudioZone(GaiaAudioZone az)
        {
            m_activeAudioZones.Remove(az);
            m_becomingInActiveAudioZones.Remove(az);
            if (!m_inactiveAudioZones.Contains(az))
            {
                m_inactiveAudioZones.Add(az);
            }
        }

        /// <summary>
        /// DeRegister an audiozone. 
        /// </summary>
        /// <param name="az">Audiozone to deregister</param>
        public void DeRegisterAudioZone(GaiaAudioZone az)
        {
            //DeRegister if not there already
            az.Stop();
            m_inactiveAudioZones.Remove(az);
            m_activeAudioZones.Remove(az);
            m_becomingInActiveAudioZones.Remove(az);
        }

        /// <summary>
        /// Activate an audiozone. Will check so that a zone is not activated twice
        /// </summary>
        /// <param name="az">Audiozone to activate</param>
        protected void ActivateAudioZone(GaiaAudioZone az)
        {
            if (!m_activeAudioZones.Contains(az))
            {
                az.Activate();
				m_activeAudioZones.Add(az);
                m_becomingInActiveAudioZones.Remove(az);
                m_inactiveAudioZones.Remove(az);
            }
        }

        /// <summary>
        /// Signal an audiozone for deactivation 
        /// </summary>
        /// <param name="az">Audiozone to deactivate</param>
        protected void FlagAudioZoneForDeactivation(GaiaAudioZone az)
        {
            if (!m_becomingInActiveAudioZones.Contains(az))
            {
                az.FlagForDeactivation();
                m_becomingInActiveAudioZones.Add(az);
                m_activeAudioZones.Remove(az);
                m_inactiveAudioZones.Remove(az);
            }
        }

        /// <summary>
        /// Queue an audiozone for deactivation at the end of its current loop
        /// </summary>
        /// <param name="az">Audiozone to activate</param>
        protected void DeActivateAudioZone(GaiaAudioZone az)
        {
            if (!m_inactiveAudioZones.Contains(az))
            {
                az.Deactivate();
                m_inactiveAudioZones.Add(az);
                m_activeAudioZones.Remove(az);
                m_becomingInActiveAudioZones.Remove(az);
            }
        }

        /// <summary>
        /// Process state changes - relatively expensive so it is limited to periodic co-routine
        /// </summary>
        private IEnumerator UpdateAudioZoneStates()
        {
            int maxUpdatesPerIteration = 20;
            while (true)
            {
                //Only valid if we have a player
                if (m_player != null)
                {
                    int currentZone = 0;
                    int currentIteration = 0;
                    Vector3 playerLocation = m_player.transform.position;

                    //Process active zones
                    currentZone = 0;
                    while (currentZone < m_activeAudioZones.Count)
                    {
                        GaiaAudioZone zone = m_activeAudioZones[currentZone];

                        //Handle deleted / null zones
                        if (zone == null)
                        {
                            m_activeAudioZones.RemoveAt(currentZone);
                        }
                        //Check / update zone state
                        else
                        {
                            //Always update its gizmos
                            zone.m_showGizmos = m_showGizmos;

                            //Are we not in range - then flag it for deactivation - results in removal from list
                            if (!zone.InRange(playerLocation))
                            {
                                FlagAudioZoneForDeactivation(zone);
                            }
                            else
                            {
                                //Do nothing and step to next
                                currentZone++;
                            }

                            //Take a little break if necessary
                            currentIteration++;
                            if (currentIteration > maxUpdatesPerIteration)
                            {
                                currentIteration = 0;
                                yield return new WaitForSeconds(0.2f);
                            }
                        }
                    }

                    //Process zones flagged for deactivation
                    currentZone = 0;
                    while (currentZone < m_becomingInActiveAudioZones.Count)
                    {
                        GaiaAudioZone zone = m_becomingInActiveAudioZones[currentZone];

                        //Handle deleted / null zones
                        if (zone == null)
                        {
                            m_becomingInActiveAudioZones.RemoveAt(currentZone);
                        }
                        //Check / update zone state
                        else
                        {
                            //Always update its gizmos
                            zone.m_showGizmos = m_showGizmos;

                            //Are we in range - then make it active - results in removal from list
                            if (zone.InRange(playerLocation))
                            {
                                ActivateAudioZone(zone);
                            }
                            else
                            {
                                //See if it can deactivate - results in removal from list
                                if (zone.CanDeactivateNow())
                                {
                                    DeActivateAudioZone(zone);
                                }
                                //Do nothing and step to next
                                else
                                {
                                    currentZone++;
                                }
                            }

                            //Take a little break if necessary
                            currentIteration++;
                            if (currentIteration > maxUpdatesPerIteration)
                            {
                                currentIteration = 0;
                                yield return new WaitForSeconds(0.2f);
                            }
                        }
                    }

                    //Process inactive zones
                    currentZone = 0;
                    while (currentZone < m_inactiveAudioZones.Count)
                    {
                        GaiaAudioZone zone = m_inactiveAudioZones[currentZone];

                        //Handle deleted / null zones
                        if (zone == null)
                        {
                            m_inactiveAudioZones.RemoveAt(currentZone);
                        }
                        //Check / update zone state
                        else
                        {
                            //Always update its gizmos
                            zone.m_showGizmos = m_showGizmos;

                            //Are we in range - then make it active - results in removal from list
                            if (zone.InRange(playerLocation))
                            {
                                ActivateAudioZone(zone);
                            }
                            else
                            {
                                currentZone++;
                            }

                            //Take a little break if necessary
                            currentIteration++;
                            if (currentIteration > maxUpdatesPerIteration)
                            {
                                currentIteration = 0;
                                yield return new WaitForSeconds(0.2f);
                            }
                        }
                    }
                }

                //Always take a break at the end of each major loop
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}
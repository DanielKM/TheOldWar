using UnityEngine;
using System.Collections.Generic;
using System;


#if GAIA_PRO_PRESENT
namespace Gaia
{

    /// <summary>
    /// This component can be attached to the main camera / main character for a so called "floating point precision fix". Unity (& many other game engines)
    /// suffer from the problem that graphic and physics calculation can become imprecise after a certain distance from the world origin. When your scene exceeds roughly 
    /// 5000 units into any direction it can happen that you see symptoms like:
    /// shadows flickering
    /// shaky physics
    /// issues in animations
    /// The floating point fix component combats this by shifting all GameObjects back to the origin after a certain threshold is reached. While this is barely noticeable for the player,
    /// it prevents such issues as mentioned above as the world never exceeds 5000 units because it is shifted back before things like these can happen.
    /// </summary>
    public class FloatingPointFix : MonoBehaviour
    {

        private static GaiaSessionManager m_sessionManager;
        private static GaiaSessionManager SessionManager
        {
            get
            {
                if (m_sessionManager == null)
                {
                    m_sessionManager = GaiaSessionManager.GetSessionManager(false, false);
                }
                return m_sessionManager;
            }
        }


        /// <summary>
        /// Singleton instance
        /// </summary>
        private static FloatingPointFix instance = null;


        /// <summary>
        /// Returns the current Floating Point Fix Instance in the scene
        /// </summary>
        public static FloatingPointFix Instance
        {
            get
            {
                if (instance == null)
                    instance = (FloatingPointFix)FindObjectOfType(typeof(FloatingPointFix));
                if (instance == null && Application.isPlaying)
                    Debug.LogError("No Floating Point Fix Instance could be found, please add a Floating Point Fix component to the main player / camera object.");
                return instance;
            }
        }
        /// <summary>
        /// Returns true if the Floating Point Fix is active in this scene
        /// </summary>
        public static bool IsActive
        {
            get
            {
                if (instance == null)
                    instance = (FloatingPointFix)FindObjectOfType(typeof(FloatingPointFix));
                return instance != null;
            }
        }


        /// <summary>
        /// The distance from origin at which the floating point fix shift will be performed. Whenever the object that contains the floating point fix component
        /// moves further than this distance from 0,0,0 all floating point fix members and sectors will be shifted back to the origin. 
        /// </summary>
        public float threshold = 1000.0f;

        /// <summary>
        /// This vector3 represents the cumulative offset so far
        /// </summary>
        public Vector3 totalOffset = Vector3.zero;



        /// <summary>
        /// Makes sure we have a singleton and sets the sectors in the scene up for the floating point fix mode.
        /// </summary>
        void OnEnable()
        {
            //Singleton pattern
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }


        /// <summary>
        /// Adds a Floating Point Fix Member. Members are GameObjects that need to be shifted when the floating point fix shift occurs.
        /// Make sure to call "Remove Member" when your object gets destroyed.
        /// </summary>
        /// <param name="member">The member to add</param>
        public void AddMember(FloatingPointFixMember member)
        {
            if (!TerrainLoaderManager.Instance.m_allFloatingPointFixMembers.Contains(member))
            {
                TerrainLoaderManager.Instance.m_allFloatingPointFixMembers.Add(member);
            }
        }

        /// <summary>
        /// Removes a Floating Point Fix Member from the tracking list. Members are GameObjects that need to be shifted when the floating point fix shift occurs.
        /// </summary>
        /// <param name="member">The member to remove</param>
        public void RemoveMember(FloatingPointFixMember member)
        {

            if (SessionManager!=null)
            {
                if (TerrainLoaderManager.Instance.m_allFloatingPointFixMembers.Contains(member))
                {
                    TerrainLoaderManager.Instance.m_allFloatingPointFixMembers.Remove(member);
                }
            }
        }

        /// <summary>
        /// Adds a particle system simulated in World Space to the tracking list. All Particle Systems included in here will have their particles shifted as well when the floating point fix shift occurs.
        /// </summary>
        /// <param name="ps">The particle system to add</param>
        public void AddWorldSpaceParticleSystem(ParticleSystem ps)
        {
            if (!TerrainLoaderManager.Instance.m_allWorldSpaceParticleSystems.Contains(ps))
            {
                TerrainLoaderManager.Instance.m_allWorldSpaceParticleSystems.Add(ps);
            }
           
        }

        /// <summary>
        /// Removes a particle system simulated in World Space from the tracking list.
        /// </summary>
        /// <param name="ps">The particle system to remove</param>
        public void RemoveWorldSpaceParticleSystem(ParticleSystem ps)
        {
            if (SessionManager != null)
            {
                if (TerrainLoaderManager.Instance.m_allWorldSpaceParticleSystems.Contains(ps))
                {
                    TerrainLoaderManager.Instance.m_allWorldSpaceParticleSystems.Remove(ps);
                }
            }
        }

        /// <summary>
        /// Converts a Vector3 to its "real" / original position as if the floating point fix would not exist.
        /// </summary>
        /// <param name="currentPosition">The </param>
        /// <returns></returns>
        public Vector3 ConvertToOriginalSpace(Vector3 position)
        {
            return position += totalOffset;
        }


        void LateUpdate()
        {
            Vector3Double currentPosition = gameObject.transform.position;
            currentPosition.y = 0;

            if (currentPosition.magnitude > threshold)
            {
                TerrainLoaderManager.Instance.SetOrigin(currentPosition + TerrainLoaderManager.Instance.GetOrigin());
                gameObject.transform.position = new Vector3(0f, transform.position.y, 0f);
            }
        }

    }
}
#endif


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Gaia
{ 
    /// <summary>
    /// Just a small helper script to wait until the terrain below a rigidbody is being loaded in before
    /// activating gravity on the rigidbody
    /// </summary>
    public class RigidbodyWaitForTerrainLoad : MonoBehaviour
    {
        Rigidbody m_watchedRigidbody;
        TerrainScene m_terrainScene;
        bool m_invokeStarted = false;
        /// <summary>
        /// Delay until the rigidbody is activated after the terrain is loaded.
        /// </summary>
        public float m_activateDelay =2f;
        /// <summary>
        /// Add additional components here that need to be activated with the rigidbody
        /// </summary>
        public List<MonoBehaviour> m_componentsToActivate = new List<MonoBehaviour>();

        // Start is called before the first frame update
        void Start()
        {

            if (TerrainLoaderManager.TerrainScenes.Count > 0)
            {
                m_watchedRigidbody = GetComponent<Rigidbody>();

                //Find the relevant scene
                foreach (TerrainScene p in TerrainLoaderManager.TerrainScenes)
                {
                    if (p.m_bounds.Contains(transform.position))
                    {
                        m_terrainScene = p;
                        m_watchedRigidbody.useGravity = false;
                    }
                }

                //no terrain scene found? nothing to do then....
                if (m_terrainScene == null)
                {
                    Destroy(this);
                }

            }
            else
            {
                Destroy(this);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (m_terrainScene!=null && m_terrainScene.m_regularLoadState != LoadState.Loaded)
            {
                m_watchedRigidbody.useGravity = false;
            }
            else
            {
                if (!m_invokeStarted)
                {
                    Invoke("ActivateRigidbody", m_activateDelay);
                }
            }

        }

        void ActivateRigidbody()
        {
            m_watchedRigidbody.useGravity = true;
            foreach (MonoBehaviour component in m_componentsToActivate)
            {
                component.enabled = true;
            }
            Destroy(this);
        }
    }
}
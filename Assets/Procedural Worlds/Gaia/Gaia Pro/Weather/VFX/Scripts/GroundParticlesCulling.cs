using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gaia
{
    public class GroundParticlesCulling : MonoBehaviour
    {
        [HideInInspector]
        public int Radius
        {
            get { return m_radius; }
            set
            {
                if (m_radius != value)
                {
                    m_radius = value;
                    //Update Radius
                    SetRadius();
                }
            }
        }
        [HideInInspector]
        public CollisionDetectionType CollisionDetectionType
        {
            get { return m_collisionDetectionType; }
            set
            {
                if (m_collisionDetectionType != value)
                {
                    m_collisionDetectionType = value;
                    //Update collision
                    SetCollider();
                }
            }
        }
        [HideInInspector]
        public Transform Player;
        [HideInInspector]
        public bool HideColliders
        {
            get { return m_hideColliders; }
            set
            {
                if (m_hideColliders != value)
                {
                    m_hideColliders = value;
                    //Update collision
                    SetCollider();
                }
            }
        }

        [HideInInspector]
        [SerializeField]
        private int m_radius = 500;
        [HideInInspector]
        [SerializeField]
        private CollisionDetectionType m_collisionDetectionType = CollisionDetectionType.Box;
        [HideInInspector]
        [SerializeField]
        private bool m_hideColliders = true;
        [HideInInspector]
        [SerializeField]
        private GameObject m_instance;
        [HideInInspector]
        [SerializeField]
        private Collider m_collider;
        [HideInInspector]
        [SerializeField]
        private ParticleSystem m_particleSystem;

        private void OnEnable()
        {
            LoadFromWeatherSystem();
            if (Application.isPlaying)
            {
                if (m_particleSystem != null)
                {
                    m_particleSystem.Stop();
                }
            }
        }

        public void SetCollider()
        {
            m_instance = gameObject;
            switch (m_collisionDetectionType)
            {
                case CollisionDetectionType.Box:
                    m_collider = m_instance.GetComponent<SphereCollider>();
                    if (m_collider != null)
                    {
                        if (Application.isPlaying)
                        {
                            Destroy(m_collider);
                        }
                        else
                        {
                            DestroyImmediate(m_collider);
                        }
                    }

                    m_collider = m_instance.GetComponent<BoxCollider>();
                    if (m_collider == null)
                    {
                        m_collider = m_instance.AddComponent<BoxCollider>();
                    }
                    BoxCollider boxCollider = (BoxCollider)m_collider;
                    boxCollider.isTrigger = true;
                    boxCollider.center = new Vector3(0f, 0f, 0f);
                    break;

                case CollisionDetectionType.Spherical:
                    m_collider = m_instance.GetComponent<BoxCollider>();
                    if (m_collider != null)
                    {
                        if (Application.isPlaying)
                        {
                            Destroy(m_collider);
                        }
                        else
                        {
                            DestroyImmediate(m_collider);
                        }
                    }

                    m_collider = m_instance.GetComponent<SphereCollider>();
                    if (m_collider == null)
                    {
                        m_collider = m_instance.AddComponent<SphereCollider>();
                    }
                    SphereCollider sphereCollider = (SphereCollider)m_collider;
                    sphereCollider.isTrigger = true;
                    sphereCollider.center = new Vector3(0f, 0f, 0f);
                    break;
            }

#if UNITY_EDITOR
            if (HideColliders)
            {
                UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(m_collider, false);
            }
            else
            {
                UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(m_collider, true);
            }
#endif
            SetRadius();
        }

        public void SetRadius()
        {
            if (m_collider == null)
            {
                SetCollider();
            }

            switch (m_collisionDetectionType)
            {
                case CollisionDetectionType.Box:
                    BoxCollider boxCollider = (BoxCollider)m_collider;
                    boxCollider.size = new Vector3(Radius, Radius, Radius);
                    break;
                case CollisionDetectionType.Spherical:
                    SphereCollider sphereCollider = (SphereCollider)m_collider;
                    sphereCollider.radius = Radius;
                    break;
            }
        }

        private void GetParticleSystem()
        {
            if (m_particleSystem == null)
            {
                m_particleSystem = m_instance.GetComponent<ParticleSystem>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Player == null)
            {
                Debug.LogError("Player Not set on " + m_instance.name);
                return;
            }
            else
            {
                if (other.tag == Player.tag)
                {
                    if (m_particleSystem != null)
                    {
                        m_particleSystem.Play();
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (Player == null)
            {
                Debug.LogError("Player Not set on " + m_instance.name);
                return;
            }
            else
            {
                if (other.tag == Player.tag)
                {
                    if (m_particleSystem != null)
                    {
                        m_particleSystem.Stop();
                    }
                }
            }
        }

        /// <summary>
        /// Loads from weather system
        /// </summary>
        private void LoadFromWeatherSystem()
        {
            m_instance = gameObject;
            GetParticleSystem();

            ProceduralWorldsGlobalWeather globalWeather = FindObjectOfType<ProceduralWorldsGlobalWeather>();
            if (globalWeather == null)
            {
                SetCollider();
                SetRadius();
                Player = ProceduralWorldsGlobalWeather.GetPlayer();
            }
            else
            {
                //Radius = globalWeather.GroundParticlesEnableDistance;
                //CollisionDetectionType = globalWeather.GroundParticlesColliderType;
                //HideColliders = globalWeather.GroundParticlesHideColliders;
                if (globalWeather.m_player == null)
                {
                    Player = ProceduralWorldsGlobalWeather.GetPlayer();
                }
                else
                {
                    Player = globalWeather.m_player;
                }
                SetCollider();
                SetRadius();
            }
        }
    }
}
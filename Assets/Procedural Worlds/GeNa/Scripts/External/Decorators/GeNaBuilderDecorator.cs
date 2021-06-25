using System.Collections;
using UnityEngine;

namespace GeNa.Core
{
    public class GeNaBuilderDecorator : MonoBehaviour, IDecorator
    {
        private Transform m_selectedTransform;
        private Bounds m_selectedTransformBounds;
        private Transform m_rootTransform;
        private Bounds m_rootTransformBounds;

        // Should GeNa Unpack the Prefab that this Decorator is attached to? 
        public bool UnpackPrefab => false;

        // Called when Decorator is Ingested into GeNa
        public void OnIngest(Resource resource)
        {
        }

        // Runs once this Decorator is Spawned
        public IEnumerator OnSelfSpawned(Resource resource)
        {
            yield return null;
        }

        // Runs directly after Spawning Children Decorators
        public void OnChildrenSpawned(Resource resource)
        {
            GeNaEvents.Destroy(this);
        }
        public void LoadReferences(Palette palette)
        {
        }
        /// <summary>
        /// Determine if root transform is selected
        /// </summary>
        /// <returns>True if the root transform is selected</returns>
        public bool IsRootTransformSelected()
        {
            if (m_rootTransform == null)
            {
                SetRootTransform();
            }
            if (m_selectedTransform == null)
            {
                SetSelectedTransform(transform);
            }
            return (m_rootTransform.GetInstanceID() == m_selectedTransform.GetInstanceID());
        }
        /// <summary>
        /// Get current root transform
        /// </summary>
        /// <returns>Root transform</returns>
        public Transform GetRootTransform()
        {
            if (m_rootTransform == null)
            {
                SetRootTransform();
            }
            if (m_selectedTransform == null)
            {
                SetSelectedTransform(transform);
            }
            return m_rootTransform;
        }
        /// <summary>
        /// Set  the root tranform and update its bounds
        /// </summary>
        public void SetRootTransform()
        {
            m_rootTransform = transform;
            UpdateRootBounds();
        }
        /// <summary>
        /// Bounds of overall object and its children
        /// </summary>
        /// <returns>Bounds of object and its children</returns>
        public Bounds GetRootBounds()
        {
            if (m_rootTransform == null)
            {
                SetRootTransform();
            }
            if (GeNaUtility.ApproximatelyEqual(m_rootTransformBounds.size, Vector3.zero))
            {
                UpdateRootBounds();
            }
            return m_rootTransformBounds;
        }
        /// <summary>
        /// Update the root bounds when selection changes
        /// </summary>
        public void UpdateRootBounds()
        {
            m_rootTransformBounds = GeNaUtility.GetObjectBounds(gameObject, true, true, false, false);
        }
        /// <summary>
        /// Select the given transform
        /// </summary>
        /// <param name="newTransform">New selected transform</param>
        public void SetSelectedTransform(Transform newTransform)
        {
            m_selectedTransform = newTransform;
            UpdateSelectedBounds();
        }
        /// <summary>
        /// Get currently selected transform - any issues then returns the root transform
        /// </summary>
        /// <returns>Child or root</returns>
        public Transform GetSelectedTransform()
        {
            if (m_rootTransform == null)
            {
                SetRootTransform();
            }
            if (m_selectedTransform == null)
            {
                SetSelectedTransform(transform);
            }
            return m_selectedTransform;
        }
        /// <summary>
        /// Get currently selected objects bounds
        /// </summary>
        /// <returns>Currently selected objects bounds</returns>
        public Bounds GetSelectedBounds()
        {
            if (m_selectedTransform == null)
            {
                SetSelectedTransform(transform);
            }
            // if (GeNaUtility.ApproximatelyEqual(m_selectedTransformBounds.size, Vector3.zero))
            // {
            UpdateSelectedBounds();
            // }
            return m_selectedTransformBounds;
        }
        /// <summary>
        /// Update the selected bounds when selection changes
        /// </summary>
        public void UpdateSelectedBounds()
        {
            if (IsRootTransformSelected())
                m_selectedTransformBounds = GeNaUtility.GetObjectBounds(gameObject, true, true, false, false);
            else
                m_selectedTransformBounds = GeNaUtility.GetObjectBounds(m_selectedTransform.gameObject, true, true, false, false);
        }
        public void OnDrawGizmos()
        {
            // if (!m_drawGizmos)
            // {
            //     return;
            // }

            //If not set then set up the relevant transforms and get bounds
            if (m_rootTransform == null || m_selectedTransform == null)
            {
                m_rootTransform = transform;
                if (m_selectedTransform == null)
                {
                    m_selectedTransform = transform;
                }
                UpdateSelectedBounds();
            }

            //Draw a north facing Gizmo on root transform
            Color oldColor = Gizmos.color;
            Gizmos.color = Color.red;
            Vector3 nextPoint = m_rootTransform.position + (m_rootTransform.forward * 40f);
            Gizmos.DrawLine(m_rootTransform.position, nextPoint);
            Gizmos.DrawWireSphere(nextPoint, 0.15f);

            //Draw selected transform
            if (m_rootTransform.GetInstanceID() == m_selectedTransform.GetInstanceID())
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawWireSphere(m_selectedTransform.position, 0.15f);
            Bounds b = GetSelectedBounds();
            Gizmos.DrawWireCube(b.center, b.size);
            Gizmos.color = oldColor;
        }
    }
}
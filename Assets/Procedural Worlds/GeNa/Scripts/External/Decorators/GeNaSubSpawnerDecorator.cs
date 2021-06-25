using UnityEngine;
using System.Collections;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for performing Sub-Spawner operations
    /// </summary>
    public class GeNaSubSpawnerDecorator : MonoBehaviour, IDecorator
    {
        [SerializeField] protected int m_maxSubSpawnerDepth = 5;
        [SerializeField] protected GeNaSpawner m_subSpawner;
        [SerializeField] protected GeNaSpawnerData m_subSpawnerData;
        [SerializeField] protected Color m_subSpawnerColor = Color.blue;
        public int MaxSubSpawnerDepth
        {
            get => m_maxSubSpawnerDepth;
            set => m_maxSubSpawnerDepth = value;
        }
        public GeNaSpawner SubSpawner
        {
            get => m_subSpawner;
            set
            {
                m_subSpawner = value;
                if (m_subSpawner != null)
                    m_subSpawnerData = m_subSpawner.SpawnerData;
            }
        }
        public GeNaSpawnerData SubSpawnerData
        {
            get => m_subSpawnerData;
            set => m_subSpawnerData = value;
        }
        public Color SubSpawnerColor
        {
            get => m_subSpawnerColor;
            set => m_subSpawnerColor = value;
        }
        public bool UnpackPrefab => false;
        public void OnIngest(Resource resource)
        {
            resource.HasSubSpawner = true;
            GeNaSpawner subSpawner = SubSpawner;
            if (subSpawner != null)
            {
                Palette palette = resource.Palette;
                resource.AddSubSpawner(subSpawner.gameObject, palette);
                GeNaSpawnerData subSpawnerData = resource.SubSpawnerData;
                if (subSpawnerData != null)
                {
                    SpawnerSettings settings = resource.SubSpawnerData.Settings;
                    settings.MaxSubSpawnerDepth = MaxSubSpawnerDepth;
                }
            }
        }
        public IEnumerator OnSelfSpawned(Resource resource)
        {
            yield break;
        }
        public void OnChildrenSpawned(Resource resource)
        {
            GeNaEvents.Destroy(this);
        }
        public void LoadReferences(Palette palette)
        {
        }
        public void Refresh()
        {
            if (m_subSpawner != null)
            {
                m_subSpawnerData = m_subSpawner.SpawnerData;
            }
        }
    }
}
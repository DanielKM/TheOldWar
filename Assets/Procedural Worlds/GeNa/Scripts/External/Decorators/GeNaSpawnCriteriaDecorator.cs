using UnityEngine;
using System.Collections;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for modifying Spawn Criteria overrides
    /// </summary>
    public class GeNaSpawnCriteriaDecorator : MonoBehaviour,IDecorator
    {
        [SerializeField] protected SpawnCriteria m_spawnCriteria = new SpawnCriteria();
        public SpawnCriteria SpawnCriteria
        {
            get => m_spawnCriteria;
            set => m_spawnCriteria = value;
        }
        public bool UnpackPrefab => false;
        public void OnIngest(Resource resource)
        {
            resource.SpawnCriteria = new SpawnCriteria();
            resource.SpawnCriteria.Copy(SpawnCriteria);
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
    }
}
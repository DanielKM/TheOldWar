using UnityEngine;
using System.Collections;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for modifying Spawn Flags of Prefab Prototype
    /// </summary>
    public class GeNaSpawnFlagsDecorator : MonoBehaviour, IDecorator
    {
        [SerializeField] protected SpawnFlags m_spawnFlags = new SpawnFlags();
        public SpawnFlags SpawnFlags => m_spawnFlags;
        public bool UnpackPrefab => false;
        public void OnIngest(Resource resource)
        {
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
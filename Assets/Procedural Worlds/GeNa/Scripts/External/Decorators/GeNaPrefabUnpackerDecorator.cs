using UnityEngine;
using System.Collections;

namespace GeNa.Core
{
    /// <summary>
    /// Decorator for unpacking prefab into Spawner
    /// </summary>
    public class GeNaPrefabUnpackerDecorator : MonoBehaviour,IDecorator
    {
        public bool UnpackPrefab => true;
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
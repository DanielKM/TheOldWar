using UnityEngine;
using System.Collections;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for converting GameObject to Terrain Tree
    /// </summary>
    public class GeNaTerrainTreeDecorator : MonoBehaviour, IDecorator
    {
        [SerializeField] protected bool m_enabled = true;
        public bool Enabled
        {
            get => m_enabled;
            set => m_enabled = value;
        }
        public bool UnpackPrefab => false;
        public void OnIngest(Resource resource)
        {
            if (Enabled)
            {
                TreePrototype treePrototype = GeNaSpawnerInternal.GetTreePrototype(resource.Prefab);
                int treePrototypeIndex = GeNaSpawnerInternal.GetTreePrototypeIndex(treePrototype);
                if (treePrototypeIndex >= 0)
                {
                    resource.Type = Constants.ResourceType.TerrainTree;
                    resource.TerrainProtoIdx = treePrototypeIndex;
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
    }
}
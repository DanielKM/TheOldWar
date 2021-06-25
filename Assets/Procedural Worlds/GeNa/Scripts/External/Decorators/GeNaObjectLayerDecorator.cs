using UnityEngine;
using System.Collections;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for handling Object Layer modifications
    /// </summary>
    public class GeNaObjectLayerDecorator : MonoBehaviour, IDecorator
    {
        public string m_layerName;
        [SerializeField] protected int m_layerMask = 0;
        public int LayerMask
        {
            get => m_layerMask;
            set
            {
                if (m_layerMask != value)
                {
                    m_layerMask = value;
                    m_layerName = UnityEngine.LayerMask.LayerToName(value);
                }
            }
        }
        [SerializeField] protected int m_tagIndex = 0;
        public int TagIndex
        {
            get => m_tagIndex;
            set => m_tagIndex = value;
        }
        [SerializeField] protected string[] m_tags;
        public string[] Tags
        {
            get => m_tags;
            set => m_tags = value;
        }

        [SerializeField] protected bool m_applyToChildern = false;
        public bool ApplyToChildern
        {
            get => m_applyToChildern;
            set => m_applyToChildern = value;
        }
        public bool UnpackPrefab { get; }
        public void OnIngest(Resource resource) { }
        public IEnumerator OnSelfSpawned(Resource resource)
        {
            yield break;
        }
        public void OnChildrenSpawned(Resource resource)
        {  
            ObjectLayerUtility.ApplyLayersToObjects(gameObject);  
            GeNaEvents.Destroy(this);
        }
        public void LoadReferences(Palette palette)
        {
        }
    }
}
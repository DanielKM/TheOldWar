using UnityEngine;
using System.Collections;

namespace GeNa.Core
{
    /// <summary>
    /// Decorator for introducing the element of chance into Spawning
    /// </summary>
    public class GeNaChanceOfDecorator : MonoBehaviour, IDecorator
    {
        [SerializeField] protected float m_successRate = 1.0f;
        public float SuccessRate
        {
            get => m_successRate;
            set => m_successRate = Mathf.Clamp01(value);
        }
        public bool UnpackPrefab => false;
        public void OnIngest(Resource resource)
        {
            Prototype prototype = resource.Prototype;
            resource.SetStatic(prototype, Constants.ResourceStatic.Dynamic);
            resource.SuccessRate = m_successRate;
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
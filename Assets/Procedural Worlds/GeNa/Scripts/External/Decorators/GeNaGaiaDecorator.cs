using UnityEngine;
using System.Collections;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for getting the Sea Level from Gaia (requires Gaia 2 / Gaia Pro)
    /// </summary>
    public class GeNaGaiaDecorator : MonoBehaviour, IDecorator
    {
        [SerializeField] protected bool m_enabled = true;
        public bool Enabled
        {
            get => m_enabled;
            set => m_enabled = value;
        }
        [SerializeField] protected bool m_getSeaLevel = true;
        public bool GetSeaLevel
        {
            get => m_getSeaLevel;
            set => m_getSeaLevel = value;
        }
        [SerializeField] protected float m_extraSeaLevel = 0f;
        public float ExtraSeaLevel
        {
            get => m_extraSeaLevel;
            set => m_extraSeaLevel = value;
        }
        public bool UnpackPrefab => false;
        public void OnIngest(Resource resource)
        {
            if (Enabled)
            {
                Prototype prototype = resource.Prototype;
                GeNaSpawnerData spawnerData = prototype.SpawnerData;
                spawnerData.SetSeaLevel(GetSeaLevel, ExtraSeaLevel);
                if (GetSeaLevel)
                    GeNaEvents.SetSeaLevel(spawnerData);
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
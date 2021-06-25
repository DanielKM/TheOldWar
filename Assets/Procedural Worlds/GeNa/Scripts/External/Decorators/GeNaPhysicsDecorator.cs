using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for performing Physics-based Spawning
    /// </summary>
    public class GeNaPhysicsDecorator : MonoBehaviour, IDecorator
    {
        [SerializeField] protected PhysicsSimulatorSettings m_settings = new PhysicsSimulatorSettings();
        public PhysicsSimulatorSettings Settings
        {
            get => m_settings;
            set => m_settings = value;
        }
        public bool UnpackPrefab => false;
        public void OnIngest(Resource resource)
        {
            resource.PhysicsEnabled = true;
        }
        public IEnumerator OnSelfSpawned(Resource resource)
        {
            GeNaSpawnerData spawnerData = resource.SpawnerData;
            List<ResourceEntity> entities = new List<ResourceEntity>();
            Vector3 location = transform.position;
            Vector3 rotation = transform.eulerAngles;
            Vector3 scale = transform.localScale;
            ResourceEntity spawnedEntity = GeNaSpawnerInternal.GetResourceEntity(location, rotation, scale, resource);
            spawnedEntity.GameObject = gameObject;
            entities.Add(spawnedEntity);
            GameObject spawnProgress = GeNaSpawnerInternal.GetSpawnProgressParent(spawnerData);
            transform.SetParent(spawnProgress.transform);
            // If no longer spawning 
            if (spawnerData.PhysicsType == Constants.PhysicsType.Resource && !GeNaGlobalReferences.GeNaManagerInstance.Cancel)
            {
                IEnumerator simulateMethod = GeNaEvents.Simulate(entities, m_settings, this);
                while (simulateMethod.MoveNext())
                {
                    yield return simulateMethod.Current;
                }
            }
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
using System;
using UnityEngine;
using System.Collections;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for modifying Terrain
    /// </summary>
    [ExecuteAlways]
    public class GeNaTerrainDecorator : MonoBehaviour, IDecorator
    {
        [SerializeField] protected TerrainModifier m_terrainModifier = new TerrainModifier();
        [NonSerialized] private bool m_isSelected = false;
        public TerrainModifier TerrainModifier => m_terrainModifier;
        public bool IsSelected
        {
            get => m_isSelected;
            set => m_isSelected = value;
        }
        public bool UnpackPrefab => false;
        public void Update()
        {
            if (!m_isSelected)
                return;
            m_terrainModifier.Position = transform.position;
            m_terrainModifier.RotationY = transform.eulerAngles.y;
            m_terrainModifier.UpdateTerrain = false;
            //   GeNaManager.Instance.ScheduleTerrainModifier(m_terrainModifier);
        }
        public void OnIngest(Resource resource)
        {
            resource.HasHeights = true;
            Palette palette = resource.Palette;
            m_terrainModifier.AddBrushTextures(m_terrainModifier.BrushTextures, palette);
        }
        public IEnumerator OnSelfSpawned(Resource resource)
        {
            Palette palette = resource.Palette;
            TerrainModifier.LoadReferences(palette);
            if (TerrainModifier != null && TerrainModifier.Enabled)
            {
                TerrainModifier.Position = transform.position;
                TerrainModifier.UpdateTerrain = true;
                GeNaSpawnerData spawnerData = resource.SpawnerData;
                TerrainEntity terrainEntity = TerrainModifier.Perform();
                if (terrainEntity != null)
                {
                    // Append the Undo Record (if there is one)
                    UndoRecord undoRecord = spawnerData.UndoRecord;
                    undoRecord.Record(terrainEntity);
                }
            }
            yield break;
        }
        public void OnChildrenSpawned(Resource resource)
        {
            GeNaEvents.Destroy(this);
        }
        public void LoadReferences(Palette palette)
        {
            TerrainModifier.LoadReferences(palette);
        }
    }
}
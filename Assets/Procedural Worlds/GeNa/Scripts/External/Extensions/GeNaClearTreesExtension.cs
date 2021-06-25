using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Spline Extension for Clearing Terrain Trees along a Spline
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "ClearTrees", menuName = "Procedural Worlds/GeNa/Extensions/ClearTrees", order = 0)]
    public class GeNaClearTreesExtension : GeNaSplineExtension
    {
        #region Variables
        // Compute Shader
        [SerializeField] protected float m_width = 1f;
        [SerializeField] protected float m_noiseStrength = 1.0f;
        // Smoothness
        [SerializeField] protected float m_smoothness = 1.5f;
        // Noise
        [SerializeField] protected bool m_noiseEnabled = false;
        [SerializeField] protected Fractal m_maskFractal = new Fractal();
        [SerializeField] protected Color m_positiveColor = new Color(0.5611f, 0.9716f, 0.5362f, 1f);
        [SerializeField] protected Color m_negativeColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField] protected AnimationCurve m_shoulderFalloff = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f));
        // Non Serialized
        [NonSerialized] protected TerrainTools m_terrainTools;
        [NonSerialized] private bool m_isDirty = false;
        [NonSerialized] protected Stack<TerrainEntity> m_undoStack = new Stack<TerrainEntity>();
        #endregion
        #region Properties
        public float Width
        {
            get => m_width;
            set => m_width = value;
        }
        // Noise
        public float NoiseStrength
        {
            get => m_noiseStrength;
            set => m_noiseStrength = value;
        }
        public bool NoiseEnabled
        {
            get => m_noiseEnabled;
            set => m_noiseEnabled = value;
        }
        // Smoothness
        public float Smoothness
        {
            get => m_smoothness;
            set => m_smoothness = value;
        }
        public Fractal MaskFractal
        {
            get => m_maskFractal;
            set => m_maskFractal = value;
        }
        public Color PositiveColor
        {
            get => m_positiveColor;
            set => m_positiveColor = value;
        }
        public Color NegativeColor
        {
            get => m_negativeColor;
            set => m_negativeColor = value;
        }
        public AnimationCurve ShoulderFalloff
        {
            get => m_shoulderFalloff;
            set => m_shoulderFalloff = value;
        }
        #endregion
        protected override void OnSceneGUI()
        {
            Visualize();
        }
        public TerrainTools GetTerrainTools()
        {
            if (m_terrainTools == null)
            {
                GeNaManager geNaManager = GeNaManager.GetInstance();
                m_terrainTools = geNaManager.TerrainTools;
            }
            return m_terrainTools;
        }
        private TerrainEntity m_terrainEntity;
        public void Visualize()
        {
            if (!m_isSelected)
                return;
            TerrainTools tools = GetTerrainTools();
            tools.Width = Width;
            tools.Smoothness = Smoothness;
            tools.HeightOffset = 0f;
            tools.NoiseStrength = NoiseStrength;
            tools.MaskFractal = MaskFractal;
            tools.PositiveColor = PositiveColor;
            tools.NegativeColor = NegativeColor;
            tools.ShoulderFalloff = ShoulderFalloff;
            tools.NoiseEnabled = NoiseEnabled;
            if (m_isDirty)
            {
                m_terrainEntity = tools.GenerateModification(EffectType.ClearTrees, Spline);
                m_isDirty = false;
            }
            if (m_terrainEntity != null)
            {
                tools.Visualize(m_terrainEntity);
            }
        }
        private void Modify(EffectType effectType, bool recordUndo = true)
        {
            TerrainTools tools = GetTerrainTools();
            tools.Width = Width;
            tools.Smoothness = Smoothness;
            tools.NoiseStrength = NoiseStrength;
            tools.HeightOffset = 0f;
            tools.MaskFractal = MaskFractal;
            tools.PositiveColor = PositiveColor;
            tools.NegativeColor = NegativeColor;
            tools.ShoulderFalloff = ShoulderFalloff;
            tools.NoiseEnabled = NoiseEnabled;
            TerrainEntity terrainEntity = tools.GenerateModification(effectType, Spline);
            if (terrainEntity != null)
            {
                terrainEntity.Perform();
                if (recordUndo)
                {
                    m_undoStack.Push(terrainEntity);
                    Spline.RecordUndo("Clear Trees", Undo);
                }
            }
        }
        public void Undo()
        {
            if (m_undoStack.Count > 0)
            {
                TerrainEntity terrainEntity = m_undoStack.Pop();
                terrainEntity?.Undo();
            }
        }
        public void Clear()
        {
            Modify(EffectType.ClearTrees);
        }
        protected override GameObject OnBake(GeNaSpline spline)
        {
            Modify(EffectType.ClearTrees, false);
            return null;
        }
        public override void Execute()
        {
            if (!m_isSelected)
                return;
            Visualize();
        }
        protected override void OnSelect()
        {
            Visualize();
            m_isSelected = true;
            OnSplineDirty();
        }
        protected override void OnDeselect()
        {
            m_isSelected = false;
        }
        protected override void OnDelete()
        {
        }
        protected override void OnSplineDirty()
        {
            m_isDirty = true;
        }
    }
}
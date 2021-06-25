using System;
using System.Collections.Generic;
using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Spline Extension for creating Extrusions along a Spline
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Extrusion", menuName = "Procedural Worlds/GeNa/Extensions/Extrusion", order = 0)]
    public class GeNaExtrusionExtension : GeNaSplineExtension
    {
        #region Variables
        [SerializeField] protected Material m_sharedMaterial;
        [SerializeField] protected float m_smoothness = 5f;
        [SerializeField] protected float m_width = 1f;
        [SerializeField] protected float m_heightOffset = 0f;
        [SerializeField] protected bool m_snapToGround = false;
        [SerializeField] protected AnimationCurve m_curve = new AnimationCurve();
        [SerializeField] protected List<GeNaMesh> m_meshes = new List<GeNaMesh>();
        [SerializeField] protected Transform m_extrusions;
        [SerializeField] public bool m_splitAtTerrains = true;
        [SerializeField] public GameObject m_parent = null;
        [NonSerialized] protected bool m_isDirty = false;
        #endregion
        #region Properties
        public bool SplitAtTerrains
        {
            get => m_splitAtTerrains;
            set => m_splitAtTerrains = value;
        }
        public Material SharedMaterial
        {
            get
            {
                if (m_sharedMaterial == null)
                {
                    m_sharedMaterial = Resources.Load<Material>("Materials/Road");
                    m_isDirty = true;
                }
                return m_sharedMaterial;
            }
            set
            {
                m_sharedMaterial = value;
                foreach (GeNaMesh mesh in m_meshes)
                    mesh.SharedMaterial = m_sharedMaterial;
                m_isDirty = true;
            }
        }
        public float Smoothness
        {
            get => m_smoothness;
            set
            {
                m_smoothness = value;
                m_isDirty = true;
            }
        }
        public float Width
        {
            get => m_width;
            set
            {
                m_width = value;
                m_isDirty = true;
            }
        }
        public float HeightOffset
        {
            get => m_heightOffset;
            set
            {
                m_heightOffset = value;
                m_isDirty = true;
            }
        }
        public bool SnapToGround
        {
            get => m_snapToGround;
            set
            {
                m_snapToGround = value;
                m_isDirty = true;
            }
        }
        public AnimationCurve Curve
        {
            get => m_curve;
            set
            {
                m_curve = value;
                m_isDirty = true;
            }
        }
        public List<GeNaMesh> Meshes => m_meshes;
        public void SetSharedMesh(Mesh sharedMesh)
        {
            foreach (GeNaMesh mesh in m_meshes)
                mesh.SharedMesh = sharedMesh;
        }
        protected override GameObject OnBake(GeNaSpline spline)
        {
            PreExecute();
            Execute();
            GameObject roadMeshes = GeNaEvents.BakeSpline(m_parent, Spline);
            if (m_splitAtTerrains)
            {
                GeNaRoadsMesh.PostProcess(roadMeshes);
            }
            return roadMeshes;
        }
        public override void PreExecute()
        {
        }
        public override void Execute()
        {
            if (Spline == null)
                return;
            Dictionary<int, List<GeNaCurve>> trees = Spline.GetTrees();
            if (trees.Count != m_meshes.Count)
            {
                if (m_extrusions == null)
                {
                    m_extrusions = new GameObject("Extrusion Meshes").transform;
                    m_extrusions.SetParent(Spline.transform);
                }
                foreach (GeNaMesh mesh in m_meshes)
                    mesh.Destroy();
                m_meshes.Clear();
                for (int i = 0; i < trees.Count; i++)
                {
                    GeNaMesh geNaMesh = new GeNaMesh {Parent = m_extrusions};
                    m_meshes.Add(geNaMesh);
                }
            }
            int index = 0;
            foreach (KeyValuePair<int, List<GeNaCurve>> pair in trees)
            {
                List<GeNaCurve> curves = pair.Value;
                GeNaMesh geNaMesh = m_meshes[index];
                geNaMesh.SharedMaterial = SharedMaterial;
                geNaMesh.Smoothness = Smoothness;
                geNaMesh.Width = Width;
                geNaMesh.HeightOffset = HeightOffset;
                geNaMesh.SnapToGround = SnapToGround;
                geNaMesh.Curve = Curve;
                geNaMesh.Update(Spline, curves);
                index++;
            }
            if (m_extrusions != null)
            {
                m_parent = m_extrusions.gameObject;
            }
            m_isDirty = false;
        }
        protected override void OnDeactivate()
        {
            // Check to make sure they haven't move the road meshes in the hierarchy
            if (m_parent != null && m_parent.transform.parent != Spline.transform)
            {
                m_parent = null;
            }
            if (m_parent != null)
            {
                foreach (GeNaMesh mesh in m_meshes)
                {
                    DestroyImmediate(mesh.GameObject);
                }
                m_meshes.Clear();
                if (m_extrusions != null)
                {
                    DestroyImmediate(m_extrusions.gameObject);
                }
            }
        }
        protected override void OnDelete()
        {
            OnDeactivate();
        }
        public void Reset()
        {
            Curve = new AnimationCurve(
                new Keyframe(-1f, -.25f),
                new Keyframe(-.687f, .014f),
                new Keyframe(.687f, .014f),
                new Keyframe(1f, -.25f));
        }
        private void RemoveWarnings()
        {
            if (m_isDirty)
            {
                m_isDirty = false;
            }
        }
        #endregion
    }
}
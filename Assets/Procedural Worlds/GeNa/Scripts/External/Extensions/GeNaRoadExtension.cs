//Copyright(c)2020 Procedural Worlds Pty Limited 
using System;
using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Spline Extension for creating Roads along a Spline
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Roads", menuName = "Procedural Worlds/GeNa/Extensions/Roads", order = 3)]
    public class GeNaRoadExtension : GeNaSplineExtension
    {
        [SerializeField] protected GeNaRoadProfile m_roadProfile;
        [SerializeField] protected float m_width = 6.2f;
        [SerializeField] protected float m_intersectionSize = 1.0f;
        [SerializeField] protected float m_groundAttractDistance = 0.0f;
        [SerializeField] protected bool m_conformToGround = false;
        [SerializeField] protected bool m_addRoadCollider = true;
        [SerializeField] protected bool m_raycastTerrainOnly = true;
        [SerializeField] protected string m_tag = "Untagged";
        [SerializeField] protected int m_layer = -1;
        [SerializeField] protected bool m_splitAtTerrains = true;
        [SerializeField] protected bool m_postProcess = false;
        [NonSerialized] protected GameObject m_roadMeshParent = null;
        public GeNaRoadProfile RoadProfile
        {
            get => m_roadProfile;
            set => m_roadProfile = value;
        }
        public float Width
        {
            get => m_width;
            set
            {
                if (!Mathf.Approximately(m_width, value))
                {
                    m_width = Mathf.Max(0.5f, value);
                    SyncCarveParameters();
                }
            }
        }
        public float IntersectionSize
        {
            get => m_intersectionSize;
            set => m_intersectionSize = value;
        }
        public float GroundAttractDistance
        {
            get => m_groundAttractDistance;
            set => m_groundAttractDistance = Mathf.Clamp(value, 0.0f, 20.0f);
        }
        public bool ConformToGround
        {
            get => m_conformToGround;
            set => m_conformToGround = value;
        }
        public bool AddRoadCollider
        {
            get => m_addRoadCollider;
            set => m_addRoadCollider = value;
        }
        public bool RaycastTerrainOnly
        {
            get => m_raycastTerrainOnly;
            set => m_raycastTerrainOnly = value;
        }
        public string Tag
        {
            get => m_tag;
            set => m_tag = value;
        }
        public int Layer
        {
            get => m_layer;
            set => m_layer = value;
        }
        public bool SplitAtTerrains
        {
            get => m_splitAtTerrains;
            set => m_splitAtTerrains = value;
        }
        public bool PostProcess
        {
            get => m_postProcess;
            set => m_postProcess = value;
        }
        /// <summary>
        /// GeNa Extension Methods
        /// </summary>
        #region GeNa Extension Methods
        public void Bake(bool postProcess)
        {
            m_postProcess = postProcess;
            Bake();
        }
        protected override GameObject OnBake(GeNaSpline spline)
        {
            PreExecute();
            Execute();
            GameObject roadMeshes = GeNaEvents.BakeSpline(m_roadMeshParent, spline);
            if (m_postProcess && m_splitAtTerrains)
                GeNaRoadsMesh.PostProcess(roadMeshes);
            return roadMeshes;
        }
        protected override void OnAttach(GeNaSpline spline)
        {
            if (RoadProfile == null)
            {
                RoadProfile = Resources.Load<GeNaRoadProfile>("Road Profiles/GeNa Road Profile");
            }
            if (m_layer < 0)
            {
                m_layer = LayerMask.NameToLayer("PW_Object_Large");
                if (m_layer < 0)
                    m_layer = 0;
            }
            SyncCarveParameters();
        }
        public override void Execute()
        {
            if (IsActive && Spline.Nodes.Count > 1)
            {
                ProcessSpline(Spline);
            }
        }
        public override void PreExecute()
        {
            SyncCarveParameters();
            DeleteRoadMeshGameobjects();
        }
        protected override void OnActivate()
        {
            if (Spline.Nodes.Count > 1)
                ProcessSpline(Spline);
        }
        protected override void OnDeactivate()
        {
            DeleteRoadMeshGameobjects();
        }
        protected override void OnDelete()
        {
            DeleteRoadMeshGameobjects();
        }
        #endregion End GeNa Extension Methods
        private void SyncCarveParameters()
        {
            GeNaCarveExtension carve = Spline.GetExtension<GeNaCarveExtension>();
            if (m_roadMeshParent == null)
            {
                Transform parent = Spline.gameObject.transform.Find("Road Meshes");
                if (parent != null)
                    m_roadMeshParent = parent.gameObject;
            }
            if (carve != null && carve.Width < Width * 0.5f)
                carve.Width = Width * 0.5f;
        }
        /// <summary>
        /// Delete all road mesh GameObjects
        /// </summary>
        private void DeleteRoadMeshGameobjects()
        {
            // Check to make sure they haven't move the road meshes in the hierarchy
            if (m_roadMeshParent != null && m_roadMeshParent.transform.parent != Spline.transform)
                m_roadMeshParent = null;
            if (m_roadMeshParent != null)
            {
                GeNaRoad[] genaRoads = m_roadMeshParent.GetComponentsInChildren<GeNaRoad>();
                foreach (GeNaRoad road in genaRoads)
                    GeNaEvents.Destroy(road.gameObject);
                GeNaEvents.Destroy(m_roadMeshParent);
            }
        }
        public void SmoothSplineForRoads()
        {
            Spline.Smooth();
            ProcessSpline(Spline);
        }
        /// <summary>
        /// Process the entire spline to create roads and intersections.
        /// </summary>
        /// <param name="spline"></param>
        private void ProcessSpline(GeNaSpline spline)
        {
            if (m_roadMeshParent == null)
            {
                m_roadMeshParent = new GameObject("Road Meshes");
                m_roadMeshParent.transform.position = Vector3.zero;
                m_roadMeshParent.transform.parent = spline.gameObject.transform;
            }
            if (RoadProfile != null)
            {
                GeNaRoadsMesh geNaRoadsMesh = new GeNaRoadsMesh(RoadProfile.ApplyRoadProfile(), RoadProfile.ApplyIntersectionProfile(), m_roadMeshParent, m_tag, m_layer);
                geNaRoadsMesh.Process(spline, Width, m_intersectionSize, m_conformToGround, m_groundAttractDistance, m_addRoadCollider, m_raycastTerrainOnly);
            }
        }
        protected override void OnDrawGizmosSelected()
        {
            if (Spline.Settings.Advanced.DebuggingEnabled == false)
                return;
            foreach (GeNaCurve curve in Spline.Curves)
                DrawCurveInfo(curve);
        }
        private void DrawCurveInfo(GeNaCurve geNaCurve)
        {
            // Draw arrows showing which direction a curve is facing (from StartNode to EndNode).
            GeNaSample geNaSample = geNaCurve.GetSample(0.45f);
            DrawArrow(geNaSample.Location, geNaSample.Forward);
            geNaSample = geNaCurve.GetSample(0.5f);
            DrawArrow(geNaSample.Location, geNaSample.Forward);
            geNaSample = geNaCurve.GetSample(0.55f);
            DrawArrow(geNaSample.Location, geNaSample.Forward);
        }
        private void DrawArrow(Vector3 position, Vector3 direction)
        {
            Gizmos.color = Color.red;
            direction.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            Ray ray = new Ray(position, (-direction + right) * 0.5f);
            Gizmos.DrawRay(ray);
            ray.direction = (-direction - right) * 0.5f;
            Gizmos.DrawRay(ray);
        }
    }
}
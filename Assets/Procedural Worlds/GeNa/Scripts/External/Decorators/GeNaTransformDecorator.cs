using UnityEngine;
using System.Collections;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for modifying Transform for Spawn
    /// </summary>
    public class GeNaTransformDecorator : MonoBehaviour, IDecorator
    {
        [SerializeField] protected bool m_snapToGround = false;
        [SerializeField] protected bool m_conformToSlope = false;
        [SerializeField] protected Vector3 m_minScale = Vector3.one;
        [SerializeField] protected Vector3 m_maxScale = Vector3.one;
        [SerializeField] protected Vector3 m_minOffset = new Vector3(0f, -0.15f, 0f);
        [SerializeField] protected Vector3 m_maxOffset = new Vector3(0f, -0.15f, 0f);
        [SerializeField] protected Vector3 m_minRotation = Vector3.zero;
        [SerializeField] protected Vector3 m_maxRotation = Vector3.zero;
        #region Properties
        public bool SnapToGround
        {
            get => m_snapToGround;
            set => m_snapToGround = value;
        }
        public bool ConformToSlope
        {
            get => m_conformToSlope;
            set => m_conformToSlope = value;
        }
        public Vector3 MinScale
        {
            get => m_minScale;
            set => m_minScale = value;
        }
        public Vector3 MaxScale
        {
            get => m_maxScale;
            set => m_maxScale = value;
        }
        public Vector3 MinOffset
        {
            get => m_minOffset;
            set => m_minOffset = value;
        }
        public Vector3 MaxOffset
        {
            get => m_maxOffset;
            set => m_maxOffset = value;
        }
        public Vector3 MinRotation
        {
            get => m_minRotation;
            set => m_minRotation = value;
        }
        public Vector3 MaxRotation
        {
            get => m_maxRotation;
            set => m_maxRotation = value;
        }
        #endregion
        public bool UnpackPrefab => false;
        public void OnIngest(Resource resource)
        {
            Prototype prototype = resource.Prototype;
            resource.SetStatic(prototype, Constants.ResourceStatic.Dynamic);
            resource.SnapToGround = SnapToGround;
            resource.ConformToSlope = ConformToSlope;
            resource.MinOffset += MinOffset;
            resource.MaxOffset += MaxOffset;
            resource.MinRotation += MinRotation;
            resource.MaxRotation += MaxRotation;
            // res.SameScale = decorator.m_uniformScale;
            resource.MinScale = Vector3.Scale(resource.MinScale, MinScale);
            resource.MaxScale = Vector3.Scale(resource.MaxScale, MaxScale);
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
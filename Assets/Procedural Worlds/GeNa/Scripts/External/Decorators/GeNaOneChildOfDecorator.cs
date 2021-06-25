using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// This decorator will randomly spawn only one of the child objects or prefabs based on the ratio provided and will ignore the rest.
    /// </summary>
    public class GeNaOneChildOfDecorator : MonoBehaviour, IDecorator
    {
        /// <summary>
        /// Container class for associating weights with Transforms
        /// </summary>
        [Serializable]
        public class Child
        {
            public Transform transform;
            public float weight = 1.0f;
        }
        [SerializeField] protected List<Child> m_children = new List<Child>();
        public List<Child> Children => m_children;
        public bool UnpackPrefab => false;
        public void Reset()
        {
            RefreshChildren();
        }
        public void RefreshChildren()
        {
            List<Child> currentChildren = new List<Child>();
            foreach (Transform child in transform)
            {
                if (!child.gameObject.activeSelf)
                    continue;
                float weight = 1.0f;
                Child existing = m_children.Find(item => item.transform == child.transform);
                if (existing != null)
                {
                    weight = existing.weight;
                }
                currentChildren.Add(new Child
                {
                    transform = child,
                    weight = weight
                });
            }
            m_children = currentChildren;
        }
        public void OnIngest(Resource resource)
        {
            resource.OneChildOf = true;
            Prototype prototype = resource.Prototype;
            List<Resource> resChildren = prototype.GetChildren(resource);
            if (resChildren != null)
            {
                List<Child> childOfChildren = Children;
                if (resChildren.Count == childOfChildren.Count)
                {
                    for (int i = 0; i < resChildren.Count; i++)
                    {
                        Resource child = resChildren[i];
                        child.OneChildOfWeight = childOfChildren[i].weight;
                    }
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
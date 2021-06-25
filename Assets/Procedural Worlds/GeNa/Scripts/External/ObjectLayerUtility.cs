using System.Collections.Generic;
using UnityEngine;
namespace GeNa.Core
{
    public static class ObjectLayerUtility
    {
        #region Public Static
        /// <summary>
        /// Function used to apply the Object layers decorator too
        /// </summary>
        /// <param name="rootGameObject"></param>
        /// <returns></returns>
        public static void ApplyLayersToObjects(GameObject rootGameObject)
        {
            if (rootGameObject == null)
            {
                return;
            }
            IEnumerable<GeNaObjectLayerDecorator> decorators = GetLayerObjectChildenObjects(rootGameObject);
            foreach (GeNaObjectLayerDecorator decorator in decorators)
            {
                int layerMask = LayerMask.NameToLayer(decorator.m_layerName);
                if (layerMask < 0 || layerMask > 31)
                {
                    layerMask = 0;
                }
                string tag = decorator.Tags[decorator.TagIndex];
                if (string.IsNullOrEmpty(tag))
                {
                    tag = "Current";
                }
                if (decorator.ApplyToChildern)
                {
                    SetLayerToObject(GetChildenObjects(decorator.gameObject), layerMask);
                    SetTagToObject(GetChildenObjects(decorator.gameObject), tag);
                }
                else
                {
                    SetLayerToObject(decorator.gameObject, layerMask);
                    SetTagToObject(decorator.gameObject, tag);
                }
            }
        }
        #endregion
        #region Private Static
        /// <summary>
        /// Apllies layer to object and it's childern
        /// </summary>
        /// <param name="currentGameObjects"></param>
        /// <param name="layerValue"></param>
        private static void SetLayerToObject(GameObject[] currentGameObjects, int layerValue)
        {
            if (currentGameObjects.Length < 1)
            {
                return;
            }
            foreach (GameObject gameObject in currentGameObjects)
            {
                gameObject.layer = layerValue;
            }
        }
        /// <summary>
        /// Apllies layer to object
        /// </summary>
        /// <param name="currentGameObject"></param>
        /// <param name="layerValue"></param>
        private static void SetLayerToObject(GameObject currentGameObject, int layerValue)
        {
            if (currentGameObject == null)
            {
                return;
            }
            currentGameObject.layer = layerValue;
        }
        /// <summary>
        /// Apllies tag to object and it's childern
        /// </summary>
        /// <param name="currentGameObjects"></param>
        /// <param name="tagValue"></param>
        private static void SetTagToObject(GameObject[] currentGameObjects, string tagValue)
        {
            if (currentGameObjects.Length < 1 || string.IsNullOrEmpty(tagValue) || tagValue == "Current")
            {
                return;
            }
            foreach (GameObject gameObject in currentGameObjects)
            {
                gameObject.tag = tagValue;
            }
        }
        /// <summary>
        /// Apllies tag to object
        /// </summary>
        /// <param name="currentGameObject"></param>
        /// <param name="tagValue"></param>
        private static void SetTagToObject(GameObject currentGameObject, string tagValue) 
        {
            if (currentGameObject == null || string.IsNullOrEmpty(tagValue) || tagValue == "Current")
            {
                return;
            }
            currentGameObject.tag = tagValue;
        }
        /// <summary>
        /// Gets all the decorators from the root prefab
        /// </summary>
        /// <param name="currentGameObject"></param>
        /// <returns></returns>
        private static IEnumerable<GeNaObjectLayerDecorator> GetLayerObjectChildenObjects(GameObject currentGameObject)
        {
            if (currentGameObject == null)
            {
                return null;
            }
            List<GeNaObjectLayerDecorator> decorators = new List<GeNaObjectLayerDecorator>();
            Transform[] transforms = currentGameObject.GetComponentsInChildren<Transform>();
            if (transforms.Length > 0)
            {
                foreach (Transform transform in transforms)
                {
                    GeNaObjectLayerDecorator decorator = transform.GetComponent<GeNaObjectLayerDecorator>();
                    if (decorator != null)
                    {
                        decorators.Add(decorator);
                    }
                }
            }
            return decorators.ToArray();
        }
        /// <summary>
        /// Gets all the childern gameobjects from the supplied object
        /// </summary>
        /// <param name="currentGameObject"></param>
        /// <returns></returns>
        private static GameObject[] GetChildenObjects(GameObject currentGameObject)
        {
            if (currentGameObject == null)
            {
                return null;
            }
            List<GameObject> objects = new List<GameObject>();
            Transform[] transforms = currentGameObject.GetComponentsInChildren<Transform>();
            if (transforms.Length > 0)
            {
                foreach (Transform transform in transforms)
                {
                    objects.Add(transform.gameObject);
                }
            }
            return objects.ToArray();
        }
        #endregion
    }
}
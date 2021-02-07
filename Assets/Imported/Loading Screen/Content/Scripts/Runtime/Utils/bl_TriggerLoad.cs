using UnityEngine;

namespace Lovatto.SceneLoader
{
    /// <summary>
    /// Attach this script to a game object with a collider set as Trigger for load a scene when detect other trigger enter
    /// </summary>
    public class bl_TriggerLoad : MonoBehaviour
    {

        public string SceneName;

        private void OnTriggerEnter(Collider other)
        {
            bl_SceneLoaderUtils.GetLoader.LoadLevel(SceneName);
        }
    }
}
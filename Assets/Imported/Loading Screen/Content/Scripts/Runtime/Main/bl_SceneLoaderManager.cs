using UnityEngine;
using System.Collections;
using System.Linq;

namespace Lovatto.SceneLoader
{
    public class bl_SceneLoaderManager : ScriptableObject
    {

        [Header("Scene Manager")]
#if !ODIN_INSPECTOR
        [ReorderableLovatto(elementNameProperty = "myString")]
#endif
        public TheSceneList List;
#if !ODIN_INSPECTOR
        [Header("Tips"), ReorderableLovatto("Tips")]
#endif
        public TheTipList TipList;

        public bl_SceneLoaderInfo GetSceneInfo(string scene)
        {
            foreach(bl_SceneLoaderInfo info in List)
            {
                if(info.SceneName == scene)
                {
                    return info;
                }
            }
            
            Debug.Log("Not found any scene with this name: " + scene);
            return null;           
        }

        public bool HasTips
        {
            get
            {
                return (TipList != null && TipList.Count > 0);
            }
        }

        public string[] GetSceneNames()
        {
            return List.Select(x => x.SceneName).ToArray();
        }

        public static IEnumerator AsyncLoadData()
        {
            if (_instance == null)
            {
                ResourceRequest rr = Resources.LoadAsync("SceneLoaderManager", typeof(bl_SceneLoaderManager));
                while (!rr.isDone) { yield return null; }
                _instance = rr.asset as bl_SceneLoaderManager;
            }
        }

        public static bl_SceneLoaderManager _instance;
        public static bl_SceneLoaderManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<bl_SceneLoaderManager>("SceneLoaderManager") as bl_SceneLoaderManager;
                }
                return _instance;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < List.Count; i++)
            {
                if(List[i].SceneAsset != null)
                {
                    List[i].SceneName = List[i].SceneAsset.name;
                }
            }
        }
#endif
    }
}
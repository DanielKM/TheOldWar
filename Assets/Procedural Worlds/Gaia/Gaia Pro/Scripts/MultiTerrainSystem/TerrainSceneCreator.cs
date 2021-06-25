using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gaia
{
    public class TerrainSceneCreator : MonoBehaviour
    {
        public static TerrainScene CreateTerrainScene(Scene originalScene, TerrainSceneStorage terrainSceneStorage, GaiaSession session, GameObject terrainGO, WorldCreationSettings worldCreationSettings)
        {
#if UNITY_EDITOR
            //Create a new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.MoveGameObjectToScene(terrainGO, newScene);
            CopyLightingSettingsToNewScene(originalScene, newScene);

            string scenePath = "";
            //if the terrain scene storage file has no own path yet that means it is not saved in the asset database yet
            //in this case we fall back to the session path
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(terrainSceneStorage)))
            {
                scenePath = GaiaDirectories.GetTerrainScenePathForSession(session) + "/" + terrainGO.name + ".unity";
            }
            else
            {
                //Terrain scene storage has its own path -> let's create the terrain scene here then
                scenePath = GaiaDirectories.GetTerrainScenePathForStorageFile(terrainSceneStorage) + "/" + terrainGO.name + ".unity";
            }
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.ImportAsset(scenePath);
            Terrain terrain = terrainGO.GetComponent<Terrain>();

            if (worldCreationSettings.m_applyFloatingPointFix)
            {
                terrainGO.isStatic = false;
            }

            //Save the restored connection in the scene
            EditorSceneManager.SaveScene(newScene, scenePath);

            //create an entry for this terrain in the terrainScene list in the session
            TerrainScene terrainScene = new TerrainScene();
            terrainScene.m_pos = terrainGO.transform.position;
            //position the bounds center on the middle of the terrain
            terrainScene.m_bounds = new BoundsDouble(terrainScene.m_pos + new Vector3Double(terrain.terrainData.size.x / 2f, 0f, terrain.terrainData.size.z / 2f), terrain.terrainData.size);
            terrainScene.m_scenePath = scenePath;
            terrainScene.m_useFloatingPointFix = worldCreationSettings.m_applyFloatingPointFix;
            //sessionMgr.m_session.m_terrainScenes.Add(terrainScene);

            //Register this terrain in the min max terrain height cache in the session
            TerrainMinMaxHeight tmmh = new TerrainMinMaxHeight();
            if (worldCreationSettings.m_isWorldMap)
            {
                tmmh.isWorldmap = true;
            }
            tmmh.min = 0;
            tmmh.max = 0;
            tmmh.guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(terrain.terrainData));
            session.m_terrainMinMaxCache.Add(tmmh);

            if (worldCreationSettings.m_autoUnloadScenes)
            {
                EditorSceneManager.CloseScene(newScene, true);
                terrainScene.m_regularLoadState = LoadState.Unloaded;
            }
            else
            {
                terrainScene.m_regularLoadState = LoadState.Loaded;
            }

            return terrainScene;
#else
            return null;
#endif
        }

        public static void CopyLightingSettingsToNewScene(Scene sourceScene, Scene targetScene)
        {
#if UNITY_EDITOR
            //Copy Render settings for the new scene to prevent warnings for unequal lighting settings across scenes
            var ambientEquatorColor = RenderSettings.ambientEquatorColor;
            var ambientGroundColor = RenderSettings.ambientGroundColor;
            var ambientIntensity = RenderSettings.ambientIntensity;
            var ambientLight = RenderSettings.ambientLight;
            var ambientMode = RenderSettings.ambientMode;
            var ambientProbe = RenderSettings.ambientProbe;
            var ambientSkyColor = RenderSettings.ambientSkyColor;
            var customReflection = RenderSettings.customReflection;
            var defaultReflectionMode = RenderSettings.defaultReflectionMode;
            var defaultReflectionResolution = RenderSettings.defaultReflectionResolution;
            var flareFadeSpeed = RenderSettings.flareFadeSpeed;
            var flareStrength = RenderSettings.flareStrength;
            var fog = RenderSettings.fog;
            var fogColor = RenderSettings.fogColor;
            var fogDensity = RenderSettings.fogDensity;
            var fogEndDistance = RenderSettings.fogEndDistance;
            var fogMode = RenderSettings.fogMode;
            var fogStartDistance = RenderSettings.fogStartDistance;
            var haloStrength = RenderSettings.haloStrength;
            var reflectionBounces = RenderSettings.reflectionBounces;
            var reflectionIntensity = RenderSettings.reflectionIntensity;
            var skybox = RenderSettings.skybox;
            var subtractiveShadowColor = RenderSettings.subtractiveShadowColor;

            var LightmappingBakedGI = Lightmapping.bakedGI;
            var LightmappingRealtimeGI = Lightmapping.realtimeGI;
            var LightmappingGIWorkflowMode = Lightmapping.giWorkflowMode;
#if UNITY_2020_1_OR_NEWER
            LightingSettings.Lightmapper LightmapEditorSettingsLightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
            LightingSettings lightingSettings = new LightingSettings();
            if (Lightmapping.TryGetLightingSettings(out lightingSettings))
            {
                LightmapEditorSettingsLightmapper = lightingSettings.lightmapper;
            }
#else
            var LightmapEditorSettingsLightmapper = LightmapEditorSettings.lightmapper;
#endif
            EditorSceneManager.SetActiveScene(targetScene);

            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientProbe = ambientProbe;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.customReflection = customReflection;
            RenderSettings.defaultReflectionMode = defaultReflectionMode;
            RenderSettings.defaultReflectionResolution = defaultReflectionResolution;
            RenderSettings.flareFadeSpeed = flareFadeSpeed;
            RenderSettings.flareStrength = flareStrength;
            RenderSettings.fog = fog;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogEndDistance = fogEndDistance;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.haloStrength = haloStrength;
            RenderSettings.reflectionBounces = reflectionBounces;
            RenderSettings.reflectionIntensity = reflectionIntensity;
            RenderSettings.skybox = skybox;
            RenderSettings.subtractiveShadowColor = subtractiveShadowColor;

            Lightmapping.bakedGI = LightmappingBakedGI;
            Lightmapping.realtimeGI = LightmappingRealtimeGI;
            Lightmapping.giWorkflowMode = LightmappingGIWorkflowMode;
#if UNITY_2020_1_OR_NEWER
            Lightmapping.lightingSettings.lightmapper = LightmapEditorSettingsLightmapper;
#else
            LightmapEditorSettings.lightmapper = LightmapEditorSettingsLightmapper;
#endif

            //switch back to source scene
            EditorSceneManager.SetActiveScene(sourceScene);
#endif 
        }

        public static Scene CreateReplacementScene(Scene originalScene, GameObject impostorGO, GaiaSession session, string basePath)
        {
#if UNITY_EDITOR
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            //newScene.name = sectorPath;
            EditorSceneManager.MoveGameObjectToScene(impostorGO, newScene);
            CopyLightingSettingsToNewScene(originalScene, newScene);

            string scenePath = basePath + "/" + impostorGO.name + ".unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.ImportAsset(scenePath);
            return newScene;
#else
            return new Scene();
#endif
        }

        public static Scene CreateBackupScene(Scene originalScene, GameObject terrainGO, GaiaSession session)
        {
#if UNITY_EDITOR
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            //newScene.name = sectorPath;
            EditorSceneManager.MoveGameObjectToScene(terrainGO, newScene);
            CopyLightingSettingsToNewScene(originalScene, newScene);

            string scenePath = GaiaDirectories.GetBackupScenePath(session) + "/" + terrainGO.name.Replace("Terrain", "Backup") + ".unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.ImportAsset(scenePath);
            return newScene;
#else
            return new Scene();
#endif
        }

    }
}

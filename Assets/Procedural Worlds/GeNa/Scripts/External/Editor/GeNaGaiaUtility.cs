using UnityEditor;
using UnityEngine;
#if GAIA_2_PRESENT
using Gaia;
using ProceduralWorlds.WaterSystem;
#endif
namespace GeNa.Core
{
    [InitializeOnLoad]
    public class GeNaGaiaUtility
    {
        static GeNaGaiaUtility()
        {
            // If GeNa Present && Gaia Present
#if GAIA_PRO_PRESENT
            GeNaUtility.Gaia2Present = true;
            GeNaEvents.SetSeaLevel = SetSeaLevel;
            GeNaUtility.GaiaSeaLevelValue = GetRuntimeSeaLevel();
            GeNaEvents.GetSeaLevel = GetRuntimeSeaLevel;
#else
            GeNaUtility.Gaia2Present = false;
#endif
            GeNaEvents.UpdateTimeOfDayLightSync = UpdateAllTimeOfDayLightSyncs;
            GeNaEvents.UpdateTimeOfDayLightSyncShadows = UpdateAllTimeOfDayLightSyncsShadows;
            GeNaEvents.UpdateTimeOfDaySyncCulling = UpdateAllTimeOfDayLightSyncsCulling;
            GeNaEvents.SetupRiverWeatherSync = SetupRiverWeatherController;
        }
        /// <summary>
        /// Sets the sea level on the spawner (Min Height)
        /// </summary>
        /// <param name="spawner"></param>
        public static float SetSeaLevel(GeNaSpawnerData spawner)
        {
            if (spawner == null)
            {
                return 0f;
            }
            float seaLevel = 0f;
#if GAIA_2_PRESENT
            if (spawner.ExtraSeaLevelHeight > 0)
            {
                seaLevel = GetGaiaSeaLevel() + spawner.ExtraSeaLevelHeight;
                GeNaSpawnerInternal.UpdateMinMaxHeight(spawner, Constants.CriteriaRangeType.MinMax, new Vector2(seaLevel, spawner.SpawnCriteria.MaxHeight), spawner.GetSeaLevel, spawner.ExtraSeaLevelHeight);
            }
            else
            {
                seaLevel = GetGaiaSeaLevel() - Mathf.Abs(spawner.ExtraSeaLevelHeight);
                GeNaSpawnerInternal.UpdateMinMaxHeight(spawner, Constants.CriteriaRangeType.MinMax, new Vector2(seaLevel, spawner.SpawnCriteria.MaxHeight), spawner.GetSeaLevel, spawner.ExtraSeaLevelHeight);
            }
#endif
            return seaLevel;
        }
        /// <summary>
        /// Gets the gaia sea level value
        /// </summary>
        /// <returns></returns>
        private static float GetGaiaSeaLevel()
        {
            float seaLevel = -20;
#if GAIA_2_PRESENT
            GaiaSessionManager manager = Object.FindObjectOfType<GaiaSessionManager>();
            if (manager != null)
            {
                seaLevel = manager.GetSeaLevel();
            }
            else
            {
                PWS_WaterSystem waterSystem = PWS_WaterSystem.Instance;
                if (waterSystem != null)
                {
                    seaLevel = waterSystem.SeaLevel;
                }
            }
#endif
            return seaLevel;
        }
        /// <summary>
        /// Gets the sea level from the water system
        /// </summary>
        /// <returns></returns>
        public static float GetRuntimeSeaLevel(float defaultValue = 0f)
        {
            float seaLevel = defaultValue;
#if GAIA_2_PRESENT
            PWS_WaterSystem waterSystem = PWS_WaterSystem.Instance;
            if (waterSystem != null)
            {
                seaLevel = waterSystem.SeaLevel;
            }
#endif
            return seaLevel;
        }
        /// <summary>
        /// Updates all the time of day sync lights in the scene
        /// </summary>
        /// <param name="value"></param>
        public static bool UpdateAllTimeOfDayLightSyncs(bool value)
        {
            GaiaTimeOfDayLightSync[] syncs = GaiaTimeOfDayLightSync.GetAllInstances();
            if (syncs.Length > 0)
            {
                foreach (GaiaTimeOfDayLightSync sync in syncs)
                {
                    sync.SystemActive = value;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Updates all the time of day sync light shadows types
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static LightShadows UpdateAllTimeOfDayLightSyncsShadows(LightShadows value)
        {
            GaiaTimeOfDayLightSync[] syncs = GaiaTimeOfDayLightSync.GetAllInstances();
            if (syncs.Length > 0)
            {
                foreach (GaiaTimeOfDayLightSync sync in syncs)
                {
                    sync.LightShadowMode = value;
                }
            }
            return value;
        }
        /// <summary>
        /// Updates all the culling settings in the light sync system
        /// </summary>
        /// <returns></returns>
        public static bool UpdateAllTimeOfDayLightSyncsCulling()
        {
            GaiaTimeOfDayLightSync[] syncs = GaiaTimeOfDayLightSync.GetAllInstances();
            if (syncs.Length > 0)
            {
                GeNaManager manager = GeNaManager.GetInstance();
                if (manager != null)
                {
                    foreach (GaiaTimeOfDayLightSync sync in syncs)
                    {
                        sync.GetLightRenderSettings();
                        sync.SystemActive = manager.EnableTimeOfDayLightSync;
                        sync.PreviewSyncLightCullingInEditor = manager.PreviewSyncLightCullingInEditor;
                        sync.LightShadowMode = manager.TimeOfDayLightSyncShadowMode;
                        sync.LightCullingMode = manager.LightCullingMode;
                        sync.LightCullingDistance = manager.LightCullingDistance;
                        sync.CullingWaitForFrames = manager.CullingWaitForFrames;
                        sync.UpdateLightSettings(true);
                    }
                    return true;
                }
            }
            return false;
        }
        public static bool SetupRiverWeatherController(GameObject go, GeNaRiverProfile profile, float seaLevel, bool isEnabled)
        {
            GeNaRiverWeatherController controller = go.GetComponent<GeNaRiverWeatherController>();
            if (isEnabled)
            {
                if (controller == null)
                {
                    controller = go.AddComponent<GeNaRiverWeatherController>();
                }
                controller.m_riverProfile = profile;
                controller.SeaLevel = seaLevel;
                return true;
            }
            else
            {
                if (controller != null)
                {
                    GeNaEvents.Destroy(controller);
                }
                return false;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if GAIA_PRO_PRESENT

namespace Gaia.GX.ProceduralWorlds
{
    public class FloatingPointFixGX
    {
#region Private values

        private static string m_unityVersion;
        private static List<string> m_profileList = new List<string>();
        private static List<Material> m_allMaterials = new List<Material>();


#endregion

#region Generic informational methods

        /// <summary>
        /// Returns the publisher name if provided. 
        /// This will override the publisher name in the namespace ie Gaia.GX.PublisherName
        /// </summary>
        /// <returns>Publisher name</returns>
        public static string GetPublisherName()
        {
            return "Procedural Worlds";
        }

        /// <summary>
        /// Returns the package name if provided
        /// This will override the package name in the class name ie public class PackageName.
        /// </summary>
        /// <returns>Package name</returns>
        public static string GetPackageName()
        {
            return "Large World Support";
        }

#endregion

#region Methods exposed by Gaia as buttons must be prefixed with GX_

        /// <summary>
        /// Adds water system to the scene
        /// </summary>
        public static void GX_FloatingPointFix_Add()
        {
            if (EditorUtility.DisplayDialog("Add Floating Point Fix?", "This will activate a fix for floating point imprecisions in your scene. This will add aditional components to your player, and other objects in your scene. Continue?", "OK", "Cancel"))
            {

                //Add main fix to player

                GameObject playerObj = GameObject.Find(GaiaConstants.playerFlyCamName);

                if (playerObj == null)
                {
                    playerObj = GameObject.Find(GaiaConstants.playerFirstPersonName);
                }

                if (playerObj == null)
                {
                    playerObj = GameObject.Find(GaiaConstants.playerThirdPersonName);
                }


                if (playerObj != null)
                {
                    FloatingPointFix fix = playerObj.GetComponent<FloatingPointFix>();
                    if (fix == null)
                    {
                        fix = playerObj.AddComponent<FloatingPointFix>();
                    }

                    fix.threshold = GaiaUtils.GetGaiaSettings().m_FPFDefaultThreshold;
                }

                //Check if we are in a placeholder / terrain loading setup - if yes switch on all placeholders for the fix
                if (GaiaUtils.HasDynamicLoadedTerrains())
                {
                    foreach (TerrainScene ts in TerrainLoaderManager.TerrainScenes)
                    {
                        ts.m_useFloatingPointFix = true;
                    }

                    if (EditorUtility.DisplayDialog("Adjust unloaded Terrains?", "You are using dynamic terrain loading with terrain placeholders. Do you want to load all terrains after another to apply the fix to them and make all objects non-static?", "OK", "Cancel"))
                    {
                        GaiaUtils.CallFunctionOnDynamicLoadedTerrains(AddFloatingPointFixToTerrain, true);
                    }
                }
                else
                {
                    //regular terrain setup - add the membership to all terrains in the scene instead.
                    foreach (Terrain terrain in Terrain.activeTerrains)
                    {
                        AddFloatingPointFixToTerrain(terrain);
                    }
                }

            }
        }

        public static void AddFloatingPointFixToTerrain(Terrain terrain)
        {
            FloatingPointFixMember ffMember = terrain.gameObject.GetComponent<FloatingPointFixMember>();
            if (ffMember == null)
            {
                ffMember = terrain.gameObject.AddComponent<FloatingPointFixMember>();
            }
            SetAllChildsNonStatic(terrain.transform);
        }

        private static void RemoveFloatingPointFixToTerrain(Terrain terrain)
        {
            FloatingPointFixMember ffMember = terrain.gameObject.GetComponent<FloatingPointFixMember>();
            if (ffMember != null)
            {
                Component.DestroyImmediate(ffMember);
            }
        }


        public static void SetAllChildsNonStatic(Transform transform)
        {
            transform.gameObject.isStatic = false;
            foreach (Transform t in transform)
            {
                SetAllChildsNonStatic(t);
            }
        }


        /// <summary>
        /// Removes water system from the scene
        /// </summary>
        public static void GX_FloatingPointFix_Remove()
        {
            if (EditorUtility.DisplayDialog("Remove Floating Point Fix?", "This will remove all floating point fix components in the scene. Continue?", "OK", "Cancel"))
            {

                //Remove main fix to player

                GameObject playerObj = GameObject.Find(GaiaConstants.playerFlyCamName);

                if (playerObj == null)
                {
                    playerObj = GameObject.Find(GaiaConstants.playerFirstPersonName);
                }

                if (playerObj == null)
                {
                    playerObj = GameObject.Find(GaiaConstants.playerThirdPersonName);
                }


                if (playerObj != null)
                {
                    FloatingPointFix fix = playerObj.GetComponent<FloatingPointFix>();
                    if (fix != null)
                    {
                        Component.DestroyImmediate(fix);
                    }
                }

                //Remove membership from water

                GameObject waterGO = GameObject.Find(GaiaConstants.waterSurfaceObject);
                if (waterGO != null)
                {
                    FloatingPointFixMember ffMember = waterGO.transform.parent.gameObject.GetComponent<FloatingPointFixMember>();
                    if (ffMember != null)
                    {
                        Component.DestroyImmediate(ffMember);
                    }
                }

                //Check if we are in a terrain loading setup - if yes switch off all placeholders for the fix
                if (GaiaUtils.HasDynamicLoadedTerrains())
                {
                    foreach (TerrainScene ts in TerrainLoaderManager.TerrainScenes)
                    {
                        ts.m_useFloatingPointFix = false;
                    }

                    if (EditorUtility.DisplayDialog("Adjust unloaded Terrains?", "You are using dynamic terrain loading with terrain placeholders. Do you want to load all terrains one after another to remove the fix from those as well?", "OK", "Cancel"))
                    {
                        GaiaUtils.CallFunctionOnDynamicLoadedTerrains(RemoveFloatingPointFixToTerrain, true);
                    }
                }
                else
                {
                    //regular terrain setup - add the membership to all terrains in the scene instead.
                    foreach (Terrain terrain in Terrain.activeTerrains)
                    {
                        RemoveFloatingPointFixToTerrain(terrain);
                    }
                }

            }
        }


#endregion

#region Utils

        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns></returns>
        private static string GetAssetPath(string name)
        {
            string[] assets = AssetDatabase.FindAssets(name, null);
            if (assets.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(assets[0]);
            }
            return null;
        }

      

        /// <summary>
        /// Removes Suffix in file formats required
        /// </summary>
        /// <param name="path"></param>
        private static List<Material> GetMaterials(string path)
        {
            List<Material> materials = new List<Material>();

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            var files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Extension.EndsWith("mat"))
                {
                    materials.Add(AssetDatabase.LoadAssetAtPath<Material>(GaiaUtils.GetAssetPath(file.Name)));
                }
            }

            m_allMaterials = materials;

            return materials;
        }

#endregion
    }
}
#endif
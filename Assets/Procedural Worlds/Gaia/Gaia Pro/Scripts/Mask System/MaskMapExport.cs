using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if GAIA_PRO_PRESENT

namespace Gaia
{



    public class MaskMapExport : MonoBehaviour
    {

#region public variables
        public List<int> m_previewRGBAIds = new List<int>();
        public bool m_drawPreview;
        public float m_maxWorldHeight;
        public float m_minWorldHeight;
        public bool m_previewDirty;
        public bool m_showSeaLevelinStampPreview = true;
        public bool m_showSeaLevelPlane = true;
        public bool m_showBoundingBox = true;
        public RenderTexture m_cachedExportTexture;
        public int m_impostorLoadingRange;
        public bool m_highlightLoadingSettings;
        public long m_highlightLoadingSettingsStartedTimeStamp;

        #endregion

        #region private variables
        private int m_currentTerrainCount;
        private RenderTexture[] m_cachedPreviewColorRenderTextures = new RenderTexture[GaiaConstants.maxPreviewedTextures];
        private RenderTexture m_exportRT;
        private BoundsDouble m_bounds = new BoundsDouble();
#endregion

#region Properties


        [SerializeField]
        private MaskMapExportSettings settings;
        /// <summary>
        /// The current spawner settings
        /// </summary>
        public MaskMapExportSettings m_settings
        {
            get
            {
                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance<MaskMapExportSettings>();
                    settings.m_exportDirectory = GaiaDirectories.GetMaskMapExportDirectory(SessionManager.m_session);
                    if(this!=null)
                    settings.name = name;
                }
                return settings;
            }
            set
            {
                settings = value;
            }
        }

        private GaiaSessionManager m_sessionManager;
        private GaiaSessionManager SessionManager
        {
            get
            {
                if (m_sessionManager == null)
                {
                    m_sessionManager = GaiaSessionManager.GetSessionManager(false);
                }
                return m_sessionManager;
            }
        }

        private GaiaSettings m_gaiaSettings;
        private GaiaSettings GaiaSettings
        {
            get
            {
                if (m_gaiaSettings == null)
                {
                    m_gaiaSettings = GaiaUtils.GetGaiaSettings();
                }
                return m_gaiaSettings;
            }
        }
#if GAIA_PRO_PRESENT
        public LoadMode m_loadTerrainMode = LoadMode.EditorSelected;

        private TerrainLoader m_terrainLoader;
        public TerrainLoader TerrainLoader
        {
            get
            {
                if (m_terrainLoader == null)
                {
                    if (this != null)
                    {
                        m_terrainLoader = gameObject.GetComponent<TerrainLoader>();

                        if (m_terrainLoader == null)
                        {
                            m_terrainLoader = gameObject.AddComponent<TerrainLoader>();
                            m_terrainLoader.hideFlags = HideFlags.HideInInspector;
                        }
                    }
                }
                return m_terrainLoader;
            }
        }
#endif
#endregion

        private MaskMapChannelSettings GetChannelSettingsByRGBAIndex(int rgbaIndex)
        {
            switch (rgbaIndex)
            {
                case 0:
                    return m_settings.m_redChannelSettings;
                case 1:
                    return m_settings.m_greenChannelSettings;
                case 2:
                    return m_settings.m_blueChannelSettings;
                case 3:
                    return m_settings.m_alphaChannelSettings;
            }
            return null;
        }

        public ImageMask[] GetImageMaskListByRGBAIndex(int rgbaIndex)
        {
            switch (rgbaIndex)
            {
                case 0:
                    return m_settings.m_red_imageMasks;
                case 1:
                    return m_settings.m_green_imageMasks;
                case 2:
                    return m_settings.m_blue_imageMasks;
                case 3:
                    return m_settings.m_alpha_imageMasks;
            }
            return null;
        }

        public Terrain GetCurrentTerrain()
        {
            Terrain currentTerrain = Gaia.TerrainHelper.GetTerrain(transform.position);
            //Check if the spawner is over a terrain currently

            return currentTerrain;
        }

        public float GetMaxSpawnerRange()
        {
            Terrain currentTerrain = GetCurrentTerrain();
            if (currentTerrain != null)
            {
                return Mathf.Round((float)4097 / (float)currentTerrain.terrainData.heightmapResolution * currentTerrain.terrainData.size.x / 2f);
            }
            else
            {
                return 1000;
            }
        }


        public void UpdateAutoLoadRange()
        {
#if GAIA_PRO_PRESENT
            if (m_loadTerrainMode != LoadMode.Disabled)
            {
                float width = m_settings.m_range * 2f;
                //reduce the loading width a bit => this is to prevent loading in terrains when the spawner bounds end exactly at the border of
                //surrounding terrains, this loads in a lot of extra terrains which are not required for the spawn 
                width -= 0.5f;
                Vector3 center = transform.position;
                TerrainLoader.m_loadingBoundsRegular.center = center;
                TerrainLoader.m_loadingBoundsRegular.size = new Vector3(width, width, width);
                TerrainLoader.m_loadingBoundsImpostor.center = center;
                if (m_impostorLoadingRange > 0)
                {
                    TerrainLoader.m_loadingBoundsImpostor.size = new Vector3(width + m_impostorLoadingRange, width + m_impostorLoadingRange, width + m_impostorLoadingRange);
                }
                else
                {
                    TerrainLoader.m_loadingBoundsImpostor.size = Vector3.zero;
                }
            }
            TerrainLoader.LoadMode = m_loadTerrainMode;
#endif
        }

        private void OnDestroy()
        {
            m_settings.ClearImageMaskTextures();
        }

        void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (Selection.activeObject == gameObject)
            {
                if (m_showBoundingBox)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(transform.position, new Vector3(m_settings.m_range * 2f, m_settings.m_range * 2f, m_settings.m_range * 2f));
                }

                //Water
                if (m_showSeaLevelPlane)
                {
                    BoundsDouble bounds = new BoundsDouble();
                    if (TerrainHelper.GetTerrainBounds(ref bounds) == true)
                    {
                        bounds.center = new Vector3Double(bounds.center.x, SessionManager.GetSeaLevel(), bounds.center.z);
                        bounds.size = new Vector3Double(bounds.size.x, 0.05f, bounds.size.z);
                        Gizmos.color = new Color(Color.blue.r, Color.blue.g, Color.blue.b, Color.blue.a / 4f);
                        Gizmos.DrawCube(bounds.center, bounds.size);
                    }
                }
            }

#endif
        }

        public void DrawMaskMapPreview(bool simulate)
        {
            List<int> activeRGBAIds = new List<int>();
            if (m_drawPreview || simulate)
            {
                //early out if no channel is active 
                bool foundActive = false;
                for (int i = 0; i <=3 ; i++)
                {
                    if (GetChannelSettingsByRGBAIndex(i).m_channelIsActive)
                    {
                        activeRGBAIds.Add(i);
                        foundActive = true;
                    }
                }
                if (!foundActive)
                {
                    return;
                }

                //Set up a multi-terrain operation once, all rules can then draw from the data collected here
                Terrain currentTerrain = GetCurrentTerrain();
                if (currentTerrain == null)
                {
                    //No terrain -> nothing to do
                    return;
                }

                GaiaMultiTerrainOperation operation = new GaiaMultiTerrainOperation(currentTerrain, transform, m_settings.m_range * 2f);
                operation.GetHeightmap();

                //only re-generate all textures etc. if settings have changed and the preview is dirty, otherwise we can just use the cached textures
                if (m_previewDirty == true)
                {
                    //To get a combined preview of different textures on a single mesh we need one color texture each per previewed 
                    // rule to determine the color areas on the heightmap mesh
                    // We need to iterate over the rules that are previewed, and build those color textures in this process

                    //Get additional op data (required for certain image masks)
                    operation.GetNormalmap();
                    operation.CollectTerrainBakedMasks();
                    //Preparing a simple add operation on the image mask shader for the combined heightmap texture
                    //Material filterMat = new Material(Shader.Find("Hidden/Gaia/FilterImageMask"));
                    //filterMat.SetFloat("_Strength", 1f);
                    //filterMat.SetInt("_Invert", 0);

                    //Store the currently active render texture here before we start manipulating
                    RenderTexture currentRT = RenderTexture.active;

                    //Clear texture cache first
                    ClearColorTextureCache();

                    //bool firstActiveRule = true;

                    for (int i = 0; i < activeRGBAIds.Count; i++)
                    {
                        MaskMapChannelSettings channelSettings = GetChannelSettingsByRGBAIndex(activeRGBAIds[i]);
                        
                        //Initialise our color texture cache at this index with this context
                        InitialiseColorTextureCache(i, operation.RTheightmap);
                        //Store result for this rule in our cache render texture array
                        Graphics.Blit(ApplyBrush(operation, MultiTerrainOperationType.Heightmap, channelSettings, activeRGBAIds[i]), m_cachedPreviewColorRenderTextures[i]);
                        RenderTexture.active = currentRT;
                    }

                    //Everything processed, check if the preview is not dirty anymore
                    m_previewDirty = false;
                }

                //Now draw the preview according to the cached textures
                if (!simulate)
                {
                    Material material = GaiaMultiTerrainOperation.GetDefaultGaiaSpawnerPreviewMaterial();
                    material.SetInt("_zTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);

                    //assign all color textures for the previewed channels in the material
                    for (int i = 0; i < activeRGBAIds.Count; i++)
                    {
                        int previewIndex = m_previewRGBAIds.LastIndexOf(activeRGBAIds[i]);

                       if (previewIndex!=-1)
                        {
                            material.SetTexture("_colorTexture" + previewIndex.ToString(), m_cachedPreviewColorRenderTextures[i]);
                        }
                    }

                    //iterate through spawn rules, and if it is a previewed texture set its color accordingly in the color slot
                    int colorIndex = 0;
                    for (int i = 0; i < m_previewRGBAIds.Count; i++)
                    {
                        material.SetColor("_previewColor" + colorIndex.ToString(), GetChannelSettingsByRGBAIndex(m_previewRGBAIds[colorIndex]).m_visualisationColor);
                        colorIndex++;
                    }

                    for (; colorIndex < GaiaConstants.maxPreviewedTextures; colorIndex++)
                    {
                        Color transparentColor = Color.white;
                        transparentColor.a = 0f;
                        material.SetColor("_previewColor" + colorIndex.ToString(), transparentColor);
                    }


                    Color seaLevelColor = GaiaSettings.m_stamperSeaLevelTintColor;
                    if (!m_showSeaLevelinStampPreview)
                    {
                        seaLevelColor.a = 0f;
                    }
                    material.SetColor("_seaLevelTintColor", seaLevelColor);
                    material.SetFloat("_seaLevel", SessionManager.m_session.m_seaLevel);
                    operation.Visualize(MultiTerrainOperationType.Heightmap, operation.RTheightmap, material, 1);

                    //Clean up
                    operation.CloseOperation();
                }

                //combine all the created color channel textures into the export texture - this texture can then be displayed in preview and exported if required
                UpdateCachedExportTexture();

                //Clean up temp textures
                GaiaUtils.ReleaseAllTempRenderTextures();

            }
            
        }

        private RenderTexture ApplyBrush(GaiaMultiTerrainOperation operation, MultiTerrainOperationType opType, MaskMapChannelSettings channelSettings, int rgbaIndex)
        {
            Terrain currentTerrain = GetCurrentTerrain();

            RenderTextureDescriptor rtDescriptor;

            switch (opType)
            {
                case MultiTerrainOperationType.Heightmap:
                    rtDescriptor = operation.RTheightmap.descriptor;
                    break;
                case MultiTerrainOperationType.Texture:
                    rtDescriptor = operation.RTtextureSplatmap.descriptor;
                    break;
                case MultiTerrainOperationType.TerrainDetail:
                    rtDescriptor = operation.RTdetailmap.descriptor;
                    break;
                case MultiTerrainOperationType.Tree:
                    rtDescriptor = operation.RTterrainTree.descriptor;
                    break;
                case MultiTerrainOperationType.GameObject:
                    rtDescriptor = operation.RTgameObject.descriptor;
                    break;
                default:
                    rtDescriptor = operation.RTheightmap.descriptor;
                    break;
            }
            //Random write needs to be enabled for certain mask types to function!
            rtDescriptor.enableRandomWrite = true;
            RenderTexture inputTexture = RenderTexture.GetTemporary(rtDescriptor);
            //RenderTexture inputTexture2 = RenderTexture.GetTemporary(rtDescriptor);

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = inputTexture;
            GL.Clear(true, true, Color.white);
            //RenderTexture.active = inputTexture2;
            //GL.Clear(true, true, Color.white);
            RenderTexture.active = currentRT;

            //fetch the biome mask stack (if any)
            //BiomeController biomeController = Resources.FindObjectsOfTypeAll<BiomeController>().FirstOrDefault(x => x.m_autoSpawners.Find(y => y.spawner == this) != null);

            ImageMask[] maskStack = new ImageMask[0];
            //set up the spawner mask stack, only if it has masks or a biome controller exists with masks
            if (GetImageMaskListByRGBAIndex(rgbaIndex).Length > 0)
            {
               
                maskStack = GetImageMaskListByRGBAIndex(rgbaIndex);
               
                //We start from a white texture, so we need the first mask action in the stack to always be "Multiply", otherwise there will be no result.
                maskStack[0].m_blendMode = ImageMaskBlendMode.Multiply;

                //Iterate through all image masks and set up the current paint context in case the shader uses heightmap data
                foreach (ImageMask mask in maskStack)
                {
                    mask.m_multiTerrainOperation = operation;
                    mask.m_seaLevel = SessionManager.GetSeaLevel();
                    mask.m_maxWorldHeight = m_maxWorldHeight;
                    mask.m_minWorldHeight = m_minWorldHeight;
                }

            }
           
            //Get the combined masks from the spawner
            RenderTexture outputTexture = RenderTexture.GetTemporary(rtDescriptor);
            Graphics.Blit(ImageProcessing.ApplyMaskStack(inputTexture, outputTexture, maskStack, ImageMaskInfluence.Local), outputTexture);

            //clean up temporary textures
            ReleaseRenderTexture(inputTexture);
            inputTexture = null;

            return outputTexture;
        }

        public void UpdateCachedExportTexture()
        {
            List<int> activeRGBAIds = new List<int>();
            for (int i = 0; i <= 3; i++)
            {
                if (GetChannelSettingsByRGBAIndex(i).m_channelIsActive)
                {
                    activeRGBAIds.Add(i);
                }
            }

            if (m_cachedExportTexture != null)
            {
                m_cachedExportTexture.Release();
                DestroyImmediate(m_cachedExportTexture);
            }
            RenderTexture currentRT = RenderTexture.active;
            m_cachedExportTexture = new RenderTexture(m_cachedPreviewColorRenderTextures[0].width, m_cachedPreviewColorRenderTextures[0].height, 0, RenderTextureFormat.ARGB32);
            Material mat = new Material(Shader.Find("Hidden/Gaia/CombineColorChannels"));

            //Grab the result from the color texture cache and combine it in the color channel combine shader
            RenderTexture inputTexture = RenderTexture.GetTemporary(m_cachedPreviewColorRenderTextures[0].width, m_cachedPreviewColorRenderTextures[0].height, 0, RenderTextureFormat.ARGB32);
            for (int i = 0; i < activeRGBAIds.Count; i++)
            {
                switch (activeRGBAIds[i])
                {
                    case 0:
                        mat.SetTexture("_RedChannelTex", m_cachedPreviewColorRenderTextures[i]);
                        break;
                    case 1:
                        mat.SetTexture("_GreenChannelTex", m_cachedPreviewColorRenderTextures[i]);
                        break;
                    case 2:
                        mat.SetTexture("_BlueChannelTex", m_cachedPreviewColorRenderTextures[i]);
                        break;
                    case 3:
                        mat.SetTexture("_AlphaChannelTex", m_cachedPreviewColorRenderTextures[i]);
                        break;
                }
            }

            Graphics.Blit(inputTexture, m_cachedExportTexture, mat);

            RenderTexture.active = currentRT;
            ReleaseRenderTexture(inputTexture);
            inputTexture = null;

        }

        public void FitToTerrain(Terrain t = null)
        {
            if (t == null)
            {
                t = Gaia.TerrainHelper.GetTerrain(transform.position);
                if (t == null)
                {
                    t = Terrain.activeTerrain;
                }
                if (t == null)
                {
                    Debug.LogWarning("Could not fit to terrain - no terrain present");
                    return;
                }
            }

            Bounds b = new Bounds();
            if (TerrainHelper.GetTerrainBounds(t, ref b))
            {
                transform.position = new Vector3(b.center.x, t.transform.position.y, b.center.z);
                m_settings.m_range = b.extents.x;
            }
        }

        /// <summary>
        /// Starts the export from the editor UI by creating an entry in the session, then executes it
        /// </summary>
        /// <param name="allTerrains">Whether the export should be applied to all terrains (global export) or just locally at the current range / position only.</param>
        public void StartExport(bool allTerrains)
        {
            ExportMaskMapOperationSettings exportMaskMapOperationSettings = ScriptableObject.CreateInstance<ExportMaskMapOperationSettings>();
            m_settings.m_x = transform.position.x;
            m_settings.m_y = transform.position.y;
            m_settings.m_z = transform.position.z;
            exportMaskMapOperationSettings.m_maskMapExportSettings = m_settings;
            exportMaskMapOperationSettings.isGlobalExport = allTerrains;

            if (allTerrains)
            {
                if (GaiaUtils.HasDynamicLoadedTerrains())
                {
                    exportMaskMapOperationSettings.m_terrainNames = TerrainLoaderManager.TerrainScenes.Select(x=>x.GetTerrainName()).ToList();
                }
                else
                {

                    foreach (Terrain terrain in Terrain.activeTerrains)
                    {
                        exportMaskMapOperationSettings.m_terrainNames.Add(terrain.name);
                    }
                }
            }
            else
            {
                exportMaskMapOperationSettings.m_terrainNames.Add(GetCurrentTerrain().name);
            }

            GaiaSessionManager.ExportMaskMap(exportMaskMapOperationSettings, true, this);

        }

        /// <summary>
        /// Actually executes the export, should only be called from the session manager unless you want to bypass the session system
        /// </summary>
        public void ExecuteExport(bool allTerrains, List<string> terrainNames)
        {
            TerrainHelper.GetTerrainBounds(ref m_bounds);
            m_currentTerrainCount = 0;
            if (allTerrains)
            {
                if (m_settings.m_multiTerrainOption == MaskMapExportMultiTerrainOption.OneCombinedTexture)
                {
                    //prepare the render texture for the combined export image
                    m_exportRT = RenderTexture.GetTemporary(m_settings.m_combinedTextureResolution, m_settings.m_combinedTextureResolution, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);
                }


                if (GaiaUtils.HasDynamicLoadedTerrains())
                {
                    GaiaUtils.CallFunctionOnDynamicLoadedTerrains(ExportMultiTerrain, false, terrainNames);
                }
                else
                {

                    foreach (Terrain terrain in Terrain.activeTerrains)
                    {
                        if (terrainNames.Contains(terrain.name))
                        {
                            //GaiaUtils.DisplayProgressBarNoEditor("Exporting Terrains", String.Format("Exported {0} of {1} Terrains", m_currentTerrainCount.ToString(), Terrain.activeTerrains.Length.ToString()), (float)m_currentTerrainCount / (float)Terrain.activeTerrains.Length);
                            if (ProgressBar.Show(ProgressBarPriority.MaskMapExport, "Exporting Terrains", "Exporting Terrains...", m_currentTerrainCount, Terrain.activeTerrains.Length, true, true))
                            {
                                //cancel was pressed
                                break;
                            }
                            ExportSingleTerrain(terrain, true);
                            m_currentTerrainCount++;
                        }

                    }
                    ProgressBar.Clear(ProgressBarPriority.MaskMapExport);
                }
                FitToAllTerrains();

                if (m_settings.m_multiTerrainOption == MaskMapExportMultiTerrainOption.OneCombinedTexture)
                {
                    WriteExportedTexture("Combined", m_exportRT);
                    m_exportRT.Release();
                    m_exportRT = null;
                }
            }
            else
            {
                ExportSingleTerrain(GetCurrentTerrain(), false);
                m_currentTerrainCount = 1;
            }

            Debug.Log(String.Format("Exported {0} terrain(s) to directory '{1}'", m_currentTerrainCount.ToString(), m_settings.m_exportDirectory));
        }


        /// <summary>
        /// Just a wrapper function for the actual ExportSingleTerrain to allow execution on all placeholder terrains.
        /// </summary>
        /// <param name="terrain"></param>
        private void ExportMultiTerrain(Terrain terrain)
        {
            ExportSingleTerrain(terrain, true);
            m_currentTerrainCount++;
        }

        
        /// <summary>
        /// Exports a single terrain with the current mask map export settings
        /// </summary>
        /// <param name="terrain">The terrain to export from</param>
        /// <param name="fitToTerrain">Controls if the export range should be fit to the terrain. If not, the export will be at the current position / range as set up by the user.</param>
        private void ExportSingleTerrain(Terrain terrain, bool fitToTerrain = true)
        {
            terrain.terrainData.SyncHeightmap();
            terrain.editorRenderFlags = TerrainRenderFlags.All;
            if (fitToTerrain)
            {
                //position tool above terrain
                FitToTerrain(terrain);
            }
            m_previewDirty = true;
            DrawMaskMapPreview(true);
            if (m_settings.m_multiTerrainOption == MaskMapExportMultiTerrainOption.OneCombinedTexture && fitToTerrain)
            {
                //User wants one combined result texture of all terrains -> copy them together in the export renderTexture
                CopyLocalResultToRenderTexture(terrain);
            }
            else
            {
                //One separate image per terrain each, export the cached export texture directly.
                WriteExportedTexture(terrain.name, m_cachedExportTexture);
                
               
            }
       }

        private void WriteExportedTexture(string nameSuffix, RenderTexture textureToWrite)
        {
            string path = m_settings.m_exportDirectory + Path.DirectorySeparatorChar;
            GaiaDirectories.CreatePathIfDoesNotExist(path);
            path += m_settings.m_exportFileName;
            if (m_settings.m_addTerrainNameToFileName)
            {
                path += "_" + nameSuffix;
            }
            else
            {
                path += "_" + m_currentTerrainCount.ToString();
            }

            path = ImageProcessing.WriteRenderTexture(path, textureToWrite, m_settings.m_exportFileType, TextureFormat.RGBAFloat, m_settings.m_exportJpgQuality);
#if UNITY_EDITOR
          
            AssetDatabase.ImportAsset(path);
            Texture2D exportedTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            if (exportedTexture != null)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.isReadable = m_settings.m_readWriteEnabled;
                }

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                EditorGUIUtility.PingObject(exportedTexture);
            }
#endif
        }

        public void LoadSettings(MaskMapExportSettings settingsToLoad)
        {
            //Set existing settings = null to force a new scriptable object
            m_settings = null;
            m_settings = Instantiate(settingsToLoad);
            transform.position = new Vector3(m_settings.m_x, m_settings.m_y, m_settings.m_z);

            //Try to look up all collision layer masks by their name where possible - layer orders could be different from when the mask map settings file was saved.
            foreach (ImageMask imageMask in m_settings.m_red_imageMasks.Where(x => x.m_operation == ImageMaskOperation.CollisionMask))
            {
                imageMask.TryRefreshCollisionMask();
            }
            foreach (ImageMask imageMask in m_settings.m_green_imageMasks.Where(x => x.m_operation == ImageMaskOperation.CollisionMask))
            {
                imageMask.TryRefreshCollisionMask();
            }
            foreach (ImageMask imageMask in m_settings.m_blue_imageMasks.Where(x => x.m_operation == ImageMaskOperation.CollisionMask))
            {
                imageMask.TryRefreshCollisionMask();
            }

        }

        /// <summary>
        /// Position and fit the spawner to the terrain
        /// </summary>
        public void FitToAllTerrains()
        {
            BoundsDouble b = new BoundsDouble();
            if (TerrainHelper.GetTerrainBounds(ref b))
            {
                transform.position = b.center;
                m_settings.m_range = (float)b.extents.x;
            }
        }


        public void UpdateMinMaxHeight()
        {
            SessionManager.GetWorldMinMax(ref m_minWorldHeight, ref m_maxWorldHeight);
            float seaLevel = SessionManager.GetSeaLevel();
            //Iterate through all image masks and set up the current min max height
            //This is fairly important to display the height-dependent mask settings correctly

            foreach (ImageMask mask in m_settings.m_red_imageMasks)
            {
                mask.m_maxWorldHeight = m_maxWorldHeight;
                mask.m_minWorldHeight = m_minWorldHeight;
                mask.m_seaLevel = seaLevel;
            }
            ImageMask.CheckMaskStackForInvalidTextureRules("Mask Map Export", this.name, m_settings.m_red_imageMasks);

            foreach (ImageMask mask in m_settings.m_green_imageMasks)
            {
                mask.m_maxWorldHeight = m_maxWorldHeight;
                mask.m_minWorldHeight = m_minWorldHeight;
                mask.m_seaLevel = seaLevel;
            }
            ImageMask.CheckMaskStackForInvalidTextureRules("Mask Map Export", this.name, m_settings.m_green_imageMasks);

            foreach (ImageMask mask in m_settings.m_blue_imageMasks)
            {
                mask.m_maxWorldHeight = m_maxWorldHeight;
                mask.m_minWorldHeight = m_minWorldHeight;
                mask.m_seaLevel = seaLevel;
            }
            ImageMask.CheckMaskStackForInvalidTextureRules("Mask Map Export", this.name, m_settings.m_blue_imageMasks);

            foreach (ImageMask mask in m_settings.m_alpha_imageMasks)
            {
                mask.m_maxWorldHeight = m_maxWorldHeight;
                mask.m_minWorldHeight = m_minWorldHeight;
                mask.m_seaLevel = seaLevel;
            }
            ImageMask.CheckMaskStackForInvalidTextureRules("Mask Map Export", this.name, m_settings.m_alpha_imageMasks);

        }


        public void HighlightLoadingSettings()
        {
            m_highlightLoadingSettings = true;
            m_highlightLoadingSettingsStartedTimeStamp = GaiaUtils.GetUnixTimestamp();
        }

        private void CopyLocalResultToRenderTexture(Terrain t)
        {
            RenderTextureDescriptor rtDesc = m_exportRT.descriptor;
            rtDesc.width = Mathd.CeilToInt(m_exportRT.width / (m_bounds.size.x / t.terrainData.bounds.size.x));
            rtDesc.height = rtDesc.width;

            RenderTexture chunkContent = RenderTexture.GetTemporary(rtDesc);
            float res = (float)rtDesc.width / (float)t.terrainData.heightmapResolution;

            Bounds worldSpaceBounds = t.terrainData.bounds;
            worldSpaceBounds.center = new Vector3(worldSpaceBounds.center.x + t.transform.position.x, worldSpaceBounds.center.y + t.transform.position.y, worldSpaceBounds.center.z + t.transform.position.z);
            Vector2Double pos = new Vector2Double(Mathd.InverseLerp(0, m_bounds.size.x, Mathd.Abs(m_bounds.min.x - worldSpaceBounds.min.x)), Mathd.InverseLerp(0, m_bounds.size.z, Mathd.Abs(m_bounds.min.z - worldSpaceBounds.min.z)));
            Graphics.Blit(m_cachedExportTexture, chunkContent, new Vector2(1, 1), new Vector2(0, 0));
            Graphics.CopyTexture(chunkContent, 0, 0, 0, 0, chunkContent.width, chunkContent.width, m_exportRT, 0, 0, Mathd.FloorToInt(pos.x * m_exportRT.width), Mathd.FloorToInt(pos.y * m_exportRT.height));

        }

        /// <summary>
        /// Inizialises or "resets" a color texture in the cache
        /// </summary>
        /// <param name="index">The index at which to initialise.</param>
        /// <param name="rtToInitialiseFrom">A sample render texture with the correct resolution & format settings etc. to initialise from</param>
        private void InitialiseColorTextureCache(int index, RenderTexture rtToInitialiseFrom)
        {
            ClearCachedTexture(m_cachedPreviewColorRenderTextures[index]);
            m_cachedPreviewColorRenderTextures[index] = new RenderTexture(rtToInitialiseFrom);
        }

        private void ClearColorTextureCache()
        {
            for (int i = 0; i < m_cachedPreviewColorRenderTextures.Length; i++)
            {
                ClearCachedTexture(m_cachedPreviewColorRenderTextures[i]);
            }
        }

        private void ClearCachedTexture(RenderTexture cachedRT)
        {
            if (cachedRT != null)
            {
                cachedRT.Release();
                DestroyImmediate(cachedRT);
            }

            cachedRT = new RenderTexture(1, 1, 1);
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = cachedRT;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = currentRT;

        }

        private void ReleaseRenderTexture(RenderTexture texture)
        {
            if (texture != null)
            {
                RenderTexture.ReleaseTemporary(texture);
                texture = null;
            }
        }
    }
}
#endif
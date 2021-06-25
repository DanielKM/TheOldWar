using Gaia.Internal;
using PWCommon5;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

#if GAIA_PRO_PRESENT

namespace Gaia
{

    /// <summary>
    /// Editor for Biome Preset settings, only offers a text & a button to create the spawner in the scene
    /// If the user wants to edit or create new spawner settings, they can do so by saving a spawner settings file from a spawner directly.
    /// </summary>
    [CustomEditor(typeof(MaskMapExport))]
    public class MaskMapEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private MaskMapExport m_maskMapExport;

        private UnityEditorInternal.ReorderableList m_masks_red_Reorderable;
        private UnityEditorInternal.ReorderableList m_masks_green_Reorderable;
        private UnityEditorInternal.ReorderableList m_masks_blue_Reorderable;
        private UnityEditorInternal.ReorderableList m_masks_alpha_Reorderable;
        private CollisionMask[] m_collisionMaskListBeingDrawn;
        private UnityEditorInternal.ReorderableList m_reorderableListBeingDrawn;
        private int m_rgbaIndexBeingDrawn = 0;
        private int m_maskListIndexBeingDrawn = 0;

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
        private GUIStyle m_imageMaskHeaderStyle;
        private GUIStyle m_errorMaskHeaderStyle;
        private GUIStyle m_colorChannelHeader;
        private GUIStyle m_colorChannelBorder;
        private GUIStyle m_panelStyle;
        private GUIStyle m_black_panelLabelStyle;
        private List<Texture2D> m_tempTextureList = new List<Texture2D>();
        private bool[] m_masksExpanded = new bool[4];
        private float m_lastZPos;
        private float m_lastXPos;
        private bool m_exportPreviewVisible;
        private string m_SaveAndLoadMessage;
        private MessageType m_SaveAndLoadMessageType;


        private void OnDisable()
        {
#if GAIA_PRO_PRESENT
            m_maskMapExport.TerrainLoader.m_isSelected = false;
#endif
            m_maskMapExport.m_settings.ClearImageMaskTextures();
        }

        private void OnEnable()
        {
            m_maskMapExport = (MaskMapExport)target;
            //Init editor utils
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }

            CreateMaskLists();

            m_masksExpanded[0] = true;
            m_masksExpanded[1] = true;
            m_masksExpanded[2] = true;
            m_masksExpanded[3] = true;

            m_maskMapExport.UpdateMinMaxHeight();
#if GAIA_PRO_PRESENT
            m_maskMapExport.TerrainLoader.m_isSelected = true;
#endif
            m_maskMapExport.m_previewDirty = true;

        }

        private void CreateMaskLists()
        {
            CreateColorChannelMaskList(ref m_masks_red_Reorderable, m_maskMapExport.m_settings.m_red_imageMasks);
            CreateColorChannelMaskList(ref m_masks_green_Reorderable, m_maskMapExport.m_settings.m_green_imageMasks);
            CreateColorChannelMaskList(ref m_masks_blue_Reorderable, m_maskMapExport.m_settings.m_blue_imageMasks);
            CreateColorChannelMaskList(ref m_masks_alpha_Reorderable, m_maskMapExport.m_settings.m_alpha_imageMasks);
        }

        private void CreateColorChannelMaskList(ref ReorderableList reorderableList, ImageMask[] imageMasks)
        {
            reorderableList = new UnityEditorInternal.ReorderableList(imageMasks, typeof(ImageMask), true, true, true, true);
            reorderableList.elementHeightCallback = OnElementHeightMaskListEntry;
            reorderableList.drawElementCallback = DrawMaskListElement; ;
            reorderableList.drawHeaderCallback = DrawMaskListHeader;
            reorderableList.onAddCallback = OnAddMaskListEntry;
            reorderableList.onRemoveCallback = OnRemoveMaskListEntry;
            reorderableList.onReorderCallback = OnReorderMaskList;

            foreach (ImageMask mask in imageMasks)
            {
                mask.m_reorderableCollisionMaskList = CreateCollisionMaskList(mask.m_reorderableCollisionMaskList, mask.m_collisionMasks);
            }
        }

        private float OnElementHeightMaskListEntry(int index)
        {
            ImageMask[] currentMaskList = m_maskMapExport.GetImageMaskListByRGBAIndex(m_rgbaIndexBeingDrawn);
            if (index < currentMaskList.Length)
            {
                return ImageMaskListEditor.OnElementHeight(index, m_maskMapExport.GetImageMaskListByRGBAIndex(m_rgbaIndexBeingDrawn)[index]);
            }
            else
                return 0;
        }

        private void DrawMaskListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            ImageMask copiedImageMask = SessionManager.m_copiedImageMask;
            ImageMask[] currentMaskList = m_maskMapExport.GetImageMaskListByRGBAIndex(m_rgbaIndexBeingDrawn);
            MaskListButtonCommand mlbc = ImageMaskListEditor.DrawMaskListElement(rect, index, currentMaskList, ref m_collisionMaskListBeingDrawn, m_editorUtils, Terrain.activeTerrain, false, copiedImageMask, m_imageMaskHeaderStyle.normal.background, m_errorMaskHeaderStyle.normal.background, GaiaSettings, SessionManager);

            switch (mlbc)
            {
                case MaskListButtonCommand.Delete:
                    m_reorderableListBeingDrawn.index = index;
                    OnRemoveMaskListEntry(m_reorderableListBeingDrawn);
                    break;
                case MaskListButtonCommand.Duplicate:
                    ImageMask newImageMask = ImageMask.Clone(currentMaskList[index]);
                    currentMaskList = GaiaUtils.InsertElementInArray(currentMaskList, newImageMask, index + 1);
                    m_reorderableListBeingDrawn.list = currentMaskList;
                    currentMaskList[index + 1].m_reorderableCollisionMaskList = CreateCollisionMaskList(currentMaskList[index + 1].m_reorderableCollisionMaskList, currentMaskList[index + 1].m_collisionMasks);
                    SetImageMaskListByRGBAIndex(currentMaskList, m_rgbaIndexBeingDrawn);
                    serializedObject.ApplyModifiedProperties();
                    break;
                case MaskListButtonCommand.Copy:
                    SessionManager.m_copiedImageMask = currentMaskList[index];
                    break;
                case MaskListButtonCommand.Paste:
                    currentMaskList[index] = ImageMask.Clone(copiedImageMask);
                    //Rebuild collsion mask list with new content from the cloning
                    currentMaskList[index].m_reorderableCollisionMaskList = CreateCollisionMaskList(currentMaskList[index].m_reorderableCollisionMaskList, currentMaskList[index].m_collisionMasks);
                    SessionManager.m_copiedImageMask = null;
                    SetImageMaskListByRGBAIndex(currentMaskList, m_rgbaIndexBeingDrawn);
                    break;
            }
            if (currentMaskList.Length - 1 >= index)
            {
                currentMaskList[index].m_imageMaskLocation = ImageMaskLocation.MaskMapExport;
            }

        }

        private void DrawMaskListHeader(Rect rect)
        {
            m_masksExpanded[m_rgbaIndexBeingDrawn] = ImageMaskListEditor.DrawFilterListHeader(rect, m_masksExpanded[m_rgbaIndexBeingDrawn], m_maskMapExport.GetImageMaskListByRGBAIndex(m_rgbaIndexBeingDrawn), m_editorUtils);
        }

        private void OnAddMaskListEntry(ReorderableList list)
        {
            float maxWorldHeight = 0f;
            float minWorldHeight = 0f;
            float seaLevel;

            SessionManager.GetWorldMinMax(ref minWorldHeight, ref maxWorldHeight);
            seaLevel = SessionManager.GetSeaLevel();
            ImageMask[] currentMaskList = m_maskMapExport.GetImageMaskListByRGBAIndex(m_rgbaIndexBeingDrawn);

            currentMaskList = ImageMaskListEditor.OnAddMaskListEntry(currentMaskList, maxWorldHeight, minWorldHeight, seaLevel);
            ImageMask lastElement = currentMaskList[currentMaskList.Length - 1];
            lastElement.m_reorderableCollisionMaskList = CreateCollisionMaskList(lastElement.m_reorderableCollisionMaskList, lastElement.m_collisionMasks);
            list.list = currentMaskList;
            SetImageMaskListByRGBAIndex(currentMaskList, m_rgbaIndexBeingDrawn);

        }

        private void OnRemoveMaskListEntry(ReorderableList list)
        {
            ImageMask[] currentMaskList = m_maskMapExport.GetImageMaskListByRGBAIndex(m_rgbaIndexBeingDrawn);
            currentMaskList = ImageMaskListEditor.OnRemoveMaskListEntry(currentMaskList, list.index);
            list.list = currentMaskList;
            SetImageMaskListByRGBAIndex(currentMaskList, m_rgbaIndexBeingDrawn);
        }

        private void OnReorderMaskList(ReorderableList list)
        {

        }

        /// <summary>
        /// Creates the reorderable collision mask list for collision masks in the spawn rules.
        /// </summary>
        public ReorderableList CreateCollisionMaskList(ReorderableList list, CollisionMask[] collisionMasks)
        {
            list = new ReorderableList(collisionMasks, typeof(CollisionMask), true, true, true, true);
            list.elementHeightCallback = OnElementHeightCollisionMaskList;
            list.drawElementCallback = DrawCollisionMaskElement;
            list.drawHeaderCallback = DrawCollisionMaskListHeader;
            list.onAddCallback = OnAddCollisionMaskListEntry;
            list.onRemoveCallback = OnRemoveCollisionMaskMaskListEntry;
            return list;
        }

        private void OnRemoveCollisionMaskMaskListEntry(ReorderableList list)
        {
            //find spawn rule index & mask index which are being edited, so we know who this list of collision masks belongs to
            int maskIndex = -99;
            int rgbaIndex = FindRGBAIndexByReorderableCollisionMaskList(list, ref maskIndex);

            ImageMask[] imageMasks = m_maskMapExport.GetImageMaskListByRGBAIndex(rgbaIndex);

            imageMasks[maskIndex].m_collisionMasks = CollisionMaskListEditor.OnRemoveMaskListEntry(imageMasks[maskIndex].m_collisionMasks, list.index);
            list.list = imageMasks[maskIndex].m_collisionMasks;
        }

        private void OnAddCollisionMaskListEntry(ReorderableList list)
        {
            //find spawn rule index & mask index which are being edited, so we know who this list of collision masks belongs to
            int maskIndex = -99;
            int rgbaIndex = FindRGBAIndexByReorderableCollisionMaskList(list, ref maskIndex);
            ImageMask[] imageMasks = m_maskMapExport.GetImageMaskListByRGBAIndex(rgbaIndex);
            imageMasks[maskIndex].m_collisionMasks = CollisionMaskListEditor.OnAddMaskListEntry(imageMasks[maskIndex].m_collisionMasks);
            list.list = imageMasks[maskIndex].m_collisionMasks;
        }

        private void DrawCollisionMaskListHeader(Rect rect)
        {
            ImageMask[] currentMaskList = m_maskMapExport.GetImageMaskListByRGBAIndex(m_rgbaIndexBeingDrawn);
            currentMaskList[m_maskListIndexBeingDrawn].m_collisionMaskExpanded = CollisionMaskListEditor.DrawFilterListHeader(rect, currentMaskList[m_maskListIndexBeingDrawn].m_collisionMaskExpanded, currentMaskList[m_maskListIndexBeingDrawn].m_collisionMasks, m_editorUtils);
            SetImageMaskListByRGBAIndex(currentMaskList, m_rgbaIndexBeingDrawn);
        }

        private void DrawCollisionMaskElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_collisionMaskListBeingDrawn == null)
            {
                m_collisionMaskListBeingDrawn = m_maskMapExport.GetImageMaskListByRGBAIndex(m_rgbaIndexBeingDrawn)[m_maskListIndexBeingDrawn].m_collisionMasks;
            }

            if (m_collisionMaskListBeingDrawn != null && m_collisionMaskListBeingDrawn.Length > index && m_collisionMaskListBeingDrawn[index] != null)
            {
                CollisionMaskListEditor.DrawMaskListElement(rect, index, m_collisionMaskListBeingDrawn[index], m_editorUtils, Terrain.activeTerrain, GaiaConstants.FeatureOperation.Contrast);
            }
        }

        private float OnElementHeightCollisionMaskList(int index)
        {
            return CollisionMaskListEditor.OnElementHeight(index, m_collisionMaskListBeingDrawn);
        }

       
        private void SetImageMaskListByRGBAIndex(ImageMask[] currentMaskList, int rgbaIndex = 0)
        {
            switch (rgbaIndex)
            {
                case 0:
                     m_maskMapExport.m_settings.m_red_imageMasks = currentMaskList;
                    break;
                case 1:
                    m_maskMapExport.m_settings.m_green_imageMasks = currentMaskList;
                    break;
                case 2:
                    m_maskMapExport.m_settings.m_blue_imageMasks = currentMaskList;
                    break;
                case 3:
                    m_maskMapExport.m_settings.m_alpha_imageMasks = currentMaskList;
                    break;
            }
        }

        int FindRGBAIndexByReorderableCollisionMaskList(ReorderableList collisionMaskList, ref int maskIndex)
        {
            //find texture index that is being edited
            int rgbaIndex = -99;

            for (int i = 0; i < m_maskMapExport.m_settings.m_red_imageMasks.Length; i++)
            {
                if(m_maskMapExport.m_settings.m_red_imageMasks[i].m_reorderableCollisionMaskList == collisionMaskList)
                {
                    rgbaIndex = 0;
                    maskIndex = i;
                    return rgbaIndex;
                }
            }

            for (int i = 0; i < m_maskMapExport.m_settings.m_green_imageMasks.Length; i++)
            {
                if (m_maskMapExport.m_settings.m_green_imageMasks[i].m_reorderableCollisionMaskList == collisionMaskList)
                {
                    rgbaIndex = 1;
                    maskIndex = i;
                    return rgbaIndex;
                }
            }

            for (int i = 0; i < m_maskMapExport.m_settings.m_blue_imageMasks.Length; i++)
            {
                if (m_maskMapExport.m_settings.m_blue_imageMasks[i].m_reorderableCollisionMaskList == collisionMaskList)
                {
                    rgbaIndex = 2;
                    maskIndex = i;
                    return rgbaIndex;
                }
            }

            for (int i = 0; i < m_maskMapExport.m_settings.m_alpha_imageMasks.Length; i++)
            {
                if (m_maskMapExport.m_settings.m_alpha_imageMasks[i].m_reorderableCollisionMaskList == collisionMaskList)
                {
                    rgbaIndex = 3;
                    maskIndex = i;
                    return rgbaIndex;
                }
            }

            return rgbaIndex;
        }

        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize(); // Do not remove this!
            m_maskMapExport = (MaskMapExport)target;
            serializedObject.Update();

            m_maskMapExport.UpdateAutoLoadRange();

            //Disable Loading settings highlighting again after 2 seconds
            if (m_maskMapExport.m_highlightLoadingSettings && m_maskMapExport.m_highlightLoadingSettingsStartedTimeStamp + 2000 < GaiaUtils.GetUnixTimestamp())
            {
                m_maskMapExport.m_highlightLoadingSettings = false;
            }

            m_maskMapExport.m_settings.m_x = m_maskMapExport.transform.position.x;
            m_maskMapExport.m_settings.m_y = m_maskMapExport.transform.position.y;
            m_maskMapExport.m_settings.m_z = m_maskMapExport.transform.position.z;

            SetupStyles();
            EditorGUI.BeginChangeCheck();
            m_editorUtils.Panel("ChannelSetup", DrawChannelSetup, true);
            m_exportPreviewVisible = false;
            m_editorUtils.Panel("ExportPreview", DrawExportPreview, false);
            if (m_maskMapExport.m_highlightLoadingSettings)
            {
                m_editorUtils.SetPanelStatus(DrawAppearance, true, false);
            }
            m_editorUtils.Panel("Appearance", DrawAppearance, false);
            m_editorUtils.Panel("SaveAndLoad", DrawSaveAndLoad, false);
            if (EditorGUI.EndChangeCheck())
            {
                m_maskMapExport.UpdateMinMaxHeight();
                m_maskMapExport.m_previewDirty = true;
                serializedObject.ApplyModifiedProperties();
                m_maskMapExport.DrawMaskMapPreview(false);
                //EditorWindow view = EditorWindow.GetWindow<SceneView>();
                //view.Repaint();
            }
            DrawMaskExportControls();
            if (m_maskMapExport.m_previewRGBAIds.Count > 0 || m_exportPreviewVisible)
            {
                m_maskMapExport.m_drawPreview = true;
            }
            else
            {
                m_maskMapExport.m_drawPreview = false;
            }
        }

        private void DrawExportPreview(bool helpEnabled)
        {
            m_maskMapExport.m_settings.m_exportDirectory = m_editorUtils.TextField("ExportDirectory", m_maskMapExport.m_settings.m_exportDirectory, helpEnabled);
            m_maskMapExport.m_settings.m_exportFileName = m_editorUtils.TextField("ExportFileName", m_maskMapExport.m_settings.m_exportFileName, helpEnabled);
            m_maskMapExport.m_settings.m_readWriteEnabled = m_editorUtils.Toggle("ExportReadWriteEnabled", m_maskMapExport.m_settings.m_readWriteEnabled, helpEnabled);
            m_maskMapExport.m_settings.m_addTerrainNameToFileName = m_editorUtils.Toggle("ExportAddTerrainName", m_maskMapExport.m_settings.m_addTerrainNameToFileName, helpEnabled);
            m_maskMapExport.m_settings.m_multiTerrainOption = (MaskMapExportMultiTerrainOption)m_editorUtils.EnumPopup("ExportMultiTerrainHandling", m_maskMapExport.m_settings.m_multiTerrainOption, helpEnabled);
            if (m_maskMapExport.m_settings.m_multiTerrainOption == MaskMapExportMultiTerrainOption.OneCombinedTexture)
            {
                m_maskMapExport.m_settings.m_combinedTextureResolution = m_editorUtils.IntField("ExportCombinedTextureResolution", m_maskMapExport.m_settings.m_combinedTextureResolution, helpEnabled);
            }
            m_maskMapExport.m_settings.m_exportFileType = (GaiaConstants.ImageFileType)m_editorUtils.EnumPopup("ExportFileType", m_maskMapExport.m_settings.m_exportFileType, helpEnabled);
            if (m_maskMapExport.m_settings.m_exportFileType == GaiaConstants.ImageFileType.Jpg)
            {
                m_maskMapExport.m_settings.m_exportJpgQuality = m_editorUtils.IntSlider("ExportJPGQuality", m_maskMapExport.m_settings.m_exportJpgQuality, 0, 100, helpEnabled);
            }
            GUILayout.Space(10);

            if (m_maskMapExport.m_cachedExportTexture != null)
            {
                m_exportPreviewVisible = true;
                Rect previewRect = EditorGUILayout.GetControlRect();
                float size = previewRect.width - EditorGUIUtility.labelWidth;
                previewRect.x = previewRect.width / 2f - size / 2f;
                previewRect.width = size;
                previewRect.height = size;
                EditorGUILayout.BeginVertical();
                EditorGUI.DrawPreviewTexture(previewRect, m_maskMapExport.m_cachedExportTexture);
                EditorGUILayout.EndVertical();
                GUILayout.Space(Mathf.Abs(previewRect.height) * 1.3f);
                GUILayout.Space(10);
            }
            else
            {
                //No preview texture yet? Force one!
                m_maskMapExport.m_previewDirty = true;
                m_maskMapExport.DrawMaskMapPreview(true);
            }
        }

        private void DrawSaveAndLoad(bool helpEnabled)
        {
            if (!String.IsNullOrEmpty(m_SaveAndLoadMessage))
                EditorGUILayout.HelpBox(m_SaveAndLoadMessage, m_SaveAndLoadMessageType, true);

            EditorGUILayout.BeginHorizontal();
            if (m_editorUtils.Button("LoadButton"))
            {
                string openFilePath = EditorUtility.OpenFilePanel("Load Mask Export settings..", GaiaDirectories.GetSettingsDirectory(), "asset");
                bool loadConditionsMet = true;

                //Do we have a path to begin with?
                if (openFilePath == null || openFilePath == "")
                {
                    //Silently abort in this case, the user has pressed "Abort" in the File Open Dialog
                    loadConditionsMet = false;
                }


                //Look for the Assets Directory
                if (!openFilePath.Contains("Assets") && loadConditionsMet)
                {
                    m_SaveAndLoadMessage = m_editorUtils.GetContent("LoadNoAssetDirectory").text;
                    m_SaveAndLoadMessageType = MessageType.Error;
                    loadConditionsMet = false;
                }
                if (loadConditionsMet)
                {

                    openFilePath = GaiaDirectories.GetPathStartingAtAssetsFolder(openFilePath);
                    MaskMapExportSettings settingsToLoad = (MaskMapExportSettings)AssetDatabase.LoadAssetAtPath(openFilePath, typeof(MaskMapExportSettings));

                    if (settingsToLoad != null)
                    {
                        m_maskMapExport.LoadSettings(settingsToLoad);
                        CreateMaskLists();
                        m_maskMapExport.m_previewDirty = true;
                        m_SaveAndLoadMessage = m_editorUtils.GetContent("LoadSuccessful").text;
                        m_SaveAndLoadMessageType = MessageType.Info;
                    }
                    else
                    {
                        m_SaveAndLoadMessage = m_editorUtils.GetContent("LoadFailed").text;
                        m_SaveAndLoadMessageType = MessageType.Error;
                    }
                }

            }
            if (m_editorUtils.Button("SaveButton"))
            {
                m_maskMapExport.m_settings.m_x = m_maskMapExport.transform.position.x;
                m_maskMapExport.m_settings.m_y = m_maskMapExport.transform.position.y;
                m_maskMapExport.m_settings.m_z = m_maskMapExport.transform.position.z;
                string dialogPath = m_maskMapExport.m_settings.lastSavePath;
                string filename = "MaskExportSettings";
                if (string.IsNullOrEmpty(dialogPath))
                {
                    dialogPath = GaiaDirectories.GetSettingsDirectory();

                }
                else
                {
                    filename = dialogPath.Substring(dialogPath.LastIndexOf('/') + 1).Replace(".asset", "");
                }


                string saveFilePath = EditorUtility.SaveFilePanel("Save Mask Map export settings as..", dialogPath, filename, "asset");

                bool saveConditionsMet = true;

                //Do we have a path to begin with?
                if (saveFilePath == null || saveFilePath == "")
                {
                    //Silently abort in this case, the user has pressed "Abort" in the File Open Dialog
                    saveConditionsMet = false;
                }

                //Look for the Assets Directory
                if (!saveFilePath.Contains("Assets") && saveConditionsMet)
                {
                    m_SaveAndLoadMessage = m_editorUtils.GetContent("SaveNoAssetDirectory").text;
                    m_SaveAndLoadMessageType = MessageType.Error;
                    saveConditionsMet = false;
                }

                if (saveConditionsMet)
                {
                    saveFilePath = GaiaDirectories.GetPathStartingAtAssetsFolder(saveFilePath);

                    MaskMapExportSettings settingsToLoad = (MaskMapExportSettings)AssetDatabase.LoadAssetAtPath(saveFilePath, typeof(MaskMapExportSettings));

                    if (settingsToLoad != null)
                    {
                        AssetDatabase.DeleteAsset(saveFilePath);
                    }
                    m_maskMapExport.m_settings.lastSavePath = saveFilePath;
                    AssetDatabase.CreateAsset(m_maskMapExport.m_settings, saveFilePath);
                    EditorUtility.SetDirty(m_maskMapExport.m_settings);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(saveFilePath);

                    //Check if save was successful
                    settingsToLoad = (MaskMapExportSettings)AssetDatabase.LoadAssetAtPath(saveFilePath, typeof(MaskMapExportSettings));
                    if (settingsToLoad != null)
                    {
                        m_SaveAndLoadMessage = m_editorUtils.GetContent("SaveSuccessful").text;
                        m_SaveAndLoadMessageType = MessageType.Info;
                        //dissociate the current stamper settings from the file we just saved, otherwise the user will continue editing the file afterwards
                        //We do this by just loading the file in again we just created
                        m_maskMapExport.LoadSettings(settingsToLoad);
                        CreateMaskLists();
                        m_maskMapExport.m_previewDirty = true;
                        //update the gaia manager window (if exists)

                    }
                    else
                    {
                        m_SaveAndLoadMessage = m_editorUtils.GetContent("SaveFailed").text;
                        m_SaveAndLoadMessageType = MessageType.Error;
                    }
                }

            }
            EditorGUILayout.EndHorizontal();


        }

        private void DrawMaskExportControls()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
         
            if (GUILayout.Button(m_editorUtils.GetTextValue("ButtonFitToTerrain")))
            {
                m_maskMapExport.FitToTerrain();
            }
            if (Terrain.activeTerrains.Length > 1)
            {
                if (GUILayout.Button(m_editorUtils.GetTextValue("ButtonFitToWorld")))
                {
                    m_maskMapExport.FitToAllTerrains();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Color normalBGColor = GUI.backgroundColor;
            GUI.backgroundColor = m_gaiaSettings.GetActionButtonColor();
            GUI.enabled = true;
            if (GUILayout.Button(m_editorUtils.GetTextValue("ButtonExportLocal")))
            {
                m_maskMapExport.StartExport(false);
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button(m_editorUtils.GetTextValue("ButtonExportWorld")))
            {
                m_maskMapExport.StartExport(true);
                GUIUtility.ExitGUI();
            }
            GUI.backgroundColor = normalBGColor;
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.EndVertical();

        }

        private void DrawChannelSetup(bool helpEnabled)
        {
            
            m_maskMapExport.m_settings.m_range = m_editorUtils.Slider("Range", m_maskMapExport.m_settings.m_range, 0, m_maskMapExport.GetMaxSpawnerRange(), helpEnabled);

            GUILayout.Space(5);
            DrawChannelMaskList(m_masks_red_Reorderable, m_maskMapExport.m_settings.m_redChannelSettings, 0);
            GUILayout.Space(5);
            DrawChannelMaskList(m_masks_green_Reorderable, m_maskMapExport.m_settings.m_greenChannelSettings, 1);
            GUILayout.Space(5);
            DrawChannelMaskList(m_masks_blue_Reorderable, m_maskMapExport.m_settings.m_blueChannelSettings, 2);
            GUILayout.Space(5);
            DrawChannelMaskList(m_masks_alpha_Reorderable, m_maskMapExport.m_settings.m_alphaChannelSettings, 3);
           
        }

        private void DrawAppearance(bool showHelp)
        {
#if GAIA_PRO_PRESENT
            bool currentGUIState = GUI.enabled;
            if (!TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainLoadingEnabled)
            {
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("AutoLoadTerrainsDisabled"), MessageType.Warning);
                GUI.enabled = false;
            }
            Color originalColor = GUI.backgroundColor;

            if (m_maskMapExport.m_highlightLoadingSettings)
            {
                GUI.backgroundColor = GaiaUtils.GetColorFromHTML(GaiaConstants.TerrainLoadingSettingsHighlightColor); ;
            }
            m_maskMapExport.m_loadTerrainMode = (LoadMode)m_editorUtils.EnumPopup("AutoLoadTerrains", m_maskMapExport.m_loadTerrainMode, showHelp);
            m_maskMapExport.m_impostorLoadingRange = m_editorUtils.IntField("ImpostorLoadingRange", m_maskMapExport.m_impostorLoadingRange, showHelp);
            GUI.enabled = currentGUIState;
            GUI.backgroundColor = originalColor;
#endif
            m_editorUtils.LabelField("SeaLevel", new GUIContent(SessionManager.GetSeaLevel().ToString() + " m"), showHelp);
            float maxSeaLevel = 2000f;
            if (m_maskMapExport.GetCurrentTerrain() != null)
            {
                maxSeaLevel = m_maskMapExport.GetCurrentTerrain().terrainData.size.y;
            }
            else
            {
                maxSeaLevel = SessionManager.GetSeaLevel() + 500f;
            }

            float newSeaLEvel = m_editorUtils.Slider("SeaLevel", SessionManager.GetSeaLevel(), 0, maxSeaLevel, showHelp);
            SessionManager.SetSeaLevel(newSeaLEvel);
            m_maskMapExport.m_showSeaLevelPlane = m_editorUtils.Toggle("ShowSeaLevelPlane", m_maskMapExport.m_showSeaLevelPlane, showHelp);
            m_maskMapExport.m_showSeaLevelinStampPreview = m_editorUtils.Toggle("ShowSeaLevelSpawnerPreview", m_maskMapExport.m_showSeaLevelinStampPreview, showHelp);
            //Color gizmoColour = EditorGUILayout.ColorField(GetLabel("Gizmo Colour"), m_stamper.m_gizmoColour);
            //alwaysShow = m_editorUtils.Toggle("AlwaysShowStamper", m_stamper.m_alwaysShow, showHelp);
            m_maskMapExport.m_showBoundingBox = m_editorUtils.Toggle("ShowBoundingBox", m_maskMapExport.m_showBoundingBox, showHelp);
            //showRulers = m_stamper.m_showRulers = m_editorUtils.Toggle("ShowRulers", m_stamper.m_showRulers, showHelp);
            //bool showTerrainHelper = m_stamper.m_showTerrainHelper = EditorGUILayout.Toggle(GetLabel("Show Terrain Helper"), m_stamper.m_showTerrainHelper);
        }

        private void SetupStyles()
        {
            if (m_imageMaskHeaderStyle == null || m_imageMaskHeaderStyle.normal.background == null)
            {
                m_imageMaskHeaderStyle = new GUIStyle();
                m_imageMaskHeaderStyle.overflow = new RectOffset(2, 2, 2, 2);
                m_errorMaskHeaderStyle = new GUIStyle(m_imageMaskHeaderStyle);

                // Setup colors for Unity Pro
                if (EditorGUIUtility.isProSkin)
                {
                    m_imageMaskHeaderStyle.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("2d2d2dff"), m_tempTextureList);
                    m_errorMaskHeaderStyle.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("804241ff"), m_tempTextureList);
                }
                // or Unity Personal
                else
                {
                    m_imageMaskHeaderStyle.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("a2a2a2ff"), m_tempTextureList);
                    m_errorMaskHeaderStyle.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("C46564ff"), m_tempTextureList);

                }
            }

            if (m_colorChannelBorder == null || m_colorChannelBorder.normal.background == null)
            {
                m_colorChannelBorder = new GUIStyle(EditorStyles.helpBox);
                m_colorChannelBorder.margin = new RectOffset(0, 0, 0, 0);
                m_colorChannelBorder.padding = new RectOffset(3, 3, 3, 3);
            }


            if (m_colorChannelHeader == null || m_colorChannelHeader.normal.background == null)
            {
                m_colorChannelHeader = new GUIStyle();
                m_colorChannelHeader.overflow = new RectOffset(2, 2, 2, 2);
                // Setup colors for Unity Pro
                if (EditorGUIUtility.isProSkin)
                {
                    m_colorChannelHeader.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("2d2d4cff"), m_tempTextureList);
                }
                // or Unity Personal
                else
                {
                    m_colorChannelHeader.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("a2a2c1ff"), m_tempTextureList);
                }
            }
            if (m_black_panelLabelStyle == null)
            {
                m_black_panelLabelStyle = new GUIStyle(GUI.skin.label);
                m_black_panelLabelStyle.normal.textColor = Color.black;
                m_black_panelLabelStyle.fontStyle = FontStyle.Bold;
                m_black_panelLabelStyle.normal.background = GUI.skin.label.normal.background;
            }
           if (m_panelStyle == null)
            {
                m_panelStyle = new GUIStyle(GUI.skin.label);
                m_panelStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_panelStyle.alignment = TextAnchor.UpperLeft;
            }
        }

        private void DrawChannelMaskList(ReorderableList reorderableList, MaskMapChannelSettings channelSettings, int rgbaIndex)
        {
            bool originalGUIState = GUI.enabled;
            GUILayout.BeginVertical(m_colorChannelBorder);
            {
                GUILayout.BeginHorizontal(m_colorChannelHeader);
                {
                    //Rect rect = EditorGUILayout.GetControlRect();
                    channelSettings.m_channelIsFoldedOut = GUILayout.Toggle(channelSettings.m_channelIsFoldedOut, channelSettings.m_channelIsFoldedOut ? "-" : "+", m_black_panelLabelStyle, GUILayout.MinWidth(10));



                    channelSettings.m_channelIsActive = GUILayout.Toggle(channelSettings.m_channelIsActive, "");


                    GUI.enabled = channelSettings.m_channelIsActive && GUI.enabled;
                    

                    channelSettings.m_channelIsFoldedOut = GUILayout.Toggle(channelSettings.m_channelIsFoldedOut, channelSettings.m_channelName, m_black_panelLabelStyle, GUILayout.MinWidth(0));
                    GUILayout.FlexibleSpace();

                    ////Deactivate upwards button for first position in the rule list
                    //if (ruleIdx == 0)
                    //{
                    //    GUI.enabled = false;
                    //}

                    float smallButtonSize = 20;
                    GUIStyle smallButtonStyle = new GUIStyle(GUI.skin.button);
                    smallButtonStyle.padding = new RectOffset(2, 2, 2, 2);
                    smallButtonStyle.margin = new RectOffset(5, 5, 0, 2);

                    //GUIContent GCupIcon = GaiaEditorUtils.GetIconGUIContent("IconUp", m_gaiaSettings.m_IconUp, m_gaiaSettings.m_IconProUp, m_editorUtils);
                    //if (m_editorUtils.Button(GCupIcon, smallButtonStyle, GUILayout.Height(smallButtonSize), GUILayout.Width(smallButtonSize)))
                    //{
                    //    SwapRules(ruleIdx - 1, ruleIdx);
                    //}

                    //GUI.enabled = originalGUIState;

                    ////Deactivate downwards button for last position in the rule list
                    //if (ruleIdx == m_spawner.m_settings.m_spawnerRules.Count() - 1)
                    //{
                    //    GUI.enabled = false;
                    //}


                    //GUIContent GCdownIcon = GaiaEditorUtils.GetIconGUIContent("IconDown", m_gaiaSettings.m_IconDown, m_gaiaSettings.m_IconProDown, m_editorUtils);
                    //if (m_editorUtils.Button(GCdownIcon, smallButtonStyle, GUILayout.Height(smallButtonSize), GUILayout.Width(smallButtonSize)))
                    //{
                    //    SwapRules(ruleIdx, ruleIdx + 1);
                    //}
                    //GUI.enabled = originalGUIState;

                    //GUIContent GCduplicateIcon = GaiaEditorUtils.GetIconGUIContent("IconDuplicate", m_gaiaSettings.m_IconDuplicate, m_gaiaSettings.m_IconProDuplicate, m_editorUtils);
                    //if (m_editorUtils.Button(GCduplicateIcon, smallButtonStyle, GUILayout.Height(smallButtonSize), GUILayout.Width(smallButtonSize)))
                    //{
                    //    DuplicateRule(ruleIdx);
                    //}

                    DrawVisualiseButton(rgbaIndex, channelSettings, smallButtonStyle, smallButtonSize);

                    ////Unless the spawner is spawning, still offer to delete even if the rule is inactive
                    //if (!m_spawner.IsSpawning())
                    //{
                    //    GUI.enabled = true;
                    //}
                    //GUIContent GCremoveIcon = GaiaEditorUtils.GetIconGUIContent("IconRemove", m_gaiaSettings.m_IconRemove, m_gaiaSettings.m_IconProRemove, m_editorUtils);
                    //if (m_editorUtils.Button(GCremoveIcon, smallButtonStyle, GUILayout.Height(smallButtonSize), GUILayout.Width(smallButtonSize)))
                    //{
                    //    m_spawner.m_settings.m_spawnerRules.Remove(rule);
                    //    RemoveMaskList(ruleIdx);
                    //    m_previewImageDisplayedDuringLayout = GaiaUtils.RemoveArrayIndexAt(m_previewImageDisplayedDuringLayout, ruleIdx);
                    //    PruneResources();
                    //    CleanPreviewRuleIDs();
                    //}

                    //GUI.enabled = !m_spawner.IsSpawning() && rule.m_isActive;

                    ////EditorGUILayout.EndVertical();
                    //m_editorUtils.HelpToggle(ref helpEnabled);

                }
                GUILayout.EndHorizontal();

                //if (helpActive)
                //{
                //    GUILayout.Space(2f);
                //    m_editorUtils.InlineHelp(nameKey, helpActive);
                //}

                if (channelSettings.m_channelIsFoldedOut)
                {
                    GUILayout.BeginVertical(m_panelStyle);
                    {


                        m_reorderableListBeingDrawn = reorderableList;
                        m_rgbaIndexBeingDrawn = rgbaIndex;

                        Rect listRect;
                        if (m_masksExpanded[rgbaIndex])
                        {
                            listRect = EditorGUILayout.GetControlRect(true, reorderableList.GetHeight());
                            reorderableList.DoList(listRect);
                            listRect.y += reorderableList.GetHeight();
                            listRect.y += EditorGUIUtility.singleLineHeight;
                            listRect.height = reorderableList.GetHeight();
                        }
                        else
                        {
                            int oldIndent = EditorGUI.indentLevel;
                            EditorGUI.indentLevel = 1;
                            m_masksExpanded[rgbaIndex] = EditorGUILayout.Foldout(m_masksExpanded[rgbaIndex], ImageMaskListEditor.PropertyCount("MaskSettings", m_maskMapExport.GetImageMaskListByRGBAIndex(m_rgbaIndexBeingDrawn), m_editorUtils), true);
                            listRect = GUILayoutUtility.GetLastRect();
                            listRect.y += EditorGUIUtility.singleLineHeight * 2f;
                            EditorGUI.indentLevel = oldIndent;
                        }
                        //if (rule != null && m_spawner.m_settings.m_spawnerRules.Count > ruleIdx)
                        //{
                        //    DrawSingleRule(rule, ruleIdx, helpEnabled);
                        //}
                    }
                    GUILayout.EndVertical();
                }               
            }
            GUILayout.EndVertical();
            GUI.enabled = originalGUIState;
            ////Leave a little space between the rules, but no extra space behind the last rule
            //if (ruleIdx < m_spawner.m_settings.m_spawnerRules.Count - 1)
            //{
            //    GUILayout.Space(8);
            //}

            //rule.m_isFoldedOut = unfolded;
            //rule.m_isHelpActive = helpEnabled;
            //GUI.enabled = !m_spawner.IsSpawning();
            //return unfolded;


        }

        private void OnSceneGUI()
        {
            // dont render preview if this isnt a repaint. losing performance if we do
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            //reset rotation, rotation for the spawner is currently not supported because it causes too many issues
            m_maskMapExport.transform.rotation = new Quaternion();

            //set the preview dirty if the transform changed so it will be redrawn correctly in the new location
            //the lastXPos & lastZPos variables are a workaround, because transform.hasChanged was triggering too often
            if (m_lastXPos != m_maskMapExport.transform.position.x || m_lastZPos != m_maskMapExport.transform.position.z)
            {
                m_lastXPos = m_maskMapExport.transform.position.x;
                m_lastZPos = m_maskMapExport.transform.position.z;
                m_maskMapExport.m_previewDirty = true;
            }

            m_maskMapExport.DrawMaskMapPreview(false);

        }


        private void DrawVisualiseButton(int rgbaIndex, MaskMapChannelSettings channelSettings, GUIStyle smallButtonStyle, float smallButtonSize)
        {
            Color currentBGColor = GUI.backgroundColor;
            if (m_maskMapExport.m_previewRGBAIds.Contains(rgbaIndex) && m_maskMapExport.m_drawPreview)
            {
                GUI.backgroundColor = channelSettings.m_visualisationColor;
            }

            GUIContent GCvisualizeIcon = GaiaEditorUtils.GetIconGUIContent("IconVisible", GaiaSettings.m_IconVisible, GaiaSettings.m_IconProVisible, m_editorUtils);
            GUILayout.Space(5);
            if (m_editorUtils.Button(GCvisualizeIcon, smallButtonStyle, GUILayout.Height(smallButtonSize), GUILayout.Width(smallButtonSize)))

            {
                //is this rule being shown already? then only remove this rule
                if (m_maskMapExport.m_previewRGBAIds.Contains(rgbaIndex) && m_maskMapExport.m_drawPreview)
                {
                    m_maskMapExport.m_previewRGBAIds.Remove(rgbaIndex);
                }
                else
                {
                    if (m_maskMapExport.m_drawPreview)
                    {
                        //this rule needs to be added for visualisation, would we exceed the maximum allowed number?
                        if (m_maskMapExport.m_previewRGBAIds.Count() >= GaiaConstants.maxPreviewedTextures)
                        {
                            //Yes, kick lowest rule in stack out first
                            m_maskMapExport.m_previewRGBAIds.RemoveAt(0);
                        }

                        //mark this rule for visualisation
                        m_maskMapExport.m_previewRGBAIds.Add(rgbaIndex);
                        //Sort the rules ascending, important because the lower rules should overwrite the earlier ones.
                        m_maskMapExport.m_previewRGBAIds.Sort();
                    }
                    else
                    {
                        //the spawner was currently not displaying the preview. Throw out any old rules first, and start fresh
                        m_maskMapExport.m_previewRGBAIds.Clear();
                        m_maskMapExport.m_previewRGBAIds.Add(rgbaIndex);
                    }
                }
                m_maskMapExport.m_previewDirty = true;
                m_maskMapExport.DrawMaskMapPreview(false);
                //force repaint
                EditorWindow view = EditorWindow.GetWindow<SceneView>();
                view.Repaint();
            }
            GUI.backgroundColor = currentBGColor;
        }
    }
}
#endif

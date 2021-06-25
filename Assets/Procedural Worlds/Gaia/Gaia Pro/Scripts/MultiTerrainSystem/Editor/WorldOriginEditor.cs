using PWCommon5;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
#if GAIA_PRO_PRESENT

namespace Gaia
{
    [InitializeOnLoad]
    class WorldOriginEditor
    {
        static WorldOriginEditor()
        {
            SceneView.duringSceneGui -= RenderSceneGUI;
            SceneView.duringSceneGui += RenderSceneGUI;
            m_lastActiveScene = SceneManager.GetActiveScene();
            CheckForSessionManager();
        }

        private static void CheckForSessionManager()
        {
            if (SessionManager != null)
            {
                m_sessionManagerExits = true;
            }
            else
            {
                m_sessionManagerExits = false;
            }
        }

        public static bool m_sessionManagerExits = false;
        private static Scene m_lastActiveScene;

        private static GUIStyle GUIStylePanel;
        private static GUIStyle GUIStyleHeader;
        private static GUIStyle m_operationCreateWorldStyle;
        private static GUIStyle m_operationFlattenTerrainStyle;
        private static GUIStyle m_operationClearSpawnsStyle;
        private static GUIStyle m_operationStampStyle;
        private static GUIStyle m_operationStampUndoRedoStyle;
        private static GUIStyle m_operationSpawnStyle;
        private static GUIStyle m_operationRemoveNonBiomeResourcesStyle;
        private static GUIStyle m_operationMaskMapExportStyle;
        private static GUIStyle m_operationCheckboxStyle;
        private static GUIStyle m_operationFoldoutStyle;
        private static GUIStyle m_operationPingButtonStyle;
        private static List<Texture2D> m_tempTextureList = new List<Texture2D>();

        private static Texture2D m_helpButtonImage;


        private static GaiaSessionManager m_sessionManager;
        private static Vector2 m_terrainOpScrollPos;
        //private static EditorUtils m_editorUtils;
        private static GaiaSessionManager SessionManager {
            get {
                if (m_sessionManager == null)
                {
                    m_sessionManager = GaiaSessionManager.GetSessionManager(false,false);
                    if (m_sessionManager == null)
                    {
                        m_sessionManagerExits = false;
                    }
                }
                return m_sessionManager;
            }
        }

        private static GaiaSettings m_gaiaSettings;
        private static GaiaSettings GaiaSettings
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

        private static Terrain m_worldMapTerrain;
        private static bool m_flaggedTerrainsFoldedOut;
        private static long m_pingStartTimeStamp;
        private static Vector3 m_pingPosition;
        private static float m_pingRange;
        private static int m_oldX;
        private static int m_oldZ;
        private static long m_tileSliderDragStartedTimeStamp;
        private static GUIStyle m_smallButtonStyle;

        private static Terrain WorldMapTerrain
        {
            get
            {
                if (m_worldMapTerrain == null)
                {
                    m_worldMapTerrain = TerrainHelper.GetWorldMapTerrain();
                }
                return m_worldMapTerrain;
            }
        }

        

        //private void OnDestroy()
        //{
        //    WorldOrigin handleExample = (WorldOrigin)target;
        //    if (handleExample.sceneGUIsubscribed)
        //    {
        //        SceneView.onSceneGUIDelegate -= RenderSceneGUI;
        //    }
        //}

        public static void RenderSceneGUI(SceneView sceneview)
        {
            if (m_lastActiveScene != SceneManager.GetActiveScene())
            {
                CheckForSessionManager();
                //if (m_sessionManagerExits)
                //{
                //    double range = TerrainLoaderManager.Instance.GetLoadingRange();
                //    Vector3Double currentPos = TerrainLoaderManager.Instance.GetOrigin();
                //    TerrainLoaderManager.Instance.SetLoadingRange(range);
                //    TerrainLoaderManager.Instance.SetOrigin(currentPos);
                //}
                m_lastActiveScene = SceneManager.GetActiveScene();
            }

            if (!m_sessionManagerExits)
            {
                return;
            }
            if (SessionManager == null)
            {
                return;
            }
            //if (m_editorUtils == null)
            //{
            //    // Get editor utils for this
            //    m_editorUtils = PWApp.GetEditorUtils(new WorldOriginEditor());
            //}
            if (m_helpButtonImage == null)
            {
                m_helpButtonImage = Resources.Load("helpBtnOffp" + PWConst.VERSION_IN_FILENAMES) as Texture2D;
            }

            if (m_smallButtonStyle == null)
            {
                m_smallButtonStyle = new GUIStyle(GUI.skin.button);
                m_smallButtonStyle.padding = new RectOffset(0, 0, 0, 0);
                //m_smallButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            }

            if (GUIStyleHeader == null)
            {
                GUIStyleHeader = new GUIStyle(GUI.skin.label);
                GUIStyleHeader.alignment = TextAnchor.MiddleCenter;
                GUIStyleHeader.fontStyle = FontStyle.Bold;
            }

            if (GUIStylePanel == null)
            {
                GUIStylePanel = new GUIStyle();
                GUIStylePanel.padding = new RectOffset(0, 0, 0, 0);
                GUIStylePanel.margin = new RectOffset(0, 0, 0, 0);
                GUIStylePanel.alignment = TextAnchor.MiddleCenter;
                GUIStylePanel.border = new RectOffset(2, 2, 2, 2);
                GUIStylePanel.imagePosition = ImagePosition.ImageOnly;
                if (EditorGUIUtility.isProSkin)
                {
                    GUIStylePanel.normal.background = GaiaSettings.m_originUIProBackgroundPro;
                    GUIStylePanel.normal.textColor = Color.white;
                }
                else
                {
                    GUIStylePanel.normal.background = GaiaSettings.m_originUIBackground;
                    GUIStylePanel.normal.textColor = Color.black;
                }
            }

            //Draw Gizmo stuff first, otherwise it will draw over the UI Panel
            DrawGizmos();

            Handles.BeginGUI();
            DrawTopPanel();
            if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(GameObject))
            {
                Terrain terrain = ((GameObject)Selection.activeObject).GetComponent<Terrain>();
                if (terrain != null && !TerrainHelper.IsWorldMapTerrain(terrain))
                {
                    DrawLeftPanel(terrain);
                }
            }

            Handles.EndGUI();
        }

        private static void DrawLeftPanel(Terrain terrain)
        {
            if (SessionManager.m_session == null)
            {
                return;
            }

            GaiaSessionManagerEditor.SetupOperationHeaderColor(ref m_operationCreateWorldStyle, "3FC1C9ff", "297e83ff", m_tempTextureList);
            GaiaSessionManagerEditor.SetupOperationHeaderColor(ref m_operationFlattenTerrainStyle, "C46564ff", "804241ff", m_tempTextureList);
            GaiaSessionManagerEditor.SetupOperationHeaderColor(ref m_operationClearSpawnsStyle, "F0E999ff", "9d9864ff", m_tempTextureList);
            GaiaSessionManagerEditor.SetupOperationHeaderColor(ref m_operationStampStyle, "B8C99Dff", "788367ff", m_tempTextureList);
            GaiaSessionManagerEditor.SetupOperationHeaderColor(ref m_operationStampUndoRedoStyle, "d1a6a3ff", "896c6bff", m_tempTextureList);
            GaiaSessionManagerEditor.SetupOperationHeaderColor(ref m_operationSpawnStyle, "EEB15Bff", "9c743bff", m_tempTextureList);
            GaiaSessionManagerEditor.SetupOperationHeaderColor(ref m_operationRemoveNonBiomeResourcesStyle, "ba7fcdff", "7a5386ff", m_tempTextureList);
            GaiaSessionManagerEditor.SetupOperationHeaderColor(ref m_operationMaskMapExportStyle, "9e955bff", "635D39ff", m_tempTextureList);




            float dpiScalingFactor = (96 / Screen.dpi);
            float scaledScreenWidth = (Camera.current.pixelRect.size.x * dpiScalingFactor);
            float scaledScreenHeight = (Camera.current.pixelRect.size.y * dpiScalingFactor);

            float sizeX = GaiaSettings.m_terrainOpListSizeType == GaiaConstants.PositionType.Relative ? scaledScreenWidth * GaiaSettings.m_terrainOpListSize.x / 100f : GaiaSettings.m_terrainOpListSize.x;
            float sizeY = GaiaSettings.m_terrainOpListSizeType == GaiaConstants.PositionType.Relative ? scaledScreenHeight * GaiaSettings.m_terrainOpListSize.y / 100f : GaiaSettings.m_terrainOpListSize.y;
            float x = GaiaSettings.m_terrainOpListPositionType == GaiaConstants.PositionType.Relative ? scaledScreenWidth * GaiaSettings.m_terrainOpListPosition.x / 100f : GaiaSettings.m_terrainOpListPosition.x;
            float y = GaiaSettings.m_terrainOpListPositionType == GaiaConstants.PositionType.Relative ? (scaledScreenHeight * GaiaSettings.m_terrainOpListPosition.y / 100f) - sizeY / 2f : GaiaSettings.m_terrainOpListPosition.y;

            if (m_operationCheckboxStyle == null)
            {
                m_operationCheckboxStyle = new GUIStyle(GUI.skin.toggle);
                m_operationCheckboxStyle.fixedWidth = 15;
                m_operationCheckboxStyle.margin = new RectOffset(2, 0, 1, 1);
            }

            if (m_operationFoldoutStyle == null)
            {
                m_operationFoldoutStyle = new GUIStyle(GUI.skin.label);
                m_operationFoldoutStyle.margin = new RectOffset(0, 5, 0, 0);
            }
            m_operationFoldoutStyle.fixedWidth = sizeX - 55;

            if (m_operationPingButtonStyle == null)
            {
                m_operationPingButtonStyle = new GUIStyle(GUI.skin.button);
                m_operationPingButtonStyle.fixedHeight = 16;
                m_operationPingButtonStyle.fixedWidth = 20;
                m_operationPingButtonStyle.margin = new RectOffset(0, 0, 1, 2);
            }

            GUILayout.BeginArea(new Rect(x,y,sizeX,sizeY));
            {
                EditorGUI.BeginChangeCheck();
                //EditorGUILayout.BeginVertical();
                //{
                    //GUILayout.Space(130);
                    EditorGUILayout.BeginVertical(GUIStylePanel, GUILayout.MaxWidth(280));
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                        GUILayout.Label("Terrain Operations", GUIStyleHeader, GUILayout.MaxHeight(16));


                        GUIContent buttonContent = null;
                            if (EditorGUIUtility.isProSkin)
                            {
                                if (SessionManager.m_showTerrainOpsPanel)
                                {
                                    buttonContent = new GUIContent(GaiaSettings.m_originUIProUnfoldUp, "Hide Terrain Operations");
                                }
                                else
                                {
                                    buttonContent = new GUIContent(GaiaSettings.m_originUIProUnfoldDown, "Show Terrain Operations");
                                }
                            }
                            else
                            {
                                if (SessionManager.m_showTerrainOpsPanel)
                                {
                                    buttonContent = new GUIContent(GaiaSettings.m_originUIUnfoldUp, "Hide Terrain Operations");
                                }
                                else
                                {
                                    buttonContent = new GUIContent(GaiaSettings.m_originUIUnfoldDown, "Show Terrain Operations");
                                }
                            }
                            if (GUILayout.Button(new GUIContent(buttonContent), GUIStylePanel, GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                SessionManager.m_showTerrainOpsPanel = !SessionManager.m_showTerrainOpsPanel;
                            }
                          
                        }
                        EditorGUILayout.EndHorizontal();

                        if (!SessionManager.m_showTerrainOpsPanel)
                        {
                            //Panel is hidden -> end drawing of GUI early
                            EditorGUILayout.EndVertical();
                            EditorGUI.EndChangeCheck();
                            GUILayout.EndArea();
                            return;
                        }

                        GUILayout.Space(5);
                        int regenCount = SessionManager.m_terrainNamesFlaggedForRegenerationDeactivation.Count() + SessionManager.m_terrainNamesFlaggedForRegeneration.Count();
                        EditorGUILayout.LabelField("Terrains flagged for Regen: " + regenCount.ToString());
                        bool currentGUIState = GUI.enabled;
                        if (regenCount <= 0)
                        {
                            GUI.enabled = false;
                        }
                        m_flaggedTerrainsFoldedOut = EditorGUILayout.Foldout(m_flaggedTerrainsFoldedOut, "View List..."  , true);
                        if (GUILayout.Button("Regenerate now"))
                        {
                            SessionManager.RegenerateFlaggedTerrains();
                        }
                        GUI.enabled = currentGUIState;
                        GUILayout.Space(10);

                        EditorGUILayout.LabelField("Selected Terrain:");
                        EditorGUILayout.LabelField(terrain.name);
                        GUILayout.Space(5);
                        EditorGUILayout.LabelField("Terrain Operations:");
                        GUILayout.Space(5);
                        m_terrainOpScrollPos = EditorGUILayout.BeginScrollView(m_terrainOpScrollPos);
                        {
                            //EditorGUI.indentLevel++;
                            for (int i = 0; i < SessionManager.m_session.m_operations.Count; i++)
                            {
                                if (SessionManager.m_session.m_operations[i].m_affectedTerrainNames.FirstOrDefault(j=>j.Contains(terrain.name))!=null)
                                {
                                    GaiaOperation op = SessionManager.m_session.m_operations[i];
                                    GUIStyle headerStyle = m_operationCreateWorldStyle;

                                    switch (op.m_operationType)
                                    {
                                        case GaiaOperation.OperationType.CreateWorld:
                                            headerStyle = m_operationCreateWorldStyle;
                                            break;
                                        case GaiaOperation.OperationType.ClearSpawns:
                                            headerStyle = m_operationClearSpawnsStyle;
                                            break;
                                        case GaiaOperation.OperationType.FlattenTerrain:
                                            headerStyle = m_operationFlattenTerrainStyle;
                                            break;
                                        case GaiaOperation.OperationType.RemoveNonBiomeResources:
                                            headerStyle = m_operationRemoveNonBiomeResourcesStyle;
                                            break;
                                        case GaiaOperation.OperationType.Spawn:
                                            headerStyle = m_operationSpawnStyle;
                                            break;
                                        case GaiaOperation.OperationType.Stamp:
                                            headerStyle = m_operationStampStyle;
                                            break;
                                        case GaiaOperation.OperationType.StampUndo:
                                            headerStyle = m_operationStampUndoRedoStyle;
                                            break;
                                        case GaiaOperation.OperationType.StampRedo:
                                            headerStyle = m_operationStampUndoRedoStyle;
                                            break;
                                        case GaiaOperation.OperationType.MaskMapExport:
                                            headerStyle = m_operationMaskMapExportStyle;
                                            break;
                                    }
                                    GUI.enabled = op.m_isActive;
                                    GUILayout.BeginHorizontal(headerStyle);
                                    GUI.enabled = currentGUIState;
                                    op.m_isActive = GUILayout.Toggle(op.m_isActive, "", m_operationCheckboxStyle);
                                    GUI.enabled = op.m_isActive;
                                    //GUILayout.Label(op.m_description);
                                    op.m_isFoldedOut = EditorGUILayout.Foldout(op.m_isFoldedOut, new GUIContent((i + 1).ToString() + " " + op.m_description.ToString()), true, m_operationFoldoutStyle);
                                    GUILayout.Space(sizeX-106);
                                    float pingButtonWidth = 20f;
                                    GUIContent pingButtonContent = new GUIContent("P", "Ping this operation in the scene view to find it");
                                    switch (op.m_operationType)
                                    {
                                        case GaiaOperation.OperationType.CreateWorld:
                                            break;
                                        case GaiaOperation.OperationType.Stamp:
                                            if (GUILayout.Button(pingButtonContent, m_operationPingButtonStyle, GUILayout.MaxWidth(pingButtonWidth)))
                                            {
                                                Vector3Double stamperPos = new Vector3Double(op.StamperSettings.m_x, op.StamperSettings.m_y, op.StamperSettings.m_z);
                                                Terrain pingTerrain = TerrainHelper.GetTerrain(stamperPos);
                                                float range = (pingTerrain.terrainData.size.x / 100f * op.StamperSettings.m_width)/2f;
                                                PingInSceneView(stamperPos, range);
                                            }
                                            break;
                                        case GaiaOperation.OperationType.Spawn:
                                            if (GUILayout.Button(pingButtonContent, m_operationPingButtonStyle, GUILayout.MaxWidth(pingButtonWidth)))
                                            {
                                                PingInSceneView(op.SpawnOperationSettings.m_spawnArea.center, (float)op.SpawnOperationSettings.m_spawnArea.extents.x);
                                            }
                                            break;
                                        case GaiaOperation.OperationType.ClearSpawns:
                                            if (GUILayout.Button(pingButtonContent, m_operationPingButtonStyle, GUILayout.MaxWidth(pingButtonWidth)))
                                            {
                                                if (op.ClearOperationSettings.m_clearSpawnFor == ClearSpawnFor.CurrentTerrainOnly)
                                                {
                                                    PingInSceneView(new Vector3Double(op.ClearOperationSettings.m_spawnerSettings.m_x, op.ClearOperationSettings.m_spawnerSettings.m_y, op.ClearOperationSettings.m_spawnerSettings.m_z), (float)op.ClearOperationSettings.m_spawnerSettings.m_spawnRange);
                                                }
                                                else
                                                {
                                                    BoundsDouble b = new BoundsDouble();
                                                    TerrainHelper.GetTerrainBounds(ref b);
                                                    PingInSceneView(b.center, (float)b.extents.x);

                                                }
                                            }
                                            break;
                                        case GaiaOperation.OperationType.RemoveNonBiomeResources:
                                            if (GUILayout.Button(pingButtonContent, m_operationPingButtonStyle, GUILayout.MaxWidth(pingButtonWidth)))
                                            {
                                                PingInSceneView(new Vector3Double(op.RemoveNonBiomeResourcesSettings.m_biomeControllerSettings.m_x, op.RemoveNonBiomeResourcesSettings.m_biomeControllerSettings.m_y, op.RemoveNonBiomeResourcesSettings.m_biomeControllerSettings.m_z), (float)op.RemoveNonBiomeResourcesSettings.m_biomeControllerSettings.m_range);
                                            }
                                            break;


                                    }
                                    GUILayout.EndHorizontal();
                                    GUI.enabled = currentGUIState;

                                    if (op.m_isFoldedOut)
                                    {
                                        DrawOperationFields(SessionManager.m_session.m_operations[i], i);
                                    }
                                    GUILayout.Space(2);
                                }
                            }
                            //EditorGUI.indentLevel--;

                        }
                        EditorGUILayout.EndScrollView();

                    }
                    EditorGUILayout.EndVertical();
                    //GUILayout.FlexibleSpace();
                //}
                //EditorGUILayout.EndVertical();
                //EditorGUI.EndChangeCheck();
            }
            GUILayout.EndArea();



        }

        private static void DrawOperationFields(GaiaOperation op, int i)
        {
            //shared default fields first
            //op.m_isActive = m_editorUtils.Toggle("Active", op.m_isActive, helpEnabled);
            bool currentGUIState = GUI.enabled;
            GUI.enabled = op.m_isActive;
            op.m_description = EditorGUILayout.TextField("Description", op.m_description);
            EditorGUILayout.LabelField("Date", op.m_operationDateTime);
            EditorGUI.indentLevel++;
            op.m_terrainsFoldedOut = EditorGUILayout.Foldout(op.m_terrainsFoldedOut, "Affected Terrains");

            if (op.m_terrainsFoldedOut)
            {
                foreach (string name in op.m_affectedTerrainNames)
                {
                    if(GUILayout.Button(name))
                    {
                        Vector3 pos = Vector3.zero;

                        if (GaiaUtils.HasDynamicLoadedTerrains())
                        {
                            TerrainScene ts = TerrainLoaderManager.TerrainScenes.Find(x => x.GetTerrainName() == name);
                            if (ts != null)
                            {
                                PingInSceneView(ts.m_pos - TerrainLoaderManager.Instance.GetOrigin(), (float)ts.m_bounds.extents.x);
                            }
                        }
                        else
                        {
                            Terrain terrain = Terrain.activeTerrains.Where(x=>x.name == name).First();
                            PingInSceneView(terrain.transform.position + terrain.terrainData.bounds.center, terrain.terrainData.bounds.extents.x);
                        }

                        
                    }
                    Rect rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Zoom);
                }
            }
            EditorGUI.indentLevel--;

            //type specific fields, switch by op type to draw additional fields suitable for the op type

            switch (op.m_operationType)
            {
                case GaiaOperation.OperationType.CreateWorld:
                    EditorGUILayout.LabelField("xTiles",op.WorldCreationSettings.m_xTiles.ToString());
                    EditorGUILayout.LabelField("zTiles", op.WorldCreationSettings.m_zTiles.ToString());
                    EditorGUILayout.LabelField("TileSize", op.WorldCreationSettings.m_tileSize.ToString());
                    break;
                case GaiaOperation.OperationType.Spawn:
                    EditorGUILayout.LabelField("NumberOfSpawners", op.SpawnOperationSettings.m_spawnerSettingsList.Count.ToString());
                    float size = (float)Mathd.Max(op.SpawnOperationSettings.m_spawnArea.size.x, op.SpawnOperationSettings.m_spawnArea.size.z);
                    EditorGUILayout.LabelField("SpawnSize", size.ToString());
                    break;
            }
            GUI.enabled = currentGUIState;
            //Button controls
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("Delete"))
            {
                if (EditorUtility.DisplayDialog("Delete Operation?","Do you want to remove this operation from the session? This operation will be removed from the Session permanently and the affected terrains will be flagged for regeneration. (Regeneration will play back the session on those tiles WITHOUT the operation you are deleting now, effectively removing the operation from the terrains.)", "OK", "Cancel"))
                {
                    SessionManager.RemoveOperation(i);
                }
            }
            GUI.enabled = op.m_isActive;
            if (GUILayout.Button("Play"))
            {
                if (EditorUtility.DisplayDialog("Play Operation?", "Do you want to play back this operation again? This will reapply the selected operation to the terrains again.", "OK", "Cancel"))
                {
                    GaiaSessionManager.ExecuteOperation(op);
                    //Destroy all temporary tools used while executing
                    //not if it is a spawn operation since that is asynchronous
                    if (op.m_operationType != GaiaOperation.OperationType.Spawn)
                    {
                        GaiaSessionManager.DestroyTempSessionTools();
                    }
                }
            }
            GUI.enabled = currentGUIState;
            //EditorGUILayout.EndHorizontal();
            //EditorGUILayout.BeginHorizontal();
            //GUILayout.Space(20);
           
        

            switch (op.m_operationType)
            {
                case GaiaOperation.OperationType.Stamp:
                    if (GUILayout.Button("Edit"))
                    {
                        Stamper stamper = GaiaSessionManager.GetOrCreateSessionStamper();
                        stamper.LoadSettings(op.StamperSettings);
                        //activate Session Edit mode
                        stamper.m_sessionEditMode = true;
                        stamper.m_sessionEditOperation = op;
                        if (GaiaUtils.HasDynamicLoadedTerrains())
                        {
                            //We got dynamic terrains, activate terrain loading
                            stamper.m_loadTerrainMode = LoadMode.EditorSelected;
                        }
                        Selection.activeObject = stamper.gameObject;
                    }

                    break;
                case GaiaOperation.OperationType.Spawn:
                    if (GUILayout.Button("Edit"))
                    {
                        BiomeController bmc = null;
                        List<Spawner> spawnerList = null;
                        Selection.activeObject = GaiaSessionManager.GetOrCreateSessionSpawners(op.SpawnOperationSettings, ref bmc, ref spawnerList);
                    }

                    break;
                case GaiaOperation.OperationType.MaskMapExport:
                    if (GUILayout.Button("Edit"))
                    {
                        MaskMapExport mme = null;
                        Selection.activeObject = GaiaSessionManager.GetOrCreateMaskMapExporter(op.ExportMaskMapOperationSettings.m_maskMapExportSettings, ref mme);
                    }

                    break;
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawTopPanel()
        {
            double range = TerrainLoaderManager.Instance.GetLoadingRange();
            double impostorRange = TerrainLoaderManager.Instance.GetImpostorLoadingRange();
            Vector3Double currentPos = TerrainLoaderManager.Instance.GetOrigin();

            float coordLabelWidth = 12f;
            float coordInputWidth = 50f;

            bool currentGUIState = GUI.enabled;

            float dpiScalingFactor = (96 / Screen.dpi);
            float scaledScreenWidth = (Camera.current.pixelRect.size.x * dpiScalingFactor);
            float scaledScreenHeight = (Camera.current.pixelRect.size.y * dpiScalingFactor);
            

            float sizeX = GaiaSettings.m_gaiaPanelSizeType == GaiaConstants.PositionType.Relative ? scaledScreenWidth * GaiaSettings.m_gaiaPanelSize.x / 100f : GaiaSettings.m_gaiaPanelSize.x;
            float sizeY = GaiaSettings.m_gaiaPanelSizeType == GaiaConstants.PositionType.Relative ? scaledScreenHeight * GaiaSettings.m_gaiaPanelSize.y / 100f : GaiaSettings.m_gaiaPanelSize.y;
            float x = GaiaSettings.m_gaiaPanelPositionType == GaiaConstants.PositionType.Relative ? (scaledScreenWidth * GaiaSettings.m_gaiaPanelPosition.x / 100f) - (sizeX + 50f) / 2f : GaiaSettings.m_gaiaPanelPosition.x;
            float y = GaiaSettings.m_gaiaPanelPositionType == GaiaConstants.PositionType.Relative ? (scaledScreenHeight * GaiaSettings.m_gaiaPanelPosition.y / 100f) : GaiaSettings.m_gaiaPanelPosition.y;

            GUILayout.BeginArea(new Rect(x, y, sizeX+50, sizeY));
            float leftSpace = 6f;
            EditorGUI.BeginChangeCheck();


            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical(GUIStylePanel, GUILayout.MaxWidth(sizeX));
                {
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(leftSpace);
                        if (GUILayout.Button("Manager", GUILayout.Height(15), GUILayout.Width(70)))
                        {
                            var manager = EditorWindow.GetWindow<Gaia.GaiaManagerEditor>(false, "Gaia Manager");
                            //Manager can be null if the dependency package installation is started upon opening the manager window.
                            if (manager != null)
                            {
                                Vector2 initialSize = new Vector2(650f, 450f);
                                manager.position = new Rect(new Vector2(Screen.currentResolution.width / 2f - initialSize.x / 2f, Screen.currentResolution.height / 2f - initialSize.y / 2f), initialSize);
                                manager.Show();
                            }
                        }
                        
                        if (TerrainLoaderManager.Instance.TerrainSceneStorage.m_hasWorldMap)
                        {
                            GUILayout.Label("Gaia", GUIStyleHeader, GUILayout.MaxHeight(16));
                            if (WorldMapTerrain != null)
                            {
                                if (!GaiaUtils.HasTerrains())
                                {
                                    GUI.enabled = false;
                                }
                                if (GUILayout.Button(new GUIContent("Terrain","Switches the Scene View to display the actual terrains of your scene."), GUILayout.Height(15), GUILayout.Width(75)))
                                {
                                    TerrainLoaderManager.Instance.SwitchToLocalMap();
                                }
                                GUI.enabled = currentGUIState;


                                if (GUILayout.Button(new GUIContent("Designer", "Switches the Scene View to display the high-level world map of the world designer."), GUILayout.Height(15), GUILayout.Width(75)))
                                {
                                    TerrainLoaderManager.Instance.SwitchToWorldMap();
                                }

                                if (GUILayout.Button(new GUIContent(m_helpButtonImage, "Opens the online help for the scene view panel."), m_smallButtonStyle, GUILayout.Height(15), GUILayout.Width(15)))
                                {
                                    Application.OpenURL("https://proceduralworlds.freshdesk.com/support/solutions/articles/33000258900-using-the-gaia-scene-view-panel");
                                }
                            }
                            else
                            {
                                //GUILayout.Space(200);
                            }
                        }
                        else
                        {
                            GUILayout.Space(-50);
                            GUILayout.Label("Gaia", GUIStyleHeader, GUILayout.MaxHeight(16));
                            if (!TerrainLoaderManager.Instance.ShowLocalTerrain)
                            {
                                TerrainLoaderManager.Instance.SwitchToLocalMap();
                            }
                            if (GUILayout.Button(new GUIContent(m_helpButtonImage, "Opens the online help for the scene view panel."), m_smallButtonStyle, GUILayout.Height(15), GUILayout.Width(15)))
                            {
                                Application.OpenURL("https://proceduralworlds.freshdesk.com/support/solutions/articles/33000258900-using-the-gaia-scene-view-panel");
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (SessionManager.m_showSceneViewPanel)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(leftSpace);
                            m_oldX = TerrainLoaderManager.Instance.m_originTargetTileX;
                            m_oldZ = TerrainLoaderManager.Instance.m_originTargetTileZ;
                            EditorGUILayout.LabelField(new GUIContent("Tile X:","Navigate through your terrains along the X coordinate"), GUILayout.Width(50));
                            TerrainLoaderManager.Instance.m_originTargetTileX = EditorGUILayout.IntSlider(TerrainLoaderManager.Instance.m_originTargetTileX, 0, TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesX - 1, GUILayout.Width(295));
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(leftSpace);
                            EditorGUILayout.LabelField(new GUIContent("Tile Z:", "Navigate through your terrains along the Z coordinate"), GUILayout.Width(50));
                            TerrainLoaderManager.Instance.m_originTargetTileZ = EditorGUILayout.IntSlider(TerrainLoaderManager.Instance.m_originTargetTileZ, 0, TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesZ - 1, GUILayout.Width(295));
                            
                            //if (GUILayout.Button("Go", GUILayout.Height(18), GUILayout.Width(30)))

                            if (m_oldX != TerrainLoaderManager.Instance.m_originTargetTileX || m_oldZ != TerrainLoaderManager.Instance.m_originTargetTileZ)
                            {
                                var tileSize = TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesSize;
                                float xPos = (TerrainLoaderManager.Instance.m_originTargetTileX + 1 - (TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesX / 2f)) * tileSize - tileSize / 2f;
                                float zPos = (TerrainLoaderManager.Instance.m_originTargetTileZ + 1 - (TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesZ / 2f)) * tileSize - tileSize / 2f;
                                Vector3 targetPos = new Vector3(xPos, 0f, zPos);
                                PingInSceneView(targetPos, tileSize / 2f);
                                SceneView.currentDrawingSceneView.LookAt(targetPos);
                            }

                            //if (m_tileSliderDragStartedTimeStamp > 0 && GaiaUtils.GetUnixTimestamp() > m_tileSliderDragStartedTimeStamp + GaiaSettings.m_gaiaPanelSliderDelay)
                            //{
                            //    //Check first if within range of valid terrains
                            //    if (TerrainLoaderManager.Instance.m_originTargetTileX < 0 || TerrainLoaderManager.Instance.m_originTargetTileZ < 0 || TerrainLoaderManager.Instance.m_originTargetTileX > TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesX - 1 || TerrainLoaderManager.Instance.m_originTargetTileZ > TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesZ - 1)
                            //    {
                            //        TerrainLoaderManager.Instance.m_originTargetTileX = m_oldX;
                            //        TerrainLoaderManager.Instance.m_originTargetTileZ = m_oldZ;
                            //        EditorUtility.DisplayDialog("Terrain selection out of bounds", String.Format("You selected a terrain X Z coordinate that does not exist, please select coordinates between 0-0 and {0}-{1}", (TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesX - 1).ToString(), (TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesZ - 1).ToString()), "OK");
                            //    }
                            //    else
                            //    {
                            //        TerrainLoaderManager.Instance.SetOriginByTargetTile();
                            //        range = TerrainLoaderManager.Instance.GetLoadingRange();
                            //        impostorRange = TerrainLoaderManager.Instance.GetImpostorLoadingRange();
                            //        currentPos = TerrainLoaderManager.Instance.GetOrigin();
                            //    }
                            //    m_tileSliderDragStartedTimeStamp = -1;
                            //}
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(leftSpace);
                            GUI.enabled = !Application.isPlaying;
                            GUILayout.Label(new GUIContent("Origin Shift:", "Shift your entire scene so that the position that you enter here becomes the new World Origin. You can use this to combat issues with floating point precision while editing your world."), GUILayout.Width(100));
                            GUILayout.Label(new GUIContent("X", "Origin offset on the X coordinate"), GUILayout.Width(coordLabelWidth));
                            currentPos.x = EditorGUILayout.DelayedDoubleField(currentPos.x, GUILayout.Width(coordInputWidth));
                            GUILayout.Space(18);
                            GUILayout.Label(new GUIContent("Y", "Origin offset on the Y coordinate"), GUILayout.Width(coordLabelWidth));
                            currentPos.y = EditorGUILayout.DelayedDoubleField(currentPos.y, GUILayout.Width(coordInputWidth));
                            GUILayout.Space(20);
                            GUILayout.Label(new GUIContent("Z", "Origin offset on the Z coordinate"), GUILayout.Width(coordLabelWidth));
                            currentPos.z = EditorGUILayout.DelayedDoubleField(currentPos.z, GUILayout.Width(coordInputWidth));
                            GUI.enabled = currentGUIState;

                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(leftSpace);
                        GUI.enabled = !Application.isPlaying;
                        {
                            GUILayout.Label(new GUIContent("Loading Range", "Scene View Loading Range - Gaia will load in terrains in this range in the scene view. You can choose to the right if the terrains should be loaded around the World Origin (X=0 Y=0 Z=0) or if this range should follow your scene camera."), GUILayout.Width(116));
                            range = EditorGUILayout.DelayedDoubleField(range, GUILayout.Width(coordInputWidth));
                            GUILayout.Space(21);
                            EditorGUILayout.BeginHorizontal();
                            //EditorGUILayout.LabelField("Center on:", GUILayout.Width(100));
                            EditorGUI.BeginChangeCheck();
                            TerrainLoaderManager.Instance.CenterSceneViewLoadingOn = (CenterSceneViewLoadingOn)EditorGUILayout.EnumPopup(new GUIContent("", "Choose here if the terrains should be loaded around the World Origin (X=0 Y=0 Z=0) or if the loading range should follow your scene camera."), TerrainLoaderManager.Instance.CenterSceneViewLoadingOn, GUILayout.Width(154));
                            if (EditorGUI.EndChangeCheck())
                            {
                                //changing the scene view loading mode can influence the loading range, we need to refresh.
                                range = TerrainLoaderManager.Instance.GetLoadingRange();
                                impostorRange = TerrainLoaderManager.Instance.GetImpostorLoadingRange();
                            }
                            EditorGUILayout.EndHorizontal();

                        }
                        GUI.enabled = currentGUIState;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(leftSpace);
                        GUI.enabled = !Application.isPlaying;
                        {
                            if (!GaiaUtils.HasImpostorTerrains())
                            {
                                impostorRange = 0;
                                if (GUILayout.Button(new GUIContent("Create Impostor Terrains...","Opens the Terrain Mesh Exporter to create Impostor Terrains. Impostors are simple, low detail mesh versions of your terrains that can be displayed as a placeholder for the full terrain in the distance."), GUILayout.Width(170)))
                                {
                                    ExportTerrain exportTerrainWindow = EditorWindow.GetWindow<ExportTerrain>();
                                    exportTerrainWindow.FindAndSetPreset("Create Impostors");
                                    exportTerrainWindow.m_settings.m_customSettingsFoldedOut = false;
                                }
                                GUILayout.Space(20);
                            }
                            else
                            { 
                                GUILayout.Label(new GUIContent("Impostor Range:","Loading Range for Impostor Terrains. Additional to the regular terrain loading in the scene view, you can choose to load Impostor terrains at a larger range. This can help with orientation in large worlds during design time."), GUILayout.Width(116));
                                impostorRange = EditorGUILayout.DelayedDoubleField(impostorRange, GUILayout.Width(coordInputWidth));
                                GUILayout.Space(20);
                            }
                            EditorGUILayout.BeginHorizontal();
                            if (TerrainLoaderManager.Instance.CenterSceneViewLoadingOn == CenterSceneViewLoadingOn.SceneViewCamera)
                            {
                                GUI.enabled = false;
                            }
                            EditorGUILayout.LabelField(new GUIContent("Show Loading Ranges:","Shows the loading ranges around the world origin as red cube Gizmos in the scene view."), GUILayout.Width(138));
                            TerrainLoaderManager.Instance.m_showOriginLoadingBounds = EditorGUILayout.Toggle(TerrainLoaderManager.Instance.m_showOriginLoadingBounds, GUILayout.Width(12));
                            EditorGUILayout.EndHorizontal();
                        }
                        GUI.enabled = currentGUIState;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(leftSpace);
                            if (GUILayout.Button(new GUIContent("Show Loader Manager...","Opens the Terrain Loader Manager with more advanced Terrain Loading Settings."), GUILayout.Width(170)))
                            {
                                GameObject loaderObj = GaiaUtils.GetTerrainLoaderManagerObject();
                                Selection.activeObject = loaderObj;
                            }
                            GUILayout.Space(20);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent("Show Terrain Bounds:","Shows the bounds of the loaded and unloaded terrain scenes as Gizmos in the scene view. This gives you an overview about the terrain layout in your scene even when terrains are currently not loaded in."), GUILayout.Width(138));
                            TerrainLoaderManager.Instance.m_showOriginTerrainBoxes = EditorGUILayout.Toggle(TerrainLoaderManager.Instance.m_showOriginTerrainBoxes, GUILayout.Width(12));
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(2f);

                    }
                }
                EditorGUILayout.EndVertical();
                GUIContent buttonContent = null;

                if (EditorGUIUtility.isProSkin)
                {
                    if (SessionManager.m_showSceneViewPanel)
                    {
                        buttonContent = new GUIContent(GaiaSettings.m_originUIProUnfoldUp, "Hide Gaia Panel");
                    }
                    else
                    {
                        buttonContent = new GUIContent(GaiaSettings.m_originUIProUnfoldDown, "Show Gaia Panel");
                    }

                }
                else
                {
                    if (SessionManager.m_showSceneViewPanel)
                    {
                        buttonContent = new GUIContent(GaiaSettings.m_originUIUnfoldUp, "Hide Gaia Panel");
                    }
                    else
                    {
                        buttonContent = new GUIContent(GaiaSettings.m_originUIUnfoldDown, "Show Gaia Panel");
                    }
                }
                if (GaiaUtils.HasDynamicLoadedTerrains())
                {
                    if (GUILayout.Button(new GUIContent(buttonContent), GUIStylePanel, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        SessionManager.m_showSceneViewPanel = !SessionManager.m_showSceneViewPanel;
                    }
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (TerrainLoaderManager.Instance.CenterSceneViewLoadingOn == CenterSceneViewLoadingOn.SceneViewCamera)
            {
                TerrainLoaderManager.Instance.SetLoadingRange(range, impostorRange);
            }
            else
            {
                if (range != TerrainLoaderManager.Instance.GetLoadingRange() || impostorRange != TerrainLoaderManager.Instance.GetImpostorLoadingRange())
                {
                    if (!TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainLoadingEnabled)
                    {
                        EditorUtility.DisplayDialog("Terrain Loading Disabled", "All Terrain Loaders are currently disabled under Gaia Runtime > Terrain Loader. Changing the Loading Range in the Gaia Panel will have no effect until the Loaders are enabled again.", "OK");
                    }
                    TerrainLoaderManager.Instance.SetLoadingRange(range, impostorRange);
                }
            }


            if (EditorGUI.EndChangeCheck())
            {
                if (!Application.isPlaying)
                {
                    if (currentPos != TerrainLoaderManager.Instance.GetOrigin())
                    {
                        TerrainLoaderManager.Instance.SetOrigin(currentPos);
                    }
                }
            }
        }

        private static void DrawGizmos()
        {
            if (!m_sessionManagerExits)
            {
                return;
            }

            if (SessionManager == null)
            {
                return;
            }

            if (GaiaUtils.GetUnixTimestamp() < m_pingStartTimeStamp + 3000)
            {
                Handles.color = new Color(1, 0, 0, 0.5f);
                float pingPongValue = (GaiaUtils.GetUnixTimestamp() - m_pingStartTimeStamp) * m_pingRange / 200f;
                Handles.DrawSolidDisc(m_pingPosition, Vector3.up, Mathf.PingPong(pingPongValue, m_pingRange));
            }


            if (TerrainLoaderManager.Instance.m_showOriginLoadingBounds && TerrainLoaderManager.Instance.CenterSceneViewLoadingOn == CenterSceneViewLoadingOn.WorldOrigin && !Application.isPlaying)
            {
                Handles.color = Color.red;
                Handles.DrawWireCube(TerrainLoaderManager.Instance.GetLoadingCenter(), TerrainLoaderManager.Instance.GetLoadingSize());
                Handles.color = Color.red * 0.75f;
                Handles.DrawWireCube(TerrainLoaderManager.Instance.GetLoadingCenter(), TerrainLoaderManager.Instance.GetImpostorLoadingSize());
            }
            if (TerrainLoaderManager.Instance.m_showOriginTerrainBoxes)
            {
                foreach (TerrainScene terrainScene in TerrainLoaderManager.TerrainScenes)
                {
                    if (terrainScene.m_regularLoadState == LoadState.Loaded)
                    {
                        Handles.color = Color.green;
                    }
                    else
                    {
                        Handles.color = Color.gray;
                    }
                    Handles.DrawWireCube(terrainScene.m_bounds.center - TerrainLoaderManager.Instance.GetOrigin(), terrainScene.m_bounds.size);
                    //Handles.DrawWireDisc(terrainScene.m_bounds.min, Vector3.up, 50f);
                    //Handles.DrawWireDisc(terrainScene.m_bounds.max, Vector3.up, 50f);
                }
            }
        }

        private static void PingInSceneView(Vector3 pos, float range)
        {
            m_pingStartTimeStamp = GaiaUtils.GetUnixTimestamp();
            m_pingPosition = pos;
            m_pingRange = range;
        }
    }
}
#endif
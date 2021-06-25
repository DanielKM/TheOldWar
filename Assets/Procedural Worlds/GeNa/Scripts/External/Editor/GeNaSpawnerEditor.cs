// .NET
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using PWCommon5;
// Unity
using UnityEngine;
using UnityEditor;
// Procedural Worlds
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace GeNa.Core
{
    internal class GeNaPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                if (str.Contains(".prefab"))
                {
                    var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(str);
                    if (prefabAsset == null)
                        continue;
                    var spawner = prefabAsset.GetComponent<GeNaSpawner>();
                    if (spawner == null)
                        continue;
                    if (GeNaEditorUtility.IsPrefab(spawner.gameObject))
                        GeNaEditorUtility.CreateMainDataBufferScriptable(spawner);
                }
            }
        }
    }
    /// <summary>
    /// Editor for GeNa spawning system
    /// </summary>
    [CustomEditor(typeof(GeNaSpawner))]
    public class GeNaSpawnerEditor : GeNaEditor
    {
        #region Variables
        //private const string UNDO_SYMBOL = "<color=#00ff00ff>\u21B6</color>";
        private const string GX_ICON = "<color=#ff0000ff>GX</color>";
        private GeNaSpawner m_spawner;
        private static bool m_yesToAllValidations = false;
        private static bool m_noToAllValidations = false;
        private GeNaSpawnerData m_spawnerData;
        private PlacementCriteria m_placementCriteria;
        private SpawnCriteria m_spawnCriteria;
        private SpawnerSettings m_settings;
        // Switch to drop custom ground level for ingestion
        private Vector2 m_scrollPosition = Vector2.zero;
        // Helpers
        private bool m_hasPrefabs;
        private bool m_hasTrees;
        private bool m_hasTextures;
        private Vector2 m_lastMousePos = Vector2.zero;
        private Vector3 m_lastSpawnPosition = Vector3.zero;
        private Color m_separatorColor = Color.black;
        private int m_instanceTopLimit;
        // Undo
        // This is needed because in latest Unity displaying the progress bar mid-GUI results in GUI exceptions popping up in that cycle.
        private int m_undoSteps = 0;
        // GUI
        private Texture2D m_overridesIco;
        private Texture2D m_ChildSpawnerIco;
        public AabbTest[,] m_fitnessArray = new AabbTest[1, 1];
        private bool m_isPartOfPrefab;
        #endregion
        #region Methods
        #region Unity
        private void OnEnable()
        {
            var tempObject = GeNaSpawnerInternal.TempGameObject;
            tempObject.SetActive(false);
            string prefString = "PWShowNews" + PWApp.CONF.NameSpace;
            EditorPrefs.SetBool(prefString, true);
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, null, null);
            // Temp
            DeleteEditorPrefsKeys();
            #region Spawner Setup
            m_spawner = target as GeNaSpawner;
            m_isPartOfPrefab = GeNaEditorUtility.IsPrefab(m_spawner.gameObject);
            m_spawner.Load();
            // Setup defaults
            // Default Spawn To Target
            m_spawner.SetDefaults(GeNaEditorUtility.Defaults);
            m_instanceTopLimit = m_spawner.GetInstancesTopLimit();
            m_spawnerData = m_spawner.SpawnerData;
            m_spawnerData.SpawnToTarget = Preferences.DefaultSpawnToTarget;
            m_placementCriteria = m_spawnerData.PlacementCriteria;
            m_spawnCriteria = m_spawnerData.SpawnCriteria;
            m_settings = m_spawnerData.Settings;
            if (m_spawnerData.GetSeaLevel)
            {
                GeNaEvents.SetSeaLevel(m_spawnerData);
            }
            m_noToAllValidations = false;
            m_yesToAllValidations = false;
            foreach (Terrain terrain in Terrain.activeTerrains)
            {
                ValidateSpawnerPrototypes(m_spawner, m_spawnerData, terrain);
            }
            #endregion
            #region Load Textures
            // If Unity Pro
            if (EditorGUIUtility.isProSkin)
            {
                if (m_overridesIco == null)
                    m_overridesIco = Resources.Load("fschklicop") as Texture2D;
                if (m_ChildSpawnerIco == null)
                    m_ChildSpawnerIco = Resources.Load("protoparentp") as Texture2D;
                m_separatorColor = new Color(0.34f, 0.34f, 0.34f);
            }
            // or Unity Personal
            else
            {
                if (m_overridesIco == null)
                    m_overridesIco = Resources.Load("fschklico") as Texture2D;
                if (m_ChildSpawnerIco == null)
                    m_ChildSpawnerIco = Resources.Load("protoparent") as Texture2D;
            }
            #endregion
            GeNaEvents.onSpawnFinished += OnSpawnFinished;
            GeNaEvents.onBeforeSpawn += OnBeforeSpawn;
            m_spawnerData.VisualizationFixed = false;
            GeNaEvents.onSpawnerDestroyed += OnSpawnerDestroyed;
            GeNaEditorEvents.onBeforeAssemblyReloads -= Cleanup;
            GeNaEditorEvents.onBeforeAssemblyReloads += Cleanup;
            if (m_spawner.gameObject.scene.isLoaded)
            {
                PrefabUtility.prefabInstanceUpdated -= PrefabInstanceUpdated;
                PrefabUtility.prefabInstanceUpdated += PrefabInstanceUpdated;
            }
            if (!m_isPartOfPrefab)
            {
                Serialize(m_spawner);
            }
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }
        private void Update()
        {
            var animators = FindObjectsOfType<GeNaAnimatorDecorator>();
            foreach (var animator in animators)
            {
                animator.UpdateState();
            }
            // Update the Animators
        }
        private void PrefabInstanceUpdated(GameObject instance)
        {
            //Debug.Log($"Spawner Instance Modified! {instance.name}");
            //EditorGUIUtility.PingObject(instance);
            GameObject prefabAsset = GeNaEditorUtility.GetPrefabAsset(instance);
            if (prefabAsset == null)
                return;
            GeNaSpawner spawner = instance.GetComponent<GeNaSpawner>();
            if (spawner == null)
                return;
            SaveChanges(spawner);
        }
        private void SaveChanges(GeNaSpawner spawner)
        {
            var dataFile = GeNaEditorUtility.ApplyToMain(spawner);
            spawner.SerializeToFile(dataFile);
            //PrefabUtility.ApplyPrefabInstance(spawner.gameObject, InteractionMode.AutomatedAction);
            EditorUtility.SetDirty(spawner);
            // PrefabUtility.RecordPrefabInstancePropertyModifications(spawner);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        private void DiscardChanges(GeNaSpawner spawner)
        {
            var dataFile = GeNaEditorUtility.RevertToMain(spawner);
            spawner.DeserializeFromFile(dataFile);
            PrefabUtility.RecordPrefabInstancePropertyModifications(spawner);
            AssetDatabase.SaveAssets();
        }
        private bool m_isDestroying = false;
        private void OnSpawnerDestroyed(GeNaSpawnerData geNaSpawnerData)
        {
            if (geNaSpawnerData.GetInstanceID() == m_spawnerData.GetInstanceID())
            {
                m_isDestroying = true;
            }
        }
        protected override void OnDestroy()
        {
            if (m_isDestroying)
                GeNaEditorUtility.RemoveDataBufferScriptable(m_spawner);
            GeNaEvents.onBeforeSpawn -= OnBeforeSpawn;
            GeNaEvents.onSpawnFinished -= OnSpawnFinished;
            GeNaEvents.onSpawnerDestroyed -= OnSpawnerDestroyed;
            PrefabUtility.prefabInstanceUpdated -= PrefabInstanceUpdated;
        }
        private void OnBeforeSpawn(GeNaSpawnerData spawnerData)
        {
            Repaint();
        }
        private void OnSpawnFinished()
        {
            m_spawner.UpdateVisualization();
            Repaint();
        }
        private void OnDisable()
        {
            if (m_spawner.IsDirty)
            {
                Serialize(m_spawner);
                m_spawner.IsDirty = false;
            }
            GeNaEvents.onBeforeSpawn -= OnBeforeSpawn;
            GeNaEvents.onSpawnFinished -= OnSpawnFinished;
            GeNaEvents.onSpawnerDestroyed -= OnSpawnerDestroyed;
            PrefabUtility.prefabInstanceUpdated -= PrefabInstanceUpdated;
            Cleanup();
            EditorApplication.update -= Update;
        }
        public void Cleanup()
        {
            var tempObject = GeNaSpawnerInternal.TempGameObject;
            if (tempObject != null)
                GeNaEvents.Destroy(tempObject);
        }
        /// <summary>
        /// Returns true if the Spawner needs to be Serialized
        /// </summary>
        /// <param name="spawner"></param>
        /// <param name="onRepaint"></param>
        /// <returns></returns>
        public static void SpawnerEditor(GeNaSpawner spawner, Action onRepaint = null)
        {
            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            // Exit if event does not have current value
            if (e == null)
                return;
            // Repaint for Visualization
            // TODO : Manny : This should only happen if the user moves their mouse
            SceneView.RepaintAll();
            GeNaSpawnerData spawnerData = spawner.SpawnerData;
            SpawnerSettings settings = spawnerData.Settings;
            PlacementCriteria placementCriteria = spawnerData.PlacementCriteria;
            SpawnCriteria spawnCriteria = spawnerData.SpawnCriteria;
            bool raycastHit = GeNaEditorUtility.ShowSpawnRange(spawnerData, out RaycastHit hitInfo);
            // If SHIFT is not held down, visualization will be of
            settings.VisualizationActive = false;
            #region Keyboard Handling
            GameObject lastSpawnedObject = spawnerData.LastSpawnedObject;
            //Keyboard handling
            if (lastSpawnedObject != null)
            {
                Transform lastSpawnedObjectTransform = lastSpawnedObject.transform;
                //Ctrl Left
                if (e.Equals(GeNaEditorUtility.Defaults.KeyLeftEvent(false, true)))
                {
                    GUIUtility.hotControl = controlID;
                    Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.left;
                    lastSpawnedObjectTransform.position += (movement * 0.05f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Shift Ctrl Left
                if (e.Equals(GeNaEditorUtility.Defaults.KeyLeftEvent(true, true)))
                {
                    GUIUtility.hotControl = controlID;
                    lastSpawnedObjectTransform.Rotate(Vector3.up, -1f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Ctrl right
                if (e.Equals(GeNaEditorUtility.Defaults.KeyRightEvent(false, true)))
                {
                    GUIUtility.hotControl = controlID;
                    Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.right;
                    lastSpawnedObjectTransform.position += (movement * 0.05f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Shift Ctrl Right
                if (e.Equals(GeNaEditorUtility.Defaults.KeyRightEvent(true, true)))
                {
                    GUIUtility.hotControl = controlID;
                    lastSpawnedObjectTransform.Rotate(Vector3.up, 1f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Ctrl Forward
                if (e.Equals(GeNaEditorUtility.Defaults.KeyForwardEvent(false, true)))
                {
                    GUIUtility.hotControl = controlID;
                    Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.forward;
                    lastSpawnedObjectTransform.position += (movement * 0.05f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Shift Ctrl Forward
                if (e.Equals(GeNaEditorUtility.Defaults.KeyForwardEvent(true, true)))
                {
                    GUIUtility.hotControl = controlID;
                    lastSpawnedObjectTransform.Translate(Vector3.up * 0.1f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Ctrl Backward
                if (e.Equals(GeNaEditorUtility.Defaults.KeyBackwardEvent(false, true)))
                {
                    GUIUtility.hotControl = controlID;
                    Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.back;
                    lastSpawnedObjectTransform.position += (movement * 0.05f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Shift Ctrl Backward
                if (e.Equals(GeNaEditorUtility.Defaults.KeyBackwardEvent(true, true)))
                {
                    GUIUtility.hotControl = controlID;
                    lastSpawnedObjectTransform.Translate(Vector3.down * 0.1f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }
            }
            #endregion
            #region Hot Key Actions
            if (e.control && e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.R)
                {
                    spawner.SpawnerData.RandomGenerator.Reset();
                }
            }
            // Ctrl Delete Backspace
            if (e.Equals(GeNaEditorUtility.Defaults.KeyDeleteEvent(false, true)))
            {
                GUIUtility.hotControl = controlID;
                if (EditorUtility.DisplayDialog("WARNING!",
                    "Are you sure you want to delete all instances of prefabs referred to by this spawner from your scene?\n\n" +
                    "NOTE: This will also clear the Undo History.",
                    "OK", "Cancel"))
                {
                    GeNaSpawnerInternal.DespawnAllPrefabs(spawnerData);
                }
                e.Use();
                GUIUtility.hotControl = 0;
                GUIUtility.ExitGUI();
                return;
            }

            // CTRL ALT Z: Undo
            if (e.control && e.alt && e.type == EventType.KeyDown && e.keyCode == KeyCode.Z)
            {
                GeNaSpawnerInternal.Undo(spawnerData);
                e.Use();
                return;
            }

            // Scroll wheel
            if (e.type == EventType.ScrollWheel)
            {
                if (e.control)
                {
                    int offset = (int) e.delta.y;
                    if (offset > 0)
                    {
                        spawnerData.MinInstances -= offset;
                        spawnerData.MaxInstances -= offset;
                    }
                    else
                    {
                        spawnerData.MaxInstances -= offset;
                        spawnerData.MinInstances -= offset;
                    }
                    e.Use();
                    //Settings changed, let's update ranges - probably no need to update child spawners, since their settings did not change.
                    GeNaSpawnerInternal.UpdateTargetSpawnerRanges(spawnerData, false);
                    return;
                }
                if (e.shift)
                {
                    spawnerData.SpawnRange -= e.delta.y;
                    e.Use();
                    //Settings changed, let's update ranges - probably no need to update child spawners, since their settings did not change.
                    SpawnCall spawnCall = new SpawnCall(spawnerData);
                    spawnCall.Normal = hitInfo.normal;
                    spawnCall.Location = hitInfo.point;
                    spawnCall.SetTarget(hitInfo.transform);
                    GeNaSpawnerInternal.SetSpawnOrigin(spawnerData, spawnCall);
                    //Let's update ranges - including child spawners.
                    GeNaSpawnerInternal.UpdateTargetSpawnerRanges(spawnerData, false);
                    GeNaSpawnerInternal.UpdateVisualization(spawnerData);
                    return;
                }
            }

            // Check for the shift + ctrl + left mouse button event - spawn entire terrain
            if (e.shift && e.control && e.isMouse)
            {
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    if (raycastHit)
                    {
                        GUIUtility.hotControl = controlID;
                        GlobalSpawnDialog window = EditorWindow.GetWindow<GlobalSpawnDialog>(true, "GeNa Global Spawn", true);
                        window.Init(spawner, hitInfo);
                        e.Use();
                    }
                    return;
                }
                return;
            }
            #endregion
            bool mouseUp = false;
            // Check for the CTRL + LEFT Mouse Button event - Spawn
            if (e.control && e.isMouse)
            {
                // Left button
                if (e.button == 0)
                {
                    switch (e.type)
                    {
                        case EventType.MouseDown:
                            // Spawn Logic
                            GUIUtility.hotControl = controlID;
                            if (raycastHit)
                            {
                                spawnerData.SpawnCalls.Clear();
                                // Generate a new spawn call on hit point
                                SpawnCall spawnCall = GeNaSpawnerInternal.GenerateSpawnCall(spawnerData, hitInfo);
                                bool singleSpawnMode = spawnerData.SpawnMode != Constants.SpawnMode.Paint;
                                SpawnerEntry entry = new SpawnerEntry(spawner)
                                {
                                    Initialize = true,
                                    Title = "GeNa Spawner",
                                    Info = spawnerData.Name + " is Spawning...",
                                    RecordUndo = singleSpawnMode,
                                    RootSpawnCall = spawnCall
                                };
                                entry.AddSpawnCall(spawnCall);
                                GeNaEditorUtility.ScheduleSpawn(entry);
                                spawnerData.LastSpawnPosition = hitInfo.point;
                            }
                            onRepaint?.Invoke();
                            // Use Event
                            e.Use();
                            break;
                        case EventType.MouseDrag:
                            GUIUtility.hotControl = controlID;
                            if (raycastHit)
                            {
                                Vector3 direction = hitInfo.point - spawnerData.LastSpawnPosition;
                                if (direction.magnitude > spawnerData.FlowRate)
                                {
                                    // Paint Mode
                                    if (spawnerData.SpawnMode == Constants.SpawnMode.Paint)
                                    {
                                        SpawnCall spawnCall = GeNaSpawnerInternal.GenerateSpawnCall(spawnerData, hitInfo);
                                        SpawnerEntry entry = new SpawnerEntry(spawner)
                                        {
                                            Initialize = false,
                                            RecordUndo = false,
                                            RootSpawnCall = spawnCall
                                        };
                                        entry.AddSpawnCall(spawnCall);
                                        // Perform Spawn
                                        GeNaEditorUtility.ScheduleSpawn(entry);
                                        spawnerData.LastSpawnPosition = hitInfo.point;
                                    }
                                }
                            }
                            e.Use();
                            break;
                        case EventType.MouseUp:
                            mouseUp = true;
                            break;
                    }
                }
            }

            // Check Raw Events
            switch (e.rawType)
            {
                case EventType.MouseUp:
                    mouseUp = true;
                    spawnerData.VisualizationFixed = false;
                    break;
            }
            if (mouseUp)
            {
                bool paintedSpawn = spawnerData.SpawnMode == Constants.SpawnMode.Paint;
                if (paintedSpawn)
                {
                    GeNaEditorUtility.ScheduleSpawn(new SpawnerEntry(spawner)
                    {
                        Initialize = false,
                        RecordUndo = true,
                        Description = "Painted Spawn"
                    });
                    SceneView.RepaintAll();
                }
                mouseUp = false;
            }

            // Check for the CTRL + SHIFT + I - Iterate
            if (e.control && e.shift && e.type == EventType.KeyDown && e.keyCode == KeyCode.I)
            {
                SpawnerEntry entry = new SpawnerEntry(spawner)
                {
                    Initialize = true,
                    Title = "GeNa Spawner",
                    Info = spawnerData.Name + " is Spawning...",
                    RecordUndo = true
                };
                entry.AddSpawnCalls(spawnerData.SpawnCalls);
                GeNaEditorUtility.ScheduleIterate(entry);
                e.Use();
                return;
            }
            if (e.type == EventType.Repaint)
                GeNaSpawnerInternal.DrawVisualization(spawnerData);
            spawnerData.VisualizationActive = false;
            bool refreshAabbManager = false;
            // Check for the SHIFT (show/move visualization) and SHIFT + left mouse events (update ranges + move visualisation - drag rotation)
            if (e.shift)
            {
                // SHIFT is down -> visualization is active
                spawnerData.VisualizationActive = true;
                // If SHIFT and CONTROL both down, update the location of the visualization
                if (e.control && raycastHit)
                {
                    SpawnCall spawnCall = new SpawnCall(spawnerData);
                    spawnCall.Location = hitInfo.point;
                    spawnCall.Normal = hitInfo.normal;
                    spawnCall.SetTarget(hitInfo.transform);
                    GeNaSpawnerInternal.SetSpawnOrigin(spawnerData, spawnCall);
                    //Let's update ranges - including child spawners.
                    GeNaSpawnerInternal.UpdateTargetSpawnerRanges(spawnerData, false);
                    GeNaSpawnerInternal.UpdateVisualization(spawnerData);
                    refreshAabbManager = true;
                }
                if (e.isMouse)
                {
                    // Left button
                    if (e.button == 0)
                    {
                        switch (e.type)
                        {
                            case EventType.MouseDown:
                            {
                                GUIUtility.hotControl = controlID;
                                if (raycastHit)
                                {
                                    SpawnCall spawnCall = new SpawnCall(spawnerData);
                                    spawnCall.Location = hitInfo.point;
                                    spawnCall.Normal = hitInfo.normal;
                                    spawnCall.SetTarget(hitInfo.transform);
                                    spawnCall.SetGround(hitInfo.transform);
                                    GeNaSpawnerInternal.SetSpawnOrigin(spawnerData, spawnCall, true);
                                    //Let's update ranges - including child spawners.
                                    GeNaSpawnerInternal.UpdateTargetSpawnerRanges(spawnerData, hitInfo, true);
                                    GeNaSpawnerInternal.UpdateVisualization(spawnerData);
                                    onRepaint?.Invoke();
                                    refreshAabbManager = true;
                                    // spawnerData.IsDirty = true;
                                }
                                break;
                            }
                            case EventType.MouseDrag when GUIUtility.hotControl == controlID &&
                                                          placementCriteria.RotationAlgorithm == Constants.RotationAlgorithm.Fixed &&
                                                          placementCriteria.EnableRotationDragUpdate && raycastHit:
                            {
                                spawnerData.VisualizationFixed = true;
                                Quaternion rot = Quaternion.LookRotation(hitInfo.point - spawnerData.SpawnOriginLocation);
                                placementCriteria.MinRotationY = placementCriteria.MinRotationY = placementCriteria.MaxRotationY = rot.eulerAngles.y;
                                GeNaSpawnerInternal.UpdateVisualization(spawnerData);
                                break;
                            }
                        }
                    }
                }
            }
            if (refreshAabbManager)
            {
                SpawnerSettings.AdvancedSettings advancedSettings = settings.Advanced;
                float radius = spawnerData.SpawnRange + advancedSettings.BoundsOffset;
                LayerMask layerMask = spawnCriteria.SpawnCollisionLayers;
                GeNaManager gm = GeNaManager.GetInstance();
                gm.LoadAabbManager(spawnerData, hitInfo.point, radius, layerMask);
                return;
            }
            // Visualise it
            if (placementCriteria.RotationAlgorithm <= Constants.RotationAlgorithm.Fixed)
            {
                if (HasTrees(spawnerData.SpawnPrototypes) || HasPrefabs(spawnerData.SpawnPrototypes) || spawnCriteria.CheckMaskType == Constants.MaskType.Image)
                {
                    Handles.color = new Color(0f, 255f, 0f, 0.25f);
                    Vector3 position = spawnerData.SpawnOriginLocation;
                    if (GeNaUtility.ApproximatelyEqual(placementCriteria.MinRotationY, placementCriteria.MaxRotationY))
                    {
                        Quaternion rotation = Quaternion.Euler(0f, placementCriteria.MinRotationY, 0f);
                        float size = Mathf.Clamp(spawnerData.SpawnRange * 0.2f, 0.25f, 40f);
                        Handles.ArrowHandleCap(controlID, position, rotation, size, EventType.Repaint);
                    }
                    else
                    {
                        Quaternion rotation = Quaternion.AngleAxis(placementCriteria.MinRotationY, Vector3.up);
                        float angle = placementCriteria.MaxRotationY - placementCriteria.MinRotationY;
                        float radius = Mathf.Clamp(spawnerData.SpawnRange / 6f, 0.25f, 1f);
                        Handles.DrawSolidArc(position, Vector3.up, rotation * Vector3.forward, angle, radius);
                    }
                }
            }
        }
        private void OnSceneGUI()
        {
            // Exit if no spawner
            if (m_spawnerData == null)
                return;
            SpawnerEditor(m_spawner, Repaint);
            GeNaManager geNaManager = GeNaManager.GetInstance();
            Queue<SpawnerEntry> spawnEntries = geNaManager.SpawnEntryQueue;
            foreach (SpawnerEntry spawnEntry in spawnEntries)
            {
                Handles.color = Color.green;
                foreach (SpawnCall spawnCall in spawnEntry.SpawnCalls)
                {
                    if (!spawnCall.CanSpawn)
                        continue;
                    Handles.DrawLine(spawnCall.Location, spawnCall.Location + spawnCall.Normal * 1.5f);
                    Handles.DrawWireArc(spawnCall.Location, spawnCall.Normal, Vector3.forward, 360f, 1f);
                }
            }
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (m_settings == null)
                return;
            #region Header
            // Set the targetPreset
            m_spawner = (GeNaSpawner) target;
            m_spawnerData = m_spawner.SpawnerData;
            m_editorUtils.GUIHeader();
            m_editorUtils.GUINewsHeader();
            #endregion
            m_hasPrefabs = HasPrefabs(m_spawnerData.SpawnPrototypes);
            m_hasTrees = HasTrees(m_spawnerData.SpawnPrototypes);
            m_hasTextures = HasTextures(m_spawnerData.SpawnPrototypes);
            if (!GeNaEditorUtility.ValidateComputeShader())
            {
                Color guiColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                EditorGUILayout.BeginVertical(Styles.box);
                m_editorUtils.Text("NoComputeShaderHelp");
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = guiColor;
                GUI.enabled = false;
            }
            #region Scroll
            var data = m_spawner.GetDataBuffer();
            //Monitor for changes
            EditorGUI.BeginChangeCheck();
            {
                if (GeNaEditorUtility.ValidateComputeShader())
                {
                    GUI.enabled = !m_spawnerData.IsProcessing;
                }
                #region Panels
                m_settings.ShowQuickStart = m_editorUtils.Panel("Quick Start", QuickStartPanel, m_settings.ShowQuickStart);
                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = (m_isPartOfPrefab && data.IsTemp) ? new Color(0.48f, 0.29f, 0.3f) : oldColor;
                // Overview Panel
                GUIStyle overviewLabelStyle = Styles.panelLabel;
                string overviewText = string.Format("{0} : {1}", m_editorUtils.GetTextValue("Overview Panel"), m_spawnerData.Name);
                GUIContent overviewPanelLabel = new GUIContent(overviewText, m_editorUtils.GetTooltip("Overview Panel"));
                m_settings.ShowOverview = m_editorUtils.Panel(overviewPanelLabel, OverviewPanel, overviewLabelStyle, m_settings.ShowOverview);
                if (m_spawner.Palette != null)
                {
                    // Placement Criteria Panel
                    m_settings.ShowPlacementCriteria = m_editorUtils.Panel("Placement Criteria Panel Label", PlacementCritPanel, m_settings.ShowPlacementCriteria);
                    // Spawn Criteria Panel
                    m_settings.ShowSpawnCriteria = m_editorUtils.Panel("Spawn Criteria Panel Label", SpawnCritPanel, m_settings.ShowSpawnCriteria);
                    // Prototypes Panel
                    GUIContent protoPanelLabel = new GUIContent(string.Format("{0} ({1}) [{2}]",
                            m_editorUtils.GetTextValue("Spawn Prototypes"), m_spawnerData.SpawnPrototypes.Count, m_spawnerData.InstancesSpawned),
                        m_editorUtils.GetTooltip("Spawn Prototypes"));
                    m_settings.ShowPrototypes = m_editorUtils.Panel(protoPanelLabel, PrototypesPanel, m_settings.ShowPrototypes);
                    // Advanced Panel
                    m_settings.ShowAdvancedSettings = m_editorUtils.Panel("Advanced Panel Label", AdvancedPanel, m_settings.ShowAdvancedSettings);
                    // Add Panel
                    AddPrototypesPanel();
                }
                GUI.backgroundColor = oldColor;
                #endregion
                if (GeNaEditorUtility.ValidateComputeShader())
                {
                    GUI.enabled = true;
                }
            }
            #region Change Check
            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                m_spawner.UpdateSpawnCritOverrides();
                if (!GeNaEditorUtility.IsPrefab(m_spawner.gameObject))
                    m_spawner.UpdateGoName();
                m_spawner.UpdateVisualization();
                // Random Generator
                m_spawner.UpdateRandom(m_spawnerData.RandomSeed);
                // Spawn Algorithm
                if (m_placementCriteria.SpawnAlgorithm == Constants.LocationAlgorithm.Every && m_spawnerData.ThrowDistance < 0.5f)
                    m_spawnerData.ThrowDistance = 0.5f;
                // Rotation Algorithm
                switch (m_placementCriteria.RotationAlgorithm)
                {
                    case Constants.RotationAlgorithm.Fixed:
                        m_placementCriteria.MaxRotationY = m_placementCriteria.MinRotationY;
                        break;
                    case Constants.RotationAlgorithm.LastSpawnCenter:
                    case Constants.RotationAlgorithm.LastSpawnClosest:
                        m_placementCriteria.MinRotationY = m_placementCriteria.MaxRotationY = 0f;
                        break;
                }
                // Min Rotation / Max Rotation
                m_placementCriteria.MinRotationY = Mathf.Min(m_placementCriteria.MinRotationY, m_placementCriteria.MaxRotationY);
                m_placementCriteria.MaxRotationY = Mathf.Max(m_placementCriteria.MinRotationY, m_placementCriteria.MaxRotationY);
                // Spawn Height
                if (m_spawnCriteria.MaxSpawnHeight < m_spawnCriteria.MinSpawnHeight)
                    m_spawnCriteria.MaxSpawnHeight = m_spawnCriteria.MinSpawnHeight;
                if (m_spawnCriteria.MaxSpawnHeight < m_spawnCriteria.MinSpawnHeight)
                    m_spawnCriteria.MinSpawnHeight = m_spawnCriteria.MaxSpawnHeight;
                // Mask Type
                switch (m_spawnCriteria.CheckMaskType)
                {
                    case Constants.MaskType.Perlin:
                        m_spawnCriteria.MaskFractal.FractalType = Fractal.GeneratedFractalType.Perlin;
                        break;
                    case Constants.MaskType.Billow:
                        m_spawnCriteria.MaskFractal.FractalType = Fractal.GeneratedFractalType.Billow;
                        break;
                    case Constants.MaskType.Ridged:
                        m_spawnCriteria.MaskFractal.FractalType = Fractal.GeneratedFractalType.RidgeMulti;
                        break;
                }
                // Mask Image
                Vector3 minScale = m_placementCriteria.MinScale;
                Vector3 maxScale = m_placementCriteria.MaxScale;
                // Max could be pushing down min
                minScale.x = Mathf.Min(minScale.x, maxScale.x);
                // Min could be pushing up max
                maxScale.x = Mathf.Max(minScale.x, maxScale.x);
                // Max could be pushing down min
                minScale.y = Mathf.Min(minScale.y, maxScale.y);
                // Min could be pushing up max
                maxScale.y = Mathf.Max(minScale.y, maxScale.y);
                // Max could be pushing down min
                minScale.z = Mathf.Min(minScale.z, maxScale.z);
                // Min could be pushing up max
                maxScale.z = Mathf.Max(minScale.z, maxScale.z);
                m_placementCriteria.MinScale = minScale;
                m_placementCriteria.MaxScale = maxScale;
                // Handle sorting
                if (m_spawnerData.SortPrototypes)
                {
                    EditorUtility.DisplayProgressBar("GeNa", "Sorting prototypes...", 0.5f);
                    m_spawner.SortPrototypesAZ();
                    EditorUtility.ClearProgressBar();
                }
                // Set name based on the first thing added
                if (m_spawnerData.SpawnPrototypes.Count == 0 && m_spawnerData.SpawnPrototypes.Count > 0)
                {
                    m_spawnerData.Name = m_spawnerData.SpawnPrototypes[0].Name;
                    m_spawner.UpdateGoName();
                }
                // Update their ID's
                GeNaEditorUtility.UpdateResourceIDs(m_spawnerData);
                // Settings changed, let's update ranges - probably no need to update child spawners, since their settings did not change.
                m_spawner.UpdateTargetSpawnerRanges(false);
            }
            if (Event.current.type != EventType.Layout)
                if (m_spawner.IsDirty)
                {
                    Serialize(m_spawner);
                    GeNaEditorUtility.ForceUpdate();
                    m_spawner.IsDirty = false;
                    m_spawner.Load();
                }
            #endregion
            GUILayout.Space(5);
            #endregion
            m_editorUtils.GUINewsFooter(false);
            // This was necessary because in latest Unity displaying the progress bar mid-GUI results in GUI exceptions popping up in that cycle.
            if (m_undoSteps > 0 && Event.current.type == EventType.Repaint)
            {
                m_spawner.Undo(m_undoSteps);
                m_undoSteps = 0;
            }
        }
        public static void Serialize(GeNaSpawner spawner)
        {
            if (GeNaEditorUtility.IsPrefab(spawner.gameObject) && !AssetDatabase.Contains(spawner.gameObject))
            {
                var dataFile = spawner.GetTempDataFile();
                if (dataFile == null)
                {
                    dataFile = ScriptableObject.CreateInstance<DataBufferScriptable>();
                    dataFile.name = "Spawner Data (Temp)";
                }
                spawner.SerializeToFile(dataFile);
                PrefabUtility.RecordPrefabInstancePropertyModifications(spawner);
                AssetDatabase.SaveAssets();
            }
            else
            {
                var dataFile = GeNaEditorUtility.GetMainDataBufferScriptable(spawner);
                spawner.SerializeToFile(dataFile);
                EditorUtility.SetDirty(dataFile);
            }
        }
        #endregion
        #region Core
        /// <summary>
        /// Ingest a resource tree or a single resource.
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="spawner">The spawner the resource tree belongs to.</param>
        /// <param name="parentResource">Null if a top level resource. Used by the method recursively to build the resource tree.</param>
        /// <param name="go">The object to be ingested as resource.</param>
        /// <param name="names">Reference to the nameset that's used to ensure unique resource names inside a prototype (May not be needed for resource trees).</param>
        /// <param name="protoBounds">Reference to Bounds for the whole prototype.</param>
        /// <param name="treeContainsPrefab">Does the tree contain a prefab?</param>
        /// <param name="structureIngestion"></param>
        /// <returns>Returns the top level resource of the tree.</returns>
        private void IngestResource(Prototype proto, GeNaSpawnerData spawner, Resource parentResource, GameObject go, ref HashSet<string> names, ref Bounds protoBounds, ref bool treeContainsPrefab, bool structureIngestion)
        {
            // Warn the user if it has more components than just the Transform since it's not a prefab.
            IDecorator[] decorators = go.GetComponents<IDecorator>();
            bool destroyUnpackedObject = false;
            foreach (IDecorator decorator in decorators)
            {
                if (decorator.UnpackPrefab)
                {
                    go = PrefabUnpackerUtility.ExecuteUnpackMasterGameObject(go);
                    destroyUnpackedObject = true;
                    break;
                }
                if (decorator is GeNaSubSpawnerDecorator)
                {
                    treeContainsPrefab = true;
                }
            }
            Resource res = new Resource(spawner);
            res.SetParent(parentResource);
            proto.AddResource(res);
            res.Name = GetUniqueName(go.name, ref names);
            // Get bounds
            Bounds localBounds = GeNaUtility.GetInstantiatedBounds(go);
            localBounds.size = Vector3.Max(localBounds.size, Vector3.one);
            Bounds localColliderBounds = GeNaUtility.GetLocalObjectBounds(go);
            // If first time then set bounds up
            if (protoBounds.size == Vector3.zero)
                protoBounds = new Bounds(localBounds.center, localBounds.size);
            // Otherwise expand on it
            else
                protoBounds.Encapsulate(localBounds);
            // Get colliders
            res.HasRootCollider = GeNaUtility.HasRootCollider(go);
            res.HasColliders = GeNaUtility.HasColliders(go);
            res.HasMeshes = GeNaUtility.HasMeshes(go);
            res.HasRigidbody = GeNaUtility.HasRigidBody(go);
            Vector3 localPosition = go.transform.localPosition;
            Vector3 localEulerAngles = go.transform.localEulerAngles;
            // If top level resource
            if (parentResource == null)
            {
                res.BasePosition = Vector3.zero;
                res.BaseRotation = Vector3.zero;
                // Top level is not static by default, but descendants are
                res.SetStatic(proto, Constants.ResourceStatic.Dynamic);
                Vector3 basePosition = res.BasePosition;
                if (structureIngestion)
                {
                    //Offsets
                    //Using global positions for x and z because the offsets for structure ingestion will be 
                    //calculated by global bounds center.
                    basePosition.x = basePosition.x = localPosition.x;
                    basePosition.y = basePosition.y = localPosition.y;
                    basePosition.z = basePosition.z = localPosition.z;
                }
                else
                {
                    //Offsets
                    //If importing a single proto, it gets no offset
                    basePosition = Vector3.zero;
                }
                res.BasePosition = basePosition;
            }
            else
            {
                res.BasePosition = localPosition;
                res.BaseRotation = localEulerAngles;
            }
            if (GeNaUtility.ApproximatelyEqual(go.transform.localScale.x, go.transform.localScale.y, 0.000001f) &&
                GeNaUtility.ApproximatelyEqual(go.transform.localScale.x, go.transform.localScale.z, 0.000001f))
                res.SameScale = true;
            else
                res.SameScale = false;
            Vector3 localScale = go.transform.localScale;
            res.MinScale = res.MaxScale = localScale;
            res.BaseScale = localScale;
            res.BaseSize = localBounds.size;
            res.BaseColliderCenter = localColliderBounds.center;
            res.BaseColliderScale = localColliderBounds.size;
            res.BoundsCenter = localBounds.center;
            if (GeNaEditorUtility.IsPrefab(go))
            {
                GameObject prefabAsset = GeNaEditorUtility.GetPrefabAsset(go);
                if (prefabAsset == null)
                    prefabAsset = go;
                res.AddPrefab(prefabAsset, m_spawner.Palette);
            }
            // This needs to be here so prefabs can get added
            foreach (IDecorator decorator in decorators)
            {
                decorator.OnIngest(res);
            }
            // We can only determine if it is a prefab in the editor
            if (GeNaEditorUtility.IsPrefab(go))
            {
                if (res.Prefab != null)
                {
                    //We got a prefab here
                    treeContainsPrefab = true;
                    //Get its asset ID
                    string path = AssetDatabase.GetAssetPath(res.Prefab);
                    if (!string.IsNullOrEmpty(path))
                    {
                        res.AssetID = AssetDatabase.AssetPathToGUID(path);
                        res.AssetName = GeNaEditorUtility.GetAssetName(path);
                    }

                    //Get flags
                    SpawnFlags spawnFlags = res.SpawnFlags;
                    StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(res.Prefab);
                    spawnFlags.FlagBatchingStatic = (flags & StaticEditorFlags.BatchingStatic) == StaticEditorFlags.BatchingStatic;
#if UNITY_5 || UNITY_2017 || UNITY_2018 || UNITY_2019_1
                    spawnFlags.FlagLightmapStatic = (flags & StaticEditorFlags.LightmapStatic) == StaticEditorFlags.LightmapStatic;
#else
                    spawnFlags.FlagLightmapStatic = (flags & StaticEditorFlags.ContributeGI) == StaticEditorFlags.ContributeGI;
#endif
                    spawnFlags.FlagNavigationStatic = (flags & StaticEditorFlags.NavigationStatic) == StaticEditorFlags.NavigationStatic;
                    spawnFlags.FlagOccludeeStatic = (flags & StaticEditorFlags.OccludeeStatic) == StaticEditorFlags.OccludeeStatic;
                    spawnFlags.FlagOccluderStatic = (flags & StaticEditorFlags.OccluderStatic) == StaticEditorFlags.OccluderStatic;
                    spawnFlags.FlagOffMeshLinkGeneration = (flags & StaticEditorFlags.OffMeshLinkGeneration) == StaticEditorFlags.OffMeshLinkGeneration;
                    spawnFlags.FlagReflectionProbeStatic = (flags & StaticEditorFlags.ReflectionProbeStatic) == StaticEditorFlags.ReflectionProbeStatic;
                }
                else
                    Debug.LogErrorFormat("Unable to get prefab for '{0}'", res.Name);
                if (go.transform.childCount < res.Prefab.transform.childCount)
                    Debug.LogErrorFormat("What's going on here? The Prefab Instance seems to have less childs than the Prefab Asset: {0} < {1}", go.transform.childCount, res.Prefab.transform.childCount);
                else
                {
                    foreach (Transform child in go.transform)
                    {
                        if (PrefabUtility.IsPartOfAnyPrefab(child.gameObject))
                            continue;
                        // This GO or Prefab is not part of the Prefab that's being ingested. Let's process it.
                        IngestResource(proto, spawner, res, child.gameObject, ref names, ref protoBounds, ref treeContainsPrefab, structureIngestion);
                    }
                }
            }
            // Else this is just a GO (container in the tree) not a prefab: Keep traversing the tree.
            else
            {
                res.ContainerOnly = true;
                foreach (IDecorator decorator in decorators)
                    res.AddDecoratorEntry(decorator);
                res.DeserializeDecorators();
                // Keep traversing the tree.
                if (go.transform.childCount > 0)
                    foreach (Transform child in go.transform)
                        IngestResource(proto, spawner, res, child.gameObject, ref names, ref protoBounds, ref treeContainsPrefab, structureIngestion);
            }
            if (destroyUnpackedObject)
            {
                GeNaEvents.Destroy(go);
            }
        }
        /// <summary>
        /// Draws a separator between secitons
        /// </summary>
        private void Separator(Rect widthRect)
        {
            GUILayout.Space(5f);
            Rect r = GUILayoutUtility.GetLastRect();
            Handles.BeginGUI();
            Color oldColor = Handles.color;
            Handles.color = m_separatorColor;
            Handles.DrawLine(new Vector3(widthRect.xMin, r.yMax), new Vector3(widthRect.xMax, r.yMax));
            Handles.color = oldColor;
            Handles.EndGUI();
        }
        /// <summary>
        /// Return the bounds of the supplied game object
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        private Bounds GetBounds(GameObject go)
        {
            Bounds bounds = new Bounds(go.transform.position, Vector3.zero);
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
                bounds.Encapsulate(r.bounds);
            foreach (Collider c in go.GetComponentsInChildren<Collider>())
                bounds.Encapsulate(c.bounds);
            return bounds;
        }
        public void AddGameObject(GeNaSpawnerData spawner, GameObject gameObject) => AddGameObjects(spawner, new List<GameObject>() {gameObject});
        /// <summary>
        /// Add the game object from a list of prefabs instantiated as game objects.
        /// </summary>
        /// <param name="spawner">Spawner to add GameObjects to</param>
        /// <param name="ingestList">The list of resources, resource trees to generate the prototypes from.</param>
        public void AddGameObjects(GeNaSpawnerData spawner, List<GameObject> ingestList)
        {
            if (ingestList == null || ingestList.Count < 1)
            {
                Debug.LogWarning("Can't add null or empty resource list");
                return;
            }
            if (spawner == null)
            {
                Debug.LogWarning("Can't add resources because spawner is missing");
                return;
            }
            bool ingestStructure = false;
            if (ingestList.Count > 1)
            {
                // Ask the user if they want to import a structure or just multiple individual items
                if (EditorUtility.DisplayDialog("GeNa Ingestion",
                    m_editorUtils.GetTextValue("Structure Ingestion Dialog Text"),
                    "Yes",
                    "No"))
                {
                    ingestStructure = true;
                    // Automatically set type to structured
                    spawner.SpawnType = Constants.SpawnerType.Random;
                    m_placementCriteria.RotationAlgorithm = Constants.RotationAlgorithm.Fixed;
                    m_placementCriteria.MaxRotationY = m_placementCriteria.MinRotationY = 0f;
                }
            }
            Bounds globalBounds = new Bounds();
            // Ingest prototypes
            foreach (GameObject go in ingestList)
            {
                // Used to track unique names in a prototype
                HashSet<string> names = new HashSet<string>();
                // Now add in the resource tree
                Bounds protoBounds = new Bounds();
                bool treeContainsPrefab = false;
                // Create and add the prototype
                Prototype proto = new Prototype(spawner)
                {
                    Name = go.name,
                    Size = protoBounds.size,
                    Extents = protoBounds.size * .5f,
                    ForwardRotation = 0f
                };
                proto.SetSpawner(m_spawner.SpawnerData);
                proto.SetPalette(m_spawner.Palette);
                // Ingest Resource
                IngestResource(proto, spawner, null, go, ref names, ref protoBounds, ref treeContainsPrefab, ingestStructure);
                // if (treeContainsPrefab == false)
                // {
                //     Debug.LogWarningFormat("{0} contains no prefab and was ignored.", go.name);
                //     continue;
                // }
                // Add proto
                m_spawner.AddProto(proto);
                // If ingested several things (a structure), we want to set their m_position offset relative to the center of their collective bounds
                if (ingestStructure)
                {
                    Resource res = proto.GetResources()[0];
                    // Proto bounds are world origin based, let's adjust
                    protoBounds.center = new Vector3(protoBounds.center.x + res.MinOffset.x,
                        protoBounds.center.y + res.MinOffset.y,
                        protoBounds.center.z + res.MinOffset.z);
                    // If first time then set bounds up
                    if (globalBounds.size == Vector3.zero)
                        globalBounds = new Bounds(protoBounds.center, protoBounds.size);
                    // Otherwise expand on it
                    else
                        globalBounds.Encapsulate(protoBounds);
                }

                // If first one, then update some settings to be more prefab friendly
                if (spawner.SpawnPrototypes.Count == 1)
                {
                    // Activate bounds checking
                    m_spawnCriteria.CheckCollisionType = Constants.VirginCheckType.Bounds;
                    m_placementCriteria.ScaleToNearestInt = false;
                    //m_spawner.ThrowDistance = Mathf.Min(proto.Size.x, proto.Size.z) * 2f;
                }
            }
            // If ingested several things, we want to set their m_position offset relative to the center of their collective bounds
            if (ingestStructure)
            {
                // Then process each resource
                foreach (Resource res in spawner.SpawnPrototypes.Select(proto => proto.GetResources()[0]))
                {
                    Vector3 basePosition = res.BasePosition;
                    basePosition.x = basePosition.x - globalBounds.center.x;
                    basePosition.z = basePosition.z - globalBounds.center.z;
                    res.BasePosition = basePosition;
                }
            }
            // Mark the Spawner as Dirty
            m_spawner.IsDirty = true;
            EditorUtility.SetDirty(m_spawner.Palette);
        }
        #endregion
        #region DropDown Menues
        private void ConformMenu()
        {
            int selection = 0;
            string[] optionsKeys = new[] {"Conform Dropdown", "Conform All", "Conform None"};
            selection = m_editorUtils.Popup(selection, optionsKeys, GUILayout.Width(70f));
            if (selection != 0)
            {
                switch (selection)
                {
                    case 1:
                        m_spawner.ForEachProtoResource(resource => resource.ConformToSlope = true);
                        break;
                    case 2:
                        m_spawner.ForEachProtoResource(resource => resource.ConformToSlope = false);
                        break;
                    default:
                        throw new NotImplementedException("[GeNa] No idea what was selected here: " + selection);
                }
                GUI.changed = true;
            }
        }
        private void SnapToGroundMenu()
        {
            int selection = 0;
            string[] optionsKeys = new[] {"Snap Dropdown", "Snap All", "Snap None"};
            selection = m_editorUtils.Popup(selection, optionsKeys);
            if (selection != 0)
            {
                switch (selection)
                {
                    case 1:
                        m_spawner.ForEachProtoResource(resource => resource.SnapToGround = true);
                        break;
                    case 2:
                        m_spawner.ForEachProtoResource(resource => resource.SnapToGround = false);
                        break;
                    default:
                        throw new NotImplementedException("[GeNa] No idea what was selected here: " + selection);
                }
                GUI.changed = true;
            }
        }
        #endregion
        #region Panels
        private void QuickStartPanel(bool helpEnabled)
        {
            if (ActiveEditorTracker.sharedTracker.isLocked)
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("Inspector locked warning"), MessageType.Warning);
            if (m_settings.Advanced.ShowDetailedHelp)
            {
                m_editorUtils.Label("Visualise Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Visualise Cursor Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Rotation Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Range Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Instances Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Move Last Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Height&Rot Last Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Delete All Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Single Spawn Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Global Spawn Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Iterate Help", m_editorUtils.Styles.help);
            }
            else
                EditorGUILayout.LabelField("Visualise: Shift + Left click.\nSingle Spawn: Ctrl + Left click.\nGlobal Spawn: Ctrl + Shift + Left click.", Styles.wrappedText);
            if (m_editorUtils.Button("View Tutorials Btn"))
                Application.OpenURL(PWApp.CONF.TutorialsLink);
        }
        /// <summary>
        /// Overview Panel
        /// </summary>
        private void OverviewPanel(bool helpEnabled)
        {
            float spawnRange = m_spawnerData.SpawnRange;
            m_editorUtils.InlineHelp("Overview Panel", helpEnabled);
            var data = m_spawner.GetDataBuffer();
            GUI.enabled = data.IsTemp && GeNaEditorUtility.IsPrefab(m_spawner.gameObject);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUIContent saveToPrefabContent = new GUIContent("Save Changes");
                    Color oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button(saveToPrefabContent, Styles.cancelBtn, GUILayout.MaxHeight(25f)))
                    {
                        SaveChanges(m_spawner);
                    }
                    GUI.backgroundColor = oldColor;
                }
                {
                    GUIContent revertToPrefab = new GUIContent("Discard Changes");
                    Color oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button(revertToPrefab, Styles.cancelBtn, GUILayout.MaxHeight(25f)))
                    {
                        DiscardChanges(m_spawner);
                    }
                    GUI.backgroundColor = oldColor;
                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("Spawner Changes", helpEnabled);
            }
            GUI.enabled = true;
            if (!GeNaManager.GetInstance().Cancel)
            {
                if (GeNaEditorUtility.ValidateComputeShader())
                {
                    GUI.enabled = true;
                }
                GUIContent cancelContent = new GUIContent("\u00D7 Cancel");
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button(cancelContent, Styles.cancelBtn, GUILayout.MaxHeight(25f)))
                {
                    GeNaGlobalReferences.GeNaManagerInstance.Cancel = true;
                    //GUIUtility.ExitGUI();
                }
                GUI.backgroundColor = oldColor;
                GUI.enabled = false;
            }
            #region Undo
            // if (m_spawnerData.CanPerformUndo)
            // {
            bool undoEmpty = true;
            string undoBtnLabel = " (-)";
            // Undo 
            UndoRecord[] undoArray = m_spawnerData.UndoArrayCopy;
            GUIContent[] undoLabelsArray = new GUIContent[undoArray.Length + 1];
            undoLabelsArray[0] = m_editorUtils.GetContent("Undo List");
            undoBtnLabel = string.Format(" ({0}/{1})", undoArray.Length, m_spawnerData.UndoSteps);
            if (undoArray.Length > 0)
            {
                undoEmpty = false;
                int groupingLimitSeconds = Preferences.UndoGroupingTime;
                int lastTime = 0;
                int burstIndex = 0;
                for (int i = 0; i < undoArray.Length; i++)
                {
                    if (lastTime - undoArray[i].Time > groupingLimitSeconds)
                        burstIndex++;
                    undoLabelsArray[i + 1] =
                        new GUIContent(
                            string.Format("{0}.  ({1}) [{2}] {3}", i + 1, burstIndex + 1,
                                GetTimeDelta(), undoArray[i].Description), "Undo");
                    lastTime = undoArray[i].Time;
                }
            }
            // If the Object is not Persistent
            if (EditorUtility.IsPersistent(m_spawner) == false)
            {
                // Draw Undo Section
                EditorGUI.BeginDisabledGroup(undoEmpty);
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(
                            new GUIContent(" Undo All", Styles.undoIco, m_editorUtils.GetTooltip("Undo")),
                            Styles.richButtonMini, GUILayout.MaxHeight(15f)))
                        {
                            if (EditorUtility.DisplayDialog("Undo Operation",
                                string.Format("Are you sure you want to Undo all [{0}] changes?", undoArray.Length),
                                "OK", "Cancel"))
                            {
                                m_spawner.UndoAll();
                            }
                            GUIUtility.ExitGUI();
                        }
                        if (GUILayout.Button(
                            new GUIContent(undoBtnLabel, Styles.undoIco, m_editorUtils.GetTooltip("Undo")),
                            Styles.richButtonMini, GUILayout.MaxHeight(15f)))
                        {
                            m_spawner.Undo();
                        }
                        m_undoSteps = EditorGUILayout.Popup(m_undoSteps, undoLabelsArray);
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();
            }
            m_editorUtils.InlineHelp("Undo", helpEnabled);
            #endregion
            // }
            EditorGUI.BeginChangeCheck();
            {
                Constants.SpawnMode spawnMode = (Constants.SpawnMode) m_spawnerData.SpawnMode;
                // Controls
                if (m_settings.Advanced.DebugEnabled)
                {
                    EditorGUILayout.LabelField($"Version: {m_spawner.VersionNumber}");
                }
                EditorGUILayout.BeginHorizontal();
                m_spawner.Palette = (Palette) m_editorUtils.ObjectField("Palette", m_spawner.Palette, typeof(Palette), false);
                if (m_editorUtils.Button("NewPalette", GUILayout.MaxWidth(40f)))
                {
                    m_spawner.Palette = CreatePalette();
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("Palette", helpEnabled);
                if (m_spawner.Palette != null)
                {
                    //m_spawner.SpawnerData = (GeNaSpawnerData) EditorGUILayout.ObjectField("Spawner Data", m_spawner.SpawnerData, typeof(GeNaSpawnerData), false);
                    m_spawnerData.Name = m_editorUtils.TextField("Spawner Name", m_spawnerData.Name, helpEnabled);
                    m_spawnerData.SpawnType = (Constants.SpawnerType) m_editorUtils.EnumPopup("Spawner Type", m_spawnerData.SpawnType, helpEnabled);
                    //m_editorUtils.PropertyField("Physics Type", m_physicsModeProperty, helpEnabled);
                    if (m_spawnerData.HasActivePhysicsProtosRecursive())
                    {
                        m_spawnerData.PhysicsType = (Constants.PhysicsType) m_editorUtils.EnumPopup("Physics Type", m_spawnerData.PhysicsType, helpEnabled);
                        if (m_spawnerData.PhysicsType == Constants.PhysicsType.Spawner)
                        {
                            EditorGUI.indentLevel++;
                            PhysicsSimulatorSettings physicsSettings = m_spawnerData.PhysicsSettings;
                            physicsSettings.Iterations = m_editorUtils.IntField("Iterations", physicsSettings.Iterations, helpEnabled);
                            physicsSettings.StepSize = m_editorUtils.Slider("Step Size", physicsSettings.StepSize, 0.01f, 0.1f, helpEnabled);
                            physicsSettings.SpawnOffsetY = m_editorUtils.Slider("Spawn Offset Y", physicsSettings.SpawnOffsetY, -5f, 5f, helpEnabled);
                            EditorGUI.indentLevel--;
                        }
                    }
                    m_spawnerData.SpawnMode = (Constants.SpawnMode) m_editorUtils.EnumPopup("Spawn Mode", m_spawnerData.SpawnMode, helpEnabled);
                    if (!m_spawnerData.UseLargeRanges)
                    {
                        if ((Constants.SpawnMode) m_spawnerData.SpawnMode > Constants.SpawnMode.Single)
                        {
                            EditorGUI.indentLevel += 1;
                            m_spawnerData.FlowRate = m_editorUtils.Slider("Flow Rate", m_spawnerData.FlowRate, 0.01f * spawnRange, 5f * spawnRange, helpEnabled);
                            EditorGUI.indentLevel -= 1;
                        }
                        m_spawnerData.SpawnRangeShape = (Constants.SpawnRangeShape) m_editorUtils.EnumPopup("Spawn Shape", m_spawnerData.SpawnRangeShape, helpEnabled);
                        m_spawnerData.RandomSeed = m_editorUtils.IntField("Spawn Seed", m_spawnerData.RandomSeed, helpEnabled);
                        spawnRange = m_editorUtils.Slider("Spawn Range", spawnRange, 0f, 200f, helpEnabled);
                        m_spawnerData.ThrowDistance = m_editorUtils.Slider("Throw Distance", m_spawnerData.ThrowDistance, 0f, spawnRange, helpEnabled);
                        if (m_spawnerData.ThrowDistance > 0)
                        {
                            int instanceLimit = m_instanceTopLimit;
                            float minInstances = (float) m_spawnerData.MinInstances;
                            float maxInstances = (float) m_spawnerData.MaxInstances;
                            m_editorUtils.MinMaxSliderWithFields("Instances", ref minInstances, ref maxInstances, 1, instanceLimit, helpEnabled);
                            int minInstancesInt = Mathf.RoundToInt(minInstances);
                            int maxInstancesInt = Mathf.RoundToInt(maxInstances);
                            // Min Instances Changed
                            if (m_spawnerData.MinInstances != minInstancesInt)
                            {
                                if (minInstances > m_spawnerData.MaxInstances)
                                    m_spawnerData.MaxInstances = minInstancesInt;
                                m_spawnerData.MinInstances = Mathf.RoundToInt(minInstances);
                            }
                            // Max Instances Changed
                            if (m_spawnerData.MaxInstances != maxInstancesInt)
                            {
                                if (maxInstances < m_spawnerData.MinInstances)
                                    m_spawnerData.MinInstances = maxInstancesInt;
                                m_spawnerData.MaxInstances = Mathf.RoundToInt(maxInstances);
                            }
                        }
                    }
                    else
                    {
                        if (spawnMode > Constants.SpawnMode.Single)
                        {
                            EditorGUI.indentLevel += 1;
                            m_spawnerData.FlowRate = m_editorUtils.FloatField("Flow Rate", m_spawnerData.FlowRate, helpEnabled);
                            EditorGUI.indentLevel -= 1;
                        }
                        m_spawnerData.SpawnRangeShape = (Constants.SpawnRangeShape) m_editorUtils.EnumPopup("Spawn Shape", m_spawnerData.SpawnRangeShape, helpEnabled);
                        m_spawnerData.RandomSeed = m_editorUtils.IntField("Spawn Seed", m_spawnerData.RandomSeed, helpEnabled);
                        spawnRange = m_editorUtils.FloatField("Spawn Range", spawnRange, helpEnabled);
                        m_spawnerData.ThrowDistance = m_editorUtils.FloatField("Throw Distance", m_spawnerData.ThrowDistance, helpEnabled);
                        if (m_spawnerData.ThrowDistance > 0)
                        {
                            m_spawnerData.MinInstances = m_editorUtils.LongField("Min Instances", m_spawnerData.MinInstances, helpEnabled);
                            m_spawnerData.MaxInstances = m_editorUtils.LongField("Max Instances", m_spawnerData.MaxInstances, helpEnabled);
                        }
                    }
                    if (m_hasPrefabs)
                    {
                        m_spawnerData.MergeSpawns = m_editorUtils.Toggle("Merge Instances", m_spawnerData.MergeSpawns, helpEnabled);
                    }
                    m_spawnerData.SpawnTimeInterval = m_editorUtils.Slider("Spawn Time Interval", m_spawnerData.SpawnTimeInterval, 0f, 1000f, helpEnabled);
                    m_spawnerData.UseLargeRanges = m_editorUtils.Toggle("Use Large Ranges", m_spawnerData.UseLargeRanges, helpEnabled);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_spawnerData.SpawnRange = spawnRange;
                m_spawner.IsDirty = true;
            }
        }
        private void PlacementCritPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                PlacementCriteria placementCriteria = m_spawnerData.PlacementCriteria;
                Constants.LocationAlgorithm spawnAlgorithm = placementCriteria.SpawnAlgorithm;
                int maxFailCount = placementCriteria.MaxFailCount;
                AnimationCurve scaleFalloff = placementCriteria.ScaleFalloff;
                float seedThrowJitter = placementCriteria.SeedThrowJitter;
                Constants.RotationAlgorithm rotationAlgorithm = placementCriteria.RotationAlgorithm;
                float minRotationY = placementCriteria.MinRotationY;
                float maxRotationY = placementCriteria.MaxRotationY;
                bool enableRotationDragUpdate = placementCriteria.EnableRotationDragUpdate;
                m_editorUtils.Label("Control how and where we can spawn.", Styles.wrappedText);
                spawnAlgorithm = (Constants.LocationAlgorithm) m_editorUtils.EnumPopup("Spawn Type", spawnAlgorithm, helpEnabled);
                EditorGUI.indentLevel++;
                switch (spawnAlgorithm)
                {
                    case Constants.LocationAlgorithm.Every:
                    {
                        seedThrowJitter = m_editorUtils.Slider("Jitter Strength", seedThrowJitter, 0f, 1f, helpEnabled);
                        break;
                    }
                    case Constants.LocationAlgorithm.Organic:
                    {
                        maxFailCount = m_editorUtils.IntField("Max Fail Count", maxFailCount, helpEnabled);
                        scaleFalloff = m_editorUtils.CurveField("Scale Falloff", scaleFalloff, helpEnabled);
                        break;
                    }
                }
                EditorGUI.indentLevel--;
                if (m_hasPrefabs || m_hasTrees || (m_spawnCriteria.CheckMask && m_spawnCriteria.CheckMaskType == Constants.MaskType.Image))
                {
                    rotationAlgorithm = (Constants.RotationAlgorithm) m_editorUtils.EnumPopup("Rotation Type", rotationAlgorithm, helpEnabled);
                    switch (rotationAlgorithm)
                    {
                        case Constants.RotationAlgorithm.Ranged:
                            EditorGUI.indentLevel++;
                            m_editorUtils.MinMaxSliderWithFields("Rotation", ref minRotationY, ref maxRotationY, 0f, 360f, helpEnabled);
                            EditorGUI.indentLevel--;
                            break;
                        case Constants.RotationAlgorithm.Fixed:
                            EditorGUI.indentLevel++;
                            minRotationY = m_editorUtils.Slider("Fixed Rotation", minRotationY, 0f, 360f, helpEnabled);
                            enableRotationDragUpdate = m_editorUtils.Toggle("Draggable Rotation", enableRotationDragUpdate, helpEnabled);
                            EditorGUI.indentLevel--;
                            break;
                        case Constants.RotationAlgorithm.LastSpawnCenter:
                            break;
                        case Constants.RotationAlgorithm.LastSpawnClosest:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    PlacementScale(m_placementCriteria, helpEnabled);
                }
                else if (m_hasTextures)
                    PlacementScale(m_placementCriteria, helpEnabled);
                placementCriteria.SpawnAlgorithm = spawnAlgorithm;
                placementCriteria.MaxFailCount = maxFailCount;
                placementCriteria.ScaleFalloff = scaleFalloff;
                placementCriteria.SeedThrowJitter = seedThrowJitter;
                placementCriteria.RotationAlgorithm = rotationAlgorithm;
                placementCriteria.MinRotationY = minRotationY;
                placementCriteria.MaxRotationY = maxRotationY;
                placementCriteria.EnableRotationDragUpdate = enableRotationDragUpdate;
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_spawner.IsDirty = true;
            }
        }
        private void PlacementScale(PlacementCriteria placementCriteria, bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                bool sameScale = placementCriteria.SameScale;
                bool scaleToNearestInt = placementCriteria.ScaleToNearestInt;
                Vector3 minScale = placementCriteria.MinScale;
                Vector3 maxScale = placementCriteria.MaxScale;
                sameScale = m_editorUtils.Toggle("Same Scale XYZ", sameScale, helpEnabled);
                EditorGUI.indentLevel++;
                if (sameScale)
                {
                    if (scaleToNearestInt)
                    {
                        int min = (int) minScale.x;
                        int max = (int) maxScale.x;
                        m_editorUtils.MinMaxSliderWithFields("Scale", ref min, ref max, 1, 100, helpEnabled);
                        minScale.x = min;
                        maxScale.x = max;
                    }
                    else
                    {
                        m_editorUtils.MinMaxSliderWithFields("Scale", ref minScale.x, ref maxScale.x, 0.1f, 100f, helpEnabled);
                    }
                }
                else
                {
                    if (scaleToNearestInt)
                    {
                        minScale.x = m_editorUtils.IntSlider("Min Scale X", (int) minScale.x, 1, 1000, helpEnabled);
                        maxScale.x = m_editorUtils.IntSlider("Max Scale X", (int) maxScale.x, 1, 1000, helpEnabled);
                        minScale.y = m_editorUtils.IntSlider("Min Scale Y", (int) minScale.y, 1, 1000, helpEnabled);
                        maxScale.y = m_editorUtils.IntSlider("Max Scale Y", (int) maxScale.y, 1, 1000, helpEnabled);
                        minScale.z = m_editorUtils.IntSlider("Min Scale Z", (int) minScale.z, 1, 1000, helpEnabled);
                        maxScale.z = m_editorUtils.IntSlider("Max Scale Z", (int) maxScale.z, 1, 1000, helpEnabled);
                    }
                    else
                    {
                        minScale.x = m_editorUtils.Slider("Min Scale X", minScale.x, 0.1f, 1000f, helpEnabled);
                        maxScale.x = m_editorUtils.Slider("Max Scale X", maxScale.x, 0.1f, 1000f, helpEnabled);
                        minScale.y = m_editorUtils.Slider("Min Scale Y", minScale.y, 0.1f, 1000f, helpEnabled);
                        maxScale.y = m_editorUtils.Slider("Max Scale Y", maxScale.y, 0.1f, 1000f, helpEnabled);
                        minScale.z = m_editorUtils.Slider("Min Scale Z", minScale.z, 0.1f, 1000f, helpEnabled);
                        maxScale.z = m_editorUtils.Slider("Max Scale Z", maxScale.z, 0.1f, 1000f, helpEnabled);
                    }
                }
                placementCriteria.SameScale = sameScale;
                placementCriteria.ScaleToNearestInt = scaleToNearestInt;
                placementCriteria.MinScale = minScale;
                placementCriteria.MaxScale = maxScale;
                EditorGUI.indentLevel--;
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_spawner.IsDirty = true;
            }
        }
        private void SpawnCritPanel(bool helpEnabled)
        {
            SpawnCriteria spawnCriteria = m_spawnerData.SpawnCriteria;
            bool forceSpawn = spawnCriteria.ForceSpawn;
            LayerMask groundLayer = m_spawnerData.GroundLayer;
            LayerMask spawnCollisionLayers = spawnCriteria.SpawnCollisionLayers;
            Constants.VirginCheckType virginCheckType = spawnCriteria.CheckCollisionType;
            float blendAmount = spawnCriteria.BlendAmount;
            float boundsExtents = spawnCriteria.BoundsExtents;
            Constants.CriteriaRangeType checkHeightType = spawnCriteria.CheckHeightType;
            bool seaLevel = m_spawnerData.GetSeaLevel;
            float extraSeaLevel = m_spawnerData.ExtraSeaLevelHeight;
            float minSpawnHeight = spawnCriteria.MinSpawnHeight;
            float maxSpawnHeight = spawnCriteria.MaxSpawnHeight;
            float minHeight = spawnCriteria.MinHeight;
            float maxHeight = spawnCriteria.MaxHeight;
            float heightVariance = spawnCriteria.HeightRange;
            float bottomBoundary = spawnCriteria.BottomBoundary;
            float topBoundary = spawnCriteria.TopBoundary;
            // Check Slopes
            Constants.CriteriaRangeType checkSlopeTypeProperty = spawnCriteria.CheckSlopeType;
            float minSpawnSlopeProperty = spawnCriteria.MinSpawnSlope;
            float maxSpawnSlopeProperty = spawnCriteria.MaxSpawnSlope;
            float minSlopeProperty = spawnCriteria.MinSlope;
            float maxSlopeProperty = spawnCriteria.MaxSlope;
            float slopeRangeProperty = spawnCriteria.SlopeRange;
            // Check Textures
            bool checkTextures = spawnCriteria.CheckTextures;
            int selectedTextureIdxProperty = spawnCriteria.SelectedTextureIdx;
            string selectedTextureNameProperty = spawnCriteria.SelectedTextureName;
            float textureStrengthProperty = spawnCriteria.TextureStrength;
            float textureVarianceProperty = spawnCriteria.TextureRange;
            // Check Mask
            bool checkMask = spawnCriteria.CheckMask;
            Constants.MaskType maskType = spawnCriteria.CheckMaskType;
            // Mask Fractal
            Fractal maskFractal = spawnCriteria.MaskFractal;
            float maskFractalSeed = maskFractal.Seed;
            int maskFractalOctaves = maskFractal.Octaves;
            float maskFractalFrequency = maskFractal.Frequency;
            float maskFractalPersistence = maskFractal.Persistence;
            float maskFractalLacunarity = maskFractal.Lacunarity;
            float midMaskFractal = spawnCriteria.MidMaskFractal;
            float maskFractalRange = spawnCriteria.MaskFractalRange;
            bool maskInvert = spawnCriteria.MaskInvert;
            Texture2D maskImage = spawnCriteria.MaskImage;
            Color imageFilterColor = spawnCriteria.ImageFilterColor;
            float imageFilterFuzzyMatch = spawnCriteria.ImageFilterFuzzyMatch;
            bool constrainWithinMaskedBounds = spawnCriteria.ConstrainWithinMaskedBounds;
            bool invertMaskedAlpha = spawnCriteria.InvertMaskedAlpha;
            bool successOnMaskedAlpha = spawnCriteria.SuccessOnMaskedAlpha;
            bool scaleOnMaskedAlpha = spawnCriteria.ScaleOnMaskedAlpha;
            float minScaleOnMaskedAlpha = spawnCriteria.MinScaleOnMaskedAlpha;
            float maxScaleOnMaskedAlpha = spawnCriteria.MaxScaleOnMaskedAlpha;
            m_editorUtils.Label("Control when we can spawn.", Styles.wrappedText);
            EditorGUI.BeginChangeCheck();
            {
                forceSpawn = m_editorUtils.Toggle("Force Spawn", forceSpawn, helpEnabled);
                if (GeNaEditorUtility.ValidateComputeShader())
                {
                    if (!m_spawnerData.IsProcessing)
                        GUI.enabled = !forceSpawn;
                }
                #region Check Collisions
                groundLayer = m_editorUtils.LayerMaskField("Ground Layer", groundLayer, helpEnabled);
                virginCheckType = (Constants.VirginCheckType) m_editorUtils.EnumPopup("Check Collisions", virginCheckType, helpEnabled);
                if (virginCheckType != Constants.VirginCheckType.None)
                {
                    EditorGUI.indentLevel++;
                    spawnCollisionLayers = m_editorUtils.LayerMaskField("Collision Layers", spawnCollisionLayers, helpEnabled);
                    EditorGUI.indentLevel--;
                    if (virginCheckType == Constants.VirginCheckType.Bounds)
                    {
                        EditorGUI.indentLevel++;
                        blendAmount = m_editorUtils.Slider("Blend Amount", blendAmount, 0.001f, 10f, helpEnabled);
                        boundsExtents = m_editorUtils.Slider("Bounds Extents", boundsExtents, 0.0f, 100f, helpEnabled);
                        EditorGUI.indentLevel--;
                    }
                    m_editorUtils.InlineHelp("Collision Layers", helpEnabled);
                }
                #endregion
                #region Check Height Type
                // Height
                checkHeightType = (Constants.CriteriaRangeType) m_editorUtils.EnumPopup("Check Height Type", checkHeightType, helpEnabled);
                if (checkHeightType != Constants.CriteriaRangeType.None)
                {
                    EditorGUI.indentLevel++;
                    seaLevel = m_editorUtils.Toggle("SeaLevel", seaLevel, helpEnabled);
                    if (seaLevel)
                    {
                        EditorGUI.indentLevel++;
                        extraSeaLevel = m_editorUtils.FloatField("ExtraSeaLevel", extraSeaLevel);
                        EditorGUI.indentLevel--;
                    }
                    if (m_spawnerData.UseLargeRanges)
                    {
                        switch (checkHeightType)
                        {
                            case Constants.CriteriaRangeType.Range:
                            case Constants.CriteriaRangeType.MinMax:
                                if (GeNaUtility.Gaia2Present)
                                {
                                    if (seaLevel)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        m_editorUtils.LabelField("Min Height", GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 15f));
                                        EditorGUILayout.LabelField(spawnCriteria.MinSpawnHeight.ToString(CultureInfo.InvariantCulture), GUILayout.MaxWidth(EditorGUIUtility.fieldWidth));
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    else
                                        minSpawnHeight = m_editorUtils.FloatField("Min Height", minSpawnHeight, helpEnabled);
                                }
                                else
                                    minSpawnHeight = m_editorUtils.FloatField("Min Height", minSpawnHeight, helpEnabled);
                                maxSpawnHeight = m_editorUtils.FloatField("Max Height", maxSpawnHeight, helpEnabled);
                                break;
                            default:
                                m_editorUtils.LabelField("Min Height", new GUIContent(minSpawnHeight.ToString(CultureInfo.InvariantCulture)), helpEnabled);
                                m_editorUtils.LabelField("Max Height", new GUIContent(maxSpawnHeight.ToString(CultureInfo.InvariantCulture)), helpEnabled);
                                break;
                        }
                        switch (checkHeightType)
                        {
                            case Constants.CriteriaRangeType.Range:
                            case Constants.CriteriaRangeType.Mixed:
                                heightVariance = m_editorUtils.FloatField("Height Range", heightVariance, helpEnabled);
                                break;
                        }
                    }
                    else
                    {
                        float minValue = minHeight;
                        float maxValue = maxHeight;
                        float minSpawnValue = minSpawnHeight;
                        float maxSpawnValue = maxSpawnHeight;
                        float minLimit = bottomBoundary;
                        float maxLimit = topBoundary;
                        if (!m_spawnerData.IsProcessing)
                            if (GeNaEditorUtility.ValidateComputeShader())
                            {
                                GUI.enabled = !forceSpawn && checkHeightType >= Constants.CriteriaRangeType.MinMax;
                            }
                        if (checkHeightType >= Constants.CriteriaRangeType.MinMax)
                            m_editorUtils.MinMaxSliderWithFields("Min Max Spawn Height", ref minSpawnValue, ref maxSpawnValue, minLimit, maxLimit, helpEnabled);
                        if (checkHeightType != Constants.CriteriaRangeType.MinMax)
                        {
                            GUI.enabled = false;
                            Color oldColor = GUI.color;
                            if (checkHeightType == Constants.CriteriaRangeType.Mixed)
                            {
                                if (maxSpawnValue <= minValue || minSpawnValue >= maxValue)
                                    GUI.color = Color.red;
                                m_editorUtils.MinMaxSliderWithFields("Min Max Height", ref minValue, ref maxValue, minLimit, maxLimit, helpEnabled);
                            }
                            else
                                m_editorUtils.MinMaxSliderWithFields("Min Max Spawn Height", ref minSpawnValue, ref maxSpawnValue, minLimit, maxLimit, helpEnabled);
                            GUI.color = oldColor;
                        }
                        if (!m_spawnerData.IsProcessing)
                            if (GeNaEditorUtility.ValidateComputeShader())
                            {
                                GUI.enabled = !forceSpawn;
                            }
                        switch (checkHeightType)
                        {
                            case Constants.CriteriaRangeType.Range:
                            case Constants.CriteriaRangeType.Mixed:
                                heightVariance = m_editorUtils.Slider("Height Range", heightVariance, 0.1f, 200f, helpEnabled);
                                break;
                        }
                        minHeight = minValue;
                        maxHeight = maxValue;
                        minSpawnHeight = minSpawnValue;
                        maxSpawnHeight = maxSpawnValue;
                        bottomBoundary = minLimit;
                        topBoundary = maxLimit;
                    }
                    m_settings.ShowCritMinSpawnHeight = m_settings.ShowCritMaxSpawnHeight = m_editorUtils.Toggle("Visualize", m_settings.ShowCritMinSpawnHeight, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                #endregion
                #region Check Slope
                // Check Slope
                checkSlopeTypeProperty = (Constants.CriteriaRangeType) m_editorUtils.EnumPopup("Check Slope Type", checkSlopeTypeProperty, helpEnabled);
                if (checkSlopeTypeProperty != Constants.CriteriaRangeType.None)
                {
                    EditorGUI.indentLevel++;
                    float minValue = minSlopeProperty;
                    float maxValue = maxSlopeProperty;
                    float minSpawnValue = minSpawnSlopeProperty;
                    float maxSpawnValue = maxSpawnSlopeProperty;
                    float minLimit = 0f;
                    float maxLimit = 90f;
                    if (!m_spawnerData.IsProcessing)
                        if (GeNaEditorUtility.ValidateComputeShader())
                        {
                            GUI.enabled = !forceSpawn && checkSlopeTypeProperty >= Constants.CriteriaRangeType.MinMax;
                        }
                    if (checkSlopeTypeProperty >= Constants.CriteriaRangeType.MinMax)
                        m_editorUtils.MinMaxSliderWithFields("Min Max Slope", ref minValue, ref maxValue, minLimit, maxLimit, helpEnabled);
                    if (checkSlopeTypeProperty != Constants.CriteriaRangeType.MinMax)
                    {
                        GUI.enabled = false;
                        Color oldColor = GUI.color;
                        if (checkSlopeTypeProperty == Constants.CriteriaRangeType.Mixed)
                            if (maxSpawnValue <= minValue || minSpawnValue >= maxValue)
                                GUI.color = Color.red;
                        m_editorUtils.MinMaxSliderWithFields("Min Max Spawn Slope", ref minSpawnValue, ref maxSpawnValue, minLimit, maxLimit, helpEnabled);
                        GUI.color = oldColor;
                    }
                    if (!m_spawnerData.IsProcessing)
                        if (GeNaEditorUtility.ValidateComputeShader())
                        {
                            GUI.enabled = !forceSpawn;
                        }
                    minSlopeProperty = minValue;
                    maxSlopeProperty = maxValue;
                    minSpawnSlopeProperty = minSpawnValue;
                    maxSpawnSlopeProperty = maxSpawnValue;
                    switch (checkSlopeTypeProperty)
                    {
                        case Constants.CriteriaRangeType.Range:
                        case Constants.CriteriaRangeType.Mixed:
                            slopeRangeProperty = m_editorUtils.Slider("Slope Range", slopeRangeProperty, 0.1f, 90f, helpEnabled);
                            break;
                    }
                    EditorGUI.indentLevel--;
                    minSlopeProperty = minValue;
                    maxSlopeProperty = maxValue;
                    minSpawnSlopeProperty = minSpawnValue;
                    maxSpawnSlopeProperty = maxSpawnValue;
                }
                #endregion
                if (m_spawnerData.SpawnOriginIsTerrain)
                {
                    #region Check Textures
                    checkTextures = m_editorUtils.Toggle("Check Textures", checkTextures, helpEnabled);
                    if (checkTextures)
                    {
                        EditorGUI.indentLevel++;
                        Terrain terrain = Terrain.activeTerrain;
                        if (terrain != null)
                        {
                            GUIContent[] assetChoices = new GUIContent[terrain.terrainData.alphamapLayers];
                            for (int assetIdx = 0; assetIdx < assetChoices.Length; assetIdx++)
                                assetChoices[assetIdx] = new GUIContent(terrain.terrainData.terrainLayers[assetIdx].diffuseTexture.name);
                            EditorGUI.BeginChangeCheck();
                            selectedTextureIdxProperty = m_editorUtils.Popup("Texture", selectedTextureIdxProperty, assetChoices, helpEnabled);
                            if (EditorGUI.EndChangeCheck())
                            {
                                string name = terrain.terrainData.terrainLayers[selectedTextureIdxProperty].diffuseTexture.name;
                                selectedTextureNameProperty = name;
                            }
                        }
                        textureStrengthProperty = m_editorUtils.Slider("Texture Strength", textureStrengthProperty, 0f, 1f, helpEnabled);
                        textureVarianceProperty = m_editorUtils.Slider("Texture Range", textureVarianceProperty, 0f, 1f, helpEnabled);
                        EditorGUI.indentLevel--;
                    }
                    #endregion
                    #region Check Mask
                    // Check Mask
                    checkMask = m_editorUtils.Toggle("Check Mask", checkMask, helpEnabled);
                    if (checkMask)
                    {
                        EditorGUI.indentLevel++;
                        maskType = (Constants.MaskType) m_editorUtils.EnumPopup("Mask Type", maskType, helpEnabled);
                        if (maskType != Constants.MaskType.Image)
                        {
                            maskFractalSeed = m_editorUtils.Slider("Seed", maskFractalSeed, 0f, 65000f, helpEnabled);
                            maskFractalOctaves = m_editorUtils.IntSlider("Octaves", maskFractalOctaves, 1, 12, helpEnabled);
                            maskFractalFrequency = m_editorUtils.Slider("Frequency", maskFractalFrequency, 0f, m_spawnerData.UseLargeRanges ? 1f : 0.3f, helpEnabled);
                            maskFractalPersistence = m_editorUtils.Slider("Persistence", maskFractalPersistence, 0f, 1f, helpEnabled);
                            maskFractalLacunarity = m_editorUtils.Slider("Lacunarity", maskFractalLacunarity, 1.5f, 3.5f, helpEnabled);
                            midMaskFractal = m_editorUtils.Slider("Midpoint", midMaskFractal, 0f, 1f, helpEnabled);
                            maskFractalRange = m_editorUtils.Slider("Range", maskFractalRange, 0f, 1f, helpEnabled);
                            maskInvert = m_editorUtils.Toggle("Invert Mask", maskInvert, helpEnabled);
                        }
                        else
                        {
                            maskImage = (Texture2D) m_editorUtils.ObjectField("Image Mask", maskImage, typeof(Texture2D), helpEnabled);
                            imageFilterColor = m_editorUtils.ColorField("Selection Color", imageFilterColor, helpEnabled);
                            imageFilterFuzzyMatch = m_editorUtils.Slider("Selection Accuracy", imageFilterFuzzyMatch, 0f, 1f, helpEnabled);
                            constrainWithinMaskedBounds = m_editorUtils.Toggle("Fit Within Mask", constrainWithinMaskedBounds, helpEnabled);
                            invertMaskedAlpha = m_editorUtils.Toggle("Invert Alpha", invertMaskedAlpha, helpEnabled);
                            successOnMaskedAlpha = m_editorUtils.Toggle("Success By Alpha", successOnMaskedAlpha, helpEnabled);
                            scaleOnMaskedAlpha = m_editorUtils.Toggle("Scale By Alpha", scaleOnMaskedAlpha, helpEnabled);
                            if (m_spawnCriteria.ScaleOnMaskedAlpha)
                            {
                                EditorGUI.indentLevel++;
                                minScaleOnMaskedAlpha = m_editorUtils.Slider("Mask Alpha Min Scale", minScaleOnMaskedAlpha, 0f, 10f, helpEnabled);
                                maxScaleOnMaskedAlpha = m_editorUtils.Slider("Mask Alpha Max Scale", maxScaleOnMaskedAlpha, 0f, 10f, helpEnabled);
                                EditorGUI.indentLevel--;
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    #endregion
                }
                if (!m_spawnerData.IsProcessing)
                {
                    if (GeNaEditorUtility.ValidateComputeShader())
                    {
                        GUI.enabled = true;
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                spawnCriteria.ForceSpawn = forceSpawn;
                m_spawnerData.GroundLayer = groundLayer;
                spawnCriteria.SpawnCollisionLayers = spawnCollisionLayers;
                spawnCriteria.CheckCollisionType = virginCheckType;
                spawnCriteria.BlendAmount = blendAmount;
                spawnCriteria.BoundsExtents = boundsExtents;
                spawnCriteria.CheckHeightType = checkHeightType;
                m_spawnerData.GetSeaLevel = seaLevel;
                if (!Mathf.Approximately(extraSeaLevel, m_spawnerData.ExtraSeaLevelHeight))
                {
                    m_spawnerData.ExtraSeaLevelHeight = extraSeaLevel;
                    GeNaEvents.SetSeaLevel(m_spawnerData);
                }
                spawnCriteria.MinSpawnHeight = minSpawnHeight;
                spawnCriteria.MaxSpawnHeight = maxSpawnHeight;
                spawnCriteria.MinHeight = minHeight;
                spawnCriteria.MaxHeight = maxHeight;
                spawnCriteria.HeightRange = heightVariance;
                spawnCriteria.BottomBoundary = bottomBoundary;
                spawnCriteria.TopBoundary = topBoundary;
                spawnCriteria.CheckSlopeType = checkSlopeTypeProperty;
                spawnCriteria.MinSpawnSlope = minSpawnSlopeProperty;
                spawnCriteria.MaxSpawnSlope = maxSpawnSlopeProperty;
                spawnCriteria.MinSlope = minSlopeProperty;
                spawnCriteria.MaxSlope = maxSlopeProperty;
                spawnCriteria.SlopeRange = slopeRangeProperty;
                spawnCriteria.CheckTextures = checkTextures;
                spawnCriteria.SelectedTextureIdx = selectedTextureIdxProperty;
                spawnCriteria.SelectedTextureName = selectedTextureNameProperty;
                spawnCriteria.TextureStrength = textureStrengthProperty;
                spawnCriteria.TextureRange = textureVarianceProperty;
                spawnCriteria.CheckMask = checkMask;
                spawnCriteria.CheckMaskType = maskType;
                spawnCriteria.MaskFractal = maskFractal;
                maskFractal.Seed = maskFractalSeed;
                maskFractal.Octaves = maskFractalOctaves;
                maskFractal.Frequency = maskFractalFrequency;
                maskFractal.Persistence = maskFractalPersistence;
                maskFractal.Lacunarity = maskFractalLacunarity;
                spawnCriteria.MidMaskFractal = midMaskFractal;
                spawnCriteria.MaskFractalRange = maskFractalRange;
                spawnCriteria.MaskInvert = maskInvert;
                spawnCriteria.MaskImage = maskImage;
                spawnCriteria.ImageFilterColor = imageFilterColor;
                spawnCriteria.ImageFilterFuzzyMatch = imageFilterFuzzyMatch;
                spawnCriteria.ConstrainWithinMaskedBounds = constrainWithinMaskedBounds;
                spawnCriteria.InvertMaskedAlpha = invertMaskedAlpha;
                spawnCriteria.SuccessOnMaskedAlpha = successOnMaskedAlpha;
                spawnCriteria.ScaleOnMaskedAlpha = scaleOnMaskedAlpha;
                spawnCriteria.MinScaleOnMaskedAlpha = minScaleOnMaskedAlpha;
                spawnCriteria.MaxScaleOnMaskedAlpha = maxScaleOnMaskedAlpha;
                m_spawner.IsDirty = true;
            }
        }
        private void PrototypesPanel(bool helpEnabled)
        {
            m_editorUtils.InlineHelp("Spawn Prototypes", helpEnabled);
            GUILayout.BeginHorizontal();
            {
                m_editorUtils.LabelField("Spawn Proto Panel Intro");
                GUILayout.FlexibleSpace();
                // ConformMenu();
                // SnapToGroundMenu();
                bool sortPrototypes = m_spawnerData.SortPrototypes;
                m_editorUtils.ToggleButtonNonLocalized(" A-Z↓", ref sortPrototypes, GUILayout.Height(18f));
                m_editorUtils.Styles.deleteButton.fixedHeight = 18f;
                m_editorUtils.Styles.deleteButton.fixedWidth = 21f;
                m_editorUtils.Styles.deleteButton.alignment = TextAnchor.LowerCenter;
                if (m_editorUtils.DeleteButton())
                {
                    if (EditorUtility.DisplayDialog("WARNING!",
                        "Are you sure you want to delete ALL of the Prototypes?",
                        "OK",
                        "Cancel"))
                    {
                        List<Prototype> spawnPrototypes = m_spawnerData.SpawnPrototypes;
                        List<Prototype> prototypesToDelete = new List<Prototype>(spawnPrototypes);
                        foreach (Prototype prototype in prototypesToDelete)
                        {
                            m_spawner.RemoveProto(prototype);
                        }
                    }
                    m_spawner.IsDirty = true;
                    GUIUtility.ExitGUI();
                }
                m_spawnerData.SortPrototypes = sortPrototypes;
            }
            GUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("Conform Dropdown", helpEnabled);
            m_editorUtils.InlineHelp("Snap Dropdown", helpEnabled);
            ProtoPanel(m_spawnerData.SpawnPrototypes, helpEnabled);
        }
        private void ProtoPanel(List<Prototype> prototypes, bool helpEnabled)
        {
            Rect protoPanelWidthRect = GUILayoutUtility.GetLastRect();
            foreach (var prototype in prototypes)
            {
                GUILayout.BeginVertical(Styles.gpanel);
                {
                    string protoName = " <b>" + prototype.Name;
                    if (prototype.HasType(Constants.ResourceType.Prefab))
                    {
                        string typeCode = " (P)";
                        if (prototype.GetResources()[0] != null && prototype.GetResources()[0].ContainerOnly)
                            typeCode = " (G)";
                        protoName += typeCode;
                        if (prototype.GetTopLevelResources().Count == 1)
                        {
                            if (prototype.GetResources()[0].ConformToSlope)
                                protoName += " *C*";
                        }
                    }
                    if (prototype.HasType(Constants.ResourceType.TerrainTree))
                    {
                        protoName += " (T)";
                    }
                    if (prototype.HasType(Constants.ResourceType.TerrainGrass))
                    {
                        protoName += " (G)";
                    }
                    if (prototype.HasType(Constants.ResourceType.TerrainTexture))
                    {
                        protoName += " (Tx)";
                    }
                    if (prototype.IsActive != true)
                        protoName += " [inactive]";
                    else
                        protoName += string.Format(" {0:0}% [{1}]", prototype.GetSuccessChance() * 100f, prototype.InstancesSpawned);
                    GUIStyle protoLabelStyle = Styles.richLabel;
                    protoName += "</b>";
                    // Let's check we are just changing the active state now
                    bool active = prototype.IsActive;
                    GUILayout.BeginHorizontal();
                    {
                        prototype.IsActive = EditorGUILayout.Toggle(prototype.IsActive, GUILayout.Width(10f));
                        if (GUILayout.Button(protoName, protoLabelStyle))
                            prototype.IsActive = !prototype.IsActive;
                        //Prep the rect if any ico needs to be drawn
                        Rect r = GUILayoutUtility.GetLastRect();
                        r = new Rect(r.xMax - 2f, r.yMin + 2f, 18f, 18f);
                        IReadOnlyList<Resource> resources = prototype.GetResources();
                        if (resources.Count > 0)
                        {
                            if (resources[0].SpawnCriteria.OverrideApplies)
                            {
                                if (m_overridesIco != null)
                                {
                                    //GUI.DrawTexture(r, m_overridesIco);
                                    GUI.Label(r, "*");
                                    //Add to rect in case another ico needs drawing after this
                                    r.x += 20f;
                                }
                                else
                                    Debug.LogWarningFormat("[GeNa] Missing overrides icon.");
                            }
                        }
                        GUILayout.FlexibleSpace();
                        bool showAdvanced = prototype.ShowAdvancedOptions;
                        m_editorUtils.ToggleButton("Advanced Toggle", ref showAdvanced, Styles.advancedToggle, Styles.advancedToggleDown);
                        prototype.ShowAdvancedOptions = showAdvanced;
                        // foreach (var prototype in prototypes)
                        // {
                        //     prototype.ShowAdvancedOptions = showAdvanced;
                        // }
                        //GUILayout.Space(10f);
                        if (m_editorUtils.DeleteButton())
                        {
                            if (EditorUtility.DisplayDialog("WARNING!", string.Format("Are you sure you want to delete the prototype [{0}]?", prototype.Name), "OK", "Cancel"))
                                m_spawner.RemoveProto(prototype);
                            m_spawner.IsDirty = true;
                            GUIUtility.ExitGUI();
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(2f);
                    // In here we can process different things if the active state changed.
                    if (active != prototype.IsActive)
                    {
                        m_spawner.UpdatePrototypes();
                    }

                    // Display the the proto properties if active
                    if (prototype.IsActive)
                    {
                        EditorGUI.indentLevel++;
                        // Only show help for static switches if they are actually drawn
                        bool staticSwitchesDrawn = false;
                        GUILayout.BeginHorizontal((prototype.GetResources().Count < 1 || prototype.GetResources()[0].Static != Constants.ResourceStatic.Dynamic) ? Styles.staticResHeader : Styles.dynamicResHeader);
                        {
                            prototype.DisplayedInEditor = m_editorUtils.Foldout(prototype.DisplayedInEditor, "Details Foldout");
                            GUILayout.FlexibleSpace();
                            // Add the static switch if not POI
                            if (prototype.GetTopLevelResources().Count == 1)
                                staticSwitchesDrawn = StaticSwitch(prototype, prototype.GetResources()[0]);
                        }
                        GUILayout.EndHorizontal();
                        // Adding help here because it should not be in the horizontal area. This way we also have the help only once to avoid cluttering the GUI.
                        if (staticSwitchesDrawn)
                            m_editorUtils.InlineHelp(Enum.GetNames(typeof(Constants.ResourceStatic)), helpEnabled);
                        GUILayout.Space(3f);
                        if (prototype.DisplayedInEditor)
                        {
                            if (prototype.HasType(Constants.ResourceType.Prefab))
                                prototype.ForwardRotation = m_editorUtils.Slider("Forward Rotation", prototype.ForwardRotation, -360f, 360f, helpEnabled);
                            if (prototype.GetTopLevelResources().Count == 1)
                            {
                                Separator(protoPanelWidthRect);
                                EditResource(prototype, prototype.GetResources()[0], false, helpEnabled, prototype.ShowAdvancedOptions);
                            }
                            else
                            {
                                prototype.Name = m_editorUtils.TextField("Proto Name", prototype.Name, helpEnabled);
                                m_editorUtils.LabelField("Proto Size", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", prototype.Size.x, prototype.Size.y, prototype.Size.z)), helpEnabled);
                                Separator(protoPanelWidthRect);
                                foreach (Resource res in prototype.GetResources())
                                {
                                    string resName = res.Name;
                                    if (res.ConformToSlope)
                                        resName += " *C*";
                                    GUILayout.BeginHorizontal((res.Static == Constants.ResourceStatic.Dynamic) ? Styles.dynamicResHeader : Styles.staticResHeader);
                                    {
                                        res.m_displayedInEditor = EditorGUILayout.Foldout(res.m_displayedInEditor, resName, true);
                                        GUILayout.FlexibleSpace();
                                        StaticSwitch(prototype, res);
                                    }
                                    GUILayout.EndHorizontal();
                                    if (res.m_displayedInEditor)
                                        EditResource(prototype, res, true, helpEnabled, prototype.ShowAdvancedOptions);
                                }
                            }
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(20);
                            //if (proto.m_resourceType == Constants.ResourceType.TerrainGrass)
                            //{
                            //    if (m_editorUtils.Button("Delete Instances", helpEnabled))
                            //    {
                            //        if (EditorUtility.DisplayDialog("WARNING!", "Are you sure you want to delete all instances of the grass [" + proto.m_name + "] from your scene?\n\n" +
                            //            "WARNING! This is destructive and non-recoverable. This deletes ALL of these details and not only those which were spawned by GeNa.\n\n" +
                            //            "NOTE: This will also clear the Undo History.", "OK", "Cancel"))
                            //        {
                            //            m_spawner.UnspawnGrass(protoIdx);
                            //            SpawnerToCache();
                            //        }
                            //    }
                            //}
                            //if (proto.m_resourceType == Constants.ResourceType.TerrainTree)
                            //{
                            //    if (m_editorUtils.Button("Delete Instances", helpEnabled))
                            //    {
                            //        if (EditorUtility.DisplayDialog("WARNING!", "Are you sure you want to delete all instances of the tree [" + proto.m_name + "] from your scene?\n\n" +
                            //            "WARNING! This is destructive and non-recoverable. This deletes ALL of these trees and not only those which were spawned by GeNa.\n\n" +
                            //            "NOTE: This will also clear the Undo History.", "OK", "Cancel"))
                            //        {
                            //            m_spawner.UnspawnTree(protoIdx);
                            //            SpawnerToCache();
                            //        }
                            //    }
                            //}
                            // TODO : Manny : Delete Instances Button - Deprecated until new Undo System gets implemented
                            // if (proto.HasType(Constants.ResourceType.Prefab))
                            // {
                            //     if (m_editorUtils.Button("Delete Instances", helpEnabled))
                            //     {
                            //         if (EditorUtility.DisplayDialog("WARNING!", "Are you sure you want to delete all instances of [" + proto.Name + "] prefabs from your scene?\n\n" +
                            //                                                     "NOTE: This will also clear the Undo History.", "OK", "Cancel"))
                            //         {
                            //             GeNaEditorUtility.DespawnGameObjects(m_spawnerData, protoIdx);
                            //         }
                            //         m_spawner.Serialize();
                            //         EditorUtility.SetDirty(m_spawner);
                            //         GUIUtility.ExitGUI();
                            //     }
                            // }
                            GUILayout.EndHorizontal();
                        }
                        EditorGUI.indentLevel--;
                    }
                    GUILayout.Space(3);
                }
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
        }
        private void AdvancedPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                SpawnerSettings.AdvancedSettings advancedSettings = m_settings.Advanced;
                m_spawnerData.SpawnToTarget = m_editorUtils.Toggle("Spawn to Target", m_spawnerData.SpawnToTarget, helpEnabled);
                m_spawnerData.MergeSpawns = m_editorUtils.Toggle("Merge Instances", m_spawnerData.MergeSpawns, helpEnabled);
                // Placement
                m_placementCriteria.ScaleToNearestInt = m_editorUtils.Toggle("Scale Nearest Int", m_placementCriteria.ScaleToNearestInt, helpEnabled);
                m_placementCriteria.GlobalSpawnJitterPct = m_editorUtils.Slider("Global Spawn Jitter", m_placementCriteria.GlobalSpawnJitterPct * 100f, 0f, 100f, helpEnabled) * 0.01f;
                //m_spawner.m_advAddColliderToSpawnedPrefabs = m_editorUtils.Toggle("Add Collider To POI", m_spawner.m_advAddColliderToSpawnedPrefabs, helpEnabled);
                m_settings.MaxSubSpawnerDepth = m_editorUtils.IntField("Max Sub Spawner Depth", m_settings.MaxSubSpawnerDepth, helpEnabled);
                m_settings.AutoProbe = m_editorUtils.Toggle("Add Light Probes", m_settings.AutoProbe, helpEnabled);
                if (m_settings.AutoProbe)
                {
                    m_settings.MinProbeGroupDistance = m_editorUtils.Slider("Min PG Dist", m_settings.MinProbeGroupDistance, 10f, 200f, helpEnabled);
                    m_settings.MinProbeDistance = m_editorUtils.Slider("Min Probe Dist", m_settings.MinProbeDistance, 5f, 50f, helpEnabled);
                }
                m_settings.AutoOptimise = m_editorUtils.Toggle("Spawn Optimizer", m_settings.AutoOptimise, helpEnabled);
                if (m_settings.AutoOptimise)
                {
                    m_settings.MaxSizeToOptimise = m_editorUtils.Slider("Smaller Than (m)", m_settings.MaxSizeToOptimise, 5f, 50f, helpEnabled);
                }
                m_settings.MaxVisualisationDimensions = m_editorUtils.IntSlider("Visualiser Resolution", m_settings.MaxVisualisationDimensions, 1, 512, helpEnabled);
                // Advanced
                advancedSettings.PerformUndoAtRuntime = m_editorUtils.Toggle("Perform Undo At Runtime", advancedSettings.PerformUndoAtRuntime, helpEnabled);
                advancedSettings.SpawnCheckOffset = m_editorUtils.FloatField("Collision Test Offset", advancedSettings.SpawnCheckOffset, helpEnabled);
                advancedSettings.BoundsOffset = m_editorUtils.FloatField("Bounds Offset", advancedSettings.BoundsOffset, helpEnabled);
                advancedSettings.DebugEnabled = m_editorUtils.Toggle("Debug Enabled", advancedSettings.DebugEnabled, helpEnabled);
                if (advancedSettings.DebugEnabled)
                {
                    if (!m_spawnerData.IsProcessing)
                        GUI.enabled = false;
                    ToggleNonLocalized("Affects Height", m_spawnerData.AffectsHeight);
                    ToggleNonLocalized("Affects Trees", m_spawnerData.AffectsTrees);
                    ToggleNonLocalized("Affects Grass", m_spawnerData.AffectsGrass);
                    ToggleNonLocalized("Affects Texture", m_spawnerData.AffectsTexture);
                    ToggleNonLocalized("Has GameObject Protos", m_spawnerData.HasGameObjectProtos);
                    ToggleNonLocalized("Has Tree Protos", m_spawnerData.HasTerrainTrees);
                    ToggleNonLocalized("Has Grass Protos", m_spawnerData.HasTerrainGrass);
                    ToggleNonLocalized("Has Texture Protos", m_spawnerData.HasTerrainTextures);
                    ToggleNonLocalized("Has Heights Protos", m_spawnerData.HasTerrainHeights);
                    ToggleNonLocalized("Has Active Physics Protos", m_spawnerData.HasActivePhysicsProtos());
                    ToggleNonLocalized("Has Active Tree Protos", m_spawnerData.HasActiveTerrainTrees);
                    ToggleNonLocalized("Has Active Grass Protos", m_spawnerData.HasActiveTerrainGrass);
                    ToggleNonLocalized("Has Active Texture Protos", m_spawnerData.HasActiveTerrainTextures);
                    ToggleNonLocalized("Has Active Heights Protos", m_spawnerData.HasActiveTerrainHeights);
                    ToggleNonLocalized("Has Active SubSpawner Protos", m_spawnerData.HasActiveSubSpawnerProtos());
                    if (!m_spawnerData.IsProcessing)
                    {
                        if (GeNaEditorUtility.ValidateComputeShader())
                        {
                            GUI.enabled = true;
                        }
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_spawner.IsDirty = true;
            }
        }
        private void AddTerrainPrototype(EffectType effectType)
        {
            string name = effectType.ToString();
            GameObject prototype = new GameObject(name);
            GeNaTerrainDecorator decorator = prototype.AddComponent<GeNaTerrainDecorator>();
            TerrainModifier terrainModifier = decorator.TerrainModifier;
            terrainModifier.EffectType = effectType;
            int selected = 0;
            LoadUnityBrushIcons(terrainModifier.AddBrushTexture, ref selected);
            AddGameObject(m_spawnerData, prototype);
            DestroyImmediate(prototype);
        }
        private void AddPrototypesPanel()
        {
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.BeginVertical(m_editorUtils.Styles.panelFrame);
                {
                    //Add prototypes
                    GUILayout.BeginHorizontal();
                    {
                        if (DrawPrefabGUI())
                            GUI.changed = true;
                        if (m_editorUtils.Button("Add Tree", Styles.addBtn, GUILayout.Width(50), GUILayout.Height(49)))
                        {
                            m_spawner.AddTreeProto();
                            GUI.changed = true;
                        }
                        if (m_editorUtils.Button("Add Grass", Styles.addBtn, GUILayout.Width(50), GUILayout.Height(49)))
                        {
                            //Add and init the brushsets for it
                            //[Obselete] m_spawner.AddGrassProto();
                            AddTerrainPrototype(EffectType.Detail);
                            GUI.changed = true;
                        }
                        if (m_editorUtils.Button("Add Tx", Styles.addBtn, GUILayout.Width(50), GUILayout.Height(49)))
                        {
                            //Add and init the brushsets for it
                            //[Obselete] m_spawner.AddTextureProto();
                            AddTerrainPrototype(EffectType.Texture);
                            GUI.changed = true;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_spawner.IsDirty = true;
            }
        }
        private void LoadUnityBrushIcons(Action<Texture2D> addMethod, ref int selected)
        {
            int num1 = 1;
            Texture2D texture2D;
            do
            {
                texture2D = Resources.Load("pwub_builtin_" + num1.ToString() + PWConst.VERSION_IN_FILENAMES) as Texture2D;
                if ((bool) (UnityEngine.Object) texture2D)
                {
                    addMethod(texture2D);
                    ++num1;
                }
                else
                    break;
            } while ((bool) (UnityEngine.Object) texture2D);
            int num2 = 0;
            Texture2D texture;
            do
            {
                texture = EditorGUIUtility.FindTexture("brush_" + num2.ToString() + ".png");
                if ((bool) (UnityEngine.Object) texture)
                {
                    addMethod(texture);
                    ++num2;
                }
                else
                    break;
            } while ((bool) (UnityEngine.Object) texture);
            if (selected < 0)
                selected = 0;
            GUI.changed = true;
        }
        #region Decorators
        [NonSerialized]
        protected readonly Dictionary<IDecorator, Editor> m_decoratorEditors =
            new Dictionary<IDecorator, Editor>();
        public Editor GetEditor(IDecorator decorator)
        {
            if (m_decoratorEditors.ContainsKey(decorator))
                return m_decoratorEditors[decorator];
            Editor editor = CreateEditor(decorator as Component);
            m_decoratorEditors.Add(decorator, editor);
            return editor;
        }
        private void EditDecorators(Resource res)
        {
            foreach (IDecorator decorator in res.Decorators)
            {
                Editor editor = GetEditor(decorator);
                if (editor == null)
                    continue;
                editor.OnInspectorGUI();
            }
        }
        public void EditOneChildOf(Prototype proto, Resource res, bool helpEnabled)
        {
            if (res.Type != Constants.ResourceType.Prefab)
                return;
            if (!res.OneChildOf)
                return;
            res.OneChildOf = m_editorUtils.Toggle("Child Of Toggle", res.OneChildOf, helpEnabled);
            if (res.OneChildOf)
            {
                List<Resource> children = proto.GetChildren(res);
                if (children != null && children.Count > 0)
                {
                    EditorGUI.indentLevel++;
                    foreach (Resource decoratorChild in children)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            string title = decoratorChild.Name;
                            EditorGUILayout.LabelField(title, Styles.body, GUILayout.Width(EditorGUIUtility.labelWidth));
                            decoratorChild.OneChildOfWeight = EditorGUILayout.Slider(decoratorChild.OneChildOfWeight, 0f, 1f);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }
        #endregion
        /// <summary>
        /// Create a new palette scriptable object
        /// </summary>
        /// <returns></returns>
        private Palette CreatePalette()
        {
            string name = "Assets/New Palette";
            if (SceneManager.GetActiveScene() != null)
            {
                name += " " + SceneManager.GetActiveScene().name;
            }
            Palette checkPalette = AssetDatabase.LoadAssetAtPath<Palette>(name + ".asset");
            if (checkPalette != null)
            {
                int index = 0;
                string[] guids = AssetDatabase.FindAssets("t:Palette", null);
                foreach (string guid in guids)
                {
                    if (!string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)))
                    {
                        index++;
                    }
                }
                name += " " + index;
            }
            Palette asset = ScriptableObject.CreateInstance<Palette>();
            AssetDatabase.CreateAsset(asset, name + ".asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            return asset;
        }
        /// <summary>
        /// Edit the selected resource
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="res"></param>
        /// <param name="child"></param>
        /// <param name="helpEnabled"></param>
        /// <param name="advanced"></param>
        private void EditResource(Prototype proto, Resource res, bool child, bool helpEnabled, bool advanced)
        {
            bool isChanged = false;
            if (proto.ShowAdvancedOptions)
            {
                GUILayout.BeginVertical(Styles.gpanel);
                {
                    if (res.SpawnCriteria.OverrideApplies)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            res.ShowSpawnCriteriaOverrides = m_editorUtils.Foldout(res.ShowSpawnCriteriaOverrides, "Proto SpCrit Overrides Label");
                            if (m_overridesIco != null)
                            {
                                Rect r = GUILayoutUtility.GetLastRect();
                                // EditorGUI.LabelField(new Rect(r.x + EditorStyles.foldout.CalcSize(m_editorUtils.GetContent("Proto SpCrit Overrides Label")).x + 3f, r.y - 1f, 18f, 18f), "*");
                                GUI.Label(new Rect(r.x + EditorStyles.foldout.CalcSize(m_editorUtils.GetContent("Proto SpCrit Overrides Label")).x + 3f, r.y + 2f, 18f, 18f), "*");
                            }
                            else
                                Debug.LogWarningFormat("[GeNa] Missing overrides icon.");
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        res.ShowSpawnCriteriaOverrides = m_editorUtils.Foldout(res.ShowSpawnCriteriaOverrides, "Proto SpCrit Overrides Label");
                    }
                    if (res.ShowSpawnCriteriaOverrides)
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            res.SpawnCriteria = m_editorUtils.SpawnCriteriaOverrides(res.SpawnCriteria, m_spawnerData.SpawnCriteria, helpEnabled);
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            isChanged = true;
                        }
                    }
                    GUILayout.Space(3);
                }
                GUILayout.EndVertical();
            }
            GUILayout.Space(3f);
            if (child)
                EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            {
                res.Name = m_editorUtils.TextField("Resource Name", res.Name, helpEnabled);
                if (res.Static > Constants.ResourceStatic.Static)
                    res.SuccessRate = 0.01f * m_editorUtils.Slider("Res Success", res.SuccessRate * 100f, 0f, 100f, helpEnabled);
            }
            if (EditorGUI.EndChangeCheck())
            {
                isChanged = true;
            }
            bool snapToGround = res.SnapToGround;
            bool conformToSlope = res.ConformToSlope;
            Vector3 basePosition = res.BasePosition;
            Vector3 baseRotation = res.BaseRotation;
            Vector3 baseScale = res.BaseScale;
            Vector3 baseSize = res.BaseSize;
            Vector3 minRotation = res.MinRotation;
            Vector3 maxRotation = res.MaxRotation;
            Vector3 minOffset = res.MinOffset;
            Vector3 maxOffset = res.MaxOffset;
            Vector3 minScale = res.MinScale;
            Vector3 maxScale = res.MaxScale;
            EditorGUI.BeginChangeCheck();
            {
                switch (res.Type)
                {
                    case Constants.ResourceType.Prefab:
                        PrefabField(res, helpEnabled);
                        if (res.Static > Constants.ResourceStatic.Static)
                        {
                            snapToGround = m_editorUtils.Toggle("Snap To Ground", snapToGround, helpEnabled);
                            conformToSlope = m_editorUtils.Toggle("Conform Slope", conformToSlope, helpEnabled);
                            if (advanced)
                            {
                                PrefabField(res, helpEnabled);
                                minOffset = m_editorUtils.Vector3Field("Min Position Offset", minOffset, helpEnabled);
                                maxOffset = m_editorUtils.Vector3Field("Max Position Offset", maxOffset, helpEnabled);
                            }
                            else
                                m_editorUtils.MinMaxSliderWithFields("Position Modifier Y", ref minOffset.y, ref maxOffset.y, -10f, 10f, helpEnabled);
                        }
                        else
                        {
                            m_editorUtils.LabelField("Static Position Offset", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", basePosition.x, basePosition.y, basePosition.z)));
                            m_editorUtils.LabelField("Static Rotation Offset", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseRotation.x, baseRotation.y, baseRotation.z)));
                            m_editorUtils.LabelField("Static Scale", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseScale.x, baseScale.y, baseScale.z)));
                        }
                        if (advanced)
                        {
                            if (res.Static > Constants.ResourceStatic.Static)
                            {
                                minRotation = m_editorUtils.Vector3Field("Min Rotation Offset", minRotation, helpEnabled);
                                maxRotation = m_editorUtils.Vector3Field("Max Rotation Offset", maxRotation, helpEnabled);
                                res.SameScale = m_editorUtils.Toggle("Same O Scale", res.SameScale, helpEnabled);
                                if (res.SameScale)
                                    m_editorUtils.MinMaxSliderWithFields("Res Scale", ref minScale.x, ref maxScale.x, 0.1f, 100f, helpEnabled);
                                else
                                {
                                    minScale = m_editorUtils.Vector3Field("Res Min Scale", minScale, helpEnabled);
                                    maxScale = m_editorUtils.Vector3Field("Res Max Scale", maxScale, helpEnabled);
                                }
                            }
                            res.BaseColliderUseConstScale = m_editorUtils.Toggle("Same C Scale", res.BaseColliderUseConstScale, helpEnabled);
                            if (res.BaseColliderUseConstScale)
                                res.BaseColliderConstScaleAmount = m_editorUtils.Slider("Collider Scale", res.BaseColliderConstScaleAmount, 0.25f, 2f, helpEnabled);
                            else
                                res.BaseColliderScale = m_editorUtils.Vector3Field("Collider Scale", res.BaseColliderScale, helpEnabled);
                            bool canUseColliders = res.HasColliders && res.Static == Constants.ResourceStatic.Dynamic;
                            m_editorUtils.SpawnFlags(res.SpawnFlags, canUseColliders, helpEnabled);
                        }
                        // if not advanced mode and not static, Height Offset only if not 
                        else if (res.Static > Constants.ResourceStatic.Static)
                            m_editorUtils.MinMaxSliderWithFields("Y Rotation Offset", ref minRotation.y, ref maxRotation.y, -180f, 180f, helpEnabled);
                        m_editorUtils.LabelField("Base Position", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", basePosition.x, basePosition.y, basePosition.z)), helpEnabled);
                        m_editorUtils.LabelField("Base Rotation", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseRotation.x, baseRotation.y, baseRotation.z)), helpEnabled);
                        if (res.Static >= Constants.ResourceStatic.Dynamic)
                            m_editorUtils.LabelField("Base Scale", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseScale.x, baseScale.y, baseScale.z)), helpEnabled);
                        m_editorUtils.LabelField("Base Size", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseSize.x, baseSize.y, baseSize.z)), helpEnabled);
                        m_editorUtils.LabelField("Res Spawned", new GUIContent(string.Format("{0}", res.InstancesSpawned)), helpEnabled);
                        break;
                    case Constants.ResourceType.TerrainGrass:
                        Terrain terrain = Terrain.activeTerrain;
                        if (terrain != null)
                        {
                            GUIContent[] assetChoices = new GUIContent[terrain.terrainData.detailPrototypes.Length];
                            DetailPrototype detailProto;
                            for (int assetIdx = 0; assetIdx < assetChoices.Length; assetIdx++)
                            {
                                detailProto = terrain.terrainData.detailPrototypes[assetIdx];
                                if (detailProto.prototypeTexture != null)
                                    assetChoices[assetIdx] = new GUIContent(detailProto.prototypeTexture.name);
                                else if (detailProto.prototype != null)
                                    assetChoices[assetIdx] = new GUIContent(detailProto.prototype.name);
                                else
                                    assetChoices[assetIdx] = new GUIContent("Unknown asset");
                            }
                            int oldIdx = res.TerrainProtoIdx;
                            res.TerrainProtoIdx = m_editorUtils.Popup("Grass", res.TerrainProtoIdx, assetChoices, helpEnabled);
                            res.SameScale = true;
                            m_editorUtils.MinMaxSliderWithFields("Grass Strength", ref minScale.x, ref maxScale.x, 0f, 1f, helpEnabled);
                            m_editorUtils.MinMaxSliderWithFields("Position Modifier X", ref minOffset.x, ref maxOffset.x, -10f, 10f, helpEnabled);
                            m_editorUtils.MinMaxSliderWithFields("Position Modifier Z", ref minOffset.z, ref maxOffset.z, -10f, 10f, helpEnabled);
                            if (res.TerrainProtoIdx != oldIdx)
                            {
                                detailProto = terrain.terrainData.detailPrototypes[res.TerrainProtoIdx];
                                if (detailProto.prototypeTexture != null)
                                {
                                    res.AddDetailPrototype(detailProto.prototypeTexture, m_spawner.Palette);
                                    res.Name = detailProto.prototypeTexture.name;
                                    proto.Name = res.Name;
                                }
                                else if (detailProto.prototype != null)
                                {
                                    res.AddDetailPrototype(detailProto.prototype, m_spawner.Palette);
                                    res.Name = detailProto.prototype.name;
                                    proto.Name = res.Name;
                                }
                                else
                                {
                                    res.Name = "Unknown asset";
                                    proto.Name = res.Name;
                                }
                                res.BaseSize = new Vector3(detailProto.minWidth, detailProto.minHeight, detailProto.minWidth);
                                proto.Size = res.BaseSize;
                                proto.Extents = res.BaseSize * 0.5f;
                            }
                            m_editorUtils.LabelField("Base Size", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", res.BaseSize.x, res.BaseSize.y, res.BaseSize.z)), helpEnabled);
                        }
                        break;
                    case Constants.ResourceType.TerrainTree:
                        terrain = Terrain.activeTerrain;
                        if (terrain != null)
                        {
                            GUIContent[] assetChoices = new GUIContent[terrain.terrainData.treePrototypes.Length];
                            TreePrototype treeProto;
                            for (int assetIdx = 0; assetIdx < assetChoices.Length; assetIdx++)
                            {
                                treeProto = terrain.terrainData.treePrototypes[assetIdx];
                                if (treeProto.prefab != null)
                                    assetChoices[assetIdx] = new GUIContent(treeProto.prefab.name);
                                else
                                    assetChoices[assetIdx] = new GUIContent("Unknown asset");
                            }
                            int oldIdx = res.TerrainProtoIdx;
                            res.TerrainProtoIdx = m_editorUtils.Popup("Tree", res.TerrainProtoIdx, assetChoices, helpEnabled);
                            if (res.TerrainProtoIdx != oldIdx)
                            {
                                treeProto = terrain.terrainData.treePrototypes[res.TerrainProtoIdx];
                                if (treeProto.prefab != null)
                                {
                                    res.AddPrefab(treeProto.prefab, m_spawner.Palette);
                                    res.Name = treeProto.prefab.name;
                                    proto.Name = res.Name;
                                    res.BaseSize = GeNaUtility.GetInstantiatedBounds(treeProto.prefab).size;
                                    res.BaseScale = treeProto.prefab.transform.localScale;
                                }
                                else
                                {
                                    res.Name = "Unknown asset";
                                    proto.Name = res.Name;
                                }
                                res.MinScale = res.BaseScale;
                                res.MinScale = res.BaseScale;
                                proto.Size = res.BaseSize;
                                proto.Extents = res.BaseSize * 0.5f;
                            }
                            m_editorUtils.MinMaxSliderWithFields("Position Modifier X", ref minOffset.x, ref maxOffset.x, -100f, 100f, helpEnabled);
                            m_editorUtils.MinMaxSliderWithFields("Position Modifier Z", ref minOffset.z, ref maxOffset.z, -100f, 100f, helpEnabled);
                            m_editorUtils.MinMaxSliderWithFields("Y Rotation Offset", ref minRotation.y, ref maxRotation.y, -180f, 180f, helpEnabled);
                            if (advanced)
                            {
                                res.SameScale = m_editorUtils.Toggle("Same O Scale", res.SameScale, helpEnabled);
                                if (res.SameScale)
                                    m_editorUtils.MinMaxSliderWithFields("Res Scale", ref minScale.x, ref maxScale.x, 0.1f, 100f, helpEnabled);
                                else
                                {
                                    minScale = m_editorUtils.Vector3Field("Res Min Scale", minScale, helpEnabled);
                                    maxScale = m_editorUtils.Vector3Field("Res Max Scale", maxScale, helpEnabled);
                                }
                            }
                            m_editorUtils.LabelField("Base Size", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseSize.x, baseSize.y, baseSize.z)), helpEnabled);
                            m_editorUtils.LabelField("Base Scale", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseScale.x, baseScale.y, baseScale.z)), helpEnabled);
                        }
                        break;
                    case Constants.ResourceType.TerrainTexture:
                        terrain = Terrain.activeTerrain;
                        if (terrain != null)
                        {
                            GUIContent[] assetChoices = new GUIContent[terrain.terrainData.alphamapLayers];
                            for (int assetIdx = 0; assetIdx < assetChoices.Length; assetIdx++)
                            {
#if UNITY_2018_3_OR_NEWER
                                assetChoices[assetIdx] = new GUIContent(terrain.terrainData.terrainLayers[assetIdx].diffuseTexture.name);
#else
                            assetChoices[assetIdx] = new GUIContent(terrain.terrainData.splatPrototypes[assetIdx].texture.name);
#endif
                            }
                            int oldIdx = res.TerrainProtoIdx;
                            res.TerrainProtoIdx = m_editorUtils.Popup("Texture", res.TerrainProtoIdx, assetChoices, helpEnabled);
                            if (res.TerrainProtoIdx != oldIdx)
                            {
                                //res.TexturePrototypeData = GeNaSpawner.UpdateTexturePrototypeData(terrain.terrainData.terrainLayers[res.TerrainProtoIdx]);
                                TerrainLayer terrainLayer = terrain.terrainData.terrainLayers[res.TerrainProtoIdx];
                                res.AddTerrainLayerAsset(terrainLayer.diffuseTexture, m_spawner.Palette);
                                //res.AssetID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(res.TexturePrototypeData.terrainLayerAsset.diffuseTexture));
#if UNITY_2018_3_OR_NEWER
                                res.Name = terrain.terrainData.terrainLayers[res.TerrainProtoIdx].diffuseTexture.name;
#else
                            res.Name = terrain.terrainData.splatPrototypes[res.TerrainProtoIdx].texture.name;
#endif
                                proto.Name = res.Name;
                            }

                            // Shape
                            res.BrushIndex = m_editorUtils.BrushSelectionGrid("TxBrush Shape",
                                res.BrushIndex,
                                out _,
                                res.BrushTextureArray,
                                res.AddBrushTexture,
                                res.RemoveBrushTexture,
                                res.ClearBrushTextures,
                                helpEnabled);
                            res.SameScale = true;
                            // Calculate the minimum and a maximum 100 final scale
                            TerrainData terrainData = terrain.terrainData;
                            float splatPixelSize = terrainData.size.x / terrainData.alphamapResolution;
                            int lowerScaleLimit = Mathf.CeilToInt((Constants.MIN_TX_BRUSH_SIZE_IN_PIX * splatPixelSize) / (m_placementCriteria.SameScale ? m_placementCriteria.MinScale.x : 0.5f * (m_placementCriteria.MinScale.x + m_placementCriteria.MinScale.z)));
                            int higherScaleLimit = Mathf.FloorToInt((100f * splatPixelSize) / (m_placementCriteria.SameScale ? m_placementCriteria.MaxScale.x : 0.5f * (m_placementCriteria.MaxScale.x + m_placementCriteria.MaxScale.z)));
                            int minScaleInt = (int) res.MinScale.x;
                            int maxScaleInt = (int) res.MaxScale.x;
                            m_editorUtils.MinMaxSliderWithFields("Texture Size", ref minScaleInt, ref maxScaleInt, lowerScaleLimit, higherScaleLimit, helpEnabled);
                            minScale.x = minScaleInt;
                            maxScale.x = maxScaleInt;
                            res.Opacity = 0.01f * m_editorUtils.Slider("Opacity", res.Opacity * 100f, 0, 100f, helpEnabled);
                            res.TargetStrength = m_editorUtils.Slider("Target Strength", res.TargetStrength, 0, 1f, helpEnabled);
                        }
                        break;
                    default:
                        throw new NotImplementedException("Not sure what to do with ResourceType '" + res.Type + "'");
                }
                EditOneChildOf(proto, res, helpEnabled);
                EditDecorators(res);
            }
            if (EditorGUI.EndChangeCheck())
            {
                isChanged = true;
            }
            // Keep traversing down the tree
            ChildResources(proto, res, helpEnabled, advanced);
            res.SnapToGround = snapToGround;
            res.ConformToSlope = conformToSlope;
            res.BasePosition = basePosition;
            res.BaseRotation = baseRotation;
            res.BaseScale = baseScale;
            res.BaseSize = baseSize;
            res.MinRotation = minRotation;
            res.MaxRotation = maxRotation;
            res.MinOffset = minOffset;
            res.MaxOffset = maxOffset;
            res.MinScale = minScale;
            res.MaxScale = maxScale;
            if (child)
                EditorGUI.indentLevel--;
            if (isChanged)
            {
                m_spawnerData.IsDirty = true;
            }
        }
        /// <summary>
        /// Displays the Children of the Resource in a Resource Tree, if there are any.
        /// </summary>
        private void ChildResources(Prototype proto, Resource res, bool helpEnabled, bool advanced)
        {
            List<Resource> children = proto.GetChildren(res);
            if (children == null)
                return;
            // Child Resources (if Resource tree)
            foreach (Resource child in children)
            {
                string childName = child.Name;
                switch (child.Type)
                {
                    case Constants.ResourceType.Prefab:
                        childName += child.ContainerOnly ? " (G)" : " (P)";
                        if (child.ConformToSlope)
                            childName += " *C*";
                        break;
                    case Constants.ResourceType.TerrainTree:
                        childName += " (T)";
                        break;
                    case Constants.ResourceType.TerrainGrass:
                        childName += " (G)";
                        break;
                    case Constants.ResourceType.TerrainTexture:
                        childName += " (Tx)";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                childName += string.Format(" {0:0}%", child.SuccessRate * 100f);
                GUILayout.BeginVertical(Styles.gpanel);
                {
                    GUILayout.BeginHorizontal((child.Static == Constants.ResourceStatic.Dynamic) ? Styles.dynamicResHeader : Styles.staticResHeader);
                    {
                        child.OpenedInGuiHierarchy = EditorGUILayout.Foldout(child.OpenedInGuiHierarchy, childName, true, Styles.resTreeFoldout);
                        GUILayout.FlexibleSpace();
                        StaticSwitch(proto, child);
                    }
                    GUILayout.EndHorizontal();
                    if (child.OpenedInGuiHierarchy)
                    {
                        // Proto won't be changed since we can't mix terrain resources into the mix.
                        // If we do later we can avoid changes being made to the proto by m_children of trees.
                        EditResource(proto, child, true, helpEnabled, advanced);
                    }
                }
                GUILayout.EndVertical();
            }
        }
        public bool ToggleNonLocalized(string label, bool value)
        {
            value = GUILayout.Toggle(value, label);
            return value;
        }
        /// <summary>
        /// Draw a prefab field that handles prefab replacement
        /// </summary>
        /// <param name="res"></param>
        /// <param name="helpEnabled"></param>
        private void PrefabField(Resource res, bool helpEnabled)
        {
            GameObject prefab = res.Prefab;
            prefab = (GameObject) m_editorUtils.ObjectField("Prefab", prefab, typeof(GameObject), false, helpEnabled);
            if (prefab != res.Prefab)
            {
                if (prefab != null)
                    ReplaceResourcePrefab(res, prefab);
                else
                    Debug.LogWarningFormat("[GeNa] Prefab was set to null for Resource [{0}]. This is an invalid operation and was ignored. You can delete the Resource or replace it with a blank GameObject if you wish.", res.Name);
            }
        }
        /// <summary>
        /// Draw the static switch for a res
        /// </summary>
        private bool StaticSwitch(Prototype proto, Resource res)
        {
            // We only use this for prefab resources
            if (res.Type == Constants.ResourceType.Prefab)
            {
                Constants.ResourceStatic val = res.Static;
                val = (Constants.ResourceStatic) m_editorUtils.Toolbar((int) res.Static, Enum.GetNames(typeof(Constants.ResourceStatic)), GUILayout.ExpandWidth(false));
                if (val != res.Static)
                    res.SetStatic(proto, val);
                return true;
            }
            return false;
        }
        private GameObject tempSpawnerObject = null;
        /// <summary>
        /// Handle drop area for new objects
        /// </summary>
        public bool DrawPrefabGUI()
        {
            // Ok - set up for drag and drop
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            string dropMsg = m_editorUtils.GetTextValue("Add proto drop box msg");
            GUI.Box(dropArea, dropMsg, Styles.gpanel);
            if (evt.type == EventType.DragPerform || evt.type == EventType.DragUpdated)
            {
                if (!dropArea.Contains(evt.mousePosition))
                    return false;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    List<GameObject> resources = new List<GameObject>();
                    // Handle game objects / prefabs
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject go)
                        {
                            GameObject geNaSpawnerPrefab = GeNaEditorUtility.GetPrefabAsset(go);
                            GeNaSpawner geNaSpawner = geNaSpawnerPrefab != null ? geNaSpawnerPrefab.GetComponent<GeNaSpawner>() : null;
                            if (geNaSpawnerPrefab != null && geNaSpawner != null && geNaSpawner != m_spawner)
                            {
                                // We dont want to spawn spawners
                                // Debug.LogWarning("You can not add spawners.");
                                tempSpawnerObject = new GameObject(geNaSpawner.name);
                                GeNaSubSpawnerDecorator subSpawnerDecorator = tempSpawnerObject.AddComponent<GeNaSubSpawnerDecorator>();
                                subSpawnerDecorator.SubSpawner = geNaSpawner;
                                resources.Add(tempSpawnerObject);
                            }
                            else if (m_spawner != null)
                                resources.Add(go);
                        }
                    }
                    // Handle speedtrees
                    foreach (string path in DragAndDrop.paths)
                    {
                        // Update in case unity has messed with it 
                        if (path.StartsWith("Assets"))
                        {
                            // Check file type and process as we can
                            string fileType = Path.GetExtension(path).ToLower();
                            // Check for speed trees - and add them
                            if (fileType == ".spm")
                            {
                                GameObject speedTree = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                                if (speedTree != null)
                                    resources.Add(speedTree);
                                else
                                    Debug.LogWarning("Unable to load " + path);
                            }
                        }
                    }

                    // Start managing them
                    AddGameObjects(m_spawnerData, resources);
                    if (tempSpawnerObject != null)
                    {
                        GeNaEvents.Destroy(tempSpawnerObject);
                    }
                    return true;
                }
            }
            return false;
        }
        #endregion
        #region Utilities
        /// <summary>
        /// Deletes the editor prefs key
        /// </summary>
        public void DeleteEditorPrefsKeys()
        {
            EditorPrefs.DeleteKey("GeNa Performance Rating");
            EditorPrefs.DeleteKey("GeNa Performance Rating Time");
        }
        /// <summary>
        /// Add new terrain tree to the terrain
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        private static int AddTreeResourceToTerrain(GameObject prefab, Terrain terrain)
        {
            int ID = 0;
            if (prefab != null && terrain != null)
            {
                TreePrototype[] prototypes = terrain.terrainData.treePrototypes;
                int index = prototypes.Length;
                TreePrototype[] newPrototypes = new TreePrototype[index + 1];
                for (int i = 0; i < newPrototypes.Length; i++)
                {
                    if (newPrototypes[i] == null)
                    {
                        newPrototypes[i] = new TreePrototype();
                    }
                    if (i != prototypes.Length)
                    {
                        newPrototypes[i].prefab = prototypes[i].prefab;
                    }
                    else
                    {
                        newPrototypes[i].prefab = prefab;
                    }
                }
                terrain.terrainData.treePrototypes = newPrototypes;
                ID = GetTreeID(newPrototypes, prefab);
                MarkObjectDirty(terrain);
            }
            return ID;
        }
        /// <summary>
        /// Add new terrain grass to the terrain
        /// </summary>
        /// <param name="activeDetailPrototype"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        private static int AddGrassResourceToTerrain(TerrainDetailPrototypeData activeDetailPrototype, Terrain terrain)
        {
            int ID = 0;
            if (activeDetailPrototype != null && terrain != null)
            {
                DetailPrototype[] prototypes = terrain.terrainData.detailPrototypes;
                int index = prototypes.Length;
                DetailPrototype[] newPrototypes = new DetailPrototype[index + 1];
                for (int i = 0; i < newPrototypes.Length; i++)
                {
                    if (newPrototypes[i] == null)
                    {
                        newPrototypes[i] = new DetailPrototype();
                    }
                    if (i != prototypes.Length)
                    {
#if !UNITY_2020_2_OR_NEWER
                        newPrototypes[i].bendFactor = prototypes[i].bendFactor;
#endif
                        newPrototypes[i].prototypeTexture = prototypes[i].prototypeTexture;
                        newPrototypes[i].dryColor = prototypes[i].dryColor;
                        newPrototypes[i].healthyColor = prototypes[i].healthyColor;
                        newPrototypes[i].maxHeight = prototypes[i].maxHeight;
                        newPrototypes[i].maxWidth = prototypes[i].maxWidth;
                        newPrototypes[i].minHeight = prototypes[i].minHeight;
                        newPrototypes[i].minWidth = prototypes[i].minWidth;
                        newPrototypes[i].noiseSpread = prototypes[i].noiseSpread;
                        newPrototypes[i].prototype = prototypes[i].prototype;
                        newPrototypes[i].renderMode = prototypes[i].renderMode;
                        newPrototypes[i].usePrototypeMesh = prototypes[i].usePrototypeMesh;
                    }
                    else
                    {
#if !UNITY_2020_2_OR_NEWER
                        newPrototypes[i].bendFactor = activeDetailPrototype.bendFactor;
#endif
                        newPrototypes[i].prototypeTexture = activeDetailPrototype.prototypeTexture;
                        newPrototypes[i].dryColor = activeDetailPrototype.dryColor;
                        newPrototypes[i].healthyColor = activeDetailPrototype.healthyColor;
                        newPrototypes[i].maxHeight = activeDetailPrototype.maxHeight;
                        newPrototypes[i].maxWidth = activeDetailPrototype.maxWidth;
                        newPrototypes[i].minHeight = activeDetailPrototype.minHeight;
                        newPrototypes[i].minWidth = activeDetailPrototype.minWidth;
                        newPrototypes[i].noiseSpread = activeDetailPrototype.noiseSpread;
                        newPrototypes[i].prototype = activeDetailPrototype.prototype;
                        newPrototypes[i].renderMode = activeDetailPrototype.renderMode;
                        newPrototypes[i].usePrototypeMesh = activeDetailPrototype.usePrototypeMesh;
                    }
                }
                terrain.terrainData.detailPrototypes = newPrototypes;
                ID = GetGrassID(newPrototypes, activeDetailPrototype.prototypeTexture);
                MarkObjectDirty(terrain);
            }
            return ID;
        }
        /// <summary>
        /// Add new terrain grass to the terrain
        /// </summary>
        /// <param name="activeTexturePrototype"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        private static int AddTextureResourceToTerrain(TerrainTexturePrototypeData activeTexturePrototype, Terrain terrain)
        {
            int ID = 0;
            if (activeTexturePrototype != null && terrain != null)
            {
                TerrainLayer[] prototypes = terrain.terrainData.terrainLayers;
                int index = prototypes.Length;
                TerrainLayer[] newPrototypes = new TerrainLayer[index + 1];
                for (int i = 0; i < newPrototypes.Length; i++)
                {
                    if (newPrototypes[i] == null)
                    {
                        newPrototypes[i] = new TerrainLayer();
                    }
                    if (i != prototypes.Length)
                    {
                        newPrototypes[i].diffuseTexture = prototypes[i].diffuseTexture;
                        newPrototypes[i].diffuseRemapMax = prototypes[i].diffuseRemapMax;
                        newPrototypes[i].diffuseRemapMin = prototypes[i].diffuseRemapMin;
                        newPrototypes[i].maskMapRemapMax = prototypes[i].maskMapRemapMax;
                        newPrototypes[i].maskMapRemapMin = prototypes[i].maskMapRemapMin;
                        newPrototypes[i].maskMapTexture = prototypes[i].maskMapTexture;
                        newPrototypes[i].metallic = prototypes[i].metallic;
                        newPrototypes[i].normalMapTexture = prototypes[i].normalMapTexture;
                        newPrototypes[i].normalScale = prototypes[i].normalScale;
                        newPrototypes[i].smoothness = prototypes[i].smoothness;
                        newPrototypes[i].specular = prototypes[i].specular;
                        newPrototypes[i].tileOffset = prototypes[i].tileOffset;
                        newPrototypes[i].tileSize = prototypes[i].tileSize;
                    }
                    else
                    {
                        TerrainLayer terrainLayer = new TerrainLayer();
                        terrainLayer.diffuseTexture = activeTexturePrototype.terrainTexture2DAsset;
                        newPrototypes[i] = terrainLayer;
                    }
                }
                terrain.terrainData.terrainLayers = newPrototypes;
                ID = GetTextureID(newPrototypes, activeTexturePrototype.terrainTexture2DAsset);
                MarkObjectDirty(terrain);
            }
            return ID;
        }
        /// <summary>
        /// Gets the tree ID
        /// </summary>
        /// <param name="prototypes"></param>
        /// <param name="treePrefab"></param>
        /// <returns></returns>
        private static int GetTreeID(TreePrototype[] prototypes, GameObject treePrefab)
        {
            int ID = -1;
            for (int i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].prefab == treePrefab)
                {
                    ID = i;
                    break;
                }
            }
            return ID;
        }
        /// <summary>
        /// Gets the grass ID
        /// </summary>
        /// <param name="prototypes"></param>
        /// <param name="grassTexture"></param>
        /// <returns></returns>
        private static int GetGrassID(DetailPrototype[] prototypes, Texture2D grassTexture)
        {
            int ID = -1;
            for (int i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].prototypeTexture == grassTexture)
                {
                    ID = i;
                    break;
                }
            }
            return ID;
        }
        /// <summary>
        /// Gets the grass ID
        /// </summary>
        /// <param name="prototypes"></param>
        /// <param name="terrainTexture"></param>
        /// <returns></returns>
        private static int GetTextureID(TerrainLayer[] prototypes, Texture2D terrainTexture)
        {
            int ID = -1;
            for (int i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].diffuseTexture == terrainTexture)
                {
                    ID = i;
                    break;
                }
            }
            return ID;
        }
        /// <summary>
        /// Checks if the tree prototype is already on the terrain.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        private static bool IsTreeOnTerrain(GameObject prefab, Terrain terrain)
        {
            if (prefab == null || terrain == null)
            {
                return true;
            }
            TreePrototype[] prototypes = terrain.terrainData.treePrototypes;
            foreach (var prototype in prototypes)
            {
                if (prototype.prefab == prefab)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Checks if the grass prototype is already on the terrain
        /// </summary>
        /// <param name="grassTexture"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        private static bool IsGrassOnTerrain(Texture2D grassTexture, Terrain terrain)
        {
            if (grassTexture == null || terrain == null)
            {
                return true;
            }
            DetailPrototype[] prototypes = terrain.terrainData.detailPrototypes;
            foreach (var prototype in prototypes)
            {
                if (prototype.prototypeTexture == grassTexture)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Checks if the grass prototype is already on the terrain
        /// </summary>
        /// <param name="terrainTexture"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        private static bool IsTextureOnTerrain(Texture2D terrainTexture, Terrain terrain)
        {
            if (terrainTexture == null || terrain == null)
            {
                return true;
            }
            TerrainLayer[] prototypes = terrain.terrainData.terrainLayers;
            foreach (var prototype in prototypes)
            {
                if (prototype.diffuseTexture == terrainTexture)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Marks the object dirty
        /// </summary>
        /// <param name="systemObject"></param>
        private static void MarkObjectDirty(Object systemObject)
        {
            EditorUtility.SetDirty(systemObject);
        }
        /// <summary>
        /// Checks to see if the asset is null
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        private static bool CheckForAsset(GameObject asset)
        {
            if (asset == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="fileName">File name to search for</param>
        /// <returns></returns>
        public static string GetAssetPath(string fileName)
        {
            string fName = Path.GetFileNameWithoutExtension(fileName);
            string[] assets = AssetDatabase.FindAssets(fName, null);
            foreach (var asset in assets)
            {
                string path = AssetDatabase.GUIDToAssetPath(asset);
                if (Path.GetFileName(path) == fileName)
                {
                    return path;
                }
            }
            return "";
        }
        /// <summary>
        /// Get a unique name
        /// </summary>
        /// <param name="name">The original name</param>
        /// <param name="names">The names dictionary</param>
        /// <returns>The new unique name</returns>
        private static string GetUniqueName(string name, ref HashSet<string> names)
        {
            int idx = 0;
            string newName = name;
            while (names.Contains(newName))
            {
                newName = name + " " + idx.ToString();
                idx++;
            }
            names.Add(newName);
            return newName;
        }
        /// <summary>
        /// Returns the formatted time since the UndoRecord was recorded.
        /// </summary>
        /// <returns></returns>
        public static string GetTimeDelta()
        {
            TimeSpan delta = TimeSpan.FromSeconds(Utils.GetFrapoch()); // - record.Time);
            return string.Format("{0}{1}m", (int) delta.TotalHours > 0 ? (int) delta.TotalHours + "h " : "", delta.Minutes);
        }
        /// <summary>
        /// Return true if the resource list provided has prefabs
        /// </summary>
        /// <param name="sourcePrototypes"></param>
        /// <returns></returns>
        public static bool HasPrefabs(IEnumerable<Prototype> sourcePrototypes)
        {
            return sourcePrototypes.Any(srcProto => srcProto.HasType(Constants.ResourceType.Prefab));
        }
        /// <summary>
        /// Return true if the resource list provided has trees
        /// </summary>
        /// <param name="sourcePrototypes"></param>
        /// <returns></returns>
        public static bool HasTrees(IEnumerable<Prototype> sourcePrototypes)
        {
            return sourcePrototypes.Any(srcProto => srcProto.HasType(Constants.ResourceType.TerrainTree));
        }
        /// <summary>
        /// Return true if the resource list provided has textures
        /// </summary>
        /// <param name="sourcePrototypes"></param>
        /// <returns></returns>
        public static bool HasTextures(IEnumerable<Prototype> sourcePrototypes)
        {
            return sourcePrototypes.Any(srcProto => srcProto.HasType(Constants.ResourceType.TerrainTexture));
        }
        public static void MakeTextureUncompressed(Texture2D texture)
        {
            if (texture == null)
                return;
            string assetPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null && tImporter.textureCompression != TextureImporterCompression.Uncompressed)
            {
                tImporter.textureCompression = TextureImporterCompression.Uncompressed;
                tImporter.SaveAndReimport();
                AssetDatabase.Refresh();
            }
        }
        public static Texture2D DuplicateTexture(Texture2D source)
        {
            byte[] pix = source.GetRawTextureData();
            Texture2D readableText = new Texture2D(source.width, source.height, source.format, source.mipmapCount, true);
            readableText.LoadRawTextureData(pix);
            readableText.Apply();
            return readableText;
        }
        public static Texture2D MakeTextureReadable(Texture2D texture)
        {
            if (texture == null)
                return null;
            if (texture.isReadable)
                return texture;
            return DuplicateTexture(texture);

            //byte[] tmp = texture.GetRawTextureData();
            //Texture2D tmpTexture = new Texture2D(texture.width, texture.height);
            //tmpTexture.LoadRawTextureData(tmp);
            //return tmpTexture;
            //string assetPath = AssetDatabase.GetAssetPath(texture);
            //var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            //if (tImporter != null && tImporter.isReadable != true)
            //{
            //    tImporter.isReadable = true;
            //    tImporter.SaveAndReimport();
            //    AssetDatabase.Refresh();
            //}
        }
        /// <summary>
        /// Process prefab replacement
        /// </summary>
        private void ReplaceResourcePrefab(Resource resource, GameObject go)
        {
            resource.Name = go.name;
            // Get bounds
            Bounds localColliderBounds = GeNaUtility.GetLocalObjectBounds(go);
            // Get colliders
            resource.HasRootCollider = GeNaUtility.HasRootCollider(go);
            resource.HasColliders = GeNaUtility.HasColliders(go);
            // Get meshes
            resource.HasMeshes = GeNaUtility.HasMeshes(go);
            // Get rigid body
            resource.HasRigidbody = GeNaUtility.HasRigidBody(go);
            // If top level resource
            resource.BasePosition = resource.ParentID == -1 ? Vector3.zero : go.transform.localPosition;
            resource.BaseRotation = go.transform.localEulerAngles;
            resource.BaseScale = go.transform.localScale;
            resource.BaseColliderCenter = localColliderBounds.center;
            resource.BaseColliderScale = localColliderBounds.size;
            if (GeNaUtility.ApproximatelyEqual(go.transform.localScale.x, go.transform.localScale.y, 0.000001f) &&
                GeNaUtility.ApproximatelyEqual(go.transform.localScale.x, go.transform.localScale.z, 0.000001f))
                resource.SameScale = true;
            else
                resource.SameScale = false;
            // We can only determine if it is a prefab in the editor
            if (GeNaEditorUtility.IsPrefab(go))
            {
#if UNITY_2018_3_OR_NEWER
                resource.Prefab = GeNaEditorUtility.GetPrefabAsset(go);
                if (resource.Prefab == null)
                    resource.Prefab = go;
#else
                if (PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance)
                {
                    resource.Prefab = GetPrefabAsset(go);
                }
                else
                {
                    resource.Prefab = go;
                }
#endif
                if (resource.Prefab != null)
                {
                    //Get its asset ID
                    string path = AssetDatabase.GetAssetPath(resource.Prefab);
                    if (!string.IsNullOrEmpty(path))
                    {
                        resource.AssetID = AssetDatabase.AssetPathToGUID(path);
                        resource.AssetName = GeNaEditorUtility.GetAssetName(path);
                    }

                    // Get flags
                    SpawnFlags spawnFlags = resource.SpawnFlags;
                    StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(resource.Prefab);
                    spawnFlags.FlagBatchingStatic = (flags & StaticEditorFlags.BatchingStatic) == StaticEditorFlags.BatchingStatic;
#if UNITY_5 || UNITY_2017 || UNITY_2018 || UNITY_2019_1
                    spawnFlags.FlagLightmapStatic = (flags & StaticEditorFlags.LightmapStatic) == StaticEditorFlags.LightmapStatic;
#else
                    spawnFlags.FlagLightmapStatic = (flags & StaticEditorFlags.ContributeGI) == StaticEditorFlags.ContributeGI;
#endif
                    spawnFlags.FlagNavigationStatic = (flags & StaticEditorFlags.NavigationStatic) == StaticEditorFlags.NavigationStatic;
                    spawnFlags.FlagOccludeeStatic = (flags & StaticEditorFlags.OccludeeStatic) == StaticEditorFlags.OccludeeStatic;
                    spawnFlags.FlagOccluderStatic = (flags & StaticEditorFlags.OccluderStatic) == StaticEditorFlags.OccluderStatic;
                    spawnFlags.FlagOffMeshLinkGeneration = (flags & StaticEditorFlags.OffMeshLinkGeneration) == StaticEditorFlags.OffMeshLinkGeneration;
                    spawnFlags.FlagReflectionProbeStatic = (flags & StaticEditorFlags.ReflectionProbeStatic) == StaticEditorFlags.ReflectionProbeStatic;
                }
                else
                    Debug.LogErrorFormat("Unable to get prefab for '{0}'", resource.Name);
            }
            //Else this is just a GO (container in the tree) not a prefab.
            else
            {
                resource.ContainerOnly = true;
                // Warn the user if it has more components than just the Transform since it's not a prefab.
                Component[] components = go.GetComponents<Component>();
                if (components != null && components.Length > 1)
                {
                    Debug.LogWarningFormat("[GeNa]: Warning! Gameobject '{0}' has Components but it's not a Prefab Instance. Make it into a Prefab if you wish to keep its Components information for spawning.",
                        go.name);
                }
            }
            resource.RecalculateBounds();
        }
        #endregion
        #region Validation
        /// <summary>
        /// Validates all the aseets in the spawner
        /// </summary>
        /// <param name="genaSpawner"></param>
        /// <param name="spawner"></param>
        /// <param name="terrain"></param>
        /// <param name="overrideSceneLoaded"></param>
        public static void ValidateSpawnerPrototypes(GeNaSpawner genaSpawner, GeNaSpawnerData spawner, Terrain terrain, bool overrideSceneLoaded = false)
        {
            if (genaSpawner == null)
            {
                return;
            }
            if (!genaSpawner.gameObject.scene.isLoaded && overrideSceneLoaded == false)
            {
                return;
            }
            if (spawner == null)
                return;
            // ValidatePrefabPrototypes(spawner);
            ValidateTerrainPrototypes(spawner, terrain);
            ValidateTerrainGrassPrototypes(spawner, terrain);
            ValidateTerrainTexturePrototypes(spawner, terrain);
            List<GeNaSpawnerData> subSpawners = new List<GeNaSpawnerData>();
            foreach (Prototype prototype in spawner.SpawnPrototypes)
            {
                foreach (Resource resource in prototype.GetResources())
                {
                    if (resource.SubSpawnerData != null)
                    {
                        subSpawners.Add(resource.SubSpawnerData);
                    }
                }
            }
            ValidateSubSpawners(subSpawners, terrain);
        }
        /// <summary>
        /// Validate all terrain trees in the spawner
        /// </summary>
        /// <param name="spawner"></param>
        /// <param name="terrain"></param>
        private static void ValidateTerrainPrototypes(GeNaSpawnerData spawner, Terrain terrain)
        {
            if (spawner == null)
            {
                return;
            }
            if (terrain != null)
            {
                List<Prototype> prototypes = spawner.SpawnPrototypes;
                foreach (Prototype prototype in prototypes)
                {
                    IReadOnlyList<Resource> resources = prototype.GetResources();
                    foreach (Resource resource in resources)
                    {
                        if (!m_noToAllValidations)
                        {
                            if (resource.Type == Constants.ResourceType.TerrainTree)
                            {
                                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(resource.AssetID));
                                if (prefab == null)
                                {
                                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetAssetPath(resource.AssetName + "Prefab"));
                                }
                                if (!IsTreeOnTerrain(prefab, terrain))
                                {
                                    if (!m_yesToAllValidations)
                                    {
                                        if (EditorUtility.DisplayDialog("Add Resources", "The terrain trees in this spawner are not found on the terrain. The spawner may not function correctly if the trees are not on the terrain, we recommend that you add them. Would you like to add them?", "Yes", "No"))
                                        {
                                            m_yesToAllValidations = true;
                                        }
                                        else
                                        {
                                            m_noToAllValidations = true;
                                        }
                                    }
                                    if (m_yesToAllValidations)
                                    {
                                        resource.TerrainProtoIdx = AddTreeResourceToTerrain(prefab, terrain);
                                    }
                                }
                                else
                                {
                                    resource.TerrainProtoIdx = GetTreeID(terrain.terrainData.treePrototypes, prefab);
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Validate all prefabs in the spawner
        /// </summary>
        /// <param name="spawner"></param>
        private static void ValidatePrefabPrototypes(GeNaSpawnerData spawner)
        {
            if (spawner == null)
            {
                return;
            }
            List<Prototype> prototypes = spawner.SpawnPrototypes;
            foreach (Prototype prototype in prototypes)
            {
                IReadOnlyList<Resource> resources = prototype.GetResources();
                foreach (Resource resource in resources)
                {
                    if (resource.Type == Constants.ResourceType.Prefab)
                    {
                        if (!CheckForAsset(resource.Prefab))
                        {
                            resource.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(resource.AssetID));
                            if (resource.Prefab == null)
                            {
                                resource.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetAssetPath(resource.AssetName + "prefab"));
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Validate sub spawners
        /// </summary>
        /// <param name="subSpawners"></param>
        /// <param name="terrain"></param>
        private static void ValidateSubSpawners(List<GeNaSpawnerData> subSpawners, Terrain terrain)
        {
            if (subSpawners.Count < 1)
            {
                return;
            }
            foreach (GeNaSpawnerData subspawner in subSpawners)
            {
                //ValidatePrefabPrototypes(subspawner);
                ValidateTerrainPrototypes(subspawner, terrain);
                ValidateTerrainGrassPrototypes(subspawner, terrain);
                ValidateTerrainTexturePrototypes(subspawner, terrain);
            }
        }
        /// <summary>
        /// Validates all terrains grass in the spawner
        /// </summary>
        /// <param name="spawner"></param>
        /// <param name="terrain"></param>
        private static void ValidateTerrainGrassPrototypes(GeNaSpawnerData spawner, Terrain terrain)
        {
            if (spawner == null)
            {
                return;
            }
            if (terrain != null)
            {
                List<Prototype> prototypes = spawner.SpawnPrototypes;
                foreach (Prototype prototype in prototypes)
                {
                    IReadOnlyList<Resource> resources = prototype.GetResources();
                    foreach (Resource resource in resources)
                    {
                        if (!m_noToAllValidations)
                        {
                            if (resource.Type == Constants.ResourceType.TerrainGrass)
                            {
                                Texture2D grassTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(resource.AssetID));
                                if (grassTexture == null)
                                {
                                    grassTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(GetAssetPath(resource.Name));
                                }
                                if (!IsGrassOnTerrain(grassTexture, terrain))
                                {
                                    if (!m_yesToAllValidations)
                                    {
                                        if (EditorUtility.DisplayDialog("Add Resources", "The terrain grasses in this spawner are not found on the terrain. The spawner may not function correctly if the grasses are not on the terrain, we recommend that you add them. Would you like to add them?", "Yes", "No"))
                                        {
                                            m_yesToAllValidations = true;
                                        }
                                        else
                                        {
                                            m_noToAllValidations = true;
                                        }
                                    }
                                    if (m_yesToAllValidations)
                                    {
                                        resource.TerrainProtoIdx = AddGrassResourceToTerrain(resource.DetailPrototypeData, terrain);
                                    }
                                }
                                else
                                {
                                    resource.TerrainProtoIdx = GetGrassID(terrain.terrainData.detailPrototypes, grassTexture);
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Validates all terrain textures in the spawner
        /// </summary>
        /// <param name="spawner"></param>
        /// <param name="terrain"></param>
        private static void ValidateTerrainTexturePrototypes(GeNaSpawnerData spawner, Terrain terrain)
        {
            if (spawner == null)
            {
                return;
            }
            if (terrain != null)
            {
                List<Prototype> prototypes = spawner.SpawnPrototypes;
                foreach (Prototype prototype in prototypes)
                {
                    IReadOnlyList<Resource> resources = prototype.GetResources();
                    foreach (Resource resource in resources)
                    {
                        if (!m_noToAllValidations)
                        {
                            if (resource.Type == Constants.ResourceType.TerrainTexture)
                            {
                                Texture2D terrainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(resource.AssetID));
                                if (terrainTexture == null)
                                {
                                    terrainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(GetAssetPath(resource.Name));
                                }
                                if (!IsTextureOnTerrain(terrainTexture, terrain))
                                {
                                    if (!m_yesToAllValidations)
                                    {
                                        if (EditorUtility.DisplayDialog("Add Resources", "The terrain textures in this spawner are not found on the terrain. The spawner may not function correctly if the textures are not on the terrain, we recommend that you add them. Would you like to add them?", "Yes", "No"))
                                        {
                                            m_yesToAllValidations = true;
                                        }
                                        else
                                        {
                                            m_noToAllValidations = true;
                                        }
                                    }
                                    if (m_yesToAllValidations)
                                    {
                                        resource.TerrainProtoIdx = AddTextureResourceToTerrain(resource.TexturePrototypeData, terrain);
                                    }
                                }
                                else
                                {
                                    resource.TerrainProtoIdx = GetTextureID(terrain.terrainData.terrainLayers, terrainTexture);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #endregion
    }
}
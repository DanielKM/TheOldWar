//Copyright(c)2021 Procedural Worlds Pty Limited 
using System;
using System.Collections.Generic;
using System.IO;
using PWCommon5;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Object = UnityEngine.Object;
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaMapBuilder))]
    public class GeNaMapBuilderEditor : GeNaEditor
    {
        #region Variables
        private bool m_showQuickStartPanel = true;
        private bool m_showAreaPanel = true;
        private bool m_showVisualizePanel = true;
        private bool m_showBuildPanel = true;
        private bool m_showRoadPanel = true;
        private bool m_showOverviewPanel = true;
        private GeNaMapBuilder m_mapBuilder;
        private static string details = string.Empty;
        private static Vector3 detailPos = Vector3.zero;
        private bool currentHit = false;
        private Vector3 currentAreaPoint = Vector3.zero;
        private int m_undoSteps = 0;
        // Reorderable List
        private ReorderableList m_spawnerReorderable;
        private MapBuilderEntry m_selectedEntry;
        #endregion
        #region Methods
        #region Events
        private void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, null, null);
            m_mapBuilder = target as GeNaMapBuilder;
            m_mapBuilder.transform.hideFlags = HideFlags.HideInInspector;
            Tools.hidden = true;
            CreateEntryList();
        }
        private void OnDisable()
        {
            m_editorUtils?.Dispose();
            m_editorUtils = null;
            Tools.hidden = false;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            m_editorUtils.GUIHeader();
            //m_editorUtils.GUINewsHeader();
            m_mapBuilder = target as GeNaMapBuilder;
            GUI.enabled = !m_mapBuilder.IsBusy;
            m_showQuickStartPanel = m_editorUtils.Panel("Quick Start", QuickStartPanel, m_showQuickStartPanel);
            m_showAreaPanel = m_editorUtils.Panel("AreaParametersLabel", AreaParametersPanel, m_showAreaPanel);
            m_showBuildPanel = m_editorUtils.Panel("BuildPanelLabel", TownBuildingParametersPanel, m_showBuildPanel);
            m_showRoadPanel = m_editorUtils.Panel("RoadPanelLabel", RoadParametersPanel, m_showRoadPanel);
            m_showVisualizePanel = m_editorUtils.Panel("VisualizePanelLabel", VisualizationParametersPanel, m_showVisualizePanel);
            m_showRoadPanel = m_editorUtils.Panel("ActionsPanelLabel", ActionsPanel, m_showOverviewPanel);
            // DrawActionPanel(m_mapBuilder);
            GUI.enabled = true;
            // DrawOverviewPanel(m_mapBuilder);
            GUI.enabled = true;
        }
        private void OnSceneGUI()
        {
            SceneView.RepaintAll();
            GeNaMapBuilder mapBuilder = target as GeNaMapBuilder;
            if (Event.current.type == EventType.MouseDown)
                details = string.Empty;
            if (Event.current.isMouse)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (mapBuilder.m_showElevationAtCursor)
                        details = $"{hit.point.y}";
                    else
                        details = string.Empty;
                    float distance = (hit.point - ray.origin).magnitude * 0.02f;
                    detailPos = hit.point + Camera.current.transform.right * distance +
                                Camera.current.transform.up * distance;
                    currentHit = true;
                    currentAreaPoint = hit.point;
                }
                else
                {
                    currentHit = false;
                    details = string.Empty;
                }
            }
            if (currentHit)
                CheckForArea(mapBuilder, currentAreaPoint);
            if (!string.IsNullOrEmpty(details))
            {
                Handles.color = mapBuilder.m_elevationTextColor;
                GUIStyle style = new GUIStyle();
                style.normal.textColor = mapBuilder.m_elevationTextColor;
                Handles.Label(detailPos, details, style);
            }
        }
        #endregion
        #region Utilities
        private void CheckForArea(GeNaMapBuilder mapBuilder, Vector3 point)
        {
            if (!mapBuilder.HasAreas)
                return;
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.MouseDown)
                return;
            for (int i = 0; i < mapBuilder.Areas.Count; i++)
            {
                if (mapBuilder.Areas[i].m_enabled == false)
                    continue;
                GeNaCircle geNaCircle = new GeNaCircle(mapBuilder.Areas[i].m_center, mapBuilder.Areas[i].m_radius);
                if (geNaCircle.Contains(point))
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        Handles.color = Color.white;
                        details = $"{currentAreaPoint.y}\nR={mapBuilder.Areas[i].m_radius}";
                        Handles.DrawWireDisc(mapBuilder.Areas[i].m_center, Vector3.up, mapBuilder.Areas[i].m_radius);
                    }
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && Event.current.control)
                    {
                        mapBuilder.Areas.RemoveAt(i);
                        mapBuilder.ApplyAreaRules();
                        GeNaEvents.MarkSceneDirty(m_mapBuilder.gameObject.scene);
                        Event.current.Use();
                    }
                    break;
                }
            }
        }
        public override bool RequiresConstantRepaint()
        {
            GeNaMapBuilder mapBuilder = target as GeNaMapBuilder;
            if (mapBuilder.IsBusy)
                return true;
            return false;
        }
        /// <summary>
        /// Returns the formatted time since the UndoRecord was recorded.
        /// </summary>
        /// <returns></returns>
        public static string GetTimeDelta(int recordTime)
        {
            TimeSpan delta = TimeSpan.FromSeconds(Utils.GetFrapoch() - recordTime); // - record.Time);
            return string.Format("{0}{1}m", (int) delta.TotalHours > 0 ? (int) delta.TotalHours + "h " : "", delta.Minutes);
        }
        #endregion
        #region Reorderable List
        private void CreateEntryList()
        {
            m_spawnerReorderable = new ReorderableList(m_mapBuilder.Entries, typeof(GeNaSpawner), true, true, true, true);
            m_spawnerReorderable.elementHeightCallback = OnElementHeightListEntry;
            m_spawnerReorderable.drawElementCallback = DrawEntryListElement;
            m_spawnerReorderable.drawHeaderCallback = DrawEntryListHeader;
            m_spawnerReorderable.onAddCallback = OnAddListEntry;
            m_spawnerReorderable.onRemoveCallback = OnRemoveListEntry;
            m_spawnerReorderable.onReorderCallback = OnReorderList;
        }
        private void OnReorderList(ReorderableList reorderableList)
        {
            //Do nothing, changing the order does not immediately affect anything in the stamper
        }
        private void OnRemoveListEntry(ReorderableList reorderableList)
        {
            MapBuilderEntry removeEntry = m_mapBuilder.Entries[reorderableList.index];
            if (removeEntry == m_selectedEntry)
                m_selectedEntry = null;
            int indexToRemove = reorderableList.index;
            m_mapBuilder.RemoveEntry(indexToRemove);
            reorderableList.list = m_mapBuilder.Entries;
            if (indexToRemove >= reorderableList.list.Count)
                indexToRemove = reorderableList.list.Count - 1;
            reorderableList.index = indexToRemove;
        }
        private void OnAddListEntry(ReorderableList reorderableList)
        {
            MapBuilderEntry extension = m_mapBuilder.AddEntry(null);
            reorderableList.list = m_mapBuilder.Entries;
            SelectExtensionEntry(extension);
        }
        private void DrawEntryListHeader(Rect rect)
        {
            DrawEntryListHeader(rect, true, m_mapBuilder.Entries, m_editorUtils);
        }
        private void DrawEntryListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            MapBuilderEntry entry = m_mapBuilder.Entries[index];
            DrawEntryListElement(rect, entry, m_editorUtils, isFocused);
        }
        private float OnElementHeightListEntry(int index)
        {
            return OnElementHeight();
        }
        public float OnElementHeight()
        {
            return EditorGUIUtility.singleLineHeight + 4f;
        }
        public void DrawEntryListHeader(Rect rect, bool currentFoldOutState, List<MapBuilderEntry> extensionList, EditorUtils editorUtils)
        {
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.LabelField(rect, editorUtils.GetContent("EntryHeader"));
            EditorGUI.indentLevel = oldIndent;
        }
        public void DrawExtensionList(ReorderableList list, EditorUtils editorUtils)
        {
            Rect maskRect = EditorGUILayout.GetControlRect(true, list.GetHeight());
            list.DoList(maskRect);
        }
        public void SelectAllExtensionEntries()
        {
            foreach (MapBuilderEntry entry in m_mapBuilder.Entries)
                entry.IsSelected = true;
        }
        public void DeselectAllExtensionEntries()
        {
            foreach (MapBuilderEntry entry in m_mapBuilder.Entries)
                entry.IsSelected = false;
        }
        public void SelectExtensionEntry(int entryIndex)
        {
            if (entryIndex < 0 || entryIndex >= m_mapBuilder.Entries.Count)
                return;
            SelectExtensionEntry(m_mapBuilder.Entries[entryIndex]);
        }
        public void SelectExtensionEntry(MapBuilderEntry entryToSelect)
        {
            foreach (MapBuilderEntry entry in m_mapBuilder.Entries)
            {
                if (entry == entryToSelect)
                    continue;
                entry.IsSelected = false;
            }
            entryToSelect.IsSelected = true;
            // if (m_selectedExtensionEditor != null)
            //     m_selectedExtensionEditor.OnDeselected();
            // m_selectedExtensionEntry = entryToSelect;
            // m_selectedExtensionEditor = CreateEditor(entryToSelect.Extension) as GeNaSplineExtensionEditor;
            // m_selectedExtension = entryToSelect.Extension;
            // int selectedExtensionIndex = m_spawnerReorderable.list.IndexOf(entryToSelect);
            // m_spawnerReorderable.index = selectedExtensionIndex;
            // m_spline.SelectedExtensionIndex = selectedExtensionIndex;
            // if (m_selectedExtensionEditor != null)
            //     m_selectedExtensionEditor.OnSelected();
        }
        public void DrawEntryListElement(Rect rect, MapBuilderEntry entry, EditorUtils editorUtils, bool isFocused)
        {
            if (isFocused)
            {
                if (m_selectedEntry != entry)
                {
                    DeselectAllExtensionEntries();
                    entry.IsSelected = true;
                    SelectExtensionEntry(entry);
                }
            }
            // Spawner Object
            EditorGUI.BeginChangeCheck();
            {
                bool defaultEnable = GUI.enabled;

                GUIContent activeLabel = editorUtils.GetContent("EntryActive");
                GUI.enabled = entry.Weighting > 0f && defaultEnable;
                Vector2 activeLabelSize = GUI.skin.label.CalcSize(activeLabel);
                Rect labelRect = new Rect(rect.x - activeLabelSize.x * .25f, rect.y + 1f, rect.width + activeLabelSize.x, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelRect, activeLabel);
                Rect toggleRect = new Rect(labelRect.x + activeLabelSize.x, rect.y, rect.width * 0.1f, EditorGUIUtility.singleLineHeight);
                Vector3 toggleSize = GUI.skin.toggle.CalcSize(GUIContent.none);
                bool isActive = EditorGUI.Toggle(toggleRect, entry.IsActive); //GUI.enabled ? entry.IsActive : false
                if (GUI.enabled)
                    entry.IsActive = isActive;
                GUI.enabled = isActive && defaultEnable;
                GUIContent weightingLabel = editorUtils.GetContent("EntryWeighting");
                Vector3 weightingLabelSize = GUI.skin.label.CalcSize(weightingLabel);
                Rect weightingLabelRect = new Rect(toggleRect.x + toggleSize.x, rect.y, rect.width + weightingLabelSize.x, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(weightingLabelRect, weightingLabel);
                Rect spawnerRect = new Rect(rect.x + rect.width * 0.6f, rect.y + 1f, rect.width * 0.4f, EditorGUIUtility.singleLineHeight);
                Rect weightingSliderRect = new Rect(weightingLabelRect.x + weightingLabelSize.x, rect.y, spawnerRect.x - (weightingLabelRect.x + weightingLabelSize.x - 10f), EditorGUIUtility.singleLineHeight);
                entry.Weighting = EditorGUI.Slider(weightingSliderRect, entry.Weighting, 0f, 1f);
                entry.Spawner = (GeNaSpawner) EditorGUI.ObjectField(spawnerRect, entry.Spawner, typeof(GeNaSpawner), true);
                GUI.enabled = defaultEnable;
            }
            if (EditorGUI.EndChangeCheck())
            {
                GeNaEvents.MarkSceneDirty(m_mapBuilder.gameObject.scene);
                SceneView.RepaintAll();
            }
        }
        /// <summary>
        /// Handle drop area for new objects
        /// </summary>
        public bool DrawSpawnerGUI()
        {
            // Ok - set up for drag and drop
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            string dropMsg = m_editorUtils.GetTextValue("Attach Spawner");
            GUI.Box(dropArea, dropMsg, Styles.gpanel);
            if (evt.type == EventType.DragPerform || evt.type == EventType.DragUpdated)
            {
                if (!dropArea.Contains(evt.mousePosition))
                    return false;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    //Handle game objects / prefabs
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject go)
                        {
                            GeNaSpawner spawner = go.GetComponent<GeNaSpawner>();
                            if (spawner != null)
                            {
                                spawner.Load();
                                MapBuilderEntry entry = m_mapBuilder.AddEntry(spawner);
                                SelectExtensionEntry(entry);
                            }
                        }
                    }
                    var gos = GeNaEditorUtility.GetAllPrefabsInPaths(DragAndDrop.paths);
                    foreach (var gameObject in gos)
                    {
                        if (gameObject is GameObject go)
                            if (PrefabUtility.IsPartOfAnyPrefab(go))
                            {
                                GeNaSpawner geNaSpawner = go.GetComponent<GeNaSpawner>();
                                if (geNaSpawner != null)
                                {
                                    MapBuilderEntry entry = m_mapBuilder.AddEntry(geNaSpawner);
                                    SelectExtensionEntry(entry);
                                }
                            }
                    }
                    return true;
                }
            }
            return false;
        }
        #endregion
        #region Panels
        private void QuickStartPanel(bool showHelp)
        {
            if (ActiveEditorTracker.sharedTracker.isLocked)
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("Inspector locked warning"), MessageType.Warning);
            EditorGUILayout.LabelField("Remove Areas: Ctrl + Right click.", Styles.wrappedText);
            if (m_editorUtils.Button("View Tutorials Btn"))
                Application.OpenURL(PWApp.CONF.TutorialsLink);
        }
        private void VisualizationParametersPanel(bool showHelp)
        {
            bool defaultEnable = GUI.enabled;
            EditorGUI.indentLevel++;
            m_mapBuilder.m_showElevationAtCursor = m_editorUtils.Toggle("Show Elevation", m_mapBuilder.m_showElevationAtCursor, showHelp);
            m_mapBuilder.m_elevationTextColor = m_editorUtils.ColorField("Text Color", m_mapBuilder.m_elevationTextColor, showHelp);
            EditorGUI.indentLevel--;
            GUI.enabled = defaultEnable;
        }
        private void AreaParametersPanel(bool showHelp)
        {
            bool defaultEnable = GUI.enabled;
            EditorGUI.indentLevel++;
            m_mapBuilder.m_minRadius = m_editorUtils.FloatField("Minimum Radius", m_mapBuilder.m_minRadius, showHelp);
            if (GeNaUtility.Gaia2Present)
            {
                EditorGUILayout.BeginHorizontal();
                m_mapBuilder.m_seaLevel = m_editorUtils.FloatField("Minimum Elevation", m_mapBuilder.m_seaLevel);
                if (m_editorUtils.Button("GetGaiaSeaLevel", GUILayout.MaxWidth(125f)))
                {
                    m_mapBuilder.m_seaLevel = GeNaEvents.GetSeaLevel(m_mapBuilder.m_seaLevel);
                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("Minimum Elevation", showHelp);
            }
            else
            {
                m_mapBuilder.m_seaLevel = m_editorUtils.FloatField("Minimum Elevation", m_mapBuilder.m_seaLevel, showHelp);
            }
            m_mapBuilder.m_maxElevation = m_editorUtils.FloatField("Maximum Elevation", m_mapBuilder.m_maxElevation, showHelp);
            m_mapBuilder.m_maxDeviation = m_editorUtils.FloatField("Maximum Deviation", m_mapBuilder.m_maxDeviation, showHelp);
            m_mapBuilder.m_minDistance = m_editorUtils.FloatField("Minimum Distance", m_mapBuilder.m_minDistance, showHelp);
            EditorGUI.BeginChangeCheck();
            m_mapBuilder.m_keepOverlapping = m_editorUtils.Toggle("Keep Overlapping", m_mapBuilder.m_keepOverlapping, showHelp);
            GUI.enabled = m_mapBuilder.m_keepOverlapping;
            EditorGUI.indentLevel++;
            m_mapBuilder.m_allowedOverlap = m_editorUtils.FloatField("Overlap Percent", m_mapBuilder.m_allowedOverlap, showHelp);
            if (EditorGUI.EndChangeCheck())
            {
                m_mapBuilder.ApplyAreaRules();
            }
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
            GUI.enabled = defaultEnable;
        }
        private void TownBuildingParametersPanel(bool showHelp)
        {
            bool defaultEnable = GUI.enabled;
            GUI.enabled = !m_mapBuilder.IsBusy;
            EditorGUI.indentLevel++;
            m_mapBuilder.m_lotSize = m_editorUtils.FloatField("Lot Size", m_mapBuilder.m_lotSize, showHelp);
            m_mapBuilder.m_flattenStrength = m_editorUtils.Slider("Flatten Strength", m_mapBuilder.m_flattenStrength, 0.0f, 1.0f, showHelp);
            m_mapBuilder.m_areaBuilder = (AreaBuilder) m_editorUtils.ObjectField("Area Builder", m_mapBuilder.m_areaBuilder, typeof(AreaBuilder), false, showHelp);
            DrawSpawnerGUI();
            //m_editorUtils.PropertyField("Building Spawners", serializedObject.FindProperty("m_buildingSpawners"), showHelp);
            Rect listRect = EditorGUILayout.GetControlRect(true, m_spawnerReorderable.GetHeight());
            m_spawnerReorderable.DoList(listRect);
            m_editorUtils.InlineHelp("Building Spawners", showHelp);
            EditorGUI.indentLevel--;
            GUI.enabled = defaultEnable;
        }
        private void RoadParametersPanel(bool showHelp)
        {
            bool defaultEnable = GUI.enabled;
            EditorGUI.indentLevel++;
            m_mapBuilder.m_roadIgnoreMask = EditorUtilsExtensions.LayerMaskField(m_editorUtils, "Ignore Mask", m_mapBuilder.m_roadIgnoreMask, showHelp);
            m_mapBuilder.m_maxGrade = m_editorUtils.Slider("Max Grade", m_mapBuilder.m_maxGrade, 1.0f, 30.0f, showHelp);
            m_mapBuilder.m_cellSize = m_editorUtils.IntSlider("Cell Size", m_mapBuilder.m_cellSize, 2, 16, showHelp);
            m_mapBuilder.m_slopeStrengthFactor = m_editorUtils.Slider("Slope Strength", m_mapBuilder.m_slopeStrengthFactor, 0.1f, 1.0f, showHelp);
            m_mapBuilder.m_splineTravelCostFactor = m_editorUtils.Slider("Spline Cost Factor", m_mapBuilder.m_splineTravelCostFactor, 0.1f, 1.0f, showHelp);
            m_mapBuilder.m_useHeuristicB = m_editorUtils.Toggle("Use Heuristic B", m_mapBuilder.m_useHeuristicB, showHelp);
            EditorGUI.indentLevel--;
            GUI.enabled = defaultEnable;
        }
        private void ActionsPanel(bool showHelp)
        {
            GUI.enabled = true;
            #region Undo
            bool undoEmpty = true;
            string undoBtnLabel = " (-)";
            // Undo 
            UndoRecord[] undoArray = m_mapBuilder.m_undoStack.ToArray();
            GUIContent[] undoLabelsArray = new GUIContent[undoArray.Length + 1];
            undoLabelsArray[0] = new GUIContent("Undo List");
            undoBtnLabel = string.Format(" ({0}/{1})", undoArray.Length, 50);
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
                            string.Format("{0}.  ({1}) [{2}] {3}", i + 1, burstIndex + 1, GetTimeDelta(undoArray[i].Time), undoArray[i].Description), "Undo");
                    lastTime = undoArray[i].Time;
                }
            }
            // Draw Undo Section
            EditorGUI.BeginDisabledGroup(undoEmpty);
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(
                        new GUIContent(" Undo All", Styles.undoIco, "Undo"),
                        Styles.richButtonMini, GUILayout.MaxHeight(15f)))
                    {
                        if (EditorUtility.DisplayDialog("Undo Operation",
                            string.Format("Are you sure you want to Undo all [{0}] changes?", undoArray.Length),
                            "OK", "Cancel"))
                        {
                            m_mapBuilder.UndoAll();
                        }
                        GUIUtility.ExitGUI();
                    }
                    if (GUILayout.Button(
                        new GUIContent(undoBtnLabel, Styles.undoIco, "Undo"),
                        Styles.richButtonMini, GUILayout.MaxHeight(15f)))
                    {
                        m_mapBuilder.Undo();
                    }
                    m_undoSteps = EditorGUILayout.Popup(m_undoSteps, undoLabelsArray);
                }
                GUILayout.EndHorizontal();
            }
            EditorGUI.EndDisabledGroup();
            // m_editorUtils.InlineHelp("Undo", helpEnabled);
            #endregion
            GUI.enabled = !m_mapBuilder.IsBusy;
            GUILayout.BeginHorizontal();
            GUI.enabled = !m_mapBuilder.IsBusy;
            if (m_editorUtils.Button("FindAllAreasBtn")) m_mapBuilder.FindAllAreas();
            if (!m_mapBuilder.HasAreas)
                GUI.enabled = false;
            if (m_editorUtils.Button("ClearAllAreasBtn")) m_mapBuilder.ClearAreas();
            GUILayout.EndHorizontal();
            GUI.enabled = true;
            m_editorUtils.InlineHelp("FindAllAreasBtn", showHelp);
            m_editorUtils.InlineHelp("ClearAllAreasBtn", showHelp);
            GUI.enabled = !m_mapBuilder.IsBusy;
            GUILayout.BeginHorizontal();
            if (!m_mapBuilder.HasAreas && m_mapBuilder.HasSpawners)
                GUI.enabled = false;
            if (m_editorUtils.Button("CreateTownsBtn"))
            {
                m_mapBuilder.BuildOnAreas();
            }
            if (m_mapBuilder.CanBuildRoads)
                GUI.enabled = !m_mapBuilder.IsBusy;
            else
                GUI.enabled = false;
            if (m_editorUtils.Button("CreateRoadsBtn"))
            {
                m_mapBuilder.ConnectAreasWithRoads();
            }
            GUILayout.EndHorizontal();
            GUI.enabled = true;
            m_editorUtils.InlineHelp("CreateTownsBtn", showHelp);
            m_editorUtils.InlineHelp("CreateRoadsBtn", showHelp);
            GUI.enabled = m_mapBuilder.IsBusy;
            if (m_mapBuilder.IsBusy)
            {
                GUILayout.Space(3);
                GUILayout.BeginHorizontal();
                int numDots = (m_mapBuilder.BuildCount % 10) + 1;
                string dots = new string('.', numDots);
                GUILayout.Label($"Working {dots}");
                GUIContent cancelContent = new GUIContent("\u00D7 Cancel");
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button(cancelContent, Styles.cancelBtn, GUILayout.MaxHeight(25f)))
                {
                    m_mapBuilder.CancelBuild();
                }
                GUILayout.EndHorizontal();
                GUI.backgroundColor = oldColor;
            }
            GUI.enabled = true;
        }
        #endregion
        #region Menu Items
        /// <summary>
        /// Adds a Map Builder
        /// </summary>
        [MenuItem("GameObject/GeNa/Add Map Builder", false, 16)]
        public static void AddGeNaMapBuilder(MenuCommand menuCommand)
        {
            //Create the spawner
            GameObject genaGo = new GameObject("Map Builder", typeof(GeNaMapBuilder));
            // GeNa Root
            // GeNa Spawner
            Transform parent = GeNaUtility.GeNaMapBuildersTransform;
            // Reparent it
            GameObjectUtility.SetParentAndAlign(genaGo, parent.gameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(genaGo, string.Format("[{0}] Created '{1}'", PWApp.CONF.Name, genaGo.name));
            //Make it active
            Selection.activeObject = genaGo;
        }
        #endregion
        #endregion
    }
}
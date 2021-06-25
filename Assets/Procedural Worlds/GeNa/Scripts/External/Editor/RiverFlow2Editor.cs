using UnityEngine;
using UnityEditor;
using PWCommon5;

namespace GeNa.Core
{
    public static class GeNaRiverFlow2
    {
#if UNITY_EDITOR
        [MenuItem("GameObject/GeNa/Add River Flow", false, 16)]
        public static void AddRiverFlow(MenuCommand command)
        {
            GameObject go = new GameObject("GeNa_RiverFlow");
            RiverFlow2 riverFlow2 = go.AddComponent<RiverFlow2>();
            if (GeNaUtility.Gaia2Present)
            {
                riverFlow2.m_seaLevel = GeNaEvents.GetSeaLevel(riverFlow2.m_seaLevel);
            }
            go.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(go, $"[{PWApp.CONF.Name}] Created '{go.name}'");
            Selection.activeGameObject = go;
        }
#endif
    }

    [CustomEditor(typeof(RiverFlow2))]
    public class RiverFlow2Editor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private RiverFlow2 flowCreator;
        private Color waterColor = new Color(0.6f, 0.6f, 1.0f);

        private void OnEnable()
        {
            m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");
        }
        /// <summary>
        /// Handle OnSceneGUI to allow user to select a point on the terrain
        /// as the start point for River generation.
        /// </summary>
        private void OnSceneGUI()
        {
            flowCreator = (RiverFlow2) target;


            Event guiEvent = Event.current;

            if (guiEvent.type == EventType.Layout && guiEvent.modifiers != EventModifiers.None)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
            else if (guiEvent.type == EventType.MouseDown && guiEvent.control)
            {
                //Debug.Log("Ctrl+Left Mouse clicked!");

                SceneView sceneView = SceneView.lastActiveSceneView;

                // Raycast to the terrain

                Vector3 mousePos = Event.current.mousePosition;
                Ray ray = HandleUtility.GUIPointToWorldRay(new Vector2(mousePos.x, mousePos.y));
                RaycastHit hit;
                if (flowCreator.m_selectTerrainOnly)
                {
                    RaycastHit[] hits = Physics.RaycastAll(ray, 2000.0f);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        //Debug.Log($"Raycast hit {hit.transform.gameObject.name}");
                        hit = hits[i];

                        if (hit.transform.GetComponent<Terrain>() != null)
                        {
                            if (guiEvent.button == 0)
                                flowCreator.AddPoint(hit.point);
                            else if (guiEvent.button == 1)
                                flowCreator.RemovePoint(hit.point);
                            sceneView.Repaint();
                            break;
                        }
                    }
                }
                else
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (guiEvent.button == 0)
                            flowCreator.AddPoint(hit.point);
                        else if (guiEvent.button == 1)
                            flowCreator.RemovePoint(hit.point);
                        sceneView.Repaint();
                    }
                }

                // Stop the propagation of this event

                Event.current.Use();
            }

            if (flowCreator.m_startPositions.Count > 0)
            {
                DrawCurrentStartPositions(flowCreator);
            }
        }

        /// <summary>
        /// Draws a disc, line and small disc at the current start position.
        /// </summary>
        void DrawCurrentStartPositions(RiverFlow2 flowCreator)
        {
            for (int i = 0; i < flowCreator.m_startPositions.Count; i++)
            {
                Handles.color = waterColor;
                Handles.DrawWireDisc(flowCreator.m_startPositions[i], Vector3.up, 0.5f);
                Handles.DrawWireDisc(flowCreator.m_startPositions[i], Vector3.up, 0.55f);
                Handles.color = Color.white;
                Handles.DrawWireDisc(flowCreator.m_startPositions[i], Vector3.up, 3.0f);
                Handles.DrawWireDisc(flowCreator.m_startPositions[i], Vector3.up, 2.95f);
            }
        }

        /// <summary>
        /// Handle OnInspectorGUI for custom inspector functionality.
        /// </summary>
        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize();

            //DrawDefaultInspector();

            flowCreator = (RiverFlow2) target;

            m_editorUtils.Panel("RiverFlow Panel", MainPanel, true);

            GUILayout.Space(5);
            if (GUILayout.Button("Clear"))
            {
                flowCreator.ClearClicked();
            }

            GUILayout.Space(10);
            if (flowCreator.m_startPositions.Count > 0)
            {
                if (GUILayout.Button("Create River Flow(s)"))
                {
                    flowCreator.CreateRiverFlowsClicked();
                }
            }
            if (flowCreator.m_simplifiedWaterFlows != null && flowCreator.m_simplifiedWaterFlows.Count > 0)
            {
                if (GUILayout.Button("Create GeNa River Spline"))
                {
                    flowCreator.CreateSplinesClicked();
                }
            }

        }

        private void MainPanel(bool helpEnabled)
        {
            m_editorUtils.InlineHelp("RiverFlow Help", helpEnabled);

            RiverFlow2 flowCreater = target as RiverFlow2;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Place start points for rivers/streams.", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Ctrl+LeftClick to Add, Ctrl+RightClick to Delete.", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Calculation Parameters", EditorStyles.boldLabel);
            if (GeNaUtility.Gaia2Present)
            {
                EditorGUILayout.BeginHorizontal();
                flowCreater.m_seaLevel = m_editorUtils.FloatField("Sea Level", flowCreator.m_seaLevel);
                if (m_editorUtils.Button("GetGaiaSeaLevel", GUILayout.MaxWidth(125f)))
                {
                    flowCreater.m_seaLevel = GeNaEvents.GetSeaLevel(flowCreater.m_seaLevel);
                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("Sea Level", helpEnabled);
            }
            else
            {
                flowCreater.m_seaLevel = m_editorUtils.FloatField("Sea Level", flowCreator.m_seaLevel, helpEnabled);
            }

            flowCreater.m_startDepth = m_editorUtils.Slider("Start Depth", flowCreater.m_startDepth, 0.05f, 3.0f, helpEnabled);
            flowCreater.m_minNodeDistance = m_editorUtils.Slider("Min Node Distance", flowCreater.m_minNodeDistance, 2.0f, 8.0f, helpEnabled);
            flowCreater.m_connectSnapDistance = m_editorUtils.Slider("Snap Connection Distance", flowCreater.m_connectSnapDistance, 1.0f, 8.0f, helpEnabled);
            flowCreater.m_selectTerrainOnly = m_editorUtils.Toggle("Select Terrain Only", flowCreater.m_selectTerrainOnly, helpEnabled);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Simplification Parameters", EditorStyles.boldLabel);
            flowCreater.m_simplifyEpsilon = m_editorUtils.Slider("Simplify Epsilon", flowCreater.m_simplifyEpsilon, 0.5f, 3.0f, helpEnabled);
            flowCreater.m_yScale = m_editorUtils.Slider("Y Scale", flowCreater.m_yScale, 0.1f, 3.0f, helpEnabled);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Visualization Parameters", EditorStyles.boldLabel);
            flowCreater.m_autoAnalyze = m_editorUtils.Toggle("Auto Analyze", flowCreater.m_autoAnalyze, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(flowCreater);
                flowCreater.UpdateFlows();
            }
            flowCreater.m_showOriginalPath = m_editorUtils.Toggle("Show Original Path", flowCreater.m_showOriginalPath, helpEnabled);
        }
    }
}
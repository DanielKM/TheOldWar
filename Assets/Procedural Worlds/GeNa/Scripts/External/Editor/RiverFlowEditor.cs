using UnityEngine;
using UnityEditor;
using PWCommon5;

namespace GeNa.Core
{
    public static class GeNaRiverFlow
    {
        /*
    #if UNITY_EDITOR
        [MenuItem("GameObject/GeNa/Add River Flow", false, 16)]
        public static void AddRiverFlow(MenuCommand command)
        {
            GameObject go = new GameObject("GeNa_RiverFlow", typeof(RiverFlow));
            go.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(go, $"[{PWApp.CONF.Name}] Created '{go.name}'");
            Selection.activeGameObject = go;
        }
    #endif
        */
    }




    [CustomEditor(typeof(RiverFlow))]
    public class RiverFlowEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private RiverFlow flowCreator;

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
            flowCreator = (RiverFlow) target;


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
        void DrawCurrentStartPositions(RiverFlow flowCreator)
        {
            for (int i = 0; i < flowCreator.m_startPositions.Count; i++)
            {
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(flowCreator.m_startPositions[i], Vector3.up, 3.0f);
                Handles.color = Color.white;
                Handles.DrawWireDisc(flowCreator.m_startPositions[i], Vector3.up, 3.0f);
                Handles.DrawWireDisc(flowCreator.m_startPositions[i], Vector3.up, 2.95f);
                Handles.color = Color.green;
                //Handles.DrawWireDisc(flowCreator..m_startPosition, Vector3.up, flowCreator.MinimumLength);
            }
        }

        /// <summary>
        /// Handle OnInspectorGUI for custom inspector functionality.
        /// </summary>
        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize();

            //DrawDefaultInspector();

            flowCreator = (RiverFlow) target;

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
            if (flowCreator.m_simpliedPaths != null)
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

            RiverFlow flowCreater = target as RiverFlow;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Place start points for rivers/streams.");
            EditorGUILayout.LabelField("Ctrl+LeftClick to Add, Ctrl+RightClick to Delete.");
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);

            flowCreater.m_seaLevel = m_editorUtils.FloatField("Sea Level", flowCreator.m_seaLevel, helpEnabled);
            flowCreater.m_startFlow = m_editorUtils.Slider("Start Flow", flowCreater.m_startFlow, 0.05f, 3.0f);
            flowCreater.SimplifyEpsilon = m_editorUtils.Slider("Simplify Epsilon", flowCreater.SimplifyEpsilon, 0.8f, 3.0f);
            flowCreater.YScale = m_editorUtils.Slider("Y Scale", flowCreater.YScale, 0.1f, 3.0f);
            flowCreater.m_selectTerrainOnly = m_editorUtils.Toggle("Select Terrain Only", flowCreater.m_selectTerrainOnly);
            flowCreater.SimpleRiverPathColor = m_editorUtils.ColorField("Path Color", flowCreater.SimpleRiverPathColor);
        }
    }
}
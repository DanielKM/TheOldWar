//Copyright(c)2020 Procedural Worlds Pty Limited 
using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaRoadExtension))]
    public class GeNaRoadExtensionEditor : GeNaSplineExtensionEditor
    {
        protected Editor m_roadProfileEditor;
        protected GeNaRoadExtension m_roadExtension;
        #region OnSceneGUI
        public override void OnSceneGUI()
        {
            base.OnSceneGUI();

            GeNaRoadExtension roadExtension = target as GeNaRoadExtension;
            if (roadExtension.Spline.Settings.Advanced.DebuggingEnabled == false)
                return;
            Handles.color = Color.red;
            foreach (GeNaCurve curve in roadExtension.Spline.Curves)
            {
                DrawCurveDirecton(curve);
            }
        }
        private void DrawCurveDirecton(GeNaCurve geNaCurve)
        {
            Vector3 forward = (geNaCurve.P3 - geNaCurve.P0).normalized;
            GeNaSample geNaSample = geNaCurve.GetSample(0.45f);
            DrawArrow(geNaSample.Location, forward);
            geNaSample = geNaCurve.GetSample(0.5f);
            DrawArrow(geNaSample.Location, forward);
            geNaSample = geNaCurve.GetSample(0.55f);
            DrawArrow(geNaSample.Location, forward);

        }
        private void DrawArrow(Vector3 position, Vector3 direction)
        {
            direction.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            Handles.DrawLine(position, position + (-direction + right) * 0.75f);
            Handles.DrawLine(position, position + (-direction - right) * 0.75f);
        }
        #endregion OnSceneGUI

        private void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");
            m_roadExtension = target as GeNaRoadExtension;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GlobalPanel();
        }

        private void GlobalPanel()
        {
            EditorGUILayout.BeginHorizontal();
            m_editorUtils.LabelField("Tag", GUILayout.MaxWidth(40));
            m_roadExtension.Tag = EditorGUILayout.TagField(m_roadExtension.Tag);
            m_editorUtils.LabelField("Layer", GUILayout.MaxWidth(40));
            m_roadExtension.Layer = EditorGUILayout.LayerField(m_roadExtension.Layer);
            EditorGUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("TagAndLayerHelp", HelpEnabled);

            m_roadExtension.Width = m_editorUtils.FloatField("MeshWidth", m_roadExtension.Width, HelpEnabled);
            m_roadExtension.IntersectionSize = m_editorUtils.Slider("IntersectionSize", m_roadExtension.IntersectionSize, 0.8f, 1.2f, HelpEnabled);
            m_roadExtension.SplitAtTerrains = m_editorUtils.Toggle("SplitMeshesAtTerrains", m_roadExtension.SplitAtTerrains, HelpEnabled);
            m_roadExtension.AddRoadCollider = m_editorUtils.Toggle("AddCollider", m_roadExtension.AddRoadCollider, HelpEnabled);
            m_roadExtension.RaycastTerrainOnly = m_editorUtils.Toggle("RaycastTerrainOnly", m_roadExtension.RaycastTerrainOnly, HelpEnabled);
            m_roadExtension.ConformToGround = m_editorUtils.Toggle("ConformToGround", m_roadExtension.ConformToGround, HelpEnabled);
            if (!m_roadExtension.ConformToGround)
            {
                EditorGUI.indentLevel++;
                m_roadExtension.GroundAttractDistance = m_editorUtils.FloatField("GroundSnapDistance", m_roadExtension.GroundAttractDistance, HelpEnabled);
                EditorGUI.indentLevel--;
            }
            m_roadExtension.RoadProfile = (GeNaRoadProfile)m_editorUtils.ObjectField("RoadProfile", m_roadExtension.RoadProfile, typeof(GeNaRoadProfile), true, HelpEnabled);
            if (m_roadExtension.RoadProfile != null)
            {
                if (m_roadProfileEditor == null)
                    m_roadProfileEditor = CreateEditor(m_roadExtension.RoadProfile);
                GeNaRoadProfileEditor.SetProfile(m_roadExtension.RoadProfile, (GeNaRoadProfileEditor)m_roadProfileEditor);
                m_roadProfileEditor.OnInspectorGUI();
            }

            if (m_editorUtils.Button("BakeRoad"))
                if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("BakeTitleRoad"), m_editorUtils.GetTextValue("BakeMessageRoad"), "Yes", "No"))
                    //TODO : Clyde : Check how to process this
                    m_roadExtension.Bake(true);
        }

        [MenuItem("GameObject/GeNa/Add Road Spline", false, 17)]
        public static void AddRoadSpline(MenuCommand command)
        {
            GeNaSpline spline = GeNaSpline.CreateSpline("Road Spline");
            if (spline != null)
            {
                Undo.RegisterCreatedObjectUndo(spline.gameObject, $"[{PWApp.CONF.Name}] Created '{spline.gameObject.name}'");
                GeNaCarveExtension carve = spline.AddExtension<GeNaCarveExtension>();
                carve.name = "Carve";
                GeNaClearDetailsExtension clearDetails = spline.AddExtension<GeNaClearDetailsExtension>();
                if (clearDetails != null)
                    clearDetails.name = "Clear Details/Grass";
                GeNaClearTreesExtension clearTrees = spline.AddExtension<GeNaClearTreesExtension>();
                if (clearTrees != null)
                    clearTrees.name = "Clear Trees";
                GeNaRoadExtension roads = spline.AddExtension<GeNaRoadExtension>();
                roads.GroundAttractDistance = 0.0f;
                roads.name = "Road";
                Selection.activeGameObject = spline.gameObject;
            }
        }
    }
}
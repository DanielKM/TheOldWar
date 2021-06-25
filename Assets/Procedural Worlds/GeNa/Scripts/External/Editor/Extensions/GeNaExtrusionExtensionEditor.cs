using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaExtrusionExtension))]
    public class GeNaExtrusionExtensionEditor : GeNaSplineExtensionEditor
    {
        private GeNaExtrusionExtension Extrusion;
        private void OnEnable()
        {
            Extrusion = target as GeNaExtrusionExtension;

            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Extrusion == null)
            {
                Extrusion = target as GeNaExtrusionExtension;
            }

            Extrusion.SharedMaterial = (Material) m_editorUtils.ObjectField("Extrusion Material", Extrusion.SharedMaterial, typeof(Material), true, HelpEnabled);
            Extrusion.Smoothness = m_editorUtils.Slider("Mesh Smoothness", Extrusion.Smoothness, 1f, 5f, HelpEnabled);
            Extrusion.Width = m_editorUtils.FloatField("Mesh Width", Extrusion.Width, HelpEnabled);
            Extrusion.HeightOffset = m_editorUtils.FloatField("Mesh Height Offset", Extrusion.HeightOffset, HelpEnabled);
            Extrusion.SnapToGround = m_editorUtils.Toggle("Mesh Snap to Terrain", Extrusion.SnapToGround, HelpEnabled);
            Extrusion.Curve = m_editorUtils.CurveField("Extrusion", Extrusion.Curve, HelpEnabled);
            Extrusion.SplitAtTerrains = m_editorUtils.Toggle("SplitMeshesAtTerrains", Extrusion.SplitAtTerrains);

            if (m_editorUtils.Button("BakeExtrusion"))
            {
                Extrusion.Bake();
            }
        }
    }
}
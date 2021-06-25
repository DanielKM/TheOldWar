using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using PWCommon5;

namespace GeNa.Core
{
    [CustomEditor(typeof(RiverReflectionProbeManager))]
    public class RiverReflectionProbeManagerEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private RiverReflectionProbeManager m_manager;

        private void OnEnable()
        {
            m_editorUtils = PWApp.GetEditorUtils(this, null, null, null);

            m_manager = (RiverReflectionProbeManager) target;
            if (m_manager.m_reflectionProbe == null)
            {
                m_manager.m_reflectionProbe = m_manager.GetComponent<ReflectionProbe>();
                if (m_manager.m_reflectionProbe != null)
                {
                    EditorUtility.SetDirty(m_manager);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize();

            if (m_manager == null)
            {
                m_manager = (RiverReflectionProbeManager) target;
            }

            m_editorUtils.Panel("GlobalSettings", GlobalPanel, true);
        }

        private void GlobalPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            m_manager.m_reflectionProbe = (ReflectionProbe)m_editorUtils.ObjectField("ReflectionProbe", m_manager.m_reflectionProbe, typeof(ReflectionProbe), true);
            m_manager.RenderDistance = m_editorUtils.FloatField("RenderDistance", m_manager.RenderDistance, helpEnabled);
            if (m_manager.RenderDistance < 0.1f)
            {
                m_manager.RenderDistance = 0.1f;
            }
            m_manager.ProbeRenderResolution = (ReflectionProbeRenderResolution)m_editorUtils.EnumPopup("ProbeRenderResolution", m_manager.ProbeRenderResolution, helpEnabled);
            m_manager.ProbeMode = (ReflectionProbeMode)m_editorUtils.EnumPopup("ProbeRenderMode", m_manager.ProbeMode, helpEnabled);
            m_manager.ProbeRefreshMode = (ReflectionProbeRefreshMode)m_editorUtils.EnumPopup("ProbeRefreshMode", m_manager.ProbeRefreshMode, helpEnabled);
            m_manager.ProbeTimeSlicingMode = (ReflectionProbeTimeSlicingMode)m_editorUtils.EnumPopup("ProbeTimeSlicingMode", m_manager.ProbeTimeSlicingMode, helpEnabled);
            m_manager.LayerMask = m_editorUtils.LayerMaskField("CullingLayerMask", m_manager.LayerMask, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_manager);
                if (m_manager.m_reflectionProbe != null)
                {
                    EditorUtility.SetDirty(m_manager.m_reflectionProbe);
                }
            }
        }
    }
}
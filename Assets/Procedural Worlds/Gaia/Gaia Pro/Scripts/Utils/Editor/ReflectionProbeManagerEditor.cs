using Gaia.Internal;
using PWCommon5;
using UnityEngine;
using UnityEditor;

namespace Gaia
{
    [CustomEditor(typeof(ReflectionProbeManager))]
    public class ReflectionProbeManagerEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private ReflectionProbeManager m_profile;

        private void OnEnable()
        {
            m_profile = (ReflectionProbeManager)target;

            //Init editor utils
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
        }

        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize(); // Do not remove this!
            m_profile = (ReflectionProbeManager)target;

            EditorGUI.BeginChangeCheck();

            m_editorUtils.Panel("GlobalSettings", GlobalSettings, true);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profile, "Changes Made");
                EditorUtility.SetDirty(m_profile);
            }
        }

        private void GlobalSettings(bool helpEnabled)
        {
            m_profile.EnableSystem = m_editorUtils.Toggle("EnableSystem", m_profile.EnableSystem, helpEnabled);
            if (m_profile.EnableSystem)
            {
                EditorGUI.indentLevel++;
                m_profile.m_checkTODEveryFrame = m_editorUtils.Toggle("CheckTODEveryFrame", m_profile.m_checkTODEveryFrame, helpEnabled);
                if (m_profile.m_checkTODEveryFrame)
                {
                    EditorGUI.indentLevel++;
                    m_profile.m_lightCheckData.m_checkMinuteTime = m_editorUtils.Slider("CheckMinute", m_profile.m_lightCheckData.m_checkMinuteTime, 1f, 59f, helpEnabled);
                    m_profile.m_lightCheckData.m_checkHourTime = m_editorUtils.IntSlider("CheckHour", m_profile.m_lightCheckData.m_checkHourTime, 1, 23, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUI.indentLevel++;
                    m_profile.m_probePriorityCheck = m_editorUtils.FloatField("ProbeUpdateCheck", m_profile.m_probePriorityCheck, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                m_profile.m_player = (Transform)m_editorUtils.ObjectField("Player", m_profile.m_player, typeof(Transform), true, helpEnabled);
                m_profile.m_playerBoundsSize = m_editorUtils.Vector3Field("PlayerBoundsSize", m_profile.m_playerBoundsSize, helpEnabled);
                m_profile.m_probeProcessTime = m_editorUtils.FloatField("ProbeProcessTime", m_profile.m_probeProcessTime, helpEnabled);
                m_profile.ProbeLayerMask = GaiaEditorUtils.LayerMaskField(new GUIContent(m_editorUtils.GetTextValue("ProbeLayerMask"), m_editorUtils.GetTooltip("ProbeLayerMask")), m_profile.ProbeLayerMask);
                m_editorUtils.InlineHelp("ProbeLayerMask", helpEnabled);

                bool useProbeCulling = m_profile.UseReflectionProbeCuller;
                useProbeCulling = m_editorUtils.Toggle("UseReflectionProbeCulling", useProbeCulling, helpEnabled);
                if (useProbeCulling != m_profile.UseReflectionProbeCuller)
                {
                    m_profile.UseReflectionProbeCuller = useProbeCulling;
                    m_profile.m_probeCullingNeedsUpdating = true;
                }
                if (m_profile.UseReflectionProbeCuller)
                {
                    EditorGUI.indentLevel++;
                    float probeCullingDistance = m_profile.ReflectionProbeCullingDistance;
                    probeCullingDistance = m_editorUtils.FloatField("ReflectionProbeCullingDIstance", probeCullingDistance, helpEnabled);
                    if (probeCullingDistance != m_profile.ReflectionProbeCullingDistance)
                    {
                        m_profile.ReflectionProbeCullingDistance = probeCullingDistance;
                        m_profile.m_probeCullingNeedsUpdating = true;
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
        }
    }
}
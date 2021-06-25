using PWCommon5;
using UnityEngine;
using UnityEditor;
using Gaia.Internal;

namespace Gaia
{
    [CustomEditor(typeof(PW_VFX_Clouds))]
    public class PW_VFX_CloudsEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private PW_VFX_Clouds m_profile;

        public void OnEnable()
        {
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
            m_profile = (PW_VFX_Clouds) target;
        }
        private void OnDestroy()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
            }
        }

        public override void OnInspectorGUI()
        {
            //Initialization
            m_editorUtils.Initialize(); // Do not remove this!

            if (m_profile == null)
            {
                m_profile = (PW_VFX_Clouds) target;
                return;
            }

            m_editorUtils.Panel("GlobalSettings", GlobalSettings, false, true, true);
        }

        private void GlobalSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            m_editorUtils.Heading("Lighting");
            EditorGUI.indentLevel++;
            m_profile.SunLight = (Light)m_editorUtils.ObjectField("SunLight", m_profile.SunLight, typeof(Light), true, helpEnabled);
            EditorGUI.indentLevel--;

            m_editorUtils.Heading("Settings");
            EditorGUI.indentLevel++;
            m_profile.GameCam = (Camera)m_editorUtils.ObjectField("Camera", m_profile.GameCam, typeof(Camera), true, helpEnabled);
            m_profile.m_trackPlayer = m_editorUtils.Toggle("TrackPlayer", m_profile.m_trackPlayer, helpEnabled);
            if (m_profile.m_trackPlayer)
            {
                EditorGUI.indentLevel++;
                m_profile.m_followCamOnYAxis = m_editorUtils.Toggle("FollowCameraOnYAxis", m_profile.m_followCamOnYAxis, helpEnabled);
                m_profile.m_yOffset = m_editorUtils.FloatField("YOffset", m_profile.m_yOffset, helpEnabled);
                EditorGUI.indentLevel--;
            }
            m_profile.UpdateTicks = m_editorUtils.IntField("UpdateTicks", m_profile.UpdateTicks, helpEnabled);
            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profile);
            }
        }
    }
}
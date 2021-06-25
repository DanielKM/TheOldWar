using PWCommon5;
using UnityEngine;
using UnityEditor;
using Gaia.Internal;

namespace Gaia
{
    [CustomEditor(typeof(PW_VFX_Atmosphere))]
    public class PW_VFX_AtmosphereEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private PW_VFX_Atmosphere m_profile;

        public void OnEnable()
        {
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
            m_profile = (PW_VFX_Atmosphere) target;
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
                m_profile = (PW_VFX_Atmosphere) target;
                return;
            }

            m_editorUtils.Panel("GlobalSettings", GlobalSettings, false, true, true);
        }

        private void GlobalSettings(bool helpEnabled)
        {
            m_editorUtils.Heading("Settings");
            EditorGUI.indentLevel++;
            m_profile.SunLight = (Light)m_editorUtils.ObjectField("SunLight", m_profile.SunLight, typeof(Light), true, helpEnabled);
            m_profile.SunAxisTransform = (Transform)m_editorUtils.ObjectField("SunAxisTransform", m_profile.SunAxisTransform, typeof(Transform), true, helpEnabled);
            m_profile.EarthAxisTransform = (Transform)m_editorUtils.ObjectField("EarthAxisTransform", m_profile.EarthAxisTransform, typeof(Transform), true, helpEnabled);
            EditorGUI.indentLevel--;
        }
    }
}
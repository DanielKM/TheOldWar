using UnityEngine;
using UnityEditor;
using PWCommon5;
namespace GeNa.Core
{
    [CustomEditor(typeof(GaiaTimeOfDayLightSync))]
    public class GaiaTimeOfDayLightSyncEditor : PWEditor
    {
        private EditorUtils m_editor;
        private GaiaTimeOfDayLightSync m_lightSync;
        
        private void OnEnable()
        {
            m_editor = PWApp.GetEditorUtils(this, null, null, null);
            m_lightSync = (GaiaTimeOfDayLightSync)target;
        }
        private void Update()
        {
            m_lightSync.UpdateLights();
        }
        
        
        public override void OnInspectorGUI()
        {
            m_editor.Initialize();
            if (m_lightSync == null)
            {
                m_lightSync = (GaiaTimeOfDayLightSync)target;
            }

            m_editor.Panel("GlobalSettings", GlobalPanel, true);
        }

        private void GlobalPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            m_lightSync.m_overrideSystemActiveState = m_editor.Toggle("OverrideSystemActiveState", m_lightSync.m_overrideSystemActiveState, helpEnabled);
            m_lightSync.m_lightComponent = (Light)m_editor.ObjectField("LightComponent", m_lightSync.m_lightComponent, typeof(Light), true, helpEnabled);
            EditorGUILayout.Space();

            m_editor.Heading("EmissionSettings");
            m_lightSync.m_emissionRenderType = (EmissionRenderType)m_editor.EnumPopup("EmissionRenderType", m_lightSync.m_emissionRenderType, helpEnabled);
            m_lightSync.m_lightEmissionObject = (GameObject)m_editor.ObjectField("LightEmissionObject", m_lightSync.m_lightEmissionObject, typeof(GameObject), true, helpEnabled);
            m_lightSync.m_emissionMaterial = (Material)m_editor.ObjectField("EmissionMaterial", m_lightSync.m_emissionMaterial, typeof(Material), true, helpEnabled);
            m_lightSync.m_enableEmissioKeyWord = m_editor.TextField("EmissionKeyWord", m_lightSync.m_enableEmissioKeyWord, helpEnabled);

            if (EditorGUI.EndChangeCheck())
            {
                m_lightSync.ValidateComponents();
                EditorUtility.SetDirty(m_lightSync);
            }
        }
    }
}
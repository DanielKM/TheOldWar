using System.ComponentModel.Design;
using Gaia.Internal;
using PWCommon5;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{	
    /// <summary>
    /// Manager for Gaia Audio
    /// </summary>
    [CustomEditor(typeof(GaiaAudioManager))]
    public class GaiaAudioManagerEditor : PWEditor
    {
		private EditorUtils m_editorUtils;
        private GaiaAudioManager m_gaiaAudioManager;
		private GUIStyle listheaderstyle; 
		    
		private void OnEnable()
        {
			m_gaiaAudioManager = (GaiaAudioManager)target;

            //Init editor utils
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
		}

        public override void OnInspectorGUI()
        {
			if (listheaderstyle == null)
			{
				listheaderstyle = new GUIStyle(GUI.skin.label);
				listheaderstyle.fontStyle = FontStyle.Bold; 
			}
            m_editorUtils.Initialize(); // Do not remove this!
            m_gaiaAudioManager = (GaiaAudioManager)target;
            m_editorUtils.Panel("GlobalVolumePanel",Volumepanel, false, true, true);
            m_editorUtils.Panel("AudioZonePanel", Audiopanel, true);
        }

		private void Volumepanel(bool helpenabled)	
		{
            EditorGUI.BeginChangeCheck();
			m_gaiaAudioManager.m_masterVolume = m_editorUtils.Slider("Master Volume", m_gaiaAudioManager.m_masterVolume, 0f, 1f, helpenabled);
			m_gaiaAudioManager.m_rainVolume = m_editorUtils.Slider("Rain Volume", m_gaiaAudioManager.m_rainVolume, 0f, 1f, helpenabled);
			m_gaiaAudioManager.m_snowVolume = m_editorUtils.Slider("Snow Volume", m_gaiaAudioManager.m_snowVolume, 0f, 1f, helpenabled);
			m_gaiaAudioManager.m_thunderVolume = m_editorUtils.Slider("Thunder Volume", m_gaiaAudioManager.m_thunderVolume, 0f, 1f, helpenabled);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_gaiaAudioManager, "Changes Made");
                EditorUtility.SetDirty(m_gaiaAudioManager);
            }
		}

        private void Audiopanel(bool helpenabled)
		{
            EditorGUI.BeginChangeCheck();

            bool showGizmos = m_gaiaAudioManager.m_showGizmos;
            m_gaiaAudioManager.m_player = (GameObject)m_editorUtils.ObjectField("Player", m_gaiaAudioManager.m_player, typeof(GameObject), true, helpenabled);
            m_gaiaAudioManager.m_showGizmos = m_editorUtils.Toggle("EnableGizmos", m_gaiaAudioManager.m_showGizmos, helpenabled);

            if (Application.isPlaying)
            {
                for (int i = 0; i < m_gaiaAudioManager.m_activeAudioZones.Count; i++)
                {
                    GaiaAudioZone az = m_gaiaAudioManager.m_activeAudioZones[i];
                    if (az.m_isGlobalAudioSource)
                    {
                        m_editorUtils.ObjectField("Global Zone", az, typeof(GaiaAudioZone), true);
                    }
                    else
                    {
                        m_editorUtils.ObjectField("Local Zone", az, typeof(GaiaAudioZone), true);
                    }

                    EditorGUI.indentLevel++;
                    m_editorUtils.LabelField("Audio Track", new GUIContent(az.m_currentAudioItem.m_audioClip.name));
                    m_editorUtils.LabelField("Audio Volume", new GUIContent(az.m_currentZoneVolume.ToString()));
                    EditorGUI.indentLevel--;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (!Application.isPlaying)
                {
                    if (showGizmos != m_gaiaAudioManager.m_showGizmos)
                    {
                        GaiaAudioZone[] audioZones = GameObject.FindObjectsOfType<GaiaAudioZone>();
                        foreach (var gaiaAudioZone in audioZones)
                        {
                            gaiaAudioZone.m_showGizmos = m_gaiaAudioManager.m_showGizmos;
                        }
                    }
                }
                Undo.RecordObject(m_gaiaAudioManager, "Changes Made");
                EditorUtility.SetDirty(m_gaiaAudioManager);
            }
        }
    }
}	
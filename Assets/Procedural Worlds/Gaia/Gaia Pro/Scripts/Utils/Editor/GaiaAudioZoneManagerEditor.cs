using Gaia.Internal;
using PWCommon5;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia

{
	/// <summary>
	/// Editor for audio zones
	/// </summary>
	[CustomEditor(typeof(GaiaAudioZone))]
	public class GaiaAudioZoneManagerEditor : PWEditor
	{
		private EditorUtils m_editorUtils;
		private GaiaAudioZone m_audioZone;
		private GUIStyle m_listHeaderStyle;
		private GUIStyle m_boxStyle;

		private void OnEnable()
		{
			m_audioZone = (GaiaAudioZone)target;
			//Init editor utils
			if (m_editorUtils == null)
			{
				// Get editor utils for this
				m_editorUtils = PWApp.GetEditorUtils(this);
			}
		}

		public override void OnInspectorGUI()
		{
			if (m_listHeaderStyle == null)
			{
				m_listHeaderStyle = new GUIStyle(GUI.skin.label);
				m_listHeaderStyle.fontStyle = FontStyle.Bold;
			}

			if (m_boxStyle == null)
			{
				m_boxStyle = new GUIStyle(GUI.skin.box);
				m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
				m_boxStyle.fontStyle = FontStyle.Bold;
				m_boxStyle.alignment = TextAnchor.UpperLeft;
			}

			m_editorUtils.Initialize(); // Do not remove this!

			m_audioZone = (GaiaAudioZone)target;

            EditorGUI.BeginChangeCheck();

            m_editorUtils.Heading("GizmoSettings");
            m_audioZone.m_gizmoColor = m_editorUtils.ColorField("GizmoColor", m_audioZone.m_gizmoColor);
			m_editorUtils.Panel("Audio", Audiopanel, true); 

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_audioZone, "Chanced GaiaAudioZone");
                EditorUtility.SetDirty(m_audioZone);
            }
		}
		private void Volumepanel(bool helpenabled)
		{
			//m_audioZone.m_zoneVolume = m_editorUtils.Slider("Zone Volume", m_audioZone.m_zoneVolume, 0f, 1f, helpenabled);
			m_audioZone.m_currentZoneVolume = m_editorUtils.Slider("Current Zone Volume", m_audioZone.m_currentZoneVolume, 0f, 1f, helpenabled); 
		}
		private void Audiopanel(bool helpenabled)
		{
            EditorGUI.BeginChangeCheck();

			m_audioZone.m_isGlobalAudioSource = m_editorUtils.Toggle("Global Audio Source", m_audioZone.m_isGlobalAudioSource, helpenabled);
            if (!m_audioZone.m_isGlobalAudioSource)
            {
                m_audioZone.m_zoneRadius = m_editorUtils.FloatField("SourceRadius", m_audioZone.m_zoneRadius, helpenabled);
                if (m_audioZone.m_zoneRadius < 0f)
                {
                    m_audioZone.m_zoneRadius = 0f;
                }
            }
			m_audioZone.m_maximumBreakTime = m_editorUtils.FloatField("Max Break Time", m_audioZone.m_maximumBreakTime, helpenabled);
			m_audioZone.m_minimumBreakTime = m_editorUtils.FloatField("Min Break Time", m_audioZone.m_minimumBreakTime, helpenabled);
			
			for (int i = 0; i < m_audioZone.m_audioList.Count; i++)
			{
				EditorGUILayout.BeginVertical(m_boxStyle);
				EditorGUILayout.BeginHorizontal();
				m_audioZone.m_audioList[i].m_audioClip = (AudioClip)m_editorUtils.ObjectField("Audio Clip", m_audioZone.m_audioList[i].m_audioClip, typeof(AudioClip), true);
				if (m_editorUtils.Button("RemoveItem", GUILayout.Width(60f)))
				{
					m_audioZone.m_audioList.RemoveAt(i);
				}
				EditorGUILayout.EndHorizontal();
				m_audioZone.m_audioList[i].m_volume = m_editorUtils.Slider("Volume", m_audioZone.m_audioList[i].m_volume, 0f, 1f, helpenabled);
				m_audioZone.m_audioList[i].m_fadeInTime = m_editorUtils.FloatField("Fade In", m_audioZone.m_audioList[i].m_fadeInTime, helpenabled);
				m_audioZone.m_audioList[i].m_fadeOutTime = m_editorUtils.FloatField("Fade Out", m_audioZone.m_audioList[i].m_fadeOutTime, helpenabled);
				EditorGUILayout.EndVertical();
			}

			//add is global

			if (m_editorUtils.Button("AddZoneInfo"))
			{
				m_audioZone.m_audioList.Add(new GaiaAudioZoneItem());
			}

            if (EditorGUI.EndChangeCheck())
            {
                if (!m_audioZone.m_isGlobalAudioSource)
                {
                    m_audioZone.Initialize();
                }
            }
		}
	}	
}	
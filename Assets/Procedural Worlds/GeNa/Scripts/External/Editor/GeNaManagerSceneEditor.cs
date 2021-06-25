// Engine
using UnityEngine;

// Editor
using UnityEditor;

// Procedural Worlds
using PWCommon5;
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaManager))]
    public class GeNaManagerSceneEditor : PWEditor
    {
        #region Variables
        #region Static
        private GeNaManager m_manager;
        #endregion
        #region GUI
        private EditorUtils m_editorUtils;
        #endregion
        #endregion
        #region Methods
        #region Unity
        private void OnEnable()
        {
            #region Initialization
            // If there isn't any Editor Utils Initialized
            if (m_editorUtils == null)
                // Get editor utils for this
                m_editorUtils =  PWApp.GetEditorUtils(this, null, null,null);
            // If there is no target associated with Editor Script
            if (target == null)
                // Exit the method
                return;
            // Get target Spline
            m_manager = (GeNaManager) target;
            //Hide its m_transform
            m_manager.transform.hideFlags = HideFlags.HideInInspector;
            m_manager.Initialize();
            #endregion
        }
        public override void OnInspectorGUI()
        {
            #region Header

            m_editorUtils.Initialize(); // Do not remove this!
            m_editorUtils.GUINewsHeader(true);

            #endregion

            #region Panel

            m_editorUtils.Panel("GeneralPanel", ManagerPanel, true);

            #endregion

            m_editorUtils.GUINewsFooter(false);
        }
        #endregion

        #region Panel

        private void ManagerPanel(bool helpEnabled)
        {
            m_editorUtils.Text("WelcomeToGeNaManager");
            if (m_editorUtils.Button("ShowGeNaManager"))
            {
                ShowGeNaManager();
            }
        }

        #endregion

        #region Utilities

        private void ShowGeNaManager()
        {
            GeNaManagerEditor.MenuGeNaMainWindow();
        }

        #endregion
        #endregion
    }
}
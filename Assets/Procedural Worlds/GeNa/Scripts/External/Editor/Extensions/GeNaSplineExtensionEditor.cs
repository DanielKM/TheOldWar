using UnityEditor;
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaSplineExtension))]
    public class GeNaSplineExtensionEditor : GeNaEditor
    {
        public bool HelpEnabled { get; set; }
        public virtual void OnSelected() { }
        public virtual void OnDeselected()
        {
            HelpEnabled = false;
        }
        public virtual void OnSceneGUI()
        {
        }
    }
}
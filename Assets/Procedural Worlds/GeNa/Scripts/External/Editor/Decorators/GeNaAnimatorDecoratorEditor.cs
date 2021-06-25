using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaAnimatorDecorator))]
    public class GeNaAnimatorDecoratorEditor : GeNaDecoratorEditor<GeNaAnimatorDecorator>
    {
        public GeNaAnimatorDecorator m_selectedAnimatorDecorator;
        public Animator[] m_animators;
        [MenuItem("GameObject/GeNa/Decorators/Bounds Decorator")]
        public static void AddDecorator(MenuCommand command)
        {
            GameObject gameObject = command.context as GameObject;
            if (gameObject != null)
            {
                GeNaBoundsDecorator decorator = gameObject.AddComponent<GeNaBoundsDecorator>();
                GeNaDecoratorEditorUtility.RegisterDecorator(gameObject, decorator);
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            if (Decorator != null)
                Decorator.UpdateAnimations = false;
            Selection.selectionChanged -= OnSelectionChange;
            Selection.selectionChanged += OnSelectionChange;
            OnSelectionChange();
        }
        protected void OnDisable()
        {
            if (Decorator != null)
                Decorator.UpdateAnimations = true;
        }
        /// <summary>
        /// See if this is an origami structure with a valid builder
        /// </summary>
        private GeNaAnimatorDecorator m_animatorDecorator;
        private void OnSelectionChange()
        {
            m_animators = null;
            var selectedGameObject = Selection.activeGameObject;
            if (selectedGameObject != null)
            {
                if (m_animatorDecorator != null)
                    m_animatorDecorator.UpdateAnimations = true;
                m_animatorDecorator = selectedGameObject.GetComponentInParent<GeNaAnimatorDecorator>();
                if (m_animatorDecorator != null)
                    m_animatorDecorator.UpdateAnimations = false;
            }
        }
        protected override void RenderPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                // Decorator.timeline = EditorGUILayout.Slider("Timeline", Decorator.timeline, 0f, 1f);
                // EditorUtility.SetDirty(target);
                // EditorUtils.BoundsModifier(Decorator.BoundsModifier, helpEnabled);
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object o in targets)
                {
                    GeNaAnimatorDecorator animatorDecorator = (GeNaAnimatorDecorator) o;
                    // boundsDecorator.BoundsModifier.CopyFrom(Decorator.BoundsModifier);
                    // EditorUtility.SetDirty(boundsDecorator);
                }
            }
        }
    }
}
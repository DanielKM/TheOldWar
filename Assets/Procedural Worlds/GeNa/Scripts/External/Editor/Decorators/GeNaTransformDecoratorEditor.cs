using UnityEngine;
using UnityEditor;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaTransformDecorator))]
    public class GeNaTransformDecoratorEditor : GeNaDecoratorEditor<GeNaTransformDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Transform Decorator")]
        public static void AddDecorator(MenuCommand command)
        {
            GameObject gameObject = command.context as GameObject;
            if (gameObject != null)
            {
                GeNaTransformDecorator decorator = gameObject.AddComponent<GeNaTransformDecorator>();
                GeNaDecoratorEditorUtility.RegisterDecorator(gameObject, decorator);
            }
        }
        protected bool m_sameOScale = true;
        protected override void RenderPanel(bool helpEnabled)
        {
            bool snapToGround = Decorator.SnapToGround;
            bool conformToSlope = Decorator.ConformToSlope;
            Vector3 minOffset = Decorator.MinOffset;
            Vector3 maxOffset = Decorator.MaxOffset;
            Vector3 minRotation = Decorator.MinRotation;
            Vector3 maxRotation = Decorator.MaxRotation;
            Vector3 minScale = Decorator.MinScale;
            Vector3 maxScale = Decorator.MaxScale;
            EditorGUI.BeginChangeCheck();
            {
                snapToGround = EditorUtils.Toggle("Snap To Ground", snapToGround, helpEnabled);
                conformToSlope = EditorUtils.Toggle("Conform Slope", conformToSlope, helpEnabled);
                if (Advanced)
                {
                    minOffset = EditorUtils.Vector3Field("Min Position Offset", minOffset, helpEnabled);
                    maxOffset = EditorUtils.Vector3Field("Max Position Offset", maxOffset, helpEnabled);
                    EditorUtils.MinMaxSliderWithFields("X Rotation Offset", ref minRotation.x, ref maxRotation.x, -180f, 180f, helpEnabled);
                    EditorUtils.MinMaxSliderWithFields("Y Rotation Offset", ref minRotation.y, ref maxRotation.y, -180f, 180f, helpEnabled);
                    EditorUtils.MinMaxSliderWithFields("Z Rotation Offset", ref minRotation.z, ref maxRotation.z, -180f, 180f, helpEnabled);
                    minScale = EditorUtils.Vector3Field("Min Scale", minScale, helpEnabled);
                    maxScale = EditorUtils.Vector3Field("Max Scale", maxScale, helpEnabled);
                }
                else
                {
                    EditorUtils.MinMaxSliderWithFields("Position Modifier Y", ref minOffset.y, ref maxOffset.y, -10f, 10f, helpEnabled);
                    EditorUtils.MinMaxSliderWithFields("Y Rotation Offset", ref minRotation.y, ref maxRotation.y, -180f, 180f, helpEnabled);
                    m_sameOScale = EditorUtils.Toggle("Same O Scale", m_sameOScale, helpEnabled);
                    if (m_sameOScale)
                        EditorUtils.MinMaxSliderWithFields("Res Scale", ref minScale.x, ref maxScale.x, 0.1f, 100f, helpEnabled);
                    else
                    {
                        minScale = EditorUtils.Vector3Field("Res Min Scale", minScale, helpEnabled);
                        maxScale = EditorUtils.Vector3Field("Res Max Scale", maxScale, helpEnabled);
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Decorator.SnapToGround = snapToGround;
                Decorator.ConformToSlope = conformToSlope;
                Decorator.MinOffset = minOffset;
                Decorator.MaxOffset = maxOffset;
                Decorator.MinRotation = minRotation;
                Decorator.MaxRotation = maxRotation;
                Decorator.MinScale = minScale;
                Decorator.MaxScale = maxScale;
                foreach (Object @object in targets)
                {
                    if (@object is GeNaTransformDecorator decorator)
                    {
                        decorator.SnapToGround = Decorator.SnapToGround;
                        decorator.ConformToSlope = Decorator.ConformToSlope;
                        decorator.MinOffset = Decorator.MinOffset;
                        decorator.MaxOffset = Decorator.MaxOffset;
                        decorator.MinRotation = Decorator.MinRotation;
                        decorator.MaxRotation = Decorator.MaxRotation;
                        decorator.MinScale = Decorator.MinScale;
                        decorator.MaxScale = Decorator.MaxScale;
                        EditorUtility.SetDirty(decorator);
                    }
                }
            }
        }
    }
}
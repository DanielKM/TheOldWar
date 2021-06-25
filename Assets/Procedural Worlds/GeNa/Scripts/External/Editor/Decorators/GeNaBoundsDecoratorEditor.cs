using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using Object = UnityEngine.Object;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaBoundsDecorator))]
    public class GeNaBoundsDecoratorEditor : GeNaDecoratorEditor<GeNaBoundsDecorator>
    {
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
        [NonSerialized] protected readonly SphereBoundsHandle m_sphereHandle = new SphereBoundsHandle();
        [NonSerialized] protected readonly CapsuleBoundsHandle m_capsuleHandle = new CapsuleBoundsHandle();
        [NonSerialized] protected readonly BoxBoundsHandle m_boxHandle = new BoxBoundsHandle();
        [NonSerialized] protected readonly CylinderBoundsHandle m_cylinderHandle = new CylinderBoundsHandle();
        [NonSerialized] protected bool m_boundsEdited = false;
        protected void DrawHandles(GeNaBoundsDecorator boundsDecorator)
        {
            BoundsModifier boundsModifier = boundsDecorator.BoundsModifier;
            if (boundsModifier == null)
                return;
            Color oldColor = Handles.color;
            Handles.color = Color.red;
            Transform transform = boundsDecorator.transform;
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            Vector3 scale = transform.localScale;
            Matrix4x4 rotatedMatrix = Handles.matrix * Matrix4x4.TRS(position, rotation, scale);
            using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
            {
                using (new Handles.DrawingScope(rotatedMatrix))
                {
                    SdfShape.Type shapeType = boundsModifier.ShapeType;
                    switch (shapeType)
                    {
                        case SdfShape.Type.Box:
                            m_boxHandle.center = boundsModifier.Center;
                            m_boxHandle.size = boundsModifier.Size;
                            m_boxHandle.DrawHandle();
                            boundsModifier.Center = m_boxHandle.center;
                            boundsModifier.Size = m_boxHandle.size;
                            break;
                        case SdfShape.Type.Sphere:
                            m_sphereHandle.center = boundsModifier.Center;
                            m_sphereHandle.radius = boundsModifier.Radius;
                            m_sphereHandle.DrawHandle();
                            boundsModifier.Center = m_sphereHandle.center;
                            boundsModifier.Radius = m_sphereHandle.radius;
                            break;
                        case SdfShape.Type.Capsule:
                            m_capsuleHandle.center = boundsModifier.Center;
                            m_capsuleHandle.radius = boundsModifier.Radius;
                            m_capsuleHandle.height = boundsModifier.Height;
                            m_capsuleHandle.DrawHandle();
                            boundsModifier.Center = m_capsuleHandle.center;
                            boundsModifier.Radius = m_capsuleHandle.radius;
                            boundsModifier.Height = m_capsuleHandle.height;
                            break;
                        case SdfShape.Type.Cylinder:
                            m_cylinderHandle.center = boundsModifier.Center;
                            m_cylinderHandle.radius = boundsModifier.Radius;
                            m_cylinderHandle.height = boundsModifier.Height;
                            m_cylinderHandle.DrawHandle();
                            boundsModifier.Center = m_cylinderHandle.center;
                            boundsModifier.Radius = m_cylinderHandle.radius;
                            boundsModifier.Height = m_cylinderHandle.height;
                            break;
                    }
                }
                if (check.changed)
                {
                    m_boundsEdited = true;
                }
            }
            if (m_boundsEdited)
            {
                EditorUtility.SetDirty(Decorator);
                m_boundsEdited = false;
            }
            Handles.color = oldColor;
        }
        public override void OnSceneGUI()
        {
            base.OnSceneGUI();
            if (target == null)
                return;
            GeNaBoundsDecorator decorator = target as GeNaBoundsDecorator;
            DrawHandles(decorator);
        }
        protected override void RenderPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                EditorUtils.BoundsModifier(Decorator.BoundsModifier, helpEnabled);
                if (EditorUtils.Button("GetObjectBounds", helpEnabled))
                {
                    MeshFilter filter = Decorator.GetComponent<MeshFilter>();
                    if (filter == null)
                    {
                        filter = Decorator.GetComponentInChildren<MeshFilter>();
                    }
                    if (filter != null)
                    {
                        AutoGetBounds(filter.sharedMesh);
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object o in targets)
                {
                    GeNaBoundsDecorator boundsDecorator = (GeNaBoundsDecorator) o;
                    boundsDecorator.BoundsModifier.CopyFrom(Decorator.BoundsModifier);
                    EditorUtility.SetDirty(boundsDecorator);
                }
            }
        }
        /// <summary>
        /// Auto sets the Center and Size on the bounds decorator
        /// </summary>
        /// <param name="renderer"></param>
        private void AutoGetBounds(Mesh renderer)
        {
            if (renderer == null)
            {
                return;
            }
            Decorator.BoundsModifier.Center = renderer.bounds.center;
            Decorator.BoundsModifier.Size = renderer.bounds.size;
        }
    }
}
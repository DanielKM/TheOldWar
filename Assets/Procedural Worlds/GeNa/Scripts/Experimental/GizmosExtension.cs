using UnityEngine;
using GeNa.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GeNa.Extensions
{
    [CreateAssetMenu(fileName = "GizmosExtension", menuName = "Procedural Worlds/GeNa/Custom Extensions/GizmosExtension", order = 1)]
    public class GizmosExtension : GeNaSplineExtension
    {
        public float spacing = 0.1f;
        public float arrowSize = 4f;
        public float dotSize = .5f;
        
        // Execute is called when Updating the Spline.
        public override void Execute()
        {
        }
        
        // Called when the user 'Bakes' the Spline.
        protected override GameObject OnBake(GeNaSpline spline)
        {
            return null;
        }
        
        // Called when OnDrawGizmos is called on the Spline.
        protected override void OnDrawGizmos()
        {
        }
        
        // Called when OnDrawGizmosSelected is called on the Spline.
        protected override void OnDrawGizmosSelected()
        {
            // float delta = Mathf.Max(spacing, 1.0f);
            // float distance = 0f;
            // while (distance < Spline.Length)
            // {
            //     distance += delta;
            //     GeNaSample sample = Spline.GetSampleAtDistance(distance);
            //     if (sample != null)
            //     {
            //         Vector3 location = sample.Location;
            //         Terrain terrain = TerrainUtility.GetTerrain(location);
            //         if (terrain != null)
            //         {
            //             Transform terrainTransform = terrain.transform;
            //             Vector3 terrainPosition = terrainTransform.position;
            //             location.y = terrain.SampleHeight(location) + terrainPosition.y;
            //             Gizmos.DrawWireSphere(location, radius);
            //         }
            //     }
            // }
        }
        
        // Called when the Spline is Marked as Dirty
        protected override void OnSplineDirty()
        {
        }
        
        // Called when the user Activates the Extension on the Spline.
        protected override void OnActivate()
        {
        }
        
        // Called when the user Deactivates the Extension on the Spline.
        protected override void OnDeactivate()
        {
        }
        
        // Called when the Extension is Attached to a Spline.
        protected override void OnAttach(GeNaSpline spline)
        {
        }
        
        // Called when the Extension is Detatched from a Spline
        protected override void OnDetach(GeNaSpline spline)
        {
        }
        
        // Called when the Extension is Selected in the Editor.
        protected override void OnSelect()
        {
        }
        
        // Called when the Extension is Deselected in the Editor.
        protected override void OnDeselect()
        {
        }
        
        // Called just before Destroying the ScriptableObject.
        protected override void OnDelete()
        {
        }
        
        // Called when the OnSceneGUI method in the Editor gets called on the Spline.
        protected override void OnSceneGUI()
        {
            #if UNITY_EDITOR
            Handles.color = Color.red;
            float delta = Mathf.Max(spacing, 1.0f);
            float distance = 0f;
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.contentOffset = new Vector2(0f, -50f);
            while (distance < Spline.Length)
            {
                distance += delta;
                GeNaSample sample = Spline.GetSampleAtDistance(distance);
                if (sample != null)
                {
                    Vector3 location = sample.Location;
                    Terrain terrain = TerrainUtility.GetTerrain(location);
                    if (terrain != null)
                    {
                        Transform terrainTransform = terrain.transform;
                        Vector3 terrainPosition = terrainTransform.position;
                        location.y = terrain.SampleHeight(location) + terrainPosition.y;
                        Handles.color = Handles.yAxisColor;
                        float offsetY = arrowSize;
                        Handles.ArrowHandleCap(
                            0,
                            location + new Vector3(0f, offsetY + (arrowSize * .2f), 0f),
                            Quaternion.LookRotation(Vector3.down),
                            arrowSize,
                            EventType.Repaint
                        );
                        Handles.color = Color.red;
                        Handles.SphereHandleCap(0,
                            location, 
                            Quaternion.identity,
                            dotSize,
                            EventType.Repaint);
                        Handles.Label(location, $"{location.y:0.0}", style);   
                    }
                }
            }
            #endif
        }
    }
}
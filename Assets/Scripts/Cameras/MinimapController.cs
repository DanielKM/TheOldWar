using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapController : MonoBehaviour
{
    public Material cameraBoxMaterial;
    public Camera minimap;

    public float lineWidth;
    public Collider mapCollider;

    public float minX = 0f;
    public float minY = 0f;
    public float maxX = 0f;
    public float maxY = 0f;

    private Vector3 GetCameraFrustumPoint(Vector3 position)
    {
        var positionRay = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        Vector3 result = mapCollider.Raycast(positionRay, out hit, Camera.main.transform.position.y * 2) ? hit.point : new Vector3();

        return result;
    }

    public void OnPostRender()
    {
        // VIEWPORT FROM MAIN CAMERA
        Vector3 minViewportPoint = minimap.WorldToViewportPoint(GetCameraFrustumPoint(new Vector3(0f, 0f)));
        Vector3 maxViewportPoint = minimap.WorldToViewportPoint(GetCameraFrustumPoint(new Vector3(800f, 600f)));

        minX = minViewportPoint.x;
        minY = minViewportPoint.y;

        maxX = maxViewportPoint.x;
        maxY = maxViewportPoint.y;

        GL.PushMatrix();
        {
            cameraBoxMaterial.SetPass(0);
            GL.LoadOrtho();

            GL.Begin(GL.QUADS);
            GL.Color(Color.green);
            {
                // Bottom line
                GL.Vertex(new Vector3(minX, minY + lineWidth, 0));
                GL.Vertex(new Vector3(minX, minY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, minY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, minY + lineWidth, 0));

                // Left line
                GL.Vertex(new Vector3(minX + lineWidth, minY, 0));
                GL.Vertex(new Vector3(minX - lineWidth, minY, 0));
                GL.Vertex(new Vector3(minX - lineWidth, maxY, 0));
                GL.Vertex(new Vector3(minX + lineWidth, maxY, 0));

                // Top line
                GL.Vertex(new Vector3(minX, maxY + lineWidth, 0));
                GL.Vertex(new Vector3(minX, maxY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, maxY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, maxY + lineWidth, 0));

                // Right line
                GL.Vertex(new Vector3(maxX + lineWidth, minY, 0));
                GL.Vertex(new Vector3(maxX - lineWidth, minY, 0));
                GL.Vertex(new Vector3(maxX - lineWidth, maxY, 0));
                GL.Vertex(new Vector3(maxX + lineWidth, maxY, 0));

            }
            GL.End();
        }
        GL.PopMatrix();
    }
}

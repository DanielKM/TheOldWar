using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeNa.Core;

public class RaceTrackMaker : MonoBehaviour
{
    GameObject gameParent = null;
    List<GameObject> pathObjects = new List<GameObject>();
    List<Vector3> path = new List<Vector3>();
    GeNaSpline m_spline = null;
    Camera m_camera;

    void Start()
    {
        m_camera = Camera.main;
        StartTrack();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetKey(KeyCode.Return))
        {
            // Carve the current path, bake the road
            // and start a new one.
            if (path.Count >= 3 && m_spline != null)
            {
                GeNaCarveExtension carve = m_spline.GetExtension<GeNaCarveExtension>();
                carve.Carve();
                GeNaRoadExtension road = m_spline.GetExtension<GeNaRoadExtension>();
                road.Bake(true);
                StartTrack();
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            // Add a another point to the road path.
            Vector3 pos = HitPos(Input.mousePosition);
            if (pos != Vector3.zero)
            {
                path.Add(pos);

                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = "Path point";
                pathObjects.Add(go);
                go.transform.position = pos;
                go.transform.parent = gameParent.transform;

                BuildRoad();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            // Delete a node if very close to the mouse click.
            Vector3 pos = HitPos(Input.mousePosition);
            if (pos != Vector3.zero)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    if (Vector3.Distance(pos, path[i]) < 1.5f)
                    {
                        path.RemoveAt(i);
                        GameObject.Destroy(pathObjects[i]);
                        pathObjects.RemoveAt(i);
                        break;
                    }
                }

                BuildRoad();
            }
        }
    }

    void StartTrack()
    {
        if (gameParent != null)
            GameObject.Destroy(gameParent);
        path.Clear();
        pathObjects.Clear();

        gameParent = new GameObject("Road Path");
        gameParent.transform.position = Vector3.zero;

        m_spline = GeNaSpline.CreateSpline("Track");
        GeNaRoadExtension roadExt = m_spline.AddExtension<GeNaRoadExtension>();
        roadExt.Width = 10.0f;
        GeNaCarveExtension carveExt = m_spline.AddExtension<GeNaCarveExtension>();
        carveExt.Width = 10.0f;
        carveExt.Smoothness = 3.0f;
    }

    void BuildRoad()
    {
        if (path.Count < 3)
            return;

        m_spline.RemoveAllNodes();

        GeNaNode firstNode = m_spline.CreateNewNode(path[0]);
        GeNaNode prevNode = firstNode;
        for (int i = 0; i < path.Count; i++)
        {
            int n = (i + 1) % path.Count;
            GeNaNode nextNode = firstNode;
            if (n != 0)
                nextNode = m_spline.CreateNewNode(path[n]);
            GeNaCurve curve = m_spline.AddCurve(prevNode, nextNode);
            prevNode = nextNode;
        }
        m_spline.Smooth();
    }

    Vector3 HitPos(Vector2 mousePos)
    {
        Ray ray = m_camera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
}

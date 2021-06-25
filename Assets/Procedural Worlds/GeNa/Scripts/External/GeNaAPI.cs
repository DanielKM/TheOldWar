using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GeNa.Core.FlowAnalyzer;
namespace GeNa.Core
{
    public static class GeNaAPI
    {
        public class GeNaSplineAPI
        {
            GameObject m_geNaGameObject = null;
            public GeNaSpline m_geNaSpline = null;
            public GeNaNode CreateNewNode(PathNode pathNode, Vector3 position, out bool alreadyExists)
            {
                GeNaNode newNode = m_geNaSpline.GetNode(pathNode.ID);
                alreadyExists = true;
                if (newNode == null)
                {
                    newNode = m_geNaSpline.CreateNewNode(position);
                    newNode.ID = pathNode.ID;
                    alreadyExists = false;
                }
                return newNode;
            }
            public void GenerateGeNaSpline(List<RootPathNode> paths, Transform parent = null)
            {
                // GeNa Spline = Instance of Node Network (To attach Spawners To)
                if (m_geNaGameObject != null && m_geNaSpline != null)
                {
                    m_geNaSpline.UndoAll();
                    GameObject.DestroyImmediate(m_geNaGameObject);
                }
                m_geNaSpline = GeNaSpline.CreateSpline("River Spline");
                m_geNaGameObject = m_geNaSpline.gameObject;
                GeNaCarveExtension carve = m_geNaSpline.AddExtension<GeNaCarveExtension>();
                if (carve != null)
                {
                    carve.name = "Carve";
                    carve.HeightOffset = -0.5f;
                    carve.Smoothness = 2.5f;
                    carve.NoiseEnabled = true;
                    carve.NoiseStrength = 0.5f;
                    carve.MaskFractal.Octaves = 4;
                    carve.MaskFractal.Lacunarity = 2.5f;
                }
                GeNaClearDetailsExtension clearDetails = m_geNaSpline.AddExtension<GeNaClearDetailsExtension>();
                if (clearDetails != null)
                {
                    clearDetails.name = "Clear Details/Grass";
                    clearDetails.Width = 1.0f;
                    clearDetails.Smoothness = 0.8f;
                }
                GeNaClearTreesExtension clearTrees = m_geNaSpline.AddExtension<GeNaClearTreesExtension>();
                if (clearTrees != null)
                {
                    clearTrees.name = "Clear Trees";
                    clearTrees.Width = 1.0f;
                    clearTrees.Smoothness = 1.5f;
                }
                GeNaRiverExtension river = m_geNaSpline.AddExtension<GeNaRiverExtension>();
                if (river != null)
                {
                    river.name = "River";
                }
                foreach (RootPathNode rootNode in paths)
                {
                    Vector3 curPos = rootNode.Position;
                    bool nodeAlreadyExisted = false;
                    GeNaNode prevNode = CreateNewNode(rootNode, curPos, out nodeAlreadyExisted);
                    m_geNaSpline.AddNode(prevNode);
                    for (int i = 1; i < rootNode.Path.Count; i++)
                    {
                        PathNode node = rootNode.Path[i];
                        curPos = node.Position;
                        GeNaNode nextNode = null;
                        if (node.ConnectedTo != null)
                        {
                            node = node.ConnectedTo;
                            nextNode = CreateNewNode(node, curPos, out nodeAlreadyExisted);
                            if (nodeAlreadyExisted)
                                m_geNaSpline.AddCurve(prevNode, nextNode);
                            else
                                m_geNaSpline.AddNode(prevNode, nextNode);
                            break;
                        }
                        nextNode = CreateNewNode(node, curPos, out nodeAlreadyExisted);
                        m_geNaSpline.AddNode(prevNode, nextNode);
                        prevNode = nextNode;
                    }
                }
                GeNaSpawnerExtension spawner = m_geNaSpline.AddExtension<GeNaSpawnerExtension>();
                GameObject spawnerObject = Resources.Load<GameObject>("Prefabs/Spawners/Spawner - Reflection Probe");
                if (spawnerObject != null)
                {
                    spawner.Spawner = spawnerObject.GetComponent<GeNaSpawner>();
                    spawner.FlowRate = 5f;
                }
#if UNITY_EDITOR
                Selection.activeGameObject = m_geNaGameObject;
#endif
            }
            public void UndoAll()
            {
                if (m_geNaSpline != null)
                {
                    m_geNaSpline.Undo();
                    m_geNaSpline.UndoAll();
                    m_geNaSpline.RemoveAllNodes();
                }
            }
        }
    }
}
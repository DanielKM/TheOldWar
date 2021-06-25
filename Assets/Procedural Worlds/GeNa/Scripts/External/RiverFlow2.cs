//Copyright(c)2020 Procedural Worlds Pty Limited 
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GeNa.Core
{
    public class RiverFlow2 : MonoBehaviour
    {
        [HideInInspector] public List<Vector3> m_startPositions = new List<Vector3>();
        [Header("Calculation Parameters")] public bool m_selectTerrainOnly = true;
        [Min(0.0f)] public float m_seaLevel = 25.0f;
        [Range(0.05f, 3.0f)] public float m_startDepth = 0.15f;
        [Range(2.0f, 8.0f)] public float m_minNodeDistance = 2.0f;
        [Range(1.0f, 8.0f)] public float m_connectSnapDistance = 4.0f;
        public bool m_autoAnalyze = true;

        [Header("Simplification Parameters")] [Range(0.5f, 1.5f)]
        public float m_yScale = 1.0f;

        [Range(0.5f, 2.5f)] public float m_simplifyEpsilon = 1.25f;
        public bool m_showOriginalPath = true;

        List<List<WaterNode>> m_waterFlows = new List<List<WaterNode>>();
        [HideInInspector] public List<List<WaterNode>> m_simplifiedWaterFlows = new List<List<WaterNode>>();
        List<Vector3> m_searchSpaces = new List<Vector3>();
        List<(Vector3 from, Vector3 to)> m_tryConnects = new List<(Vector3 from, Vector3 to)>();

        [HideInInspector] public float selectionDiameter = 3.0f;

        private void OnEnable()
        {
            m_seaLevel = GeNaEvents.GetSeaLevel(m_seaLevel);
        }

        public void UpdateFlows()
        {
            if (m_autoAnalyze)
            {
#if UNITY_EDITOR
                CreateRiverFlowsClicked();
#endif
            }

        }

        public void CreateRiverFlowsClicked()
        {
            if (m_startPositions.Count == 0)
                return;
            ClearOutputs();
            m_searchSpaces = null;
            m_tryConnects = null;
            WaterFlow._raycastTerrainOnly = m_selectTerrainOnly;
            WaterFlow._checkDistance = m_minNodeDistance;
            WaterFlow._attractDistance = m_connectSnapDistance;
            m_waterFlows =
                WaterFlow.FindFlows(m_startPositions, m_startDepth, m_seaLevel, m_searchSpaces, m_tryConnects);
            Vector3 scale = new Vector3(1.0f, m_yScale, 1.0f);
            m_simplifiedWaterFlows = WaterFlow.SimplifyFlows(m_waterFlows, scale, m_simplifyEpsilon);
        }

        public void CreateSplinesClicked()
        {
            CreateRiverSpline(m_simplifiedWaterFlows);
        }

        public void ClearClicked()
        {
            m_startPositions.Clear();
            ClearOutputs();
        }

        public void AddPoint(Vector3 point)
        {
            int index = GetPointIndex(point);
            if (index < 0)
            {
                m_startPositions.Add(point);
                CheckAutoAnalyze(true);
            }
        }

        public void RemovePoint(Vector3 point)
        {
            int index = GetPointIndex(point);
            if (index >= 0)
            {
                m_startPositions.RemoveAt(index);
                CheckAutoAnalyze(true);
            }
        }

        private void ClearOutputs()
        {
            m_waterFlows.Clear();
            m_simplifiedWaterFlows.Clear();

            if (m_searchSpaces != null)
                m_searchSpaces.Clear();
            if (m_tryConnects != null)
                m_tryConnects.Clear();
        }

        private List<Vector3> CreateFlowNodes()
        {
            List<Vector3> startNodes = new List<Vector3>();

            // Make sure they are on the ground.
            for (int i = 0; i < m_startPositions.Count; i++)
            {
                startNodes.Add(m_startPositions[i]);
            }

            return startNodes;
        }

        private void CheckAutoAnalyze(bool pointAdded)
        {
            if (m_autoAnalyze && pointAdded)
            {
                CreateRiverFlowsClicked();
            }
        }

        private int GetPointIndex(Vector3 point)
        {
            for (int i = 0; i < m_startPositions.Count; i++)
            {
                if ((m_startPositions[i] - point).magnitude <= selectionDiameter)
                    return i;
            }

            return -1;
        }

        private void CreateRiverSpline(List<List<WaterNode>> flows)
        {
            GeNaSpline spline = GeNaSpline.CreateSpline("River Spline");

            foreach (List<WaterNode> waterNodes in flows)
            {
                GeNaNode prevNode = spline.CreateNewNode(waterNodes[0].m_position);
                spline.AddNode(prevNode);
                waterNodes[0].m_id = prevNode.ID;

                for (int i = 1; i < waterNodes.Count; i++)
                {
                    GeNaNode nextNode = null;
                    if (waterNodes[i].m_connected != null && waterNodes[i].m_connected.m_id != 0)
                    {
                        nextNode = spline.GetNode(waterNodes[i].m_connected.m_id);
                        if (nextNode != null)
                        {
                            spline.AddCurve(prevNode, nextNode);
                            prevNode = nextNode;
                            waterNodes[i].m_id = nextNode.ID;
                            continue;
                        }
                    }

                    nextNode = spline.CreateNewNode(waterNodes[i].m_position);
                    waterNodes[i].m_id = nextNode.ID;
                    spline.AddCurve(prevNode, nextNode);
                    prevNode = nextNode;
                }
            }

            GeNaCarveExtension carve = spline.AddExtension<GeNaCarveExtension>();
            if (carve != null)
            {
                carve.name = "Carve";
                carve.HeightOffset = -0.8f;
                carve.Width = 4f;
                carve.Smoothness = 3.5f;
                carve.NoiseEnabled = true;
                carve.NoiseStrength = 0.2f;
                carve.MaskFractal.Octaves = 4;
                carve.MaskFractal.Lacunarity = 2.5f;
            }

            GeNaClearDetailsExtension clearDetails = spline.AddExtension<GeNaClearDetailsExtension>();
            if (clearDetails != null)
            {
                clearDetails.name = "Clear Details/Grass";
                clearDetails.Width = 2f;
                clearDetails.Smoothness = 1f;
            }

            GeNaClearTreesExtension clearTrees = spline.AddExtension<GeNaClearTreesExtension>();
            if (clearTrees != null)
            {
                clearTrees.name = "Clear Trees";
                clearTrees.Width = 2f;
                clearTrees.Smoothness = 1.5f;
            }

            GeNaRiverExtension river = spline.AddExtension<GeNaRiverExtension>();
            if (river != null)
            {
                river.name = "River";
                river.RiverProfile = Resources.Load<GeNaRiverProfile>("River Profiles/GeNa River Profile Green");
                spline.Smooth();
            }

            GeNaSpawnerExtension spawner = spline.AddExtension<GeNaSpawnerExtension>();
            GameObject spawnerObject = Resources.Load<GameObject>("Prefabs/Spawners/Spawner - Reflection Probe");
            if (spawnerObject != null)
            {
                spawner.Spawner = spawnerObject.GetComponent<GeNaSpawner>();
                spawner.FlowRate = 30f;
            }

            spline.UpdateSpline();

#if UNITY_EDITOR
            Selection.activeGameObject = spline.gameObject;
#endif
        }

        private void DisplayWaterFlowsGizmos(List<List<WaterNode>> flows, Color lineColor, bool showAxis)
        {
            foreach (List<WaterNode> flowNodes in flows)
            {
                if (flowNodes.Count > 0)
                {
                    if (showAxis)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawRay(flowNodes[0].m_position, Vector3.forward * 3.0f);
                        Gizmos.color = Color.red;
                        Gizmos.DrawRay(flowNodes[0].m_position, Vector3.right * 3.0f);
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawRay(flowNodes[0].m_position, Vector3.up * 3.0f);
                    }

                    for (int i = 0; i < flowNodes.Count; i++)
                    {
                        if (flowNodes[i].m_connected != null)
                            Gizmos.color = Color.blue;
                        else
                            Gizmos.color = Color.green;
                        Gizmos.DrawSphere(flowNodes[i].m_position, flowNodes[i].m_depth);
                        Gizmos.DrawRay(flowNodes[i].m_position, flowNodes[i].m_direction * 1.0f);

                        if (i < flowNodes.Count - 1)
                        {
                            Gizmos.color = lineColor;
                            Gizmos.DrawLine(flowNodes[i].m_position, flowNodes[i + 1].m_position);
                        }
                    }
                }
            }

        }

        private void OnDrawGizmosSelected()
        {
            if (m_waterFlows == null)
                return;
            if (m_showOriginalPath)
                DisplayWaterFlowsGizmos(m_waterFlows, Color.magenta, false);
            DisplayWaterFlowsGizmos(m_simplifiedWaterFlows, Color.yellow, true);

            if (m_searchSpaces == null)
                return;

            Gizmos.color = Color.red;
            for (int i = 0; i < m_searchSpaces.Count; i++)
            {
                Gizmos.DrawWireSphere(m_searchSpaces[i], 0.13f);
            }

            if (m_tryConnects == null)
                return;

            for (int i = 0; i < m_tryConnects.Count; i++)
            {
                Gizmos.color = Color.blue;
                if (m_tryConnects[i].from.y == m_tryConnects[i].to.y)
                    Gizmos.color = Color.white;

                Gizmos.DrawLine(m_tryConnects[i].from, m_tryConnects[i].to);
                Gizmos.color = Color.green;

                Vector3 direction = (m_tryConnects[i].to - m_tryConnects[i].from);
                float halfDist = direction.magnitude * 0.5f;
                direction = direction.normalized;
                Gizmos.DrawRay(m_tryConnects[i].from, direction * halfDist);
            }

        }
    }
}
//Copyright(c)2020 Procedural Worlds Pty Limited 
using System.Collections.Generic;
using UnityEngine;
using GeNa.Core.FlowAnalyzer;
namespace GeNa.Core
{
    [ExecuteAlways]
    public class RiverFlow : MonoBehaviour
    {
        [HideInInspector]
        public List<Vector3> m_startPositions = new List<Vector3>();
        [Header("River Flow Calculation Parameters")]
        public bool m_selectTerrainOnly = true;
        [Min(0.0f)]
        public float m_seaLevel = 25.0f;
        [Range(0.05f, 3.0f)]
        public float m_startFlow = 0.15f;
        [Range(0.5f, 1.5f)]
        public float YScale = 1.0f;
        [Range(0.8f, 2.5f)]
        public float SimplifyEpsilon = 1.25f;
        [Header("Visual Preferences")]
        public Color SimpleRiverPathColor = Color.yellow;
        public bool ShowSimpleRiverPath = true;
        public bool ShowRiverConnections = true;
        public bool AutoAnalyze = false;
        private bool ShowOriginalRiverPaths = false;
        private Color RiverPathColor = Color.blue;
        private bool ShowRiverVelocity = false;
        private bool ShowFlowDirectionMap = false;
        [HideInInspector]
        public float selectionDiameter = 3.0f;
        ElevationFlowAnalyzer m_flowAnalyzer = null;
        private List<RootPathNode> m_riverPaths = null;
        public List<RootPathNode> m_simpliedPaths = null;
        private bool m_simplifiedParmsChanged = false;
        private float epsilonScale = 1.0f;
        private GeNaAPI.GeNaSplineAPI m_genaAPI = new GeNaAPI.GeNaSplineAPI();
        private void OnEnable()
        {
            m_seaLevel = GeNaEvents.GetSeaLevel(m_seaLevel);
        }
        public void CreateRiverFlowsClicked()
        {
            if (m_startPositions.Count < 1)
                return;
            Terrain terrain = GetTerrainAtPoint(m_startPositions[0]);
            if (terrain == null)
                return;
            m_flowAnalyzer = new ElevationFlowAnalyzer(terrain);
            List<FlowFromNode> startNodes = CreateFlowNodes();
            m_riverPaths = m_flowAnalyzer.CreateRiverPaths(startNodes, m_seaLevel);
            float width = Terrain.activeTerrain.terrainData.heightmapResolution;
            Vector3 size = Terrain.activeTerrain.terrainData.size;
            epsilonScale = (size / width).x;
            //m_flowAnalyzer.SimplifyPaths(m_riverPaths, new Vector3(1.0f, YScale, 1.0f), SimplifyEpsilon * epsilonScale);
            if (m_genaAPI == null)
                m_genaAPI = new GeNaAPI.GeNaSplineAPI();
            m_simplifiedParmsChanged = true;
        }
        private Terrain GetTerrainAtPoint(Vector3 point)
        {
            Terrain[] terrains = Terrain.activeTerrains;
            if (terrains.Length < 1)
                return null;
            Terrain curTerrain = terrains[0];
            Vector3 terrainPos = GetTerrainCenter(curTerrain);
            float curDistance = (terrainPos - point).sqrMagnitude;
            for (int i = 1; i < terrains.Length; i++)
            {
                terrainPos = GetTerrainCenter(terrains[i]);
                float distance = (terrainPos - point).sqrMagnitude;
                if (distance < curDistance)
                {
                    curDistance = distance;
                    curTerrain = terrains[i];
                }
            }
            return curTerrain;
        }
        private Vector3 GetTerrainCenter(Terrain terrain)
        {
            return terrain.GetPosition() + terrain.terrainData.size * 0.5f;
        }
        public void CreateSplinesClicked()
        {
            m_genaAPI.GenerateGeNaSpline(m_simpliedPaths);
        }
        public void ClearClicked()
        {
            m_flowAnalyzer = null;
            m_riverPaths = null;
            m_simpliedPaths = null;
            m_startPositions.Clear();
            if (m_genaAPI != null)
            {
                //m_genaAPI.Clear();
            }
        }
        List<FlowFromNode> CreateFlowNodes()
        {
            List<FlowFromNode> startNodes = new List<FlowFromNode>();
            for (int i = 0; i < m_startPositions.Count; i++)
            {
                float height = 0.0f;
                Vector2Int gridLoc = m_flowAnalyzer.GetGridLocation(m_startPositions[i], out height);
                //Debug.Log($"Creating startNode for {m_startPositions[i] - Terrain.activeTerrain.GetPosition()} at gridLoc {gridLoc} = {m_flowAnalyzer.GetWorldPosition(gridLoc)}");
                FlowFromNode node = new FlowFromNode(height, gridLoc.x, gridLoc.y, m_startFlow);
                startNodes.Add(node);
            }
            return startNodes;
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
        void CheckAutoAnalyze(bool pointAdded)
        {
            if (AutoAnalyze && pointAdded)
            {
                CreateRiverFlowsClicked();
            }
            else if (m_riverPaths != null)
            {
                for (int i = m_riverPaths.Count - 1; i >= 0; i--)
                {
                    int index = GetPointIndex(m_riverPaths[i].Position);
                    if (index < 0)
                    {
                        m_riverPaths.RemoveAt(i);
                        m_simpliedPaths.RemoveAt(i);
                    }
                }
            }
        }
        public void RemovePoint(Vector3 point)
        {
            int index = GetPointIndex(point);
            if (index >= 0)
            {
                m_startPositions.RemoveAt(index);
                CheckAutoAnalyze(false);
            }
        }
        int GetPointIndex(Vector3 point)
        {
            for (int i = 0; i < m_startPositions.Count; i++)
            {
                if ((m_startPositions[i] - point).magnitude <= selectionDiameter)
                    return i;
            }
            return -1;
        }
        float lastEpsilon = -99.0f;
        float lastYScale = -99.0f;
        private void OnDrawGizmosSelected()
        {
            if (m_riverPaths == null)
                return;
            if (ShowFlowDirectionMap)
                OnDrawGizmosFlowDirectionMap();
            if (ShowOriginalRiverPaths)
                OnDrawGizmosRiverPaths();
            if (m_simpliedPaths == null || lastEpsilon != SimplifyEpsilon)
                m_simplifiedParmsChanged = true;
            if (lastYScale != YScale)
                m_simplifiedParmsChanged = true;
            lastEpsilon = SimplifyEpsilon;
            lastYScale = YScale;
            OnDrawGizmosSimplifiedRivers();
        }
        private void OnDrawGizmosRiverPaths()
        {
            foreach (RootPathNode rootNode in m_riverPaths)
            {
                Vector3 curPos = rootNode.Position + Vector3.up * rootNode.Flow * 1.0f;
                foreach (PathNode pathNode in rootNode.Path)
                {
                    Gizmos.color = RiverPathColor;
                    Gizmos.DrawLine(curPos, pathNode.Position + Vector3.up * pathNode.Flow * 2.0f);
                    if (ShowRiverConnections)
                    {
                        if (pathNode.ConnectedTo != null)
                        {
                            Gizmos.color = new Color(0.4f, 0.6f, 1.0f);
                            Vector3 pos = pathNode.ConnectedTo.Position;
                            Gizmos.DrawWireCube(pos, Vector3.one);
                        }
                        if (pathNode.ConnectedFrom != null)
                        {
                            Gizmos.color = Color.magenta;
                            Vector3 pos = pathNode.ConnectedFrom.Position;
                            Gizmos.DrawWireCube(pos, Vector3.one);
                        }
                    }
                    Gizmos.color = Color.cyan;
                    if (ShowRiverVelocity)
                    {
                        Vector3 velocity = pathNode.Velocity;
                        Vector3 target = curPos + velocity;
                        Gizmos.DrawLine(curPos, target);
                    }
                    curPos = pathNode.Position + Vector3.up * pathNode.Flow * 2.0f;
                }
            }
        }
        void OnDrawGizmosSimplifiedRivers()
        {
            if (m_flowAnalyzer == null)
                return;
            if (m_simpliedPaths == null)
                m_simplifiedParmsChanged = true;
            if (m_simplifiedParmsChanged)
            {
                Vector3 scale = new Vector3(1.0f, YScale, 1.0f);
                m_simpliedPaths = m_flowAnalyzer.SimplifyedPaths(m_riverPaths, scale, SimplifyEpsilon * epsilonScale);
                m_simplifiedParmsChanged = false;
            }
            if (!ShowSimpleRiverPath)
                return;
            foreach (RootPathNode rootNode in m_simpliedPaths)
            {
                Vector3 scale = new Vector3(1.0f, YScale, 1.0f);
                //List<ElevationFlowAnalyzer.PathNode> nodes = Simplify.DouglasPeucker(rootNode.Path, scale, SimplifyEpsilon * epsilonScale);
                List<PathNode> nodes = rootNode.Path;
                Gizmos.color = SimpleRiverPathColor;
                Vector3 curPos = nodes[0].Position;
                Gizmos.DrawWireCube(curPos, Vector3.one * 0.25f);
                foreach (PathNode node in nodes)
                {
                    if (node != rootNode)
                    {
                        Gizmos.color = SimpleRiverPathColor;
                        Vector3 point = node.Position;
                        Gizmos.DrawLine(curPos, point);
                        curPos = point;
                        Gizmos.DrawWireCube(curPos, Vector3.one * 0.25f);
                    }
                    if (ShowRiverConnections)
                    {
                        if (node.ConnectedTo != null)
                        {
                            Gizmos.color = new Color(0.4f, 0.6f, 1.0f);
                            Vector3 pos = node.ConnectedTo.Position;
                            Gizmos.DrawWireCube(pos, Vector3.one);
                        }
                        if (node.ConnectedFrom != null)
                        {
                            Gizmos.color = Color.magenta;
                            Vector3 pos = node.ConnectedFrom.Position;
                            Gizmos.DrawWireCube(pos, Vector3.one);
                        }
                    }
                }
            }
        }
        private void OnDrawGizmosFlowDirectionMap()
        {
            if (m_flowAnalyzer == null || m_flowAnalyzer.m_flowDirectionMap == null)
                return;
            Vector3[,] dirMap = m_flowAnalyzer.m_flowDirectionMap;
            int width = dirMap.GetLength(0);
            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null)
                return;
            Transform terrainTransform = terrain.transform;
            Vector3 terrainPosition = terrainTransform.position;
            float scale = terrain.terrainData.size.x / (width - 1);
            Vector3 tPos = terrain.GetPosition();
            Color shortColor = new Color(0.5f, 0.2f, 0.0f);
            Color longColor = new Color(1.0f, 1.0f, 0.0f);
            Gizmos.color = Color.grey;
            for (int r = 1; r < width - 1; r++)
            {
                for (int c = 1; c < width - 1; c++)
                {
                    //if (dirMap[r, c].sqrMagnitude >= 0.001f)
                    {
                        float x = c * scale + tPos.x;
                        float z = r * scale + tPos.z;
                        float y = terrain.SampleHeight(new Vector3(x, 1000.0f, z)) + terrainPosition.y;
                        Vector3 pos = new Vector3(x, y, z);
                        if (y >= m_seaLevel)
                            Gizmos.DrawLine(pos, pos + dirMap[r, c]);
                    }
                }
            }
        }
    }
}
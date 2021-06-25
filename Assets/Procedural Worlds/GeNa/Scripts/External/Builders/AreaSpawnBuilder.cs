//Copyright(c)2021 Procedural Worlds Pty Limited 
using System.Collections.Generic;
using UnityEngine;

namespace GeNa.Core
{
    [CreateAssetMenu(fileName = "Area Spawner Builder", menuName = "Procedural Worlds/GeNa/Builders/Area Spawner Builder", order = 1)]
    public class AreaSpawnBuilder : AreaBuilder
    {
        #region Definitions
        private class GridNode
        {
            public uint id;
            public Vector3 position;
            public bool isIntersection;
            public GridNode(Vector3 position)
            {
                id = 0;
                this.position = position;
                isIntersection = false;
            }
        }
        /// <summary>
        /// Divides up a circle into a grid of blockSize cells.
        /// The grid is a 2 dimensional array of enum BlockType.
        /// Some roads are assigned space at creation time.
        /// The grid is axis aligned in the XZ plane, and
        /// will therefore need to be rotated by the consumer.
        /// </summary>
        private class AreaGrid
        {
            private enum BlockType
            {
                Empty = 0,
                Road = 1,
                Buiding = 2,
                Vacant = 3,
                Reserved = 4
            }
            private float BlockSize;
            private BlockType[,] Grid;
            #region Properties
            public bool IsFullTown
            {
                get
                {
                    int divisions = Grid.GetLength(0);
                    for (int r = 0; r < divisions; r++)
                        for (int c = 0; c < divisions; c++)
                            if (Grid[r, c] == BlockType.Empty)
                                return false;
                    return true;
                }
            }

            public int EmptyBlocks
            {
                get
                {
                    int divisions = Grid.GetLength(0);
                    int count = 0;
                    for (int r = 0; r < divisions; r++)
                        for (int c = 0; c < divisions; c++)
                            if (Grid[r, c] == BlockType.Empty)
                                count++;
                    return count;
                }
            }
            #endregion
            public AreaGrid(float radius,  float blockSize)
            {
                float sideLength = radius * 1.4142135f;
                int divisions = (int)(sideLength / blockSize);
                if ((divisions & 1) == 0)
                    divisions--;
                BlockSize = sideLength / divisions;
                Grid = new BlockType[divisions, divisions];
                // Reserve the corners as "unused".
                Grid[0, 0] = BlockType.Reserved;
                Grid[0, divisions - 1] = BlockType.Reserved;
                Grid[divisions - 1, 0] = BlockType.Reserved;
                Grid[divisions - 1, divisions - 1] = BlockType.Reserved;
                // Reserve the verticle roads. There is always 1 down the center.
                int centerColumn = divisions >> 1;
                int sideSize;
                if (divisions < 14)
                    sideSize = 4;
                else
                    sideSize = Random.Range(3, 6) * 2;
                int numVertRoads = (divisions - 1 - sideSize) / 6;
                numVertRoads = (numVertRoads < 0) ? 0 : numVertRoads;
                for (int i = 0; i <= numVertRoads; i++)
                {
                    for (int row = 0; row < divisions; row++)
                    {
                        Grid[row, centerColumn + i * 3] = BlockType.Road;
                        Grid[row, centerColumn - i * 3] = BlockType.Road;
                    }
                }
                int horzTownBlockSize = Random.Range(5, 8);
                int numHorzRoads = (divisions - 10) / horzTownBlockSize;
                numHorzRoads = (numHorzRoads < 0) ? 0 : numHorzRoads + 1;
                int horzStart = Random.Range(4, 7);
                if (numHorzRoads <= 1 || horzStart >= divisions)
                {
                    horzStart = divisions / 2;
                    numHorzRoads = 1;
                }
                for (int i = 0; i < numHorzRoads; i++)
                {
                    int row = horzStart + i * horzTownBlockSize;
                    if (row >= divisions)
                        break;
                    for (int col = 0; col < divisions; col++)
                        Grid[row, col] = BlockType.Road;
                }
                FindVacantLots();
                //Debug.Log($"Area with Radius = {Radius} has {numVertRoads * 2 + 1} verticle road(s), {numHorzRoads} horizontal road(s), and {numVacantLots} vacant lots.");
            }
            #region Methods
            #region Internal
            private void FindVacantLots()
            {
                int divisions = Grid.GetLength(0);
                for (int r = 1; r < divisions - 1; r++)
                {
                    for (int c = 1; c < divisions - 1; c++)
                    {
                        if (Grid[r - 1, c] != BlockType.Road && Grid[r + 1, c] != BlockType.Road && Grid[r, c - 1] != BlockType.Road && Grid[r, c + 1] != BlockType.Road)
                            Grid[r, c] = BlockType.Vacant;
                    }
                }
            }
            internal Vector2 Internal_CalcBuildingPosition(int row, int col, int numBlocksWide)
            {
                int divisions = Grid.GetLength(0);
                float offset = (divisions * 0.5f) * BlockSize;
                float x = (float)col + (numBlocksWide * 0.5f);
                float z = (float)divisions - ((float)row + (numBlocksWide * 0.5f));
                Vector2 position = new Vector2(x * BlockSize, z * BlockSize);
                position -= new Vector2(offset, offset);
                return position;
            }
            internal Vector3 Internal_GridToPosition(int row, int col)
            {
                int divisions = Grid.GetLength(0);
                float offset = (divisions * 0.5f) * BlockSize;
                float x = (float)col + 0.5f;
                float z = (float)divisions - ((float)row + 0.5f);
                Vector3 position = new Vector3(x * BlockSize, 0.0f, z * BlockSize);
                position -= new Vector3(offset, 0.0f, offset);
                return position;
            }
            internal float Internal_CalcBuildingRotation(int row, int col, int numBlocksWide)
            {
                int divisions = Grid.GetLength(0);
                int roadMod = divisions % 3;
                roadMod = (roadMod == 1) ? 0 : (roadMod == 0) ? 1 : roadMod;
                float rotation = ((col % 3) == ((roadMod + 1) % 3)) ? -90f : 90.0f;
                if (numBlocksWide > 1)
                    rotation = (col < divisions / 2) ? 90.0f : -90.0f;
                // If there is a road to the left or the right, we are done.
                if (col > 0 && Grid[row, col - 1] == BlockType.Road)
                    return rotation;
                if (col + numBlocksWide < divisions && Grid[row, col + numBlocksWide] == BlockType.Road)
                    return rotation;
                // Check above and below.
                if (row > 0 && Grid[row - 1, col] == BlockType.Road)
                    rotation = 0.0f;
                if (row + numBlocksWide < divisions && Grid[row + numBlocksWide, col] == BlockType.Road)
                    rotation = 180.0f;

                return rotation;
            }
            /// <summary>
            /// Returns the outer nodes that can be connected to externally to the town.
            /// </summary>
            /// <returns>List of internal nodes.</returns>
            internal List<List<GridNode>> Internal_CreateInternalRoads()
            {
                List<List<GridNode>> internalNodes = new List<List<GridNode>>();

                int divisions = Grid.GetLength(0);
                for (int c = 1; c < divisions - 1; c++)
                {
                    if (Grid[0, c] == BlockType.Road)
                    {
                        List<GridNode> thisRoad = new List<GridNode>();
                        internalNodes.Add(thisRoad);
                        Vector3 pos = Internal_GridToPosition(0, c);
                        thisRoad.Add(new GridNode(pos + Vector3.forward * BlockSize));
                        thisRoad.Add(new GridNode(pos));

                        for (int r = 1; r < divisions - 1; r++)
                        {
                            if (Grid[r, c - 1] == BlockType.Road || Grid[r, c + 1] == BlockType.Road)
                            {
                                GridNode node = new GridNode(Internal_GridToPosition(r, c));
                                node.id = (uint )r * 1000 + (uint )c;
                                node.isIntersection = true;
                                thisRoad.Add(node);
                            }
                        }
                        pos = Internal_GridToPosition(divisions - 1, c);
                        thisRoad.Add(new GridNode(pos));
                        thisRoad.Add(new GridNode(pos + Vector3.back * BlockSize));
                    }
                }

                for (int r = 1; r < divisions - 1; r++)
                {
                    if (Grid[r, 0] == BlockType.Road)
                    {
                        List<GridNode> thisRoad = new List<GridNode>();
                        internalNodes.Add(thisRoad);
                        Vector3 pos = Internal_GridToPosition(r, 0);
                        thisRoad.Add(new GridNode(pos + Vector3.left * BlockSize));
                        thisRoad.Add(new GridNode(pos));

                        for (int c = 1; c < divisions - 1; c++)
                        {
                            if (Grid[r - 1, c] == BlockType.Road || Grid[r + 1, c] == BlockType.Road)
                            {
                                GridNode node = new GridNode(Internal_GridToPosition(r, c));
                                node.id = (uint)r * 1000 + (uint)c;
                                node.isIntersection = true;
                                thisRoad.Add(node);
                            }
                        }

                        pos = Internal_GridToPosition(r, divisions - 1);
                        thisRoad.Add(new GridNode(pos));
                        thisRoad.Add(new GridNode(pos + Vector3.right * BlockSize));
                    }
                }

                return internalNodes;
            }
            internal bool Internal_CanFit(float size) => Internal_FindBlocks(size, out _, out _, false);
            /// <summary>
            /// Returns the axis aligned relative position of the
            /// reserved blocks center if enough room found.
            /// </summary>
            /// <param name="size"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            /// <param name="reserve"></param>
            /// <returns></returns>
            internal bool Internal_FindBlocks(float size, out Vector2 position, out float rotation , bool reserve = true)
            {
                position = Vector2.zero;
                rotation = 0.0f;
                int divisions = Grid.GetLength(0);
                int numBlocksWide = (int )(size / BlockSize + 1.0f);
                int count = divisions - numBlocksWide + 1;
                for (int r = 0; r < count; r++)
                {
                    for (int c = 0; c < count; c++)
                    {
                        bool found = true;
                        int vacants = 0;
                        for (int i = 0; i < numBlocksWide; i++)
                        {
                            for (int j = 0; j < numBlocksWide; j++)
                            {
                                if (Grid[r + i, c + j] != BlockType.Empty)
                                {
                                    if (numBlocksWide > 1 && Grid[r + i, c + j] == BlockType.Vacant)
                                    {
                                        vacants++;
                                        continue;
                                    }
                                    found = false;
                                    break;
                                }
                            }
                            if (!found)
                                break;
                        }
                        if (numBlocksWide < 3 &&  vacants == numBlocksWide * numBlocksWide)
                            found = false;
                        if (found)
                        {
                            if (reserve)
                                for (int i = 0; i < numBlocksWide; i++)
                                    for (int j = 0; j < numBlocksWide; j++)
                                        Grid[r + i, c + j] = BlockType.Buiding;
                            position = Internal_CalcBuildingPosition(r, c, numBlocksWide);
                            rotation = Internal_CalcBuildingRotation(r, c, numBlocksWide);
                            return true;
                        }
                    }
                }
                return false;
            }
            #endregion
            #endregion
        }
        #endregion
        #region Methods
        #region Internal
        internal List<GeNaNode> Internal_BuildArea(Vector2 axis, AreaFinder.AreaPoly area, float cellSize, List<GeNaSpawner> buildingSpawners, List<GameObject> buildingPrefabs, GeNaSpline roadSpline)
        {
            if (buildingSpawners == null || buildingSpawners.Count == 0)
                return new List<GeNaNode>();
            List<GeNaSpawner> randomSpawners = new List<GeNaSpawner>(buildingSpawners);
            List<GameObject> randomPrefabs = null;
            if (buildingPrefabs != null)
                randomPrefabs = new List<GameObject>(buildingPrefabs);
            else
                randomPrefabs = new List<GameObject>();

            // divide the area up based on spawners available and fitting.
            axis.Normalize();
            area.m_axis = axis;

            float areaAngle = Vector2.SignedAngle(axis, Vector3.up);
            Quaternion axisRot = Quaternion.AngleAxis(areaAngle, Vector3.up);

            // Vector3 forward = new Vector3(axis.x, 0.0f, axis.y).normalized;
            // Vector3 right = new Vector3(forward.z, 0.0f, -forward.x).normalized;
            // Vector3 movForward = forward * area.m_radius;
            // Vector3 movRight = right * area.m_radius;
            Vector3 spawnPoint = Vector3.zero;

            AreaGrid areaGrid = new AreaGrid(area.m_radius, cellSize);
            int totalPlaced = 0;

            int totalCount = randomSpawners.Count + randomPrefabs.Count;
            while (totalCount > 0)
            {
                if (Random.Range(0, totalCount) < randomSpawners.Count)
                {
                    GeNaSpawner spawner = Internal_GetNextRandomSpawner(randomSpawners);
                    if (spawner != null)
                    {
                        spawner.Load();

                        GeNaSpawnerData data = spawner.SpawnerData;
                        if (areaGrid.Internal_FindBlocks(data.SpawnRange, out Vector2 buildingLocalPos, out float buildingLocalRot))
                        {
                            if (m_buildingParent != null)
                                data.SpawnParent = m_buildingParent.transform;
                            data.SpawnToTarget = true;
                            data.PlacementCriteria.MinRotationY = data.PlacementCriteria.MaxRotationY = buildingLocalRot + areaAngle;
                            data.SpawnCriteria.ForceSpawn = true;
                            Vector3 pos = new Vector3(buildingLocalPos.x, 0.0f, buildingLocalPos.y);
                            pos = axisRot * pos;
                            spawnPoint = area.m_center + pos;
                            spawnPoint.y = HeightAt(spawnPoint, spawnPoint.y);
                            SpawnCall spawnCall = spawner.Spawn(spawnPoint);
                            var spawnedEntities = spawnCall.GetSpawnedEntities();
                            m_undoRecord.Record(spawnedEntities);
                            totalPlaced++;
                        }
                    }
                }
                else
                {
                    GameObject prefab = Internal_GetNextRandomPrefab(randomPrefabs);
                    if (prefab != null)
                    {
                        float prefabSize = GetSizeOfPrefab(prefab);
                        if (areaGrid.Internal_FindBlocks(prefabSize, out Vector2 buildingLocalPos, out float buildingLocalRot))
                        {
                            GameObject go = Instantiate(prefab);
                            Vector3 pos = new Vector3(buildingLocalPos.x, 0.0f, buildingLocalPos.y);
                            pos = axisRot * pos;
                            spawnPoint = area.m_center + pos;
                            spawnPoint.y = HeightAt(spawnPoint, spawnPoint.y);
                            go.transform.localPosition = spawnPoint;
                            go.transform.rotation = Quaternion.AngleAxis(buildingLocalRot + areaAngle, Vector3.up);
                            if (m_buildingParent != null)
                                go.transform.parent = m_buildingParent.transform;
                            totalPlaced++;
                        }
                    }
                }

                totalCount = randomSpawners.Count + randomPrefabs.Count;
                if (totalPlaced > 0 && totalCount == 0 && areaGrid.EmptyBlocks > 3)
                {
                    totalPlaced = 0;
                    if (buildingSpawners != null)
                        randomSpawners = new List<GeNaSpawner>(buildingSpawners);
                    else
                        randomSpawners = new List<GeNaSpawner>();
                    if (buildingPrefabs != null)
                        randomPrefabs = new List<GameObject>(buildingPrefabs);
                    else
                        randomPrefabs = new List<GameObject>();
                    totalCount = randomSpawners.Count + randomPrefabs.Count;
                }
            }

            List<List<GridNode>> internalNodes = areaGrid.Internal_CreateInternalRoads();
            // rotate the external nodes into the area axis and position into world space.

            List<GeNaNode> externalConnectingNodes = new List<GeNaNode>();
            Dictionary<uint, uint> GridNodeToGenaNodeID = new Dictionary<uint, uint>();

            foreach (List<GridNode> node in internalNodes)
            {
                GeNaNode prevNode = null;
                for (int j = 0; j < node.Count; j++)
                {
                    GeNaNode genaNode = null;
                    Vector3 pos = area.m_center + (axisRot * node[j].position);
                    if (node[j].isIntersection)
                    {
                        if (GridNodeToGenaNodeID.ContainsKey(node[j].id))
                            genaNode = roadSpline.GetNode(GridNodeToGenaNodeID[node[j].id]);
                        else
                        {
                            genaNode = roadSpline.CreateNewNode(pos);
                            GridNodeToGenaNodeID.Add(node[j].id, genaNode.ID);
                        }
                    }
                    if (genaNode == null)
                        genaNode = roadSpline.CreateNewNode(pos);
                    if (j == 0)
                        externalConnectingNodes.Add(genaNode);
                    else
                    {
                        roadSpline.AddCurve(prevNode, genaNode);
                        if (j == node.Count - 1)
                            externalConnectingNodes.Add(genaNode);
                    }
                    prevNode = genaNode;
                }
            }
            //Debug.Log($"Number of cache hits for Prefab Size = {NumCacheHits}.");
            return externalConnectingNodes;
        }
        internal GeNaSpawner Internal_GetNextRandomSpawner(List<GeNaSpawner> list)
        {
            GeNaSpawner spawner = null;
            while (spawner == null)
            {
                if (list == null || list.Count == 0)
                    return null;
                int index = Random.Range(0, list.Count);
                spawner = list[index];
                GeNaSpawner last = list[list.Count - 1];
                list[index] = last;
                list.RemoveAt(list.Count - 1);
            }
            return spawner;
        }
        internal GameObject Internal_GetNextRandomPrefab(List<GameObject> list)
        {
            if (list == null || list.Count == 0)
                return null;

            int index = Random.Range(0, list.Count);
            GameObject go = list[index];
            GameObject last = list[list.Count - 1];
            list[index] = last;
            list.RemoveAt(list.Count - 1);
            return go;
        }
        #endregion
        #region External
        protected override List<GeNaNode> BuildArea(Vector2 axis, AreaFinder.AreaPoly area, float lotSize, List<GeNaSpawner> buildingSpawners, List<GameObject> buildingPrefabs, GeNaSpline roadSpline) => Internal_BuildArea(axis, area, lotSize, buildingSpawners, buildingPrefabs, roadSpline);
        #endregion
        #endregion
    }
}

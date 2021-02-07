using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace FoW
{
    public class FogOfWar2DTest : MonoBehaviour
    {
        public FogOfWar2DSimpleAnimator playerAnimator;
        public Transform playerTransform { get { return playerAnimator.transform; } }
        public Transform cameraTransform;
        public float movementSpeed = 4;
        public Text statsText;
        public TerrainSet[] terrainSets;
        Vector2 _targetPosition;
        public int chunkSize { get { return Mathf.RoundToInt(FogOfWarTeam.instances[0].mapSize / 2); } }
        List<Chunk> _chunks = new List<Chunk>();
        FogOfWarChunkManager _chunkManager;

        [System.Serializable]
        public class TerrainSet
        {
            public Sprite ground;
            public Sprite wall;
        }

        class Chunk
        {
            public Transform parent;
            public Vector3Int chunkPosition { get; private set; }
            FogOfWar2DTest _test;
            int _chunkSize { get { return _test.chunkSize; } }
            bool[] _walls = null;
            int _zoneSize { get { return _chunkSize / 2; } }
            int[] _zones = null;

            bool TryMerge(Vector2Int zonepos, Vector2Int offset)
            {
                Vector2Int targetzonepos = zonepos + offset;

                // don't do anything if it is on the edge of the chunk
                if (targetzonepos.x < 0 || targetzonepos.x >= _zoneSize || targetzonepos.y < 0 || targetzonepos.y >= _zoneSize)
                    return false;

                // check that they are not the same zone
                int zone = _zones[zonepos.y * _zoneSize + zonepos.x];
                int targetzone = _zones[targetzonepos.y * _zoneSize + targetzonepos.x];
                if (zone == targetzone)
                    return false;

                // remove wall
                Vector2Int wallpos = zonepos * 2 + offset;
                _walls[wallpos.y * _chunkSize + wallpos.x] = false;

                // merge zones
                for (int i = 0; i < _zones.Length; ++i)
                {
                    if (_zones[i] == targetzone)
                        _zones[i] = zone;
                }

                return true;
            }

            public Chunk(FogOfWar2DTest test, Vector3Int chunkpos, TerrainSet[] terrainsets)
            {
                _test = test;
                chunkPosition = chunkpos;
                Random.InitState(chunkPosition.y * 10000 + chunkPosition.x);

                // find where walls need to be
                _walls = new bool[_chunkSize * _chunkSize];
                for (int i = 0; i < _walls.Length; ++i)
                    _walls[i] = true;

                _zones = new int[_zoneSize * _zoneSize];
                int availablezones = _zones.Length;
                for (int i = 0; i < _zones.Length; ++i)
                {
                    _zones[i] = i;

                    int x = i % _zoneSize * 2;
                    int y = i / _zoneSize * 2;
                    _walls[y * _chunkSize + x] = false;
                }

                int attempts = 0;
                while (availablezones > 1 && attempts < 100)
                {
                    ++attempts;

                    int zoneindex = Random.Range(0, _zones.Length - 1);
                    int zone = _zones[zoneindex];
                    Vector2Int zonepos = new Vector2Int(zoneindex % _zoneSize, zoneindex / _zoneSize);

                    if (TryMerge(zonepos, new Vector2Int(-1, 0)) ||
                        TryMerge(zonepos, new Vector2Int(0, -1)) ||
                        TryMerge(zonepos, new Vector2Int(1, 0)) ||
                        TryMerge(zonepos, new Vector2Int(0, 1)))
                    {
                        --availablezones;
                    }
                }

                // remove walls on chunk edges
                _walls[Random.Range(0, _zoneSize - 1) * 2 * _chunkSize + _chunkSize - 1] = false;
                _walls[_chunkSize * (_chunkSize - 1) + Random.Range(0, _zoneSize - 2) * 2] = false;

                // generate game objects
                TerrainSet terraintset = terrainsets[Mathf.Abs(chunkPosition.x + chunkPosition.y + chunkPosition.z) % terrainsets.Length];
                parent = new GameObject(string.Format("Chunk_{0}_{1}_{2}", chunkPosition.x, chunkPosition.y, chunkPosition.z)).transform;
                parent.transform.parent = test.transform;
                float chunkoffset = _chunkSize * 0.5f + 1;
                parent.position = _test._chunkManager.ChunkToWorldPosition(chunkPosition);
                for (int y = 0; y < _chunkSize; ++y)
                {
                    for (int x = 0; x < _chunkSize; ++x)
                    {
                        bool iswall = _walls[y * _chunkSize + x];
                        GameObject obj = new GameObject(iswall ? "Wall" : "Floor");
                        obj.transform.SetParent(parent.transform);
                        obj.transform.localPosition = new Vector3(x - _chunkSize / 2, y - _chunkSize / 2, 0);
                        obj.AddComponent<SpriteRenderer>().sprite = iswall ? terraintset.wall : terraintset.ground;
                        if (iswall)
                            obj.AddComponent<BoxCollider2D>().size = Vector2.one;
                    }
                }
            }
        }

        void Start()
        {
            _targetPosition = playerTransform.position;
            _chunkManager = GetComponent<FogOfWarChunkManager>();
        }

        Vector2Int PositionToChunk(Vector2 pos)
        {
            return (Vector2Int)_chunkManager.WorldPositionToChunk(pos);
        }

        void UpdateChunks()
        {
            Camera cam = cameraTransform.GetComponent<Camera>();
            Vector2 orthosize = new Vector2(cam.aspect * cam.orthographicSize, cam.orthographicSize);
            Vector2 camerapos = cameraTransform.position;
            
            Vector2Int minchunk = PositionToChunk(camerapos - orthosize);
            Vector2Int maxchunk = PositionToChunk(camerapos + orthosize);

            // remove old chunks
            for (int i = 0; i < _chunks.Count; ++i)
            {
                Chunk chunk = _chunks[i];
                if (chunk.chunkPosition.x < minchunk.x || chunk.chunkPosition.x > maxchunk.x || chunk.chunkPosition.y < minchunk.y || chunk.chunkPosition.y > maxchunk.y)
                {
                    Destroy(chunk.parent.gameObject);
                    _chunks.RemoveAt(i);
                    --i;
                }
            }

            // create new chunks
            for (int y = minchunk.y; y <= maxchunk.y; ++y)
            {
                for (int x = minchunk.x; x <= maxchunk.x; ++x)
                {
                    if (!_chunks.Exists(c => c.chunkPosition.x == x && c.chunkPosition.y == y))
                        _chunks.Add(new Chunk(this, new Vector3Int(x, y, 0), terrainSets));
                }
            }
        }

        void UpdatePlayerMovement()
        {
            Vector2 playerpos = Vector2.MoveTowards(playerTransform.position, _targetPosition, Time.deltaTime * movementSpeed); ;
            playerTransform.position = playerpos;
            if (cameraTransform != null)
                cameraTransform.position = new Vector3(playerpos.x, playerpos.y, -10);

            if (Input.GetKeyDown(KeyCode.Space))
                FogOfWarTeam.GetTeam(0).SetAll(255);

            if (Vector2.Distance(playerpos, _targetPosition) < 0.01f)
            {
                Vector2 dir = Vector2.zero;
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                    ++dir.y;
                else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                    ++dir.x;
                else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                    --dir.y;
                else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                    --dir.x;

                if (dir.sqrMagnitude > 0.1f)
                {
                    RaycastHit2D hit = Physics2D.Raycast(_targetPosition, dir, 1);
                    if (hit.collider == null)
                        _targetPosition += dir;
                }
            }

            playerAnimator.movement = (_targetPosition - playerpos).normalized;
        }

        void Update()
        {
            UpdatePlayerMovement();
            UpdateChunks();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Active Chunk: " + PositionToChunk(playerTransform.position));
            sb.AppendLine("Chunks Loaded: 4");
            sb.Append("Chunks Stored: " + _chunkManager.loadedChunkCount);
            statsText.text = sb.ToString();
        }
    }
}

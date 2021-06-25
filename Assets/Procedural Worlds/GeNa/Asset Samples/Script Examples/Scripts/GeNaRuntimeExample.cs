using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// To use this Script, hit 'Play' and change the Example Type
    /// </summary>
    public class GeNaRuntimeExample : MonoBehaviour
    {
        #region Definitions
        public enum ExampleType
        {
            SimpleSpawn,
            RandomSpawn,
            AdvancedSpawn,
            SplineSpawn
        }
        #endregion
        #region Variables
        public ExampleType exampleType; // Example Type
        public Constants.SpawnRangeShape
            spawnShape = Constants.SpawnRangeShape.Circle; // Spawn Range Shape - Circle, Square
        public GeNaSpawner spawner; // Referenced GeNa Spawner
        public Transform[] splineNodes;
        private ExampleType previousType;
        private Constants.SpawnRangeShape previousShape;
        #endregion
        #region Unity Events
        // Start is called before the first frame update
        private void Start()
        {
            RunExample();
            previousType = exampleType;
            previousShape = spawnShape;
        }
        private void OnDrawGizmosSelected()
        {
            if (spawner == null)
                return;
            // Get Spawner Data
            GeNaSpawnerData data = spawner.SpawnerData;
            float spawnRange = data.SpawnRange;
            float halfRange = spawnRange * .5f;
            switch (data.SpawnRangeShape)
            {
                case Constants.SpawnRangeShape.Circle:
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(transform.position, halfRange);
                    break;
                case Constants.SpawnRangeShape.Square:
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(transform.position, Vector3.one * spawnRange);
                    break;
            }
        }
        #endregion
        #region Examples
        /// <summary>
        /// Runs an Example function with Example Type
        /// </summary>
        public void RunExample()
        {
            switch (exampleType)
            {
                case ExampleType.SimpleSpawn:
                    SimpleSpawn();
                    return;
                case ExampleType.RandomSpawn:
                    RandomSpawn();
                    return;
                case ExampleType.AdvancedSpawn:
                    AdvancedSpawn();
                    return;
                case ExampleType.SplineSpawn:
                    SplineSpawn();
                    return;
            }
        }
        private void SimpleSpawn()
        {
            // Deserialize the Spawner and Load all references
            spawner.Load();
            // Grab Spawner Data information
            GeNaSpawnerData data = spawner.SpawnerData;
            // Modify Spawner
            data.SpawnRange = 100f;
            data.ThrowDistance = 0f;
            data.MinInstances = 1;
            data.MaxInstances = 1;
            // Serialize the Spawner settings
            spawner.Save();
            // Perform the Spawn!
            spawner.Spawn(transform.position);
        }
        private void RandomSpawn()
        {
            // Deserialize the Spawner and Load all references
            spawner.Load();
            // Grab Spawner Data information
            GeNaSpawnerData data = spawner.SpawnerData;
            // Modify Spawner
            data.SpawnRange = 100f;
            data.ThrowDistance = 100f;
            data.MinInstances = 10;
            data.MaxInstances = 20;
            // Serialize the Spawner settings
            spawner.Save();
            // Perform the Spawn!
            spawner.Spawn(transform.position);
        }
        private void AdvancedSpawn()
        {
            // Deserialize the Spawner and Load all references
            spawner.Load();
            // Grab Spawner Data information
            GeNaSpawnerData data = spawner.SpawnerData;
            data.SpawnRange = 100f;
            data.ThrowDistance = 100f;
            data.MinInstances = 100;
            data.MaxInstances = 200;
            // Get Placement Criteria from Spawner Data
            PlacementCriteria placementCriteria = data.PlacementCriteria;
            // Modify Placement Criteria
            placementCriteria.SpawnAlgorithm = Constants.LocationAlgorithm.Every;
            placementCriteria.MinScale = Vector3.one;
            placementCriteria.MaxScale = Vector3.one * 2;
            // Get Spawn Criteria from Spawner Data
            SpawnCriteria spawnCriteria = data.SpawnCriteria;
            // Modify Spawn Criteria
            spawnCriteria.CheckCollisionType = Constants.VirginCheckType.Bounds;
            spawnCriteria.CheckHeightType = Constants.CriteriaRangeType.Range;
            spawnCriteria.HeightRange = 10f;
            spawnCriteria.MinHeight = -1.0f;
            spawnCriteria.MaxHeight = 100f;
            // Serialize the Spawner settings
            spawner.Save();
            // Perform the Spawn!
            spawner.Spawn(transform.position);
        }
        private void SplineSpawn()
        {
            // If there are no spline nodes
            if (splineNodes == null)
                return; // Exit method
            // Create a new Spline
            GeNaSpline spline = GeNaSpline.CreateSpline("GeNa Spline");
            // Add Curves to Spline
            GeNaNode prevNode = spline.CreateNewNode(splineNodes[0].position);
            for (int i = 1; i < splineNodes.Length; i++)
            {
                GeNaNode nextNode = spline.CreateNewNode(splineNodes[i].position);
                spline.AddCurve(prevNode, nextNode);
            }
            // Perform Simplification, Smoothing, etc.
            spline.Smooth();
            // Create and add a Spawner Extension
            GeNaSpawnerExtension extension = spline.AddExtension<GeNaSpawnerExtension>();
            // Attach Spawner to Spawner Extension
            extension.Spawner = spawner;
            // Adjust Spawner settings
            spawner.Load();
            GeNaSpawnerData data = spawner.SpawnerData;
            data.SpawnRange = 10f;
            data.ThrowDistance = 5f;
            data.FlowRate = 10.0f;
            spawner.Save();
            // Perform the Spawn!
            extension.Spawn();
        }
        private void Update()
        {
            // Example Type enum changed?
            if (exampleType != previousType)
            {
                // Perform example!
                RunExample();
                // Record previous example type
                previousType = exampleType;
            }

            // Spawn Shape enum changed?
            if (spawnShape != previousShape)
            {
                // Get Spawner Data
                GeNaSpawnerData data = spawner.SpawnerData;
                // Change the Spawn Shape
                data.SpawnRangeShape = spawnShape;
                // Serialize the Spawner settings
                spawner.Save();
                // Record previous spawn shape
                previousShape = spawnShape;
            }
        }
        #endregion
    }
}
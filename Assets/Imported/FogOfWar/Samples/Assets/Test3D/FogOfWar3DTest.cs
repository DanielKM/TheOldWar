using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace FoW
{
    class FOWUnit
    {
        public Vector3? destination = null;
        public FogOfWarUnit unit;
        public int team { get { return unit.team; } }
        public Transform transform { get; private set; }
        public Vector3 position { get { return transform.localPosition; } set { transform.localPosition = value; } }

        public FOWUnit(FogOfWarUnit u)
        {
            unit = u;
            transform = unit.transform;
            destination = unit.transform.localPosition;
        }
    }

    public abstract class FogOfWarTestPlatform : MonoBehaviour
    {
        public abstract void SetCameraTeam(Camera cam, int team);
    }

    public class FogOfWar3DTest : MonoBehaviour
    {
        public int team = 0;
        public Camera mainCamera;
        public float unitMoveSpeed = 3.0f;
        public float cameraSpeed = 20.0f;
        public Transform highlight;
        public float unfogSize = 2;
        public FogOfWarMinimap softwareMinimap;
        public GameObject hardwareMinimap;
        public Camera hardwareMinimapCamera;
        public Graphic teamColorGraphic;
        public Color[] teamColors;
        public FogOfWarClearFog clearFog;
        FogOfWarTestPlatform _platform;

        FogOfWarTeam _team { get { return FogOfWarTeam.GetTeam(team); } }

        List<FOWUnit> _units;
        FOWUnit _selectedUnit = null;

        void Start()
        {
            _platform = GetComponent<FogOfWarTestPlatform>();
            if (_platform == null)
                Debug.LogError("No platform component has been set up.");

            // get all units
            FogOfWarUnit[] units = FindObjectsOfType<FogOfWarUnit>();
            _units = new List<FOWUnit>();
            foreach (FogOfWarUnit unit in units)
                _units.Add(new FOWUnit(unit));

            hardwareMinimap.SetActive(true);
            softwareMinimap.gameObject.SetActive(false);

            OnTeamChanged();
        }

        void OnTeamChanged()
        {
            _selectedUnit = null;
            softwareMinimap.team = team;
            teamColorGraphic.color = teamColors[team];

            _platform.SetCameraTeam(mainCamera, team);

            // make it so only the current team will update their texture (this is an optimisation)
            foreach (FogOfWarTeam fowteam in FogOfWarTeam.instances)
                fowteam.outputToTexture = fowteam.team == team;
        }

        void Update()
        {
            // change team
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                team = (team + 1) % FogOfWarTeam.instances.Count;
                OnTeamChanged();
            }

            // reset fog
            if (Input.GetKeyDown(KeyCode.R))
                _team.Reinitialize();

            // toggle clear fog
            if (Input.GetKeyDown(KeyCode.C))
                clearFog.enabled = !clearFog.enabled;

            // toggle minimap
            if (Input.GetKeyDown(KeyCode.M))
            {
                softwareMinimap.gameObject.SetActive(!softwareMinimap.gameObject.activeSelf);
                hardwareMinimap.SetActive(!hardwareMinimap.activeSelf);
            }

            // select unit
            if (Input.GetKeyDown(KeyCode.Mouse0) && Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                FogOfWarUnit unit = hit.collider.GetComponent<FogOfWarUnit>();
                _selectedUnit = null;
                if (unit != null && unit.team == team)
                {
                    int index = _units.FindIndex(((u) => u.unit == unit));
                    if (index != -1)
                        _selectedUnit = _units[index];
                }
            }

            // move selected unit
            if (_selectedUnit != null && Input.GetKeyDown(KeyCode.Mouse1) && Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit selecthit))
            {
                Vector3 p = selecthit.point;
                if (_selectedUnit.transform.parent != null)
                    p = _selectedUnit.transform.parent.InverseTransformPoint(p);
                p.y = _selectedUnit.position.y;
                _selectedUnit.destination = p;
            }

            // clear fogged area
            if (Input.GetKeyDown(KeyCode.Mouse2) && Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit))
                _team.SetFog(new Bounds(hit.point, Vector3.one * unfogSize), 0);

            // update unit movements
            float moveamount = unitMoveSpeed * Time.deltaTime;
            foreach (FOWUnit unit in _units)
            {
                // find new position to move to
                if (unit.destination == null)
                {
                    if (unit.team != team)
                    {
                        float maxmovement = 10;
                        unit.destination = unit.position + new Vector3(Random.Range(-maxmovement, maxmovement), 0, Random.Range(-maxmovement, maxmovement));
                        if (!Physics.Raycast(unit.destination.Value + Vector3.up, Vector3.down))
                            unit.destination = null;
                    }

                    if (unit.destination == null)
                        continue;
                }

                // perform movement
                Vector3 offset = unit.destination.Value - unit.position;
                unit.position = Vector3.MoveTowards(unit.position, unit.destination.Value, moveamount);
                if (offset.sqrMagnitude > 0.01f)
                    unit.transform.localRotation = Quaternion.Slerp(unit.transform.localRotation, Quaternion.LookRotation(offset, Vector3.up), moveamount);

                // if we have arrived at the destination
                if (offset.sqrMagnitude < 0.01f)
                    unit.destination = null;
            }

            // update highlight
            if (_selectedUnit != null)
            {
                Vector3 pos = _selectedUnit.position;
                pos.y = 0.1f;
                highlight.localPosition = pos;
                highlight.gameObject.SetActive(true);
            }
            else
                highlight.gameObject.SetActive(false);

            // update camera
            Transform camtransform = mainCamera.transform;
            camtransform.localPosition += new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * (Time.deltaTime * cameraSpeed);
            
            if (Input.touchCount == 1)
            {
                Vector2 delta = Input.GetTouch(0).deltaPosition;
                camtransform.localPosition += new Vector3(-delta.x, 0, -delta.y);
            }

            // update camera zooming
            float zoomchange = Input.GetAxis("Mouse ScrollWheel");
            camtransform.localPosition = new Vector3(camtransform.localPosition.x, Mathf.Clamp(camtransform.localPosition.y - zoomchange * 10, 25, 50), camtransform.localPosition.z);
        }
    }
}
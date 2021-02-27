using System;
using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class EventCycle : NetworkBehaviour
{
    [Header("References")]
    public Transform sunTransform;
    public Light sun;
    public Text timeText;
    public GameObject skystone;
    public GameObject firestorm;
    public GameObject blizzard;
    public GameObject tornado;
    private Camera mainCamera;

    [Header("Time Settings")]
    // Time
    public float time;
    public TimeSpan currentTime;
    public int days;
    public int speed;
    public int dayLength = 86400;

    [Header("Fog Settings")]
    public Color fogDay = Color.grey;
    public Color fogNight = Color.black;
    public float intensity;

    [Header("Instability Settings")]
    [SyncVar(hook = nameof(HandleInstabilityUpdated))]
    public int instability = 1;
    public int maxInstability = 100;

    [Header("Event Settings")]
    public float lastEventTime = 0;
    public float nextEventTime = 0;
    public int lastEventDay = 0;
    public int nextEventDay = 0;
    public int eventFrequency = 1;
    public float eventDelayDays = 0.1f;

    Bounds bounds;
    public event Action<int, int> ClientOnInstabilityUpdated;

    void Start()
    {
        mainCamera = Camera.main;
        SetNextEventTime();
        bounds = new Bounds();
        foreach (Renderer r in GameObject.FindObjectsOfType<Renderer>())
        {
            bounds.Encapsulate(r.bounds);
        }
        StartCoroutine(Instability());
    }
    // Update is called once per frame
    void Update()
    {
        ChangeTime();
        UpdateSun();
        CreateEvent();
    }

    public void ChangeTime() {
        int spellPower = speed;
        if(instability > 0) 
        {
            spellPower = instability * speed;
        }
        time += Time.deltaTime * spellPower;

        if(time > dayLength) {
            days += 1;
            time = 0;
        }
        currentTime = TimeSpan.FromSeconds (time);
    }

    public void UpdateSun()
    {
        string[] tempTime = currentTime.ToString().Split(":"[0]);
        timeText.text = tempTime[0] + ":" + tempTime[1];
        sunTransform.rotation = Quaternion.Euler ( new Vector3((time - dayLength/4)/dayLength * 360, 0, 0));

        if(time < dayLength/2) {
            intensity = 1 - (dayLength/2 - time)/(dayLength/2);
        } else {
            intensity = 1 - ((dayLength/2 - time)/(dayLength/2 * -1));
        }
        RenderSettings.fogColor = Color.Lerp(fogNight, fogDay, intensity * intensity);
        sun.intensity = intensity;
    }

    public void CreateEvent()
    {
        if(time >= nextEventTime && days == nextEventDay) {
            GameEvents chosenEvent = ChooseEvent();
            // GameEvents chosenEvent = GameEvents.MeteorShower;
            lastEventTime = time;
            lastEventDay = days;
            SetNextEventTime();
            InitiateEvent(chosenEvent);
        }
    }

    public void SetNextEventTime()
    {
        System.Random rnd = new System.Random();

        float selectedTime = (dayLength * eventDelayDays) + rnd.Next(0, dayLength);

        if(selectedTime > dayLength) 
        { 
            selectedTime = selectedTime/4; 
        }

        if(time >= nextEventTime) {
            nextEventDay = days + 1;
        }

        nextEventTime = selectedTime;
    }

    public GameEvents ChooseEvent()
    {
        int myEnumMemberCount = Enum.GetNames(typeof(GameEvents)).Length;

        int chosenEvent = UnityEngine.Random.Range(0, myEnumMemberCount);
        
        GameEvents[] words = (GameEvents[])Enum.GetValues(typeof(GameEvents));

        GameEvents events = words[chosenEvent];
        
        return events;
    }

    public void InitiateEvent(GameEvents chosenEvent)
    {
        Vector3 spawnPoint;
        switch(chosenEvent) 
        {
            case GameEvents.MeteorShower:
                int numberToSpawn = 5;
                for(int i=0; i<numberToSpawn; i++)
                {
                spawnPoint = new Vector3(
                        UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                        50f,
                        UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
                    );
                    GameObject meteor = Instantiate(skystone, spawnPoint, new Quaternion(0,0,0,0));
                    
                    NetworkServer.Spawn(meteor);
                }
            return;
            case GameEvents.Firestorm:
                spawnPoint = new Vector3(
                    UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                    50f,
                    UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
                );
                GameObject fireStormInstance = Instantiate(firestorm, spawnPoint, new Quaternion(0,0,0,0));
                
                NetworkServer.Spawn(fireStormInstance);
            return;
            case GameEvents.Blizzard:
                spawnPoint = new Vector3(0, 10, 0);
                GameObject blizzardInstance = Instantiate(blizzard, spawnPoint, new Quaternion(0,0,0,0));

                NetworkServer.Spawn(blizzardInstance);
            return;
            case GameEvents.Vortex:
                spawnPoint = new Vector3(
                    UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                    50f,
                    UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
                );
                GameObject vortexInstance = Instantiate(tornado, spawnPoint, new Quaternion(0,0,0,0));
                
                NetworkServer.Spawn(vortexInstance);
            return;
        }
    }

    #region Client

    private void HandleInstabilityUpdated(int oldInstability, int newInstability)
    {
        ClientOnInstabilityUpdated?.Invoke(newInstability, maxInstability);
        Debug.Log("Event cycle event hit");
        // if(gameObject.GetComponent<UnitInformation>().selected == false) { return; }

        // unitSelection.UpdateUnitPanel(unit);
    }

    #endregion

    IEnumerator Instability()
    {
        if(instability > 0) { 
            instability -= 5;
        }
        yield return new WaitForSeconds(1);
        StartCoroutine(Instability());
    }
}

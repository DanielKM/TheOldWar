using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    [Header("References")]
    // Sun
    public Transform sunTransform;
    public Light sun;
    public Text timeText;

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

    [Header("Event Settings")]
    public float lastEventTime = 0;
    public float nextEventTime = 0;
    public int lastEventDay = 0;
    public int nextEventDay = 0;
    public int eventFrequency = 1;
    public float eventDelayDays = 0.1f;
    
    void Start()
    {
        SetNextEventTime();
    }
    // Update is called once per frame
    void Update()
    {
        ChangeTime();
        UpdateSun();
        CreateEvent();
    }

    public void ChangeTime() {
        time += Time.deltaTime * speed;
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
            // GameEvents chosenEvent = ChooseEvent();
            GameEvents chosenEvent = GameEvents.MeteorShower;
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
        Debug.Log("INITIATING " + chosenEvent);
        switch(chosenEvent) 
        {
            case GameEvents.MeteorShower:
            
            return;
        }
    }
}

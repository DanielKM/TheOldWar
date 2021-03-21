using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    [SerializeField] public RTSPlayer[] teamPlayers = null;
    [SerializeField] public string teamName = null;
    [SerializeField] string teamColor = null;
    [SerializeField] public TeamLetter teamLetter = TeamLetter.A;
    
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}

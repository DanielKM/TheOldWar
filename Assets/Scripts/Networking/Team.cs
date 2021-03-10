using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    enum TeamLetter { A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P }
    [SerializeField] public RTSPlayer[] teamPlayers = null;
    [SerializeField] string teamName = null;
    [SerializeField] string teamColor = null;
    [SerializeField] TeamLetter teamLetter = TeamLetter.A;
}

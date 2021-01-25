using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellHandler : MonoBehaviour
{
    public SpellList spellToCast;
    public RTSPlayer player;
    public Unit spellCaster;
    public bool castingSpell = false;
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stats
{
    public int lvl;
    public float mHp;
    public float mPosture;
    public float hp;
    [HideInInspector]
    public float posture;
    public float healthRegen;
    [HideInInspector]
    public float shield;

    
    public float strength;
    public float sinergy;
    public float rupture;
    public float ruptureMult = 135;
    public float critChance;
    public float critDmgMult = 75;

    public float resist;

    public float spd;

    public float atSpd;
    public float cdr;
}

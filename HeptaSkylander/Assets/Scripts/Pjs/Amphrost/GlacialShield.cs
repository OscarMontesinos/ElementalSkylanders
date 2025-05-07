using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlacialShield : Shield
{
    public Amphrost amphrost;
    public override void Update()
    {
        if(shieldAmount > 0)
        {
            ChangeShieldAmount(amphrost.hab2ShieldLosePerSecond *(-Time.deltaTime));
        }
        base.Update();
    }
    public override void Die()
    {
        
    }
}

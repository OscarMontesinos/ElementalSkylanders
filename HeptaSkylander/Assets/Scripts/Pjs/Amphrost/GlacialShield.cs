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
            if (amphrost.upgrades.path2Upg1)
            {
                amphrost.Heal(amphrost,amphrost.hab2ShieldLosePerSecond * 0.5f * Time.deltaTime, amphrost.element);
            }
        }
        base.Update();
    }
    public override void Die()
    {
        
    }
}

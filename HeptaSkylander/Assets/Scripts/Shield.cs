using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : Buff
{
    public float shieldAmount;
    public float maxShieldAmount;

    public void ShieldSetUp(PjBase user,PjBase target, float shieldAmount,float maxShieldAmmount, float duration, GameObject particleFx)
    {
        this.user = user;
        this.target = target;
        this.maxShieldAmount = maxShieldAmmount;
        ChangeShieldAmount(shieldAmount);
        if (duration == 0)
        {
            this.untimed = true;
        }
        else
        {
            this.duration = duration;
        }
        this.particleFx = particleFx;

    }

    public virtual float ChangeShieldAmount(float value)
    {
        if (value >= -shieldAmount)
        {
            if (value < 0)
            {
                user.RegisterDamage(-value);
            }
            shieldAmount += value;
            target.stats.shield += value;
            value = 0;
        }
        else
        {
            user.RegisterDamage(-value);
            value += shieldAmount;
            shieldAmount = 0;
            target.stats.shield += value;
        }

        if(target.stats.shield < 0)
        {
            target.stats.shield = 0;
        }

        if(maxShieldAmount>0 && maxShieldAmount < shieldAmount)
        {
            target.stats.shield -= (target.stats.shield - maxShieldAmount);
            shieldAmount = maxShieldAmount;
        }

        return -value;
    }

    public override void Die()
    {
        if (particleFx)
        {
            Destroy(particleFx);
        }

        user.RegisterDamage(-shieldAmount);
        ChangeShieldAmount(-shieldAmount);


        Destroy(this);
    }
}

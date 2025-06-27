using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class Licon : PjBase
{
    public GameObject basicObject;
    public GameObject basicPoint;
    public float basicCd;
    public float basicDmg;
    public float basicMinRange;
    public float basicRange;
    public float basicArea;
    public float basicMaxSpd;
    public float basicMinSpd;

    public GameObject hab1Object;
    LiconWall hab1ActualWall;
    public float hab1Range;
    public float hab1Duration;
    public float hab1Hp;

    public ParticleSystem hab2DashFx;
    public int hab2Charges;
    public float hab2Spd;
    public float hab2Range;
    float hab2MinCd;

    public ParticleSystem up1Fx;
    public bool up1Active;
    public float up1Spd;
    public float up1Cd;
    float up1CurrentCd;

    public float up2TicksPerSecond;
    public float up2DmgPerTick;
    public float up2RupturePerTick;
    public float up2Slow;

    public int up3Charges;

    public float up4Dmg;
    public float up4Area;


    public override void Start()
    {
        base.Start();
        if (upgrades.upg1)
        {
            stats.spd += up1Spd;
            up1Active = true;
            up1Fx.Play();
        }
        if (upgrades.upg2)
        {
            hab2Charges = up3Charges;
            currentHab2Charges = hab2Charges;
        }
        if (upgrades.upg4)
        {
            basicDmg += up4Dmg;
            basicArea = up4Area;
        }
    }
    public override void Update()
    {
        base.Update();

        if(up1CurrentCd > 0)
        {
            up1CurrentCd -= Time.deltaTime;
            if(up1CurrentCd <= 0)
            {
                stats.spd += up1Spd;
                up1Active = true;
                up1Fx.Play();
            }
        }

        if (currentHab2Cd <= 0 && currentHab2Charges < hab2Charges)
        {
            currentHab2Charges++;
            if (currentHab2Charges < hab2Charges)
            {
                currentHab2Cd = hab2Cd;
            }
        }

        if (hab1ActualWall)
        {
            currentHab1Cd = hab1ActualWall.duration;
        }

        if (hab2MinCd > 0)
        {
            hab2MinCd -= Time.deltaTime;
        }
    }

    public override IEnumerator MainAttack()
    {
        if (currentBasicCd <= 0 && !IsCasting() && !dashing)
        {
            currentBasicCd = CalculateAtSpd(basicCd * stats.atSpd);
            lockPointer = false;
            softCasting = false;
            lookAtPointer = true;

            Vector2 dir = cursor.transform.position - pointer.transform.position;
            pointer.transform.up = dir;
            yield return null;
            lockPointer = true;

            StartCoroutine(PlayAnimation("Basic"));
        }

        yield return base.MainAttack();

    }

    public void BasicAttack()
    {
        float dmg;
        dmg = CalculateSinergy(basicDmg);

        float range = 0;
        Vector2 dist = cursor.transform.position - transform.position;

        if(dist.magnitude > basicRange)
        {
            range = basicRange;
        }
        else
        {
            range = dist.magnitude;
        }

        range -= (transform.position - basicPoint.transform.position).magnitude;

        if(range < basicMinRange)
        {
            range = basicMinRange;
        }

        Bomb bomb = Instantiate(basicObject,basicPoint.transform.position,pointer.transform.rotation).GetComponent<Bomb>();
        bomb.SetUp(this, element, AttackType.Magical, dmg, basicMaxSpd, basicMinSpd, range, basicArea);
    }

    public override IEnumerator Hab1()
    {
        if (hab1ActualWall && hab1ActualWall.duration < hab1Duration - 1)
        {
            hab1ActualWall.Die();
        }
        else if (currentHab1Cd <= 0 && !IsCasting() && !dashing && !hab1ActualWall)
        {
            lockPointer = false;
            softCasting = true;
            lookAtPointer = true;
            yield return null;
            lookAtPointer = false;

            float hp;
            hp = CalculateSinergy(hab1Hp);


            float dmgPerSecond;
            dmgPerSecond = CalculateSinergy(up2DmgPerTick);

            float ruptureDmg;
            ruptureDmg = CalculateRupture(up2RupturePerTick);

            float range = 0;
            Vector2 dist = cursor.transform.position - transform.position;

            if (dist.magnitude > hab1Range)
            {
                range = hab1Range;
            }
            else
            {
                range = dist.magnitude;
            }

            hab1ActualWall = Instantiate(hab1Object, transform.position + pointer.transform.up * range, pointer.transform.rotation).GetComponent<LiconWall>();
            hab1ActualWall.SetUp(this, hab1Duration, hp,up2TicksPerSecond,dmgPerSecond,ruptureDmg,up2Slow);

            StartCoroutine(PlayAnimation("Burst"));

            yield return base.Hab1();
        }
    }

    public override IEnumerator Hab2()
    {
        if (currentHab2Charges > 0 && !IsCasting() && !dashing && hab2MinCd <= 0)
        {
            hab2MinCd = 0.4f;
            if (currentHab2Charges == hab2Charges)
            {
                currentHab2Cd = CDR(hab2Cd);
            }
            currentHab2Charges--;
            lockPointer = false;
            casting = true;
            lookAtPointer = false;
            Vector2 dir = rb.velocity;
            pointer.transform.up = dir;
            yield return null;
            lookAtPointer = false;
            hab2DashFx.Play();
            StartCoroutine(Dash(dir.normalized, hab2Spd, hab2Range, false, true));

            while (dashing)
            {
                yield return null;
            }

            hab2DashFx.Stop();
            
        }
        yield return base.Hab2();
    }


    public override void OnDamageTaken(PjBase user, float value)
    {
        if (upgrades.upg1 && up1Active)
        {
            stats.spd -= up1Spd;
            up1Active = false;
            up1CurrentCd = CDR(up1Cd);
            up1Fx.Stop();
        }
        base.OnDamageTaken(user, value);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, basicMinRange);
        Gizmos.DrawWireSphere(transform.position, basicRange);

        if (upgrades.upg4)
        {
            Gizmos.DrawWireSphere(transform.position + (transform.up.normalized * basicRange), up4Area);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position + (transform.up.normalized * basicRange), basicArea);
        }

        Gizmos.DrawWireSphere(transform.position, hab2Range);
    }
}

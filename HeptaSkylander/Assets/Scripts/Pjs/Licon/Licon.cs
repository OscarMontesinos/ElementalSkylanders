using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using static UnityEngine.Rendering.HableCurve;

public class Licon : PjBase
{
    [Header("Base")]
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

    [Header("Upgrades")]
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

    [Header("Path 1")]
    public int p1Charges;
    [HideInInspector]
    public int p1CurrentCharges;
    public float p1DmgPerCharge;

    public float p1Up1DmgMod;

    public float p1Up2Dmg;
    public int p1Up2Ticks;
    public float p1Up2InitialDelay;
    public float p1Up2TickDelay;

    public float p1up3Dmg;
    public float p1up3Area;

    [Header("Path 2")]
    public float p2AtSpd;
    public float p2ExtraDmg;

    public int p2Up1Charges;

    public GameObject p2Up2Object;
    public List<GameObject> p2up2Indicators;
    public float p2Up2Seconds;
    float p2Up2CurrentSeconds;
    public float p2Up2Area;
    public float p2Up2Dmg;

    public float p2Up3SecondsRefunded;


    public override void Start()
    {
        base.Start();
        if (upgrades.upg1)
        {
            stats.spd += up1Spd;
            up1Active = true;
            up1Fx.Play();
        }
        if (upgrades.upg3)
        {
            hab2Charges = up3Charges;
            currentHab2Charges = hab2Charges;
        }
        if (upgrades.upg4)
        {
            basicDmg += up4Dmg;
            basicArea = up4Area;
        }
        if (upgrades.path2)
        {
            stats.atSpd = p2AtSpd;
        }
        if (upgrades.path2Upg1)
        {
            hab2Charges = p2Up1Charges;
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

        if (upgrades.path2Upg2 && p2Up2CurrentSeconds <= p2Up2Seconds)
        {
            p2Up2CurrentSeconds += Time.deltaTime;

            float segments = p2Up2Seconds / p2up2Indicators.Count;

            if( p2Up2CurrentSeconds / segments >= 1)
            {
                p2up2Indicators[0].SetActive(true);
            }
            else
            {
                p2up2Indicators[0].SetActive(false);
                p2up2Indicators[1].SetActive(false);
                p2up2Indicators[2].SetActive(false);
                p2up2Indicators[3].SetActive(false);
                p2up2Indicators[4].SetActive(false);
            }

            if(p2Up2CurrentSeconds / segments >= 2)
            {
                p2up2Indicators[1].SetActive(true);
            }
            if(p2Up2CurrentSeconds / segments >= 3)
            {
                p2up2Indicators[2].SetActive(true);
            }
            if(p2Up2CurrentSeconds / segments >= 4)
            {
                p2up2Indicators[3].SetActive(true);
            }
            if(p2Up2CurrentSeconds / segments >= 5)
            {
                p2up2Indicators[4].SetActive(true);

                currentBasicCd = 0;
            }
        }

        if (upgrades.path1)
        {
            if (p1CurrentCharges > 0)
            {
                p2up2Indicators[0].SetActive(true);
            }
            else
            {
                p2up2Indicators[0].SetActive(false);
                p2up2Indicators[1].SetActive(false);
                p2up2Indicators[2].SetActive(false);
                p2up2Indicators[3].SetActive(false);
                p2up2Indicators[4].SetActive(false);
            }

            if (p1CurrentCharges > 1)
            {
                p2up2Indicators[1].SetActive(true);
            }
            if (p1CurrentCharges > 2)
            {
                p2up2Indicators[2].SetActive(true);
            }
            if (p1CurrentCharges > 3)
            {
                p2up2Indicators[3].SetActive(true);
            }
            if (p1CurrentCharges > 4)
            {
                p2up2Indicators[4].SetActive(true);

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

        if(up1Active && upgrades.path2)
        {
            dmg += CalculateSinergy(p2ExtraDmg);
        }

        if (upgrades.path2Upg2 && p2Up2CurrentSeconds >= p2Up2Seconds)
        {
            dmg += CalculateSinergy(p2Up2Dmg);
        }

        if (upgrades.path1)
        {
            dmg += CalculateSinergy(p1DmgPerCharge * p1CurrentCharges);
        }

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

        if (upgrades.path2Upg2 && p2Up2CurrentSeconds >= p2Up2Seconds)
        {
            p2Up2CurrentSeconds = 0;
            HiperBomb bomb = Instantiate(p2Up2Object, basicPoint.transform.position, pointer.transform.rotation).GetComponent<HiperBomb>();
            bomb.SetUp(this, element, AttackType.Magical, dmg, basicMaxSpd, range, p2Up2Area);
        }
        else
        {
            Bomb bomb = Instantiate(basicObject, basicPoint.transform.position, pointer.transform.rotation).GetComponent<Bomb>();
            bomb.SetUp(this, element, AttackType.Magical, dmg, basicMaxSpd, basicMinSpd, range, basicArea);
        }
            
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
            if (upgrades.path1)
            {
                p1CurrentCharges = 0;
            }
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
            p2Up2CurrentSeconds = 0;
        }
        base.OnDamageTaken(user, value);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, basicMinRange);
        Gizmos.DrawWireSphere(transform.position, basicRange);
        if (upgrades.path1Upg3)
        {
            Gizmos.DrawWireSphere(transform.position + (transform.up.normalized * basicRange), p1up3Area);
        }
        else if (upgrades.upg4)
        {
            Gizmos.DrawWireSphere(transform.position + (transform.up.normalized * basicRange), up4Area);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position + (transform.up.normalized * basicRange), basicArea);
        }

        if (upgrades.path2Upg2)
        {
            Gizmos.DrawWireSphere(transform.position + (transform.up.normalized * basicRange), p2Up2Area);
        }

        Gizmos.DrawWireSphere(transform.position, hab2Range);
    }
}

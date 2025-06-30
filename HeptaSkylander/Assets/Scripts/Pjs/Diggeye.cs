using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Diggeye : PjBase
{
    public float basicDashRange;
    public float basicCd;
    public float basicDmg;
    public float basicRange;
    public float basicArea;
    public GameObject basicFxImpact;

    bool hab1Dashing = false;
    public ParticleSystem undergroundFx;
    public int hab1Charges;
    public float hab1Dmg;
    public float hab1Spd;
    public float hab1Range;
    public float hab1Area;
    public float hab1Offset;
    public float hab1RangeExt;
    float hab1MinCd;

    public GameObject hab2Mark;
    public List<GameObject> hab2Marks;
    public List<PjBase> hab2Targets;
    public float hab2Area;
    public float hab2HPThreshold;
    public float hab2BaseDmg;
    public float hab2Spd;

    public float up1DashRange;
    public float up1DashSpd;

    public int up2Charges;

    public float up3DmgBoost;

    public GameObject up4Fx;
    public float up4Area;
    public float up4Dmg;
    public float up4Rupture;

    public GameObject p1Up0Mark;
    PjBase p1Up0Target = null;

    public float p1Up3Threshold;
    public float p1Up3BaseDmg;

    public GameObject p2Up0ParticleToDisactivate;
    public GameObject p2Up0ParticleToActivate;
    public float p2Up0Range;
    public float p2Up0Area;
    public float p2Up0DashRange;
    public float p2Up0AnimSpd;
    public int p2Up0Charges;

    List<PjBase> p2Up1enemiesSucked= new List<PjBase>();
    public float p2Up1LifePercentageHeal;

    public float p2up2DmgBoost;
    public float p2up2HpBoost;
    public float p2up2ResisBoost;

    bool p2up3Active;
    public override void Awake()
    {
        if (upgrades.path2Upg2)
        {
            stats.mHp += p2up2HpBoost;
            stats.resist += p2up2ResisBoost;
        }
        base.Awake();
    }

    public override void Start()
    {
        if (upgrades.upg3)
        {
            hab1Charges = up2Charges;
        }

        currentHab1Charges = hab1Charges;

        if (upgrades.upg3)
        {
            basicDmg += ((basicDmg * up3DmgBoost) / 100);
            hab1Dmg += ((hab1Dmg * up3DmgBoost) / 100);
        }
        if (upgrades.path2Upg2)
        {
            basicDmg += ((basicDmg * p2up2DmgBoost) / 100);
            hab1Dmg += ((hab1Dmg * p2up2DmgBoost) / 100);
        }
        if (upgrades.path1Upg3)
        {
            hab2HPThreshold = p1Up3Threshold;
            hab2BaseDmg = p1Up3BaseDmg;
        }
        if(upgrades.path2) 
        { 
            p2Up0ParticleToActivate.SetActive(true);
            p2Up0ParticleToDisactivate.SetActive(false);
            basicArea = p2Up0Area;
            basicRange = p2Up0Range;
            animator.speed = p2Up0AnimSpd;
            hab1Charges = p2Up0Charges;
        }
        base.Start();
    }
    public override void Update()
    {
        base.Update();

        p1Up0Mark.SetActive(p1Up0Target);
        if (p1Up0Target)
        {
            p1Up0Mark.transform.position = p1Up0Target.transform.position;
        }

        if(currentBasicCd == 0)
        {
            p2up3Active = false;
        }
        if(currentHab1Cd <= 0 && currentHab1Charges < hab1Charges)
        {
            currentHab1Charges++;
            if (currentHab1Charges < hab1Charges)
            {
                currentHab1Cd = hab1Cd;
            }
        }
        if(hab1MinCd > 0)
        {
            hab1MinCd -= Time.deltaTime;
        }

        if (upgrades.upg1 && upgrades.upg2)
        {
            Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, hab2Area, GameManager.Instance.unitLayer);
            PjBase enemy;
            foreach (Collider2D enemyColl in enemiesHit)
            {
                enemy = enemyColl.GetComponent<PjBase>();
                if (enemy.team != team)
                {
                    if (hab2Targets.Contains(enemy) && enemy.stats.hp > ((enemy.stats.mHp * hab2HPThreshold) / 100) + hab2BaseDmg)
                    {
                        Destroy(hab2Marks[hab2Targets.IndexOf(enemy)]);
                        hab2Marks.Remove(hab2Marks[hab2Targets.IndexOf(enemy)]);
                        hab2Targets.Remove(enemy);
                        break;
                    }
                    else if (!hab2Targets.Contains(enemy) && enemy.stats.hp <= ((enemy.stats.mHp * hab2HPThreshold) / 100) + hab2BaseDmg)
                    {
                        hab2Marks.Add(Instantiate(hab2Mark));
                        hab2Targets.Add(enemy);
                        break;
                    }
                }
            }
        }

        foreach(GameObject mark in hab2Marks)
        {
            mark.transform.position = hab2Targets[hab2Marks.IndexOf(mark)].transform.position;

            Vector2 dist = mark.transform.position - transform.position;
            if (dist.magnitude > hab2Area)
            {
                hab2Targets.Remove(hab2Targets[hab2Marks.IndexOf(mark)]);
                hab2Marks.Remove(mark);
                Destroy(mark);
                break;
            }
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

            if (basicComboCount == 0)
            {
                StartCoroutine(PlayAnimation("DiggeyeBasic1"));
                basicComboCount += 1;
            }
            else if (basicComboCount == 1)
            {
                StartCoroutine(PlayAnimation("DiggeyeBasic2"));
                basicComboCount += 1;
                if (!upgrades.upg1)
                {
                    basicComboCount = 0;
                    if (!upgrades.upg1)
                    {
                        basicComboCount = 0;
                    }
                }
            }
            else if (basicComboCount == 2)
            {
                StartCoroutine(PlayAnimation("DiggeyeBasic3"));
                basicComboCount += 1;
                if (!upgrades.path2Upg3)
                {
                    basicComboCount = 0;
                }
            }
            else if (basicComboCount == 3)
            {
                StartCoroutine(PlayAnimation("DiggeyeBasic4"));
                basicComboCount = 0;
            }
            comboReset = 1.25f;
        }

        yield return base.MainAttack();

    }

    public void BasicAttack()
    {
        float dmg;
        dmg = CalculateStrength(basicDmg);

        Vector2 dir = transform.position + (pointer.transform.up.normalized * basicRange);
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(dir, basicArea, GameManager.Instance.unitLayer);
        PjBase enemy;
        foreach (Collider2D enemyColl in enemiesHit)
        {
            enemy = enemyColl.GetComponent<PjBase>();
            if (enemy.team != team)
            {
                if (upgrades.path1 && enemy == p1Up0Target)
                {
                    enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, 100, out bool isCrit), element, isCrit);
                }
                else
                {
                    enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                }
                Instantiate(basicFxImpact, enemy.transform.position, transform.rotation);
            }
        }
    }

    public void UnburyAttack(Vector2 position)
    {
        currentBasicCd = CalculateAtSpd(basicCd * stats.atSpd);
        Instantiate(up4Fx, position, transform.rotation);

        float dmg;
        dmg = CalculateStrength(up4Dmg);
        float rupture;
        rupture = CalculateRupture(up4Rupture);

        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(position, up4Area, GameManager.Instance.unitLayer);
        PjBase enemy;
        foreach (Collider2D enemyColl in enemiesHit)
        {
            enemy = enemyColl.GetComponent<PjBase>();
            if (enemy.team != team)
            {
                if (upgrades.path1 && enemy == p1Up0Target)
                {
                    enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, 100, out bool isCrit), element, isCrit);
                }
                else
                {
                    enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                }
                enemy.GetComponent<TakeDamage>().TakeRupture(this, rupture);
            }
        }
    }

    public override IEnumerator Hab1()
    {
        if (currentHab1Charges > 0 && !IsCasting() && hab1MinCd <= 0)
        {
            hab1MinCd = 0.4f;
            hab1Dashing = true;
            if (currentHab1Charges == hab1Charges)
            {
                currentHab1Cd = CDR(hab1Cd);
            }
            currentHab1Charges--;
            lockPointer = false;
            casting = true;
            lookAtPointer = true;
            Vector2 dir = cursor.transform.position - pointer.transform.position;
            pointer.transform.up = dir;
            yield return null;
            lookAtPointer = false;
            float dmg;
            dmg = CalculateStrength(hab1Dmg);
            StartCoroutine(Dash(pointer.transform.up, hab1Spd, hab1Range, true, true));
            undergroundFx.Play();
            while (!dashing)
            {
                yield return null;
            }
            List<PjBase> targetsAffected = new List<PjBase>();
            bool hit = false;
            while (dashing)
            {
                yield return null;
                dir = transform.position + (pointer.transform.up.normalized * hab1Offset);
                Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(dir, hab1Area, GameManager.Instance.unitLayer);
                PjBase enemy;
                foreach (Collider2D enemyColl in enemiesHit)
                {
                    enemy = enemyColl.GetComponent<PjBase>();
                    if (!hit&&enemy.team != team && !targetsAffected.Contains(enemy))
                    {
                        if (upgrades.path1 && enemy == p1Up0Target)
                        {
                            enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, 100, out bool isCrit), element, isCrit);
                        }
                        else
                        {
                            enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                        }
                        if (upgrades.upg4)
                        {
                            UnburyAttack(enemy.transform.position);
                        }
                        Instantiate(basicFxImpact, enemy.transform.position, transform.rotation);
                        targetsAffected.Add(enemy);
                        
                        hit = true;
                        undergroundFx.Stop();
                        
                        if (hit)
                        {
                            StartCoroutine(Dash(pointer.transform.up, hab1Spd, hab1RangeExt,true, true));
                        }
                        if (upgrades.path1)
                        {
                            p1Up0Target = enemy;
                        }
                    }
                }
            }

            currentBasicCd = 0;
            undergroundFx.Stop();

            yield return null;
            hab1Dashing = false;



            yield return base.Hab1();
        }
    }

    public override IEnumerator Hab2()
    {
        if (!IsCasting() && !dashing &&  upgrades.upg1 && upgrades.upg2)
        {
            PjBase target = null;
            Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, hab2Area, GameManager.Instance.unitLayer);
            PjBase enemy;

            foreach (Collider2D enemyColl in enemiesHit)
            {
                enemy = enemyColl.GetComponent<PjBase>();
                if (enemy.team != team)
                {
                    if (enemy.stats.hp <= ((enemy.stats.mHp * hab2HPThreshold) / 100) + hab2BaseDmg)
                    {
                        if (target == null)
                        {
                            target = enemy;
                        }
                        else
                        {
                            Vector2 actualDist = transform.position - target.transform.position;
                            Vector2 dist = transform.position - enemy.transform.position;
                            if (dist.magnitude < actualDist.magnitude)
                            {
                                target = enemy;
                            }
                        }
                    }
                }
            }

            if (target)
            {
                softCasting = true;
                lookAtPointer = true;
                lockPointer = true;

                Vector2 dir = target.transform.position - pointer.transform.position;
                pointer.transform.up = dir;

                Destroy(hab2Marks[hab2Targets.IndexOf(target)]);
                hab2Marks.Remove(hab2Marks[hab2Targets.IndexOf(target)]);
                hab2Targets.Remove(target);

                StartCoroutine(Dash(target.transform, hab2Spd, true, true, true));

                while (!dashing)
                {
                    yield return null;
                }
                List<PjBase> targetsAffected = new List<PjBase>();
                bool hit = false;
                while (dashing)
                {
                    yield return null;
                    enemiesHit = Physics2D.OverlapCircleAll(transform.position, hab1Area, GameManager.Instance.unitLayer);
                    foreach (Collider2D enemyColl in enemiesHit)
                    {
                        enemy = enemyColl.GetComponent<PjBase>();
                        if (!hit && enemy.team != team && !targetsAffected.Contains(enemy) && enemy == target)
                        {
                            Instantiate(basicFxImpact, enemy.transform.position, transform.rotation);
                            enemy.GetComponent<TakeDamage>().Die(this);
                            StopCoroutine(Dash(target.transform, hab2Spd, true, true, true));

                            targetsAffected.Add(enemy);
                        }
                    }
                }
            }

        }
        yield return base.Hab2();
    }

   
    public override void DamageDealed(PjBase target, float amount)
    {
        if (upgrades.path2Upg1 && !p2Up1enemiesSucked.Contains(target))
        {
            p2Up1enemiesSucked.Add(target);
            Heal(this, ((p2Up1LifePercentageHeal * stats.mHp) / 100), element);
        }
        base.DamageDealed(target, amount);
    }

    public override void OnKill(PjBase target)
    {
        if (upgrades.path1)
        {
            currentHab1Charges++;
            if(currentHab1Charges > hab1Charges)
            {
                currentHab1Charges = hab1Charges;
            }
            if(currentHab1Charges == hab1Charges)
            {
                currentHab1Cd = 0;
            }
            if(target == p1Up0Target)
            {
                p1Up0Target = null;
            }
        }
        if (upgrades.path1Upg2)
        {
            Heal(this, ((p2Up1LifePercentageHeal * stats.mHp) / 100), element);
        }
        base.OnKill(target);
    }

    public override bool IsCasting()
    {
        return base.IsCasting() && hab1Dashing;
    }

    public void OnDrawGizmosSelected()
    {
        if (!upgrades.path2)
        {
            Gizmos.DrawWireSphere(transform.position + (transform.up.normalized * basicRange), basicArea);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position + (transform.up.normalized * p2Up0Range), p2Up0Area);
        }

        Gizmos.DrawWireSphere(transform.position + (transform.up.normalized * hab1Offset), hab1Area);

        Gizmos.DrawWireSphere(transform.position, up4Area);
        Gizmos.DrawWireSphere(transform.position, hab2Area);
    }
}

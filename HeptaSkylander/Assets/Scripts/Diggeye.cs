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

    public float hab1Dmg;
    public float hab1Spd;
    public float hab1Range;
    public float hab1Area;
    public float hab1Offset;
    public float hab1RangeExt;

    public GameObject undergroundFx;
    bool hab2Underground;
    float hab2ActualEnergy;
    public float hab2ExtraSpd;
    public float hab2MinTime;

    public float up1DashRange;
    public float up1DashSpd;

    public int up2Dashes;
    int up2CurrentDashes;

    public float up3DmgBoost;

    public GameObject up4Fx;
    public float up4Area;
    public float up4Dmg;
    public float up4Rupture;

    bool p1Up1Active;

    bool p1Up2Active;
    public float p1Up2DmgMult;

    public override void Start()
    {
        if (upgrades.upg3)
        {
            basicDmg += ((basicDmg * up3DmgBoost) / 100);
            hab1Dmg += ((hab1Dmg * up3DmgBoost) / 100);
        }
        hab2ActualEnergy = hab2Cd;
        base.Start();
    }
    public override void Update()
    {
        if (hab2Underground)
        {
            if (upgrades.path1)
            {
                hab2ActualEnergy -= Time.deltaTime;
            }
            else
            {
                hab2ActualEnergy -= Time.deltaTime * 10;
            }
            animator.Play("Dash");
            if(hab2ActualEnergy <= 0)
            {
                Unbury();
            }
        }
        else if(hab2ActualEnergy<hab2Cd)
        {
            hab2ActualEnergy += Time.deltaTime;
        }
        currentHab2Cd = hab2ActualEnergy;
        base.Update();
    }

    public override IEnumerator MainAttack()
    {
        if (hab2Underground)
        {
            UnburyAttack();
            StartCoroutine(PlayAnimation("Special"));
        }
        else if (currentBasicCd <= 0 && !casting && !dashing)
        {
            currentBasicCd = CalculateAtSpd(basicCd * stats.atSpd);
            lockPointer = false;
            casting = false;
            lookAtPointer = true;

            Vector2 dir = cursor.transform.position - pointer.transform.position;
            pointer.transform.up = dir;
            yield return null;
            lockPointer = true;

            float range = 0;
            float speed = 0;
            if (Physics2D.CircleCast(transform.position + (pointer.transform.up.normalized * basicRange), 2, pointer.transform.up, up1DashRange, GameManager.Instance.wallLayer + GameManager.Instance.unitLayer))
            {
                Vector2 dist = Physics2D.CircleCast(transform.position + (pointer.transform.up.normalized * basicRange), 1, pointer.transform.up, up1DashRange, GameManager.Instance.unitLayer).point - new Vector2(transform.position.x, transform.position.y);
                range = basicDashRange - ((basicDashRange - dist.magnitude) + 0.5f);
                speed = up1DashSpd / (up1DashRange / range);
                if (speed > up1DashSpd)
                {
                    speed = up1DashSpd;
                }
                if (range > basicDashRange)
                {
                    range = 0;
                }

                if (range > 0)
                {
                    StartCoroutine(Dash(pointer.transform.up, speed, range, false, false, false));
                }
            }

            if (basicComboCount % 2 == 0)
            {
                StartCoroutine(PlayAnimation("DiggeyeBasic1"));
            }
            else
            {
                StartCoroutine(PlayAnimation("DiggeyeBasic2"));
            }
            basicComboCount += 1;
            comboReset = 0.75f;
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
                enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element,isCrit);
                Instantiate(basicFxImpact, enemy.transform.position, transform.rotation);
            }
        }
    }

    public void UnburyAttack()
    {
        currentBasicCd = CalculateAtSpd(basicCd * stats.atSpd);
        Unbury();
        Instantiate(up4Fx, transform.position, transform.rotation);

        float dmg;
        dmg = CalculateStrength(up4Dmg);
        float rupture;
        rupture = CalculateRupture(up4Rupture);

        Vector2 dir = transform.position + (pointer.transform.up.normalized * basicRange);
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, up4Area, GameManager.Instance.unitLayer);
        PjBase enemy;
        foreach (Collider2D enemyColl in enemiesHit)
        {
            enemy = enemyColl.GetComponent<PjBase>();
            if (enemy.team != team)
            {
                enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                enemy.GetComponent<TakeDamage>().TakeRupture(this, rupture);
            }
        }
    }

    public override IEnumerator Hab1()
    {
        if (basicComboCount >= 2 && upgrades.upg1 && !casting && !dashing)
        {
            basicComboCount = 0;
            if (currentHab1Cd < 1)
            {
                currentHab1Cd = 1;
            }
            currentBasicCd = CalculateAtSpd(basicCd * stats.atSpd);
            lockPointer = false;
            casting = true;
            lookAtPointer = true;

            yield return null;
            lockPointer = true;
            Vector2 dir = cursor.transform.position - pointer.transform.position;
            pointer.transform.up = dir;
            float range = 0;
            float speed = 0;
            if (Physics2D.CircleCast(transform.position + (pointer.transform.up.normalized * basicRange), 2, pointer.transform.up, up1DashRange, GameManager.Instance.unitLayer))
            {
                Vector2 dist = Physics2D.CircleCast(transform.position + (pointer.transform.up.normalized * basicRange), 1, pointer.transform.up, up1DashRange, GameManager.Instance.wallLayer + GameManager.Instance.unitLayer).point - new Vector2(transform.position.x, transform.position.y);
                range = up1DashRange - ((up1DashRange - dist.magnitude) + 0.5f);
                speed = up1DashSpd / (up1DashRange / range);
                if (speed > up1DashSpd)
                {
                    speed = up1DashSpd;
                }
                if (range > up1DashRange)
                {
                    range = 0;
                }

                StartCoroutine(Dash(pointer.transform.up, speed, range, false, false, false));
            }
            yield return null;

            StartCoroutine(PlayAnimation("DiggeyeBasic3"));
        }
        else if (hab2Underground && upgrades.path1Upg1 && !casting && !dashing)
        {
            p1Up1Active = true;
            if (upgrades.path1Upg2)
            {
                p1Up2Active = true;
            }
            StartCoroutine(UseHab1());
        }
        else if (currentHab1Cd <= 0 && !casting && !dashing)
        {
            if (hab2Underground)
            {
                if (upgrades.path1Upg2)
                {
                    p1Up2Active = true;
                }
            }
            StartCoroutine(UseHab1());
            currentHab1Cd = CDR(hab1Cd);

            yield return base.Hab1();
        }
    }

    IEnumerator UseHab1()
    {
        Unbury();
        lockPointer = false;
        casting = true;
        lookAtPointer = true;
        Vector2 dir = cursor.transform.position - pointer.transform.position;
        pointer.transform.up = dir;
        yield return null;
        lookAtPointer = false;
        float dmg;
        dmg = CalculateStrength(hab1Dmg);
        if (p1Up2Active)
        {
            dmg += (dmg * p1Up2DmgMult) / 100;
        }

        StartCoroutine(Dash(pointer.transform.up, hab1Spd, hab1Range));

        while (!dashing)
        {
            yield return null;
        }
        List<PjBase> targetsAffected = new List<PjBase>();
        while (dashing)
        {
            dir = transform.position + (pointer.transform.up.normalized * hab1Offset);
            Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(dir, hab1Area, GameManager.Instance.unitLayer);
            PjBase enemy;
            foreach (Collider2D enemyColl in enemiesHit)
            {
                enemy = enemyColl.GetComponent<PjBase>();
                if (enemy.team != team && !targetsAffected.Contains(enemy))
                {
                    if (!p1Up1Active)
                    {
                        enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                    }
                    else
                    {
                        enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, 100, out bool isCrit), element, isCrit);
                    }
                    Instantiate(basicFxImpact, enemy.transform.position, transform.rotation);
                    StartCoroutine(Dash(pointer.transform.up, hab1Spd, hab1RangeExt));
                    if (p1Up2Active)
                    {
                        UnburyAttack();
                    }

                    targetsAffected.Add(enemy);
                    while (!dashing)
                    {
                        yield return null;
                    }

                    while (dashing)
                    {
                        yield return null;
                    }

                    if (upgrades.upg2)
                    {
                        up2CurrentDashes = up2Dashes;

                        while (up2CurrentDashes > 0)
                        {
                            PjBase enemy2;
                            enemiesHit = Physics2D.OverlapCircleAll(transform.position, hab1Range, GameManager.Instance.unitLayer);
                            bool checker = false;
                            foreach (Collider2D enemyColl2 in enemiesHit)
                            {
                                enemy2 = enemyColl2.GetComponent<PjBase>();
                                if (enemy2.team != team)
                                {
                                    if (!targetsAffected.Contains(enemy2))
                                    {
                                        checker = true;
                                    }
                                }

                            }

                            if (!checker)
                            {
                                targetsAffected.Clear();
                            }
                            enemy2 = null;
                            foreach (Collider2D enemyColl2 in enemiesHit)
                            {
                                if (!enemy2 && enemyColl2.GetComponent<PjBase>().team != team)
                                {
                                    enemy2 = enemyColl2.GetComponent<PjBase>();
                                }
                                else if (enemy2 && enemyColl2.GetComponent<PjBase>().team != team)
                                {
                                    if (targetsAffected.Contains(enemy2) && !targetsAffected.Contains(enemyColl2.GetComponent<PjBase>()))
                                    {
                                        enemy2 = enemyColl2.GetComponent<PjBase>();
                                    }
                                    else if (!targetsAffected.Contains(enemyColl2.GetComponent<PjBase>()))
                                    {
                                        float enemyDist = Vector2.Distance(enemy2.transform.position, transform.position);
                                        float newEnemyDist = Vector2.Distance(enemyColl2.GetComponent<PjBase>().transform.position, transform.position);
                                        if (newEnemyDist > enemyDist)
                                        {
                                            enemy2 = enemyColl2.GetComponent<PjBase>();

                                        }
                                    }

                                }

                            }
                            targetsAffected.Add(enemy2);
                            if (!enemy2)
                            {
                                break;
                            }
                            StartCoroutine(Dash(enemy2.transform, hab1Spd, hab1Range, false, true, true));

                            while (!dashing)
                            {
                                yield return null;
                            }

                            List<PjBase> currentTargetsAffected = new List<PjBase>();
                            while (dashing)
                            {
                                enemiesHit = Physics2D.OverlapCircleAll(transform.position, hab1Area + 0.75f, GameManager.Instance.unitLayer);
                                PjBase potentialEnemy;
                                foreach (Collider2D enemyColl3 in enemiesHit)
                                {
                                    potentialEnemy = enemyColl3.GetComponent<PjBase>();
                                    if (potentialEnemy == enemy2 && !currentTargetsAffected.Contains(enemy2))
                                    {
                                        if (p1Up2Active)
                                        {
                                            UnburyAttack();
                                        }

                                        if (!p1Up1Active)
                                        {
                                            enemy2.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                                        }
                                        else
                                        {
                                            enemy2.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, 100,out bool isCrit), element, isCrit);
                                        }
                                        Instantiate(basicFxImpact, enemy2.transform.position, transform.rotation);
                                        StartCoroutine(Dash((enemy2.transform.position - transform.position).normalized, hab1Spd, hab1RangeExt));
                                        currentTargetsAffected.Add(enemy2);
                                        while (!dashing)
                                        {
                                            yield return null;
                                        }

                                        while (dashing)
                                        {
                                            yield return null;
                                        }

                                    }
                                }
                                yield return null;
                            }
                            up2CurrentDashes--;
                        }
                    }
                }
            }
            yield return null;
        }

        p1Up2Active = false;
        p1Up1Active = false;
    }
    

    public override IEnumerator Hab2()
    {
        if ((hab2ActualEnergy >= hab2Cd || hab2Underground) && !casting && !dashing && upgrades.upg1 && upgrades.upg2)
        {
            if (!hab2Underground)
            {
                Bury();
            }
            else
            {
                Unbury();
            }
            casting = true;
            yield return new WaitForSeconds(hab2MinTime);
            casting = false;
        }
        yield return base.Hab2();
    }

    public void Bury()
    {
        if (!hab2Underground)
        {
            hab2Underground = true;
            invulnerable = true;
            stats.spd += hab2ExtraSpd;
            undergroundFx.SetActive(true);
        }
    }
    public void Unbury()
    {
        if (hab2Underground)
        {
            hab2ActualEnergy = 0;
            hab2Underground = false;
            invulnerable = false;
            stats.spd -= hab2ExtraSpd;
            undergroundFx.SetActive(false);
            animator.Play("Idle");
        }
    }

    public override void OnKill(PjBase target)
    {
        if (upgrades.path1)
        {
            hab2ActualEnergy = hab2Cd;
        }
        base.OnKill(target);
    }


    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + (transform.up.normalized * basicRange), basicArea);

        Gizmos.DrawWireSphere(transform.position + (transform.up.normalized * hab1Offset), hab1Area);

        Gizmos.DrawWireSphere(transform.position, up4Area);
    }
}

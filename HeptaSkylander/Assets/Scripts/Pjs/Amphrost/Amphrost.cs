using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
using static UnityEditor.FilePathAttribute;

public class Amphrost : PjBase
{
    public float basicDashRange;
    public float basicCd;
    public float basicDmg;
    public float basicRuptureDmg;
    public float basicArea;
    public GameObject basicFxImpact;

    float hab1TemporalSpd;
    public float hab1Dmg;
    public float hab1RuptureDmg;
    public float hab1Spd;
    public float hab1Range;
    public float hab1Area;
    Transform hab1EnemyTrnsfm;

    public GlacialShield hab2Shield;
    public GameObject hab2FxObject;
    public GameObject hab2ShieldParticle;
    public float hab2TotalDmg;
    public float hab2Area;
    public float hab2TotalDuration;
    public float hab2MaxShield;
    public float hab2ShieldLosePerSecond;
    public float hab2ShieldPerTick;
    public int hab2TicksPerSecond;

    public GameObject up1Fx;
    public float up1Dmg;
    public float up1RuptureDmg;
    public float up1Area;

    public GameObject up2Fx;
    public float up2Dmg;
    public float up2RuptureDmg;
    public float up2Area;

    public float up3Range;
    public float up3Spd;

    public GameObject p1Mark;
    public List<IceMark> p1IceMarks = new List<IceMark> ();
    public float p1ruptureMult;
    public float p1MarkDmg;
    public float p1MarkRuptureDmg;

    public GameObject p1up2SpikesObject;
    public GameObject p1up2Fx1;
    public GameObject p1up2Fx2;
    public float p1up2Delay;
    public float p1up2Area1;
    public float p1up2Area2;

    public float p1up3BuffDuration;
    float p1up3CurrentBuffDuration;
    public float p1up3ExtraAtSpd;
    public float p1up3CdReduc;

    public float p2HpBoost;
    public int p2ExtraTicks;
    public float p2MaxExplosionValue;
    public float p2MinExplosionValue;
    [HideInInspector]
    public float p2CurrentExplosionValue;
    public GameObject p2ExplosionFx;

    public float p2up2TimePerTick;
    float p2up2CurrentTimePerTick;

    public override void Awake()
    {
        if (upgrades.path2)
        {
            stats.mHp += p2HpBoost;
            hab2TicksPerSecond += p2ExtraTicks;
        }
        base.Awake();
    }

    public override void Update()
    {
        base.Update();
        if(p1up3CurrentBuffDuration> 0)
        {
            p1up3CurrentBuffDuration-=Time.deltaTime;
            if(p1up3CurrentBuffDuration <= 0)
            {
                stats.atSpd -= p1up3ExtraAtSpd;
            }
        }

        if (upgrades.path2Upg2 && hab2Shield.shieldAmount > 0)
        {
            if(p2up2CurrentTimePerTick > 0)
            {
                p2up2CurrentTimePerTick -= Time.deltaTime;
                if(p2up2CurrentTimePerTick <= 0)
                {
                    IceExplosion(transform.position,true);
                    p2up2CurrentTimePerTick = p2up2TimePerTick;
                }
            }
        }
    }
    public override void Start()
    {
        base.Start();
        hab2Shield = gameObject.AddComponent<GlacialShield>();
        hab2Shield.ShieldSetUp(this, this, 0, CalculateMHp(hab2MaxShield), 0, null);
        hab2Shield.amphrost = this;

        if (upgrades.path1)
        {
            basicRuptureDmg += ((basicRuptureDmg * p1ruptureMult) / 100);
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

            if (basicComboCount % 2 == 0)
            {
                StartCoroutine(PlayAnimation("AmphrostBasic1"));
            }
            else
            {
                StartCoroutine(PlayAnimation("AmphrostBasic2"));
            }
            basicComboCount += 1;
            comboReset = 1.25f;
        }

        yield return base.MainAttack();

    }
    public void BasicAttack()
    {
        float dmg;
        dmg = CalculateStrength(basicDmg);
        float rupture;
        rupture = CalculateRupture(basicRuptureDmg);

        Vector2 dir = transform.position;
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(dir, basicArea, GameManager.Instance.unitLayer);
        PjBase enemy;
        bool impact = false;
        foreach (Collider2D enemyColl in enemiesHit)
        {
            enemy = enemyColl.GetComponent<PjBase>();
            if (enemy.team != team)
            {
                enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                enemy.GetComponent<TakeDamage>().TakeRupture(this, rupture);
                GameObject fx = Instantiate(basicFxImpact, enemy.transform.position, transform.rotation);
                fx.transform.up = fx.transform.position - transform.position;

                if (upgrades.path1)
                {
                    SetMark(enemy);
                }

                if (upgrades.path1Upg3)
                {
                    impact = true;
                }


            }
        }
        if (impact)
        {
            currentHab1Cd -= p1up3CdReduc;
        }
    }
    public void SpikesAttack()
    {
        Instantiate(up1Fx, transform.position, transform.rotation);

        float dmg;
        dmg = CalculateStrength(up1Dmg);
        float rupture;
        rupture = CalculateRupture(up1RuptureDmg);

        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, up1Area, GameManager.Instance.unitLayer);
        PjBase enemy;

        bool impact = false;
        foreach (Collider2D enemyColl in enemiesHit)
        {
            enemy = enemyColl.GetComponent<PjBase>();
            if (enemy.team != team)
            {
                enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                enemy.GetComponent<TakeDamage>().TakeRupture(this, rupture); 
                GameObject fx = Instantiate(basicFxImpact, enemy.transform.position, transform.rotation);
                fx.transform.up = fx.transform.position - transform.position;

                if (upgrades.path1)
                {
                    SetMark(enemy);
                }


                if (upgrades.path1Upg3)
                {
                    impact = true;
                }
            }
        }

        if (upgrades.path1Upg2)
        {
            StartCoroutine(UpgradedSpikesAttack());
        }

        if (impact)
        {
            currentHab1Cd -= p1up3CdReduc;
        }
    }

    public IEnumerator UpgradedSpikesAttack()
    {
        float dmg;
        dmg = CalculateStrength(up1Dmg);
        float rupture;
        rupture = CalculateRupture(up1RuptureDmg);

        GameObject spikes = Instantiate(p1up2SpikesObject, transform.position, transform.rotation);

        yield return new WaitForSeconds(p1up2Delay);

        Instantiate(p1up2Fx1, spikes.transform.position, spikes.transform.rotation);
        spikes.transform.localScale = new Vector3(22, 22, 22);


        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(spikes.transform.position, p1up2Area1, GameManager.Instance.unitLayer);
        PjBase enemy;

        foreach (Collider2D enemyColl in enemiesHit)
        {
            enemy = enemyColl.GetComponent<PjBase>();
            if (enemy.team != team)
            {
                enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                enemy.GetComponent<TakeDamage>().TakeRupture(this, rupture);
                GameObject fx = Instantiate(basicFxImpact, enemy.transform.position, transform.rotation);
                fx.transform.up = fx.transform.position - spikes.transform.position;

                if (upgrades.path1)
                {
                    SetMark(enemy);
                }
            }
        }


        yield return new WaitForSeconds(p1up2Delay);

        Instantiate(p1up2Fx2, spikes.transform.position, spikes.transform.rotation);

        enemiesHit = Physics2D.OverlapCircleAll(spikes.transform.position, p1up2Area2, GameManager.Instance.unitLayer);

        foreach (Collider2D enemyColl in enemiesHit)
        {
            enemy = enemyColl.GetComponent<PjBase>();
            if (enemy.team != team)
            {
                enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                enemy.GetComponent<TakeDamage>().TakeRupture(this, rupture);
                GameObject fx = Instantiate(basicFxImpact, enemy.transform.position, transform.rotation);
                fx.transform.up = fx.transform.position - spikes.transform.position;

                if (upgrades.path1)
                {
                    SetMark(enemy);
                }
            }
        }

        Destroy(spikes);
    }

    public void SetMark(PjBase target)
    {
        float dmg;
        dmg = CalculateStrength(p1MarkDmg);
        float rupture;
        rupture = CalculateRupture(p1MarkRuptureDmg);
        bool hasMark = false;
        IceMark iceMark = null;
        if (p1IceMarks.Count > 0)
        {
            foreach (IceMark mark in p1IceMarks)
            {
                if(mark.target == target)
                {
                    hasMark = true;
                    iceMark = mark;
                }
            }
        }

        if (hasMark)
        {
            if (iceMark.isStage2)
            {
                target.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                target.GetComponent<TakeDamage>().TakeRupture(this, rupture);
                if (upgrades.path1Upg1)
                {
                    IceExplosion(target.transform.position,false);
                }
                p1IceMarks.Remove(iceMark);
            }
            iceMark.UpdateMark();
        }
        else
        {
            iceMark = Instantiate(p1Mark,target.transform).GetComponent<IceMark>();
            p1IceMarks.Add(iceMark);
            iceMark.target = target;
        }
    }

    public override IEnumerator Hab1()
    {
        if (basicComboCount >= 2 && upgrades.upg1 && !IsCasting() && !dashing)
        {
            basicComboCount = 0;
            if (currentHab1Cd < 1)
            {
                currentHab1Cd = 1;
            }
            currentBasicCd = CalculateAtSpd(basicCd * stats.atSpd);
            lockPointer = false;
            softCasting = true;
            lookAtPointer = true;

            yield return null;
            lockPointer = true;
            StartCoroutine(PlayAnimation("AmphrostBasic3"));
        }
        else if (currentHab1Cd <= 0 && !IsCasting() && !dashing)
        {
            lockPointer = false;
            casting = true;
            lookAtPointer = true;

            Vector2 dir = cursor.transform.position - pointer.transform.position;
            pointer.transform.up = dir;
            yield return null;
            lockPointer = true;
            if (Physics2D.CircleCast(transform.position + (pointer.transform.up.normalized * 3.3f), 2, pointer.transform.up, hab1Range -5, GameManager.Instance.unitLayer))
            {
                hab1EnemyTrnsfm = Physics2D.CircleCast(transform.position + (pointer.transform.up.normalized * 3.3f), 2, pointer.transform.up, hab1Range -5, GameManager.Instance.unitLayer).collider.gameObject.transform;
            }
            else
            {
                hab1EnemyTrnsfm = null;
            }
            StartCoroutine(PlayAnimation("Special"));

            yield return base.Hab1();
        }
    }

    public void UseCharge()
    {
        StartCoroutine(Charge());
    }
    public IEnumerator Charge()
    {
        float dmg;
        dmg = CalculateStrength(hab1Dmg);
        float rupture;
        rupture = CalculateRupture(hab1RuptureDmg);
        if (hab1EnemyTrnsfm)
        {
            PjBase targetEnemy = hab1EnemyTrnsfm.gameObject.GetComponent<PjBase>();

            if (targetEnemy.team != team)
            {
                StartCoroutine(Dash(hab1EnemyTrnsfm, hab1Spd, hab1Range, false, false, true));

                currentHab1Cd = CDR(hab1Cd);

                while (!dashing)
                {
                    yield return null;
                }
                PjBase potentialEnemy = null;
                List<PjBase> targetsAffected = new List<PjBase>();
                while (dashing)
                {
                    Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, hab1Area + 0.75f, GameManager.Instance.unitLayer);

                    foreach (Collider2D enemyColl3 in enemiesHit)
                    {
                        potentialEnemy = enemyColl3.GetComponent<PjBase>();
                        if (potentialEnemy == targetEnemy && !targetsAffected.Contains(targetEnemy))
                        {
                            targetEnemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                            targetEnemy.GetComponent<TakeDamage>().TakeRupture(this, rupture);
                            GameObject fx = Instantiate(basicFxImpact, targetEnemy.transform.position, transform.rotation);
                            fx.transform.up = fx.transform.position - transform.position;
                            targetsAffected.Add(targetEnemy);

                            if (upgrades.upg2)
                            {
                                IceExplosion(targetEnemy.transform.position, false);
                            }

                            if (upgrades.path1)
                            {
                                SetMark(targetEnemy);
                            }

                            if (upgrades.path1Upg3)
                            {
                                stats.atSpd += p1up3ExtraAtSpd;
                                p1up3CurrentBuffDuration = p1up3BuffDuration;
                            }
                        }
                    }
                    yield return null;
                }
            }
        }
    }

    public void IceExplosion(Vector3 location, bool isGlacial)
    {
        Instantiate(up2Fx, location, transform.rotation);

        float dmg;
        dmg = CalculateStrength(up2Dmg);
        float rupture;
        rupture = CalculateRupture(up2RuptureDmg);

        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(location, up2Area, GameManager.Instance.unitLayer);
        PjBase enemy;
        foreach (Collider2D enemyColl in enemiesHit)
        {
            enemy = enemyColl.GetComponent<PjBase>();
            if (enemy.team != team)
            {
                enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                enemy.GetComponent<TakeDamage>().TakeRupture(this, rupture);
                GameObject fx = Instantiate(basicFxImpact, enemy.transform.position, transform.rotation);
                fx.transform.up = fx.transform.position - location;

                if (isGlacial && upgrades.path2Upg3)
                {
                    Instantiate(hab2ShieldParticle, enemy.transform.position, transform.rotation).GetComponent<GlacialProjectile>().user = gameObject;
                }
            }
        }

        
    }

    public override IEnumerator Hab2()
    {
        if (currentHab2Cd <= 0 && !IsCasting() && !dashing && upgrades.upg1 && upgrades.upg2)
        {
            if (upgrades.path2Upg2)
            {
                p2up2CurrentTimePerTick = p2up2TimePerTick;
            }

            p2CurrentExplosionValue = 0 + hab2Shield.shieldAmount;

            hab2Shield.ChangeShieldAmount(-hab2Shield.shieldAmount);

            currentHab2Cd = CDR(hab2Cd);
            float dmg;
            dmg = CalculateStrength(hab2TotalDmg) / (hab2TotalDuration * hab2TicksPerSecond) ;

            lockPointer = false;
            casting = true;
            lookAtPointer = false;

            yield return null;
            lockPointer = true;

            StartCoroutine(PlayAnimation("Burst"));

            float duration = hab2TotalDuration;
            float tickCounter = 1f / hab2TicksPerSecond;
            while (duration > 0)
            {
                if (tickCounter <= 0)
                {
                    Collider2D[] enemiesHit1 = Physics2D.OverlapCircleAll(transform.position, hab2Area, GameManager.Instance.unitLayer);
                    PjBase enemy1;
                    foreach (Collider2D enemyColl in enemiesHit1)
                    {
                        enemy1 = enemyColl.GetComponent<PjBase>();
                        if (enemy1.team != team)
                        {
                            enemy1.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg, out bool isCrit), element, isCrit);
                            Instantiate(hab2ShieldParticle, enemy1.transform.position, transform.rotation).GetComponent<GlacialProjectile>().user = gameObject;
                        }
                    }
                    tickCounter = 1f / hab2TicksPerSecond;
                }
                tickCounter -= Time.deltaTime;
                duration -= Time.deltaTime;
                yield return null;
            }

            if(p2CurrentExplosionValue > p2MaxExplosionValue)
            {
                p2CurrentExplosionValue = p2MaxExplosionValue;
            }

            Instantiate(p2ExplosionFx, transform.position, transform.rotation);

            float dmg1;
            dmg1 = CalculateStrength(p2CurrentExplosionValue + p2MinExplosionValue);

            Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, hab2Area, GameManager.Instance.unitLayer);
            PjBase enemy;
            foreach (Collider2D enemyColl in enemiesHit)
            {
                enemy = enemyColl.GetComponent<PjBase>();
                if (enemy.team != team)
                {
                    enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg1, out bool isCrit), element, isCrit);
                    GameObject fx = Instantiate(basicFxImpact, enemy.transform.position, transform.rotation);
                    fx.transform.up = fx.transform.position - transform.position;
                }
            }

            StartCoroutine(PlayAnimation("Idle"));
            yield return null;
            AnimationCallStopAnim();
        }
        yield return base.Hab2();
    }

    public void GlacialExplosion()
    {
        if (upgrades.upg3)
        {
            Instantiate(up2Fx, transform.position, transform.rotation);

            float dmg1;
            dmg1 = CalculateStrength(up2Dmg);
            float rupture;
            rupture = CalculateRupture(up2RuptureDmg);

            Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, up2Area, GameManager.Instance.unitLayer);
            PjBase enemy;
            foreach (Collider2D enemyColl in enemiesHit)
            {
                enemy = enemyColl.GetComponent<PjBase>();
                if (enemy.team != team)
                {
                    enemy.GetComponent<TakeDamage>().TakeDamage(this, CalculateDmg(dmg1, out bool isCrit), element, isCrit);
                    enemy.GetComponent<TakeDamage>().TakeRupture(this, rupture);
                    Vector2 dir = enemy.transform.position - transform.position;
                    StartCoroutine(enemy.Dash(dir, up3Spd, up3Range - dir.magnitude));
                }
            }
        }
    }

    public void HealShield()
    {
        if (upgrades.path2)
        {
            float value = (hab2Shield.shieldAmount + CalculateMHp(hab2ShieldPerTick)) - hab2Shield.maxShieldAmount;
            if (value>0)
            {
                p2CurrentExplosionValue += value;
            }
        }

        hab2Shield.ChangeShieldAmount(CalculateMHp(hab2ShieldPerTick));
    }

    public override void OnEnemyBreak(PjBase target, float amount)
    {
        base.OnEnemyBreak(target, amount);
        if (upgrades.upg4)
        {
            IceExplosion(target.transform.position, false);
        }
    }
    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, basicArea);
        Gizmos.DrawWireSphere(transform.position, hab1Area);
        Gizmos.DrawWireSphere(transform.position, hab1Range);

        Gizmos.DrawWireSphere(transform.position + (transform.up.normalized * 3.25f), 2);

        Gizmos.DrawWireSphere(transform.position, up2Area);
    }
}

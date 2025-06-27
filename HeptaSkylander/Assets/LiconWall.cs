using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class LiconWall : MonoBehaviour
{
    bool activated = false;
    PjBase user;
    public ParticleSystem fx1;
    public ParticleSystem fx2;
    [HideInInspector]
    public float duration;
    float mHp;
    float hp;

    List<PjBase> targets;
    public float up2Wide;
    public float up2Length;
    float ticksPerSecond;
    float dmgPerTick;
    float rupturePerTick;
    float slow;
    float tick;

    public void SetUp(PjBase user, float duration, float mHp, float ticksPerSecond, float dmgPerTick, float rupturePerTick, float slow)
    {
        this.user = user;
        this.duration = duration;
        this.mHp = mHp;
        hp = mHp;

        this.ticksPerSecond = ticksPerSecond;
        this.dmgPerTick = dmgPerTick;
        this.rupturePerTick = rupturePerTick;
        tick = 1 / ticksPerSecond;
        this.slow = slow;
    }

    public void ActivateWall()
    {
        activated = true;
        fx1.Play();
        fx2.Play();
    }

    private void Update()
    {
        duration -=Time.deltaTime;
        if(duration <= 0)
        {
            Die();
        }

        if(user.upgrades.upg2 && tick > 0 && activated)
        {
            tick -=Time.deltaTime;
            if (tick <= 0)
            {
                tick = 1 / ticksPerSecond;
                Collider2D[] enemiesHit = Physics2D.OverlapBoxAll(transform.position, new Vector2(up2Wide, up2Length), transform.localEulerAngles.z, GameManager.Instance.unitLayer);
                PjBase target;
                foreach (Collider2D enemyColl in enemiesHit)
                {
                    target = enemyColl.GetComponent<PjBase>();

                    if (target.team != user.team)
                    {
                        target.GetComponent<TakeDamage>().TakeDamage(user, user.CalculateDmg(dmgPerTick, out bool isCrit), user.element, isCrit);
                        target.GetComponent<TakeDamage>().TakeRupture(user, rupturePerTick);

                        if (!targets.Contains(target))
                        {
                            target.stats.spd -= slow;
                            targets.Add(target);
                        }
                    }
                }
            }
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated)
        {
            if (collision.GetComponent<Projectile>())
            {
                Projectile bullet = collision.GetComponent<Projectile>();
                if (bullet.user.team != user.team)
                {
                    hp -= bullet.dmg;
                    bullet.Die();
                }
            }

            if (user.upgrades.upg2 && collision.GetComponent<PjBase>())
            {
                PjBase enemy = collision.GetComponent<PjBase>();
                if (enemy.team != user.team)
                {
                    enemy.stats.spd -= slow;
                    targets.Add(enemy);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (user.upgrades.upg2 && collision.GetComponent<PjBase>())
        {
            PjBase enemy = collision.GetComponent<PjBase>();
            if (enemy.team != user.team && targets.Contains(enemy))
            {
                enemy.stats.spd += slow;
                targets.Remove(enemy);
            }
        }
    }

    public void Die()
    {
        user.currentHab1Cd = user.CDR(user.hab1Cd);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(up2Wide, up2Length, 1));
    }
}

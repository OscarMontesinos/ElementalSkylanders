using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PostExplosion : MonoBehaviour
{
    PjBase user;
    public ParticleSystem explosionFx;
    public ParticleSystem explosionFx2;
    float dmg;
    float area;
    int ticks;
    float initialDelay;
    float tickDelay;


    public void SetUp(PjBase user, float dmg, float area, int ticks, float initialDelay, float tickDelay)
    {
        this.user = user;
        this.dmg = dmg;
        this.area = area;
        this.ticks = ticks;
        this.initialDelay = initialDelay;
        this.tickDelay = tickDelay;
    }

    private void Update()
    {
        if (initialDelay > 0)
        {
            initialDelay -= Time.deltaTime;
            if(initialDelay  <= 0)
            {
                if (ticks <= 0)
                {
                    Die();
                }
                else
                {
                    Explosion();
                    ticks--;
                    initialDelay = tickDelay;
                    if (ticks <= 0)
                    {
                        initialDelay = 1;
                    }
                }

            }
        }

        
    }

    public void Explosion()
    {
        if (user.upgrades.path1Upg3)
        {
            explosionFx2.Play();
        }
        else
        {
            explosionFx.Play();
        }
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, area, GameManager.Instance.unitLayer);
        PjBase enemy;
        foreach (Collider2D enemyColl in enemiesHit)
        {
            enemy = enemyColl.GetComponent<PjBase>();
            if (enemy.team != user.team)
            {
                enemy.GetComponent<TakeDamage>().TakeDamage(user, user.CalculateDmg(dmg, out bool isCrit), user.element, isCrit);
            }
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}

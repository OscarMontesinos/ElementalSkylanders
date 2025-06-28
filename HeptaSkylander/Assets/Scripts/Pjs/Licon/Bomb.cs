using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

public class Bomb : Projectile
{
    Licon licon;
    public GameObject explosionFx;
    public GameObject explosionFxUp4;
    public GameObject explosionFxP1Up3;
    public float area;
    public float maxSpd;
    public float minSpd;

    public GameObject postExplosion;

    public void SetUp(PjBase user, HitData.Element element, PjBase.AttackType type, float dmg, float maxSpeed, float minSpeed, float range, float area)
    {
        maxSpd = maxSpeed;
        minSpd = minSpeed;
        this.area = area;
        licon = user.GetComponent<Licon>();
        NormalSetUp(user, element, type, dmg, maxSpeed, range);
    }

    public override void Update()
    {
        base.Update();

        if(range == 0)
        {
            Explosion();
        }

        speed = Mathf.Lerp(maxSpd,minSpd,dist / range);
    }

    public void Explosion()
    {
        GameObject explosionFx = this.explosionFx;
        if (user.upgrades.path1Upg3)
        {
            explosionFx = explosionFxP1Up3;
        }
        else if (user.upgrades.upg4)
        {
            explosionFx = explosionFxUp4;
        }
        
        Instantiate(explosionFx,transform.position,transform.rotation);
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, area, GameManager.Instance.unitLayer);
        PjBase enemy;
        bool p1 = false;
        foreach (Collider2D enemyColl in enemiesHit)
        {
            enemy = enemyColl.GetComponent<PjBase>();
            if (enemy.team != user.team)
            {
                enemy.GetComponent<TakeDamage>().TakeDamage(user, user.CalculateDmg(dmg, out bool isCrit), element, isCrit);
                if (user.upgrades.path2Upg3)
                {
                    user.currentHab2Cd -= licon.p2Up3SecondsRefunded;
                }

                p1 = true;

                
            }
        }
        if (p1 && user.upgrades.path1)
        {
            licon.p1CurrentCharges++;
            if (licon.p1CurrentCharges > licon.p1Charges)
            {
                licon.p1CurrentCharges = licon.p1Charges;
            }
        }

        if (user.upgrades.path1Upg2)
        {
            PostExplosion bomb = Instantiate(postExplosion, transform.position, transform.rotation).GetComponent<PostExplosion>();
            bomb.SetUp(user,dmg * (licon.p1Up2Dmg/100), licon.basicArea, licon.p1Up2Ticks, licon.p1Up2InitialDelay, licon.p1Up2TickDelay);
        }

        base.Die();
    }

    public override void Die()
    {
        Explosion();
    }

}

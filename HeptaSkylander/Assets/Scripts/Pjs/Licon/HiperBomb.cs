using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

public class HiperBomb : Projectile
{
    Licon licon;
    public GameObject explosionFx;
    public float area;
    public float spd;


    public void SetUp(PjBase user, HitData.Element element, PjBase.AttackType type, float dmg, float spd, float range, float area)
    {
        this.spd = spd;
        this.area = area;
        licon = user.GetComponent<Licon>();
        NormalSetUp(user, element, type, dmg, spd, range);
    }

    public override void Update()
    {
        base.Update();

        if(range == 0)
        {
            Explosion();
        }

    }

    public void Explosion()
    {
        GameObject explosionFx = this.explosionFx;
        Instantiate(explosionFx,transform.position,transform.rotation);
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, area, GameManager.Instance.unitLayer);
        PjBase enemy;
        foreach (Collider2D enemyColl in enemiesHit)
        {
            enemy = enemyColl.GetComponent<PjBase>();
            if (enemy.team != user.team)
            {
                enemy.GetComponent<TakeDamage>().TakeDamage(user, user.CalculateDmg(dmg, out bool isCrit), element, isCrit);
                if (user.upgrades.path2Upg3)
                {
                    user.currentHab2Cd -= (licon.p2Up3SecondsRefunded * 2) ;
                }
            }
        }
        base.Die();
    }

    public override void Die()
    {
        Explosion();
    }

}

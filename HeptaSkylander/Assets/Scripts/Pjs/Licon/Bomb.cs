using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

public class Bomb : Projectile
{
    public GameObject explosionFx;
    public GameObject explosionFxUp4;
    public float area;
    public float maxSpd;
    public float minSpd;


    public void SetUp(PjBase user, HitData.Element element, PjBase.AttackType type, float dmg, float maxSpeed, float minSpeed, float range, float area)
    {
        maxSpd = maxSpeed;
        minSpd = minSpeed;
        this.area = area;
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
        if (user.upgrades.upg4)
        {
            explosionFx = explosionFxUp4;
        }
        Instantiate(explosionFx,transform.position,transform.rotation);
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, area, GameManager.Instance.unitLayer);
        PjBase enemy;
        foreach (Collider2D enemyColl in enemiesHit)
        {
            enemy = enemyColl.GetComponent<PjBase>();
            if (enemy.team != user.team)
            {
                enemy.GetComponent<TakeDamage>().TakeDamage(user, user.CalculateDmg(dmg, out bool isCrit), element, isCrit);
            }
        }
        base.Die();
    }

    public override void Die()
    {
        Explosion();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    bool ready = false;
    public enum CoinType
    {
        money,quartz,xp,heal
    }
    public CoinType type;
    int quantity;
    public Color moneyColor;
    public Color quartzColor;
    public Color xpColor;
    public Color healColor;

    public ParticleSystem traceFx;
    public ParticleSystem sparklesFx;

    public float minDetour;
    public float maxDetour;

    public float speed;
    public float rotateSpeed;

    public PjBase target;
    Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void SetUp(PjBase target,int quantity,CoinType type)
    {
        this.target = target;
        this.type = type;
        this.quantity = quantity;

        switch (type)
        {
            case CoinType.money:
                traceFx.startColor = moneyColor;
                sparklesFx.startColor = moneyColor;
                break;
            case CoinType.quartz:
                traceFx.startColor = quartzColor;
                sparklesFx.startColor = quartzColor;
                break;
            case CoinType.xp:
                traceFx.startColor = xpColor;
                sparklesFx.startColor = xpColor;
                break;
            case CoinType.heal:
                traceFx.startColor = healColor;
                sparklesFx.startColor = healColor;
                break;
        }

        transform.up = target.transform.position - transform.position;
        transform.eulerAngles = new Vector3 (transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z + Random.Range(minDetour, maxDetour));
        if(Random.Range(0,100) > 50)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + Random.Range(minDetour, maxDetour));
        }
        else
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z - Random.Range(minDetour, maxDetour));
        }
        ready = true;
    }

    private void FixedUpdate()
    {
        if (ready)
        {
            Vector2 dir = target.transform.position - transform.position;
            dir.Normalize();
            float rotateAmount = Vector3.Cross(dir, transform.up).z;
            rb.angularVelocity = -rotateAmount * rotateSpeed;
            rb.velocity = transform.up * speed;
            rotateSpeed += 300 * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<PjBase>() == target)
        {
            switch (type)
            {
                case CoinType.money:
                    target.stats.money += quantity;
                    break;
                case CoinType.quartz:
                    target.controller.quartz += quantity;
                    break;
                case CoinType.xp:
                    target.stats.exp += quantity;
                    break;
                case CoinType.heal:
                    target.Heal(target, quantity, target.element);
                    break;
            }

            Destroy(gameObject);
        }
    }
}

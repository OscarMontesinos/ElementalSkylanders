using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GlacialProjectile : MonoBehaviour
{
    public float spd;
    public GameObject user;

    private void Update()
    {
        if (user != null)
        {
            transform.Translate((user.transform.position - transform.position).normalized * spd*Time.deltaTime);
        }
        spd += (spd * 1.15f*Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == user)
        {
            Amphrost amph = user.GetComponent<Amphrost>();
            amph.HealShield();

            Destroy(gameObject);
        }
    }
}

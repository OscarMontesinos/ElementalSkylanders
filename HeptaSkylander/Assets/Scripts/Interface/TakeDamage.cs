using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface TakeDamage
{
    void TakeDamage(PjBase user,float value, HitData.Element element);
    void TakeRupture(PjBase user, float value);
    void Die(PjBase killer);
}

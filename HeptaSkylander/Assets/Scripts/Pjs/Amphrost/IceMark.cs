using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceMark : MonoBehaviour
{
    [HideInInspector]
    public PjBase target;
    [HideInInspector]
    public bool isStage2;
    public GameObject stage1;
    public GameObject stage2;

    public void UpdateMark()
    {
        if (!isStage2)
        {
            isStage2 = true;
            stage1.SetActive(false);
            stage2.SetActive(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

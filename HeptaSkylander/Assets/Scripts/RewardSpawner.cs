using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RewardSpawner : MonoBehaviour
{
    public PjBase target;
    public float delay;
    [Serializable]
    public struct RewardStruct
    {   
        public Coin.CoinType coinType;
        public int quantity;
    }
    public List<RewardStruct> rewardList=  new List<RewardStruct>();
    
    public void SetUp(PjBase target, List<RewardStruct> rewardList)
    {
        this.target = target;
        this.rewardList = new List<RewardStruct>(rewardList);

        StartCoroutine(SpawnRewards());
    }

    public IEnumerator SpawnRewards()
    {
        while (rewardList.Count > 0)
        {
            int index = Random.Range(0, rewardList.Count-1);
            Coin reward = Instantiate(GameManager.Instance.coin,transform.position,transform.rotation).GetComponent<Coin>();
            reward.SetUp(target, rewardList[index].quantity, rewardList[index].coinType);
            rewardList.RemoveAt(index);
            yield return new WaitForSeconds(delay);
        }
        Destroy(gameObject);
    }
}

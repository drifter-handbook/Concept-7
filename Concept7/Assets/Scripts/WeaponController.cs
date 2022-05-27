using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponPrefabList weaponPrefabs;

    [SerializeField] private Animator[] pips;

    public void Start()
    {
        ResetQueue();
    }

    private float timeStamp;
    private Queue<WeaponType> alchemyQueue = new Queue<WeaponType>();
    public void Fire(WeaponType type) {
        if (type == WeaponType.DEFAULT) {
            ResetQueue();
            return;
        }
        if (Time.time >= timeStamp) {
            GameObject prefab = weaponPrefabs.Get(type);
            timeStamp = Time.time + prefab.GetComponent<PlayerWeapon>().weaponData.fireRate;
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }

    public void TryAddAlchemy(WeaponType type) {
        alchemyQueue.Enqueue(type);
        while (alchemyQueue.Count > 3) //TODO have a group discussion on what we want to do when the list fills up
            alchemyQueue.Dequeue();

        WeaponType[] alchemyArray = alchemyQueue.ToArray();
        for(int i = 0; i < alchemyArray.Length; i++)
            pips[i].SetInteger("Color",(int)alchemyArray[i]);
    
    }

    public void TryFireAlchemy() {
        ResetQueue();

    }

    private void ResetQueue() {

        alchemyQueue.Clear();
        for(int i = 0; i < 3; i++)
        {
            alchemyQueue.Enqueue(WeaponType.DEFAULT);
            pips[i].SetInteger("Color",-1);
        }

    }
}

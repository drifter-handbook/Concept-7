using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponPrefabList weaponPrefabs;

    [SerializeField] private Animator[] pips;

    private WeaponData[] alchemyWeapons = new WeaponData[100];

    public void Start()
    {
        ResetQueue();

        AggregateAlchemy();
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

    private void ResetQueue() {

        alchemyQueue.Clear();
        for(int i = 0; i < 3; i++)
        {
            alchemyQueue.Enqueue(WeaponType.DEFAULT);
            pips[i].SetInteger("Color",-1);
        }

    }


    //Converts the currently queued alchemy combo into an alchemyCode and grabs the associated weapon data from the list
    public void TryFireAlchemy() {
        int code = 0;
        int curr;

        //Icky
        while(alchemyQueue.Count > 0)
        {
            curr = (int)alchemyQueue.Dequeue();
            switch(curr)
            {
                case 0:
                    code++;
                    break;
                case 1:
                    code+=5;
                    break;
                case 2:
                    code+=21;
                    break;
                default:
                    break;
            }
            //Grab the data here and instantite its associated prefab
            UnityEngine.Debug.Log(alchemyWeapons[code]);
        }

        ResetQueue();
    }

    
    //Grabs all Weapon data objects from the WeaponData folder and adds them to the alchemy outcome list to be queried from later
    private void AggregateAlchemy() {

        UnityEngine.Object[] loadedWeapons = Resources.LoadAll("WeaponData", typeof(WeaponData));

        foreach(WeaponData weapon in loadedWeapons)
            if(weapon.alchemyCode >= 0)
            {
                alchemyWeapons[weapon.alchemyCode] = weapon; 
                UnityEngine.Debug.Log("Added " + weapon.alchemyCode);
            }
    }

    
}

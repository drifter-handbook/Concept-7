using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    //[SerializeField] private WeaponPrefabList weaponPrefabs;

    [SerializeField] private Animator[] pips;

    [SerializeField] private int MaxComboLength = 3;

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
            GameObject prefab = null;
            switch(type)
            {
                case WeaponType.PRIMARYRED:
                    prefab = StageDirector.FindWeapon(-1, 0, 0);
                    break;
                case WeaponType.PRIMARYYELLOW:
                    prefab = StageDirector.FindWeapon(0, -1, 0);
                    break;
                case WeaponType.PRIMARYBLUE:
                    prefab = StageDirector.FindWeapon(0, 0, -1);
                    break;
                default:
                    break;
            }       
            
            timeStamp = Time.time + prefab.GetComponent<PlayerWeapon>().weaponData.fireRate;
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }

    public void TryAddAlchemy(WeaponType type) {
        alchemyQueue.Enqueue(type);
        while (alchemyQueue.Count > MaxComboLength) //TODO have a group discussion on what we want to do when the list fills up
            alchemyQueue.Dequeue();

        WeaponType[] alchemyArray = alchemyQueue.ToArray();
        for(int i = 0; i < pips.Length; i++)
            pips[i].SetInteger("Color",(int)alchemyArray[i]);
    
    }

    public void TryFireAlchemy() {

        int r = 0;
        int y = 0;
        int b = 0;

        while (alchemyQueue.Count > 0) //TODO have a group discussion on what we want to do when the list fills up
        {
            WeaponType type = alchemyQueue.Dequeue();

            switch(type)
            {
                case WeaponType.PRIMARYRED:
                    r++;
                    break;
                case WeaponType.PRIMARYYELLOW:
                    y++;
                    break;
                case WeaponType.PRIMARYBLUE:
                    b++;
                    break;
                default:
                    break;
            }
            
        } 

        GameObject prefab = StageDirector.FindWeapon(r, y, b);

        if(prefab != null)
            Instantiate(prefab, transform.position, Quaternion.identity);

        ResetQueue();

    }

    private void ResetQueue() {

        alchemyQueue.Clear();
        for(int i = 0; i < pips.Length; i++)
        {
            alchemyQueue.Enqueue(WeaponType.DEFAULT);
            pips[i].SetInteger("Color",-1);
        }

    }
}

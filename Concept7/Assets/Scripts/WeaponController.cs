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
            StageData.Actor actor = null;
            switch(type)
            {
                case WeaponType.PRIMARYRED:
                    actor = StageDirector.FindWeapon(-1, 0, 0);
                    break;
                case WeaponType.PRIMARYYELLOW:
                    actor = StageDirector.FindWeapon(0, -1, 0);
                    break;
                case WeaponType.PRIMARYBLUE:
                    actor = StageDirector.FindWeapon(0, 0, -1);
                    break;
                default:
                    break;
            }       
            
            timeStamp = Time.time + actor.PrefabObj.GetComponent<PlayerWeapon>().weaponData.fireRate;
            StageDirector.Spawn(actor.Name, transform.position, 0f);
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

        StageData.Actor actor = StageDirector.FindWeapon(r, y, b);
        if(actor != null)
            StageDirector.Spawn(actor.Name, transform.position, 0f);
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

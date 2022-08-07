using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    //[SerializeField] private WeaponPrefabList weaponPrefabs;

    [SerializeField] private Animator[] pips;

    [SerializeField] private int MaxComboLength = 3;

    private GameScreen screen;

    public void Start()
    {
        ResetQueue();
    }

    public void Initialize(GameScreen screen){
        this.screen = screen;
    }

    private float timeStamp;
    private Queue<WeaponType> alchemyQueue = new Queue<WeaponType>();
    public void Fire(WeaponType type) {
        if (type == WeaponType.DEFAULT) {
            ResetQueue();
            return;
        }
        if (Time.time >= timeStamp) {
            string ryb = null;
            switch(type)
            {
                case WeaponType.PRIMARYRED:
                    ryb = StageDirector.RYBStr(-1, 0, 0);
                    break;
                case WeaponType.PRIMARYYELLOW:
                    ryb = StageDirector.RYBStr(0, -1, 0);
                    break;
                case WeaponType.PRIMARYBLUE:
                    ryb = StageDirector.RYBStr(0, 0, -1);
                    break;
                default:
                    break;
            }
            WeaponData weaponData = StageDirector.FindWeapon(ryb)?.PrefabObj?.GetComponent<PlayerWeapon>()?.weaponData;
            if (weaponData != null)
            {
                timeStamp = Time.time + weaponData.fireRate;
            }
            StageActor actor = StageDirector.FindCurrentActor("player").GetComponent<StageActor>();
            if (actor != null && actor.Actor.Timelines.ContainsKey(ryb))
            {
                actor.RunTimeline(ryb);
            }
            screen.Game.PlaySFX("player_pew", 0.1f, Random.Range(0.8f, 1));
        }
    }

    public void TryAddAlchemy(WeaponType type) {

        alchemyQueue.Enqueue(type);
        while (alchemyQueue.Count > MaxComboLength) {
            //TODO have a group discussion on what we want to do when the list fills up
            alchemyQueue.Dequeue();
        }
        screen.AddQueueElement(type);//update the UI
        WeaponType[] alchemyArray = alchemyQueue.ToArray();
        for(int i = 0; i < pips.Length && i < alchemyArray.Length; i++)
            pips[i].SetInteger("Color",(int)alchemyArray[i]);
        
        //update alchemy queue with new queued move's name

        int r = 0;
        int y = 0;
        int b = 0;

        foreach (WeaponType letter in alchemyQueue) 
        {
            switch(letter)
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

        screen.UpdateAlchemyText(r,y,b);
    }




    public WeaponData TryGetAlchemyCombo() {
        int r = 0;
        int y = 0;
        int b = 0;

        foreach (WeaponType letter in alchemyQueue) 
        //TODO have a group discussion on what we want to do when the list fills up
        {
            switch(letter)
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
             // TODO: This is where we would add the ammo system
        }
        StageActor actor = StageDirector.FindCurrentActor("player").GetComponent<StageActor>();
        string timeline = StageDirector.RYBStr(r, y, b);

        if (actor != null && !string.IsNullOrWhiteSpace(timeline))
        {
            if (actor.Actor.Timelines.ContainsKey(timeline))
            {
                UnityEngine.Debug.Log("Found is " + StageDirector.FindWeapon(timeline)?.PrefabObj?.GetComponent<PlayerWeapon>()?.weaponData.attackName);
                 return StageDirector.FindWeapon(timeline)?.PrefabObj?.GetComponent<PlayerWeapon>()?.weaponData;
            }
            else
            {
                Debug.Log($"Warning: No weapon with timeline {timeline} found on player.");
            }
        }
        return null;
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
             // TODO: This is where we would add the ammo system
             
        }

        StageActor actor = StageDirector.FindCurrentActor("player").GetComponent<StageActor>();
        string timeline = StageDirector.RYBStr(r, y, b);
        if (actor != null && !string.IsNullOrWhiteSpace(timeline))
        {
            if (actor.Actor.Timelines.ContainsKey(timeline))
            {
                actor.RunTimeline(timeline);
            }
            else
            {
                Debug.Log($"Warning: No weapon with timeline {timeline} found on player.");
            }
        }
        ResetQueue();

    }

    private void ResetQueue() {

        alchemyQueue.Clear();
        screen.ClearQueue();
        for(int i = 0; i < pips.Length; i++)
        {
            alchemyQueue.Enqueue(WeaponType.DEFAULT);
            pips[i].SetInteger("Color",-1);
        }
        screen.UpdateAlchemyText(0,0,0);
    }
}

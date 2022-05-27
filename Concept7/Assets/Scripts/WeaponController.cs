using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponPrefabList weaponPrefabs;
    private float timeStamp;
    private List<WeaponType> alchemyQueue = new List<WeaponType>();
    public void Fire(WeaponType type) {
        if (type == WeaponType.DEFAULT) {
            alchemyQueue.Clear();
            return;
        }
        if (Time.time >= timeStamp) {
            GameObject prefab = weaponPrefabs.Get(type);
            timeStamp = Time.time + prefab.GetComponent<PlayerWeapon>().weaponData.fireRate;
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }

    public void TryAddAlchemy(WeaponType type) {
        alchemyQueue.Insert(0, type);
        while (alchemyQueue.Count > 3) //TODO have a group discussion on what we want to do when the list fills up
            alchemyQueue.RemoveAt(alchemyQueue.Count - 1);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponPrefabList weaponPrefabs;
    private float timeStamp;
    public void Fire(WeaponType type) {
        if (Time.time >= timeStamp) {
            GameObject prefab = weaponPrefabs.Get(type);
            timeStamp = Time.time + prefab.GetComponent<PlayerWeapon>().weaponData.fireRate;
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }
}

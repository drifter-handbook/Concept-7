using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponPrefabList", menuName = "Concept7/WeaponPrefabList", order = 0)]
public class WeaponPrefabList : ScriptableObject {
    
    [SerializeField] private GameObject[] weaponPrefabs;

    public GameObject Get(WeaponType type) {
        return weaponPrefabs[(int)type];
    }
}

//Possibly make these values equal to their alchemy code value?
public enum WeaponType {
    DEFAULT = -1, PRIMARYRED, PRIMARYYELLOW, PRIMARYBLUE,
}

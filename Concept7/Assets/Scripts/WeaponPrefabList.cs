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

public enum WeaponType {
    DEFAULT = -1, PRIMARYRED, PRIMARYYELLOW, PRIMARYBLUE,
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboPanel : MonoBehaviour
{
    public GameObject comboDisplayPrefab;
    public Transform contentParent;

    public List<WeaponData> weaponInfo;

    void Start()
    {
        foreach(WeaponData data in weaponInfo){
            ComboDisplay combo = Instantiate(comboDisplayPrefab, contentParent).GetComponent<ComboDisplay>();
            combo.Initialize(data);
        }

    }

}

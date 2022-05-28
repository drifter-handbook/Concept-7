using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "WeaponData", menuName = "Concept7/WeaponData", order = 70)]

public class WeaponData : ScriptableObject
{
	//color values
	public int r = -1;
	public int y = -1;
	public int b = -1;

    public int damage = 1;
    public float fireRate = 0.5f;
    
}

public enum WeaponType {
    DEFAULT = -1, PRIMARYRED, PRIMARYYELLOW, PRIMARYBLUE,
}

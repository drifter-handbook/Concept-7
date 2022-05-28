using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "WeaponData", menuName = "Concept7/WeaponData", order = 70)]


public class WeaponData : ScriptableObject
{
	/*
		Alchemy code determines the combination of colors that creates a given effect
		A code of -1 is used for default weapons and makes them ignored by the aggregator
		A code is comprsed of the sum of the colors base values:
			Red = 1
			Yellow = 5
			Blue = 21

		Thus a combination of BBY would have a code of 31
	*/
	public int alchemyCode = -1;
    public int damage = 1;
    public float fireRate = 0.5f;
    //Put the prefab being fired here
    public GameObject prefab;
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLookup", menuName = "ScriptableObjects/SoundLookup", order = 2)]
public class SoundLookup : ScriptableObject
{
    public List<Sound> sounds = new List<Sound>();

    [System.Serializable]
    public struct Sound{
        [SerializeField] public string name;
        [SerializeField] public AudioClip clip;
    }
   
    public AudioClip GetSound(string name){
        foreach(Sound sound in sounds){
            if(sound.name == name) return sound.clip;
        }
        return null;
    }

}

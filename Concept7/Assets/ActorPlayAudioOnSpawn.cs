using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorPlayAudioOnSpawn : MonoBehaviour
{
    public string AudioName;
    public bool RandomizePitch;
    // Start is called before the first frame update
    void Start()
    {
        Game.Instance.PlaySFX(AudioName, 0.1f, RandomizePitch ? Random.Range(0.5f, 1) : 1);
    }
}

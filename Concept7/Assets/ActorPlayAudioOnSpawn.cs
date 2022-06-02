using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorPlayAudioOnSpawn : MonoBehaviour
{
    public string AudioName;
    // Start is called before the first frame update
    void Start()
    {
      Game game = FindObjectOfType<Game>();  
      game.PlaySFX(AudioName, 0.1f, Random.RandomRange(0.5f, 1));
    }

}

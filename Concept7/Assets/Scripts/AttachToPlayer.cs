using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachToPlayer : MonoBehaviour
{
    [SerializeField] private InputHandler input;
    [SerializeField] private GameObject playerPrefab;
    
    void Start()
    {
        GameObject player = StageDirector.Spawn("player", Vector2.zero, 0f);
        player.GetComponent<StageActor>().FinishSpawn(null);
        PlayerController controller = player.GetComponent<PlayerController>();
        controller.SetInput(input);
    }
}

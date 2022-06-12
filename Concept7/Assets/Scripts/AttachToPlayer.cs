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
        player.GetComponent<StageActor>().FinishSpawn();
        PlayerController controller = player.GetComponent<PlayerController>();
        controller.SetInput(input);
        if (Game.Instance.StartStage)
        {
            StageDirector.StartStage(0);
        }
    }
}

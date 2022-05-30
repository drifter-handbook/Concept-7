using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachToPlayer : MonoBehaviour
{
    [SerializeField] private InputHandler input;
    [SerializeField] private GameObject playerPrefab;
    void Start()
    {
        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        player.GetComponent<PlayerController>().SetInput(input);
        // StageDirector.StartStage(0);
    }
}

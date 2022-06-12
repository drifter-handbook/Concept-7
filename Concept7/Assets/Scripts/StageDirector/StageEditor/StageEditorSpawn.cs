using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEditorSpawn : StageEditorBehaviour
{
    public string ActorType;
    public Vector2 Dir;
    public bool PointAtPlayer;
    public bool SetSpeed;
    public float Speed;
    public Vector2 Mirror;
    public string Timeline;
    public float Lifetime;

    // Start is called before the first frame update
    void Start()
    {

    }

    public override void StageEditorStart()
    {
        if (string.IsNullOrWhiteSpace(ActorType))
        {
            Debug.Log("Warning: No ActorType specified on spawner.");
        }
        if (!StageDirector.Instance.Data.Actors.ContainsKey(ActorType))
        {
            Debug.Log($"Warning: ActorType {ActorType} does not exist.");
        }
        GameObject go = StageDirector.Spawn(ActorType, new Vector3(transform.position.x, transform.position.y), 0f);
        StageActor spawned = go.GetComponent<StageActor>();
        float mirrorX = Mirror.x < 0 ? -1 : 1;
        float mirrorY = Mirror.y < 0 ? -1 : 1;
        spawned.Mirror = new Vector2(mirrorX, mirrorY);
        StageActor playerActor = PlayerController.Instance?.GetComponent<StageActor>();
        if (playerActor != null && PointAtPlayer)
        {
            spawned.Direction = ((Vector2)(playerActor.transform.position - transform.position)).normalized;
        }
        else
        {
            spawned.Direction = Dir == Vector2.zero ? Vector2.left : Dir.normalized;
        }
        if (SetSpeed)
        {
            spawned.Speed = Speed;
        }
        spawned.FinishSpawn(string.IsNullOrWhiteSpace(Timeline) ? null : Timeline, Lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

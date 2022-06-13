using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEditor : StageEditorBehaviour
{
    public static StageEditor Instance;

    public bool DrawMovementPath;
    public GameObject DrawMovementPathPrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StageEditorReload()
    {
        StageEditorStop();
        StageDirector.Instance.Data = new StageData();
    }

    public void StageEditorStop()
    {
        // destroy all actors except the one player
        StageActor playerActor = PlayerController.Instance?.GetComponent<StageActor>();
        foreach (StageActor comp in FindObjectsOfType<StageActor>())
        {
            if (playerActor == null || comp != playerActor)
            {
                Destroy(comp.gameObject);
            }
        }
    }

    public override void StageEditorStart()
    {
        StageEditorStop();
        // call all restarts except self
        foreach (StageEditorBehaviour comp in FindObjectsOfType<StageEditorBehaviour>())
        {
            // do not call self
            if (comp as StageEditor == null)
            {
                comp.StageEditorStart();
            }
        }
    }
}

public abstract class StageEditorBehaviour : MonoBehaviour
{
    public abstract void StageEditorStart();
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDotter : MonoBehaviour
{
    public static DebugDotter Instance { get; private set; }

    public GameObject DotPrefab;

    List<GameObject> dots = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

    public static GameObject Dot(Vector2 position, Color c)
    {
        GameObject go = Instantiate(Instance.DotPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
        Instance.dots.Add(go);
        go.GetComponent<SpriteRenderer>().color = c;
        return go;
    }
    public static void Clear()
    {
        foreach (GameObject go in Instance.dots)
        {
            Destroy(go);
        }
        Instance.dots.Clear();
    }
}

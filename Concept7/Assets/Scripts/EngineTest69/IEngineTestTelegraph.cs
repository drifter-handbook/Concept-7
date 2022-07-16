using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEngineTestTelegraph
{
    void Initialize(Vector2 position, float rotation, Vector2 size);
    void Finish(float dur);
}

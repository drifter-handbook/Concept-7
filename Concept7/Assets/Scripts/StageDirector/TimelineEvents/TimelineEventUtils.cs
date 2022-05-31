using System;
using UnityEngine;

public class TimelineEventUtils
{
    // Dist cannot be null.
    public static Vector2 FindDestPosition(float? X, float? Y, float? Dir, float? Dist, string Rel, Vector2 pos, Vector2 dir)
    {
        string rel = Rel ?? "pos";
        // relative position, absolute rotation
        if (rel == "pos")
        {
            if (X != null || Y != null)
            {
                return new Vector2(pos.x + (X ?? 0), pos.y + (Y ?? 0));
            }
            else if (Dir != null)
            {
                return pos + (Vector2)(Quaternion.Euler(0, 0, Dir.Value) * Vector2.right * Dist.Value);
            }
            return pos;
        }
        // set absolute (world) position
        if (rel == "abs")
        {
            return new Vector2(X ?? pos.x, Y ?? pos.y);
        }
        // relative to dir when command began
        if (rel == "dir")
        {
            if (X != null || Y != null)
            {
                Vector2 diff = new Vector2(X ?? 0, Y ?? 0);
                return pos + (Vector2)(Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x)) * diff);
            }
            else if (Dir != null)
            {
                Vector2 diff = Quaternion.Euler(0, 0, Dir.Value) * Vector2.right * Dist.Value;
                return pos + (Vector2)(Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x)) * diff);
            }
            return pos;
        }
        throw new StageDataException($"Invalid rel value: {rel}, allowed values are ['abs', 'pos', 'dir']");
    }

    public static GameObject GetParent(string parenting, Vector2 pos, GameObject actor, string emitter=null)
    {
        // if set no parent
        if (parenting == null)
        {
            return null;
        }
        if (parenting == "new")
        {
            if (actor == null)
            {
                actor = UnityEngine.Object.Instantiate(StageDirector.Instance.Prefabs["SpawnParent"]);
                actor.transform.position = new Vector3(pos.x, pos.y, actor.transform.position.z);
            }
            return actor;
        }
        if (parenting == "actor")
        {
            return actor;
        }
        if (parenting == "emitter")
        {
            return actor.GetComponent<StageActor>().Emitters[emitter];
        }
        throw new StageDataException($"Invalid parent value: {parenting}, allowed values are [null, 'new', 'actor', 'emitter']");
    }
}

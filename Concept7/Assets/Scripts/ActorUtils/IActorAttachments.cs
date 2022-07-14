using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActorAttachment
{
    void Attach(StageActor target, StageActor source);
}

public interface IActorSpawnHandler
{
    void HandleSpawn(StageActor newActor);
}

public enum ActorDestroyReason
{
    Offscreen, Impact, Health, Event
}

public interface IActorDestroyHandler
{
    void HandleDestroy(ActorDestroyReason reason);
}

public interface IActorLifetimeHandler
{
    void HandleLifetime(float dur);
}

public interface IActorCollisionHandler
{
    void HandleCollision(GameObject other);
    int Order { get; }
}

# Concept 7 Stage Director
Everything is YAML

## TODO
Interval 0 repeats, so that you can use loops that set vars while running a shoot command with interval zero correctly.

## Overview
The Concept 7 Stage Director (aka "The Director") allows developers to write easy-to-use YAML files which create things like bullets, bullet patterns, enemies and bosses. These files are human-readable, and are loaded during runtime. This allows us to easily test bullet patterns and modify sprites without recompiling.

These YAML files are located in `Assets/StreamingAssets`.

## Basic Concepts
There are several basic components to the Concept 7 Stage Director: Actors, Emitters, Timelines, and Variables.

#### Actors
An `Actor` is a GameObject which is managed by the Director. Each Actor type has its own YAML file.

An Actor can be anything, such as a boss, an enemy, or a bullet. Actors have emitters attached to them, and these emitters can create other actors. For example, an enemy actor can create a bullet actor, which would cause the enemy to shoot a bullet.

Each stage has its own "starting entity" which manages the entire stage's timeline, as specified in `stages.yaml`.

#### Emitters
An `Emitter` is a point attached to an actor from which other actors are created. For example, a spaceship might have a gun. This would be an emitter, and bullets would be created from that point.

#### Timelines
A `Timeline` is a list of events, each with a timestamp at which they occur relative to the start of the event. So, if the timeline states to create a bullet at 1 second, then when the timeline is run, a bullet will spawn 1 second afterwards.

#### Variables
A `Variable` is a floating-point value with a string name. They can be set or added/subtracted using the `setvar` command in a timeline.

## Actor Field Reference

### core

| Field             | Type         | Description                                                                          |
|-------------------|--------------|--------------------------------------------------------------------------------------|
| name              | string       | Required. Unique name for this actor type.                                           |
| copy_from         | string       | Inherit fields from other actor.                                                     |
| prefab            | string       | Name of prefab in Resources/Prefabs to use for this actor. Defaults to DefaultActor. |
| invuln            | bool         | Whether or not the actor can be hit. Defaults to false.                              |
| hp                | int          | How much HP the actor should have. Defaults to 1.                                    |
| default_run       | string       | What timeline to run when the actor spawns.                                          |
| turn_on_move      | bool         | Whether the actor should rotate when moving in a direction. Defaults to false.       |
| tags              | list         | List of string tags. Empty list by default.                                          |
| speed             | float        | Default speed for movement events. 1 by default.                                     |
| depth             | float        | Render depth (Z position). 0 by default.                                             |
| destroy_offscreen | bool         | Destroy actor when it leaves the play area. Defaults to true.                        |
| destroy_on_impact | bool         | Destroy actor when it impacts another actor. Defaults to false.                      |
| lifetime          | float        | Destroy actor after this number of seconds. Negative values disable this behavior.   |
| on_destroy        | dict         | When destroyed for the specified reason, run the given timeline.                     |
| classification    | list[string] | Whether this actor is a ship or bullet. See `Classifications` section.               |
| attach_on_impact  | string       | An actor to attach (as a child object) to the target when it impacts another actor.  |

Within the `on_destroy` dict, the following fields can be supplied:
| Field     | Type   | Description                                                                                          |
|-----------|--------|------------------------------------------------------------------------------------------------------|
| offscreen | string | Run the timeline when destroyed due to going offscreen. Only events at time 0 are run.               |
| impact    | string | Run the timeline when destroyed due to impacting another actor. Only events at time 0 are run.       |
| event     | string | Run the timeline when destroyed due to lifetime or the destroy event. Only events at time 0 are run. |

The possible values in the `classification` field are:
| Value   | Description                                                                                                          |
|---------|----------------------------------------------------------------------------------------------------------------------|
| actor   | An actor that can't be destroyed by projectile-reflecting or projectile-intercepting objects, such as an enemy ship. |
| bullet  | A projectile that moves and cannot be shot down.                                                                     |
| missile | A projectile that moves and can be shot down.                                                                        |
| beam    | A projectile that cannot be shot down and damages a continuous area.                                                 |

Possible useful values for the `prefab` field include:
| Value                | Description                                                                                                           |
|----------------------|-----------------------------------------------------------------------------------------------------------------------|
| DefaultActor         | Empty actor with no extra Monobehaviours attached.                                                                    |
| ActorGroup           | Actor that destroys itself if it has no children attached.                                                            |
| Screenwide           | Actor that immediately impacts all enemy projectiles onscreen.                                                        |
| ActorAttachDestroy   | Used in `attach_on_impact` to destroy the impacted projectile.                                                        |
| ActorAttachReflector | Used in `attach_on_impact` to reflect the impacted projectile.                                                        |
| ActorAttachOnHit     | Used in `attach_on_impact`. Creates an attached actor that lasts for one frame on the target.                         |
| ActorAttachOnHitOnly | Used in `attach_on_impact`. Same as ActorAttachOnHit but calls `on_destroy.event` if the actor is hit but not killed. |

### emitter_NAME

| Field | Type   | Description                                                                            |
|-------|--------|----------------------------------------------------------------------------------------|
| x     | float  | X Position of the emitter, relative to the actor. Defaults to 0.                       |
| y     | float  | Y Position of the emitter, relative to the actor. Defaults to 0.                       |
| actor | string | Name of the actor type this emitter creates by default when running the 'shoot' event. |

### timeline_NAME

Each timeline is made up of a list of events. Each event has the following fields, and then exactly one additional field from the list of actions:

| Field    | Type   | Description                                                                      |
|----------|--------|----------------------------------------------------------------------------------|
| time     | float  | Required. Number of seconds after timeline begins to execute this event.         |
| repeat   | float  | Number of times to repeat this event. Defaults to 1.                             |
| interval | float  | Number of seconds to wait between repeating the action each time. Defaults to 0. |

The list of actions is as follows:

| Field    | Type   | Description                                                      |
|----------|--------|------------------------------------------------------------------|
| destroy  | bool   | Destroy the actor if true.                                       |
| move     | object | Move the actor relative to its current position.                 |
| move_at  | object | Move the actor towards the player or another actor type.         |
| orbit    | object | Orbit the actor's parent position, or change the current orbit.  |
| rotate   | object | Rotate the actor.                                                |
| run      | object | Run a timeline.                                                  |
| setspeed | object | Set the actor's speed.                                           |
| setvar   | object | Set or increment a variable.                                     |
| spawn    | object | Spawn another actor at a position.                               |
| shoot    | object | Shoot out an actor, giving it an initial direction and position. |
| link     | object | Set actor's parent to be another actor.                          |
| detach   | bool   | Set actor to be an independent entity, unattached to any parent. |
| reattach | bool   | Set actor to be a child of the parent it was detached from.      |

#### spawn

| Field      | Type   | Description                                                                                    |
|------------|--------|------------------------------------------------------------------------------------------------|
| actor      | string | Required. Spawn this actor type.                                                               |
| x          | float  | Spawn the actor at this X position in the world.                                               |
| y          | float  | Spawn the actor at this Y position in the world.                                               |
| dir        | float  | Spawn the actor at this direction from the current actor.                                      |
| dist       | float  | Spawn the actor at this distance from the current actor.                                       |
| rel        | string | Set what coords to use when spawning the actor. Defaults to `abs`. See the `Rel` section.      |
| run        | string | Run this timeline instead of its default timeline when spawned.                                |
| parent     | string | Set the parent of the spawned actor. See the `Parent` section.                                 |
| mirror_x   | float  | Flip all X coords of movement commands in the spawned actor.                                   |
| mirror_y   | float  | Flip all Y coords of movement commands in the spawned actor.                                   |
| lifetime   | float  | Override the lifetime of the spawned actor.                                                    |
| x_modifier | float  | Variable to add to the value of the `x` field.                                                 |
| y_modifier | float  | Variable to add to the value of the `y` field.                                                 |

##### rel

The following values are usable with the `rel` field:

| Value | Description                                                                     |
|-------|---------------------------------------------------------------------------------|
| abs   | Use absolute coordinates.                                                       |
| pos   | Relative to the current actor's position, but treats actor's direction as 0.    |
| dir   | Relative to the current actor's position, rotated by current actor's direction. |

##### parent

The following values are usable with the `parent` field:

| Value   | Description                                                             |
|---------|-------------------------------------------------------------------------|
| actor   | Use absolute world coordinates.                                         |
| emitter | Relative to the current position, but treats actor's direction as 0.    |
| new     | Relative to the current position, rotated by current actor's direction. |

#### shoot

| Field             | Type   | Description                                                                                                               |
|-------------------|--------|---------------------------------------------------------------------------------------------------------------------------|
| num               | int    | Number of actors to shoot out. Defaults to 1.                                                                             |
| angle             | float  | Angle spread to use when shooting out the actors, if shooting multiple.                                                   |
| emitters          | string | Emitter to use when shooting out the actors, WITHOUT the 'emitter_' prefix. Defaults to one of the emitters on the actor. |
| actor             | string | Actor to shoot out. Defaults to using the emitter's default actor, if it exists.                                          |
| run               | string | Timeline to run on the spawned actor.                                                                                     |
| speed             | float  | Shoot out the actor using the given speed. Defaults to using the spawned actor's default speed.                           |
| dir               | float  | Shoot at a fixed angle, rather than at the player. Omit this to shoot at the player.                                      |
| interval          | float  | Wait this number of seconds between each shot if the 'num' field is specified. Defaults to 0.                             |
| parent            | string | Set the parent of the spawned actor. See the `Parent` section.                                                            |
| mirror_x          | float  | Flip all X coords of movement commands in the spawned actor.                                                              |
| mirror_y          | float  | Flip all Y coords of movement commands in the spawned actor.                                                              |
| lifetime          | float  | Override the lifetime of the spawned actor.                                                                               |
| ring              | bool   | Offset the spread angle by one shot. Useful for shooting rings outward.                                                   |
| num_modifier      | string | Variable to add to the value of the `num` field.                                                                          |
| angle_modifier    | string | Variable to add to the value of the `angle` field.                                                                        |
| lifetime_modifier | string | Variable to add to the value of the `lifetime` field.                                                                     |
| dir_modifier      | string | Variable to add to the value of the `dir` field.                                                                          |
| speed_modifier    | string | Variable to add to the value of the `speed` field.                                                                        |

#### move

Either 'x' and 'y' can be specified, or 'dir' and 'dist' can be specified. If both are specified, 'x' and 'y' will take priority.
If only 'dir' is specified, The actor will move forever in a specified direction.
If only 'dist' is specified, the actor will move the distance, continuing in a straight line.

| Field   | Type       | Description                                                     |
|---------|------------|-----------------------------------------------------------------|
| x       | float      | Units left/right to move.                                       |
| y       | float      | Units up/down to move.                                          |
| dir     | float      | Direction to move in.                                           |
| dist    | float      | Total units to move.                                            |
| speed   | float      | Speed to move. Defaults to using the actor's speed.             |
| instant | bool       | If true, instantly moves to the destination. Defaults to false. |
| loop    | bool       | Repeat the movement indefinitely.                               |
| dest    | list[dict] | List of keyframes to move to.                                   |

The objects in the `dest` list can have the following fields:

| Field | Type  | Description                                                                  |
|-------|-------|------------------------------------------------------------------------------|
| x     | float | X position of the keyframe.                                                  |
| y     | float | Y position of the keyframe.                                                  |
| dir   | float | Direction of the position of the keyframe compared to the previous keyframe. |
| dust  | float | Distance of the position of the keyframe compared to the previous keyframe.  |
| rel   | float | What coords to use. Defaults to `pos`. See the `Rel` section above.          |
| pre   | dict  | Position of the cubic bezier control point before the keyframe.              |
| post  | dict  | Position of the cubic bezier control point after the keyframe.               |

The objects `pre` and `post` can have the following fields:

| Field | Type  | Description                                                              |
|-------|-------|--------------------------------------------------------------------------|
| x     | float | X position of the control point.                                         |
| y     | float | Y position of the control point.                                         |
| dir   | float | Direction of the position of the control point compared to the keyframe. |
| dust  | float | Distance of the position of the control point compared to the keyframe.  |
| rel   | float | What coords to use. Defaults to `pos`. See the `Rel` section above.      |

#### move_at

Moves toward the closest actor found to match the game_tag, actor, and classification fields.

| Field          | Type   | Description                                                                     |
|----------------|--------|---------------------------------------------------------------------------------|
| max_turn       | float  | Maximum amount the unit can turn towards the player. Useful for homing bullets. |
| speed          | float  | Speed to move. Defaults to using the actor's speed.                             |
| instant        | bool   | If true, instantly moves to the specified actor. Defaults to false.             |
| game_tag       | string | Unity game tag to filter for.                                                   |
| actor          | string | Actor type to filter for.                                                       |
| classification | string | Classification to filter for.                                                   |

#### orbit

| Field  | Type    | Description                                           |
|--------|---------|-------------------------------------------------------|
| speed  | float   | Angular speed to orbit at.                            |
| radius | float   | Radius at which to orbit the parent object at.        |
| tilt   | Vector3 | Euler angles to tilt the orbit circle by.             |
| dur    | float   | Duration over which to interpolate speed/radius/tilt. |

#### rotate

| Field | Type  | Description                                                  |
|-------|-------|--------------------------------------------------------------|
| set   | float | Angle to rotate to. Specify either `set` or `inc`, not both. |
| inc   | float | Angle to rotate by. Specify either `set` or `inc`, not both. |
| dur   | float | Duration over which to rotate.                               |

#### run

| Field    | Type   | Description                  |
|----------|--------|------------------------------|
| timeline | string | Name of the timeline to run. |

#### setspeed

| Field | Type  | Description                                   |
|-------|-------|-----------------------------------------------|
| speed | float | Speed to move at by the end of the event.     |
| dur   | float | Duration over which to accelerate/decelerate. |

#### setvar

| Field | Type  | Description                                                                  |
|-------|-------|------------------------------------------------------------------------------|
| var   | float | Name of variable to set or increment.                                        |
| set   | float | Value to set the variable to. Specify either `set` or `inc`, not both.       |
| inc   | float | Value to increment the variable by. Specify either `set` or `inc`, not both. |

#### link

A niche timeline event used for firing bullets which maintain their formation. The usual way to set an actor's parent is using the `parent` field on the `shoot` event.

| Field         | Type   | Description                                                                                    |
|---------------|--------|------------------------------------------------------------------------------------------------|
| actor         | string | Actor type to become a child of. Spawns one if it does not already exist.                      |
| from_actor    | string | Spawn parent actor in the average positions of the matching actor type.                        |
| from_timeline | string | Spawn parent actor in the average positions of the matching actor types running this timeline. |


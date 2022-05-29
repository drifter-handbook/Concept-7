# Concept 7 Stage Director
Everything is YAML

TODO: Linked shots
TODO: Autogenerate RBY combos
    output: GameObject FindWeapon(int r, int b, int y)

## Overview
The Concept 7 Stage Director (aka "The Director") allows developers to write easy-to-use YAML files which create things like bullets, bullet patterns, enemies and bosses. These files are human-readable, and are loaded during runtime. This allows us to easily test bullet patterns and modify sprites without recompiling.

These YAML files are located in `Assets/StreamingAssets`.

## Basic Concepts
There are several basic components to the Concept 7 Stage Director: Actors, Emitters, and Timelines.

#### Actors
An `Actor` is a GameObject which is managed by the Director. Each Actor type has its own YAML file.

An Actor can be anything, such as a boss, an enemy, or a bullet. Actors have emitters attached to them, and these emitters can create other actors. For example, an enemy actor can create a bullet actor, which would cause the enemy to shoot a bullet.

Each stage has its own "starting entity" which manages the entire stage's timeline, as specified in `stages.yaml`.

#### Emitters
An `Emitter` is a point attached to an actor from which other actors are created. For example, a spaceship might have a gun. This would be an emitter, and bullets would be created from that point.

#### Timelines
A `Timeline` is a list of events, each with a timestamp at which they occur relative to the start of the event. So, if the timeline states to create a bullet at 1 second, then when the timeline is run, a bullet will spawn 1 second afterwards.

## Actor Field Reference

### core

| Field        | Type   | Description                                                                          |
|--------------|--------|--------------------------------------------------------------------------------------|
| name         | string | Required. Unique name for this actor type.                                           |
| copy_from    | string | Inherit fields from other actor.                                                     |
| prefab       | string | Name of prefab in Resources/Prefabs to use for this actor. Defaults to DefaultActor. |
| invuln       | bool   | Whether or not the actor can be hit. Defaults to false.                              |
| hp           | int    | How much HP the actor should have. Defaults to 1.                                    |
| default_run  | string | What timeline to run when the actor spawns.                                          |
| turn_on_move | bool   | Whether the actor should rotate when moving in a direction. Defaults to false.       |
| tags         | list   | List of string tags. Empty list by default.                                          |
| speed        | float  | Default speed for movement events. 1 by default.                                     |
| depth        | float  | Render depth (Z position). 0 by default.                                             |

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

| Field          | Type   | Description                                                      |
|----------------|--------|------------------------------------------------------------------|
| spawn          | object | Spawn another actor at a position.                               |
| shoot          | object | Shoot out an actor, giving it an initial direction and position. |
| move           | object | Move the actor relative to its current position.                 |
| move_abs       | object | Move the actor to an absolute position in the world.             |
| move_at_player | object | Move the actor towards the player.                               |
| destroy        | bool   | Destroy the actor if true.                                       |

#### Spawn

| Field | Type   | Description                                                     |
|-------|--------|-----------------------------------------------------------------|
| actor | string | Required. Spawn this actor type.                                |
| x     | float  | Spawn the actor at this X position in the world.                |
| y     | float  | Spawn the actor at this Y position in the world                 |
| run   | string | Run this timeline instead of its default timeline when spawned. |

#### Shoot

| Field    | Type   | Description                                                                                     |
|----------|--------|-------------------------------------------------------------------------------------------------|
| num      | int    | Number of actors to shoot out. Defaults to 1.                                                   |
| angle    | float  | Angle spread to use when shooting out the actors, if shooting multiple.                         |
| emitter  | string | Emitter to use when shooting out the actors. Defaults to one of the emitters on the actor.      |
| actor    | string | Actor to shoot out. Defaults to using the emitter's default actor, if it exists.                |
| run      | string | Timeline to run on the spawned actor.                                                           |
| speed    | float  | Shoot out the actor using the given speed. Defaults to using the spawned actor's default speed. |
| dir      | float  | Shoot at a fixed angle, rather than at the player. Omit this to shoot at the player.            |
| interval | float  | Wait this number of seconds between each shot if the 'num' field is specified. Defaults to 0.   |

#### Move

Either 'x' and 'y' can be specified, or 'dir' and 'dist' can be specified. If both are specified, 'x' and 'y' will take priority.
If only 'dir' is specified, The actor will move forever in a specified direction.
If only 'dist' is specified, the actor will move the distance, continuing in a straight line.

| Field      | Type  | Description                                                     |
|------------|-------|-----------------------------------------------------------------|
| x          | float | Units left/right to move.                                       |
| y          | float | Units up/down to move.                                          |
| dir        | float | Direction to move in.                                           |
| dist       | float | Total units to move.                                            |
| speed      | float | Speed to move. Defaults to using the actor's speed.             |
| instant    | bool  | If true, instantly moves to the destination. Defaults to false. |
| smoothness | float | Uses a softer start and finish speed.                           |

#### Move_Abs

| Field      | Type  | Description                                                     |
|------------|-------|-----------------------------------------------------------------|
| x          | float | X World position to move to.                                    |
| y          | float | Y World position to move to.                                    |
| speed      | float | Speed to move. Defaults to using the actor's speed.             |
| instant    | bool  | If true, instantly moves to the destination. Defaults to false. |
| smoothness | float | Uses a softer start and finish speed.                           |

#### Move_At_Player

| Field      | Type  | Description                                                                     |
|------------|-------|---------------------------------------------------------------------------------|
| max_turn   | float | Maximum amount the unit can turn towards the player. Useful for homing bullets. |
| speed      | float | Speed to move. Defaults to using the actor's speed.                             |
| instant    | bool  | If true, instantly moves to the player. Defaults to false.                      |
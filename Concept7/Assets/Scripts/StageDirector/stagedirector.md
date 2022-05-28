# Concept 7 Stage Director
Everything is YAML

TODO: Smooth sigmoid movement
TODO: Linked shots
TODO: Load Prefabs from Resources
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


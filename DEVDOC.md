# ISIS - GODOT

## To-do

- [X] Craft Tracker HUD
- [ ] Collision Avoidance AI
- [ ] Turrets
- [ ] Missiles
- [ ] consider CraftMaster C# rewrite? 
- [ ] A Demo

## design-doc

### Features

- [ ] Newtonian Flight Model
- [ ] Cool Cockpits
- [ ] 2d World Map, 3d Combat Map (M&B Style)
- [ ] To Scale Solar System
  - [ ] Procedurally Generate Planet Models (Stylized)
- [ ] Craft Pack Fighting
  - [ ] Calendars! (Yoon Ha Lee's)
- [ ] Dockable Stations

### Backlog

- [ ] Craft
  - [x] Newtonian Flight Model Driver
  - [ ] Drone
    - [x] Model
    - [ ] Mind
  - [ ] Wing
    - [ ] Model
    - [ ] Mind
  - [ ] Droneship
    - [x] Model
    - [ ] Mind

- [ ] Armament
  - [ ] Beam
  - [ ] Turreted
  - [ ] Fixed
  - [x] MeshCannon
  - [ ] Particle Cannon
  - [ ] Missles

- [ ] Defense
  - [ ] Attire
  - [ ] Collision Damage Detection
    - [ ] Contact Reporting
    - [ ] Particle Collision Detection

- [ ] Basic Level

- [ ] Skybox

- [ ] HUD
  - [x] Simple Target Tracker
  - [x] Aim Leading Marker
  - [x] Velocity Direction Marker

- [ ] Cockpit
  - [ ] Engine Display
  - [ ] Arms Display
  - [ ] Attire Display

- [ ] AI
  - [x] Behavior Trees
  - [ ] Finite State Machines
  - [ ] MasterMind
  - [ ] GroupMind
  - [x] SubMind
  - [ ] Craft Mind
    - [ ] Steering Behaviors
    - [ ] Paths
    - [ ] Context Steering
    - [ ] Formations

- [ ] Targets

- [ ] 2D

### Attire System

Layout:

```diagram
AttireMaster
    ↓↓
AttireProfile
    ↓↓
Attire
```

### AI

#### Current Setup

```diagram

ActionSelection:
    GroupMind BT
       ↓↓
    SteeringRoutine BT (Optional)
       ↓↓
Steering:
    SteeringRoutines (using Steering Behaviors)
       ↓↓
Locomotion:
    CraftEngine
```

#### Steering Routines

##### Description

A `SteeringRoutine` is something that determines craft input every frame. Currently, a lightweight form of the Strategy pattern using closures is being used.

BehaviorTree based selection `SteeringRoutines` for each craft.

Each craft has an active `SteeringRoutine`.

Some `SteeringRoutine`s are simple they might not even need non-transient state. Others are dynamic, using state (currently stored in closures) or even their own behavior trees to modify their behavior.

These `SteeringRoutine`s are used everyframe and one runs for each AI controlled craft. Performance matters.

Having complex trees embedded inside the `SteeringRoutine`s doesn't sound something that'd scale.

Instead, we use decorator behavior nodes, like the `Checked` or `SimpleInclude` from `GreenBehaviors`, to hook into the ticking of whatever behavior selected the `SteeringRoutine` in the first place. While this creates a coupling, it provides certain advantages:

- Performance improvements since we probably won't be ticking the tree every frame (skipping frames or running on another thread).
- A cleaner way for the `SteeringRoutine` to communicate with the tree.
- Look at source of AttackPersue routine.

`SteeringRoutine` are more than steering behaviors. They're a step above them to be exact. From other BehaviorTree schemes, you might liken them to a Task. In simple terms, they're behaviors (in the behavior tree sense) that are used to control a `Craft`.

Right now, they're implemented using  closures that return Linear/Angular input and it works alright but I can imagine some issues along the way. Primarly:

- `SteeringRoutine` are meant to control more than the motion, like weapons and the like. Separating the Linear/Angular input from the rest doesn't offer much advantages if this is true. For example, if you want to poll a number of `SteeringRoutine` to determine movement for a frame (for combining or choosing), it might be found necessary to invalidate other decisions the `SteeringRoutine` has made inside. A choice lays ahead.

##### Routines

- [ ] AttackPersue
- [ ] ShootDown
- [ ] Intercept
- [ ] Evade
- [ ] Hide
- [ ] Line Up Shot

#### Steering Behaviors

- [x] Seek
- [x] Flee
- [ ] Avoid Obstacle
  - [ ] Mesh Colliders
  - [ ] Detection When Inside Collider
- [ ] Raycast Obstacle Avoidance
- [x] Intercept
- [x] Evade
- [ ] Arrival
- [ ] Collision Avoidance
- [ ] Separation
- [ ] Allignment
- [ ] Cohesion
- [ ] Interpose

#### BehaviorTrees

##### EliminateHostiles

```BehaviorTreeDiagram

         --MainTree       blackboard = [members, hostiles]
             |
      M(HostilesAround)
             |
          Sequence
          |        \
        Assign      Parallel Selector Resume
        Targets       \    |    /
        ToMembers     AttackPersue
                    (members, targets)
                           |  |
```

##### AttackPursue Behavior

```BehaviorTreeDiagram


                 |
            AttackPersue(quarry)       blackboard = [quarry]
                 |
           Parallel Select Resume
           /           |
          /            |
   IsTargetDead     Parallel Select Resume
                             |            \
                         m(IsFar)          \
                             |              \  
                          Intecept          Attack
                          (quarry)         (quarry)
                                              | |
```

##### Attack Behavior

```BehaviorTreeDiagram

      --Attack(quarry)       blackboard = Static[quarry]{targetDirecton}
          |
        Parallel Sequence Resume
          |                 |
        Calculate       ProritizedSelect
        {target         /        |       \
       Directon}       /         |        \
                   M(isAhead)  M(isAside)  +
                      |          |         |
                 Line Up       Loop    Brake With
                  Shot                  Flair💅
                  And
                 Fire
```

## dev-log

### BUG: PhysicsDirectBodyState's inv_inertia value is Vec3(0, 0, 0)

And it shouldn't be. This only happens for the CraftMaster node's _integrate_forces callback. It works for some simple test scenes.

Nevermind. It appears the caclulated inertia was so small it was getting past my float inequality test. I miss me machine epsilon bad.

Well, the test corvette's still getting 3 zeroes. There's something I'm misunderstanding.

I beleive I've finally found the issue. Godot's editor has a limit to how many decimal places it'll show and the inverse inertias seems to be so small that they show up as 0.0. The to_str methods don't suffer from this problem and that's how I discovered this.

### Godot's forward direction

Looking at the docs, the Vector3.FORWARD const is facing in the negative Z. The look_at methods on the transforms face towards -Z as well. Even though that's very official  The official documentation did not mention this at all! I've worked on the assumption that it's otherwise and the thought of -ve velocites for forward motion isn't very comfortable. Chances are, all the craft_configs will have to have negative values for z.

Happend upon this in the docs:

> Because the camera is rotated by -180 degrees, we have to flip the Z directional vector. Normally forward would be the positive Z axis, so using basis.z.normalized() would work, but we are using -basis.z.normalized() because our camera’s Z axis faces backwards in relation to the rest of the player.

### Viweports and flipped input

While working on the cockpit world, which requires having one viewport/world for the game world and one for the cockpit world, I found out that V-Flip option, required so that the image is appropriatley flippped also flips the a lot the mouse input. Makes absolute sense. But then, I had to also flip the facing offset on the CraftCamera, which is rather strange since that in in world space, not screen space. Something's not being understood...

Found out the altitiude raise direction is flipped as well...hmmm.

It turns out using V-Flip is only useful when rendering on a mesh suraface. I was alternating between rendering on a surface and rendering through a ViewportContainer, which I suppose isn't the same and thus the issue. Switching the setting on/off appropriatley fixes the problem; a very subtle problem as it got it's wires' crossed and flipped it horizontally which isn't as obvious.

### Rigidbody2D and the applied_force/torque properties

What would you assume was the purpose of such properties when you see them on the Rigidbody2D object with the doc vaguely describing them as "The body's total applied force."? Let's say you were also aware of a method called apply_centeral_force(Vector2) on the same object? I assumed they displayed the force that was applied during the **last** frame. You might assume, more closer to the mark, that the afforementioned method is just a helper function to adjust your force vector for centeral alignment and that the properties hold what force is **to be** applied the next frame. Chances are, if you assumed that, you've had not worked with the 3D counterpart of the Rigidbody, which doesn't have such properties (only the methods). The reason I'm writing down all this is that I spent the last few hours horribly confused as I had no idea that you had to clear the properties every frame or else they would retain the applied force from the last frame. Which is unlike anything I've worked before.

### BUG: acceleration limiter

The acceleration limit seems to be errenous on the larger craft but works observably right on the test fighter. Could that editor range limit on a Rigidbody's mass be actually a thing? I had worked assuming it's there for a reason (using multipliers and stuff) for a while but after retrieveing the mass in code noticing it wasn't clamped, I decided it was a helpful guide or a bug. Limiting mass doesn't make sense anyhow. But this enigmatic bug makes me wonder. But say, even massive crafts under the "mass limit" are exhibiting the bug; I might have typed this all down for no reason.

### Underscore prefixes on private gdscript functions

Only just noticed this style guide. Damn.

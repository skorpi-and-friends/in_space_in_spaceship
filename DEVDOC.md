# ISIS - GODOT

## To-do

- [x] PID Driver
- [ ] armament
  - [ ] missile
  - [ ] particle cannon
  - [x] mesh cannon
- [ ] attire
  - [ ] collision damage detection
    - [ ] contact reporting
- [ ] ai

## design-doc

### Features

- [ ] Crafts

## dev-log

### BUG: PhysicsDirectBodyState's inv_inertia value is Vec3(0, 0, 0)

And it shouldn't be. This only happens for the CraftMaster node's _integrate_forces callback. It works for some simple test scenes.

Nevermind. It appears the caclulated inertia was so small it was getting past my float inequality test. I miss me machine epsilon bad.

Well, the test corvette's still getting 3 zeroes. There's something I'm misunderstanding.

I beleive I've finally found the issue. Godot's editor has a limit to how many decimal places it'll show and the inverse inertias seems to be so small that they show up as 0.0. The to_str methods don't suffer from this problem and that's how I discovered this.

### Godot's forward direction

Looking at the docs, the Vector3.FORWARD const is facing in the negative Z. The look_at methods on the transforms face towards -Z as well. Even though that's very official  The official documentation did not mention this at all! I've worked on the assumption that it's otherwise and the thought of -ve velocites for forward motion isn't very comfortable. Chances are, all the craft_configs will have to have negative values for z.

Happend upon this in the docs:

> Because the camera is rotated by -180 degrees, we have to flip the Z directional
vector. Normally forward would be the positive Z axis, so using
basis.z.normalized() would work, but we are using -basis.z.normalized() because
our camera’s Z axis faces backwards in relation to the rest of the player.
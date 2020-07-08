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

And it shouldn't be. This only happens for the CraftMaster node's _integrate_forces
callback. It works for some simple test scenes.

Nevermind. It appears the caclulated inertia was so small it was getting past my
float inequality test. I miss me machine epsilon bad.

Well, the test corvette's still getting 3 zeroes. There's something I'm misunderstanding.

I beleive I've finally found the issue. Godot's editor has a limit to how many decimal
places it'll show and the inverse inertias seems to be so small that they show up as 0.0.
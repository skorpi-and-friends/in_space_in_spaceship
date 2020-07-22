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
 - [ ] cockpit
	- [ ] engine display
	- [ ] arms display
	- [ ] attire display
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

> Because the camera is rotated by -180 degrees, we have to flip the Z directional vector. Normally forward would be the positive Z axis, so using basis.z.normalized() would work, but we are using -basis.z.normalized() because our camera’s Z axis faces backwards in relation to the rest of the player.

### Viweports and flipped input

While working on the cockpit world, which requires having one viewport/world for the game world and one for the cockpit world, I found out that V-Flip option, required so that the image is appropriatley flippped also flips the a lot the mouse input. Makes absolute sense. But then, I had to also flip the facing offset on the CraftCamera, which is rather strange since that in in world space, not screen space. Something's not being understood...

Found out the altitiude raise direction is flipped as well...hmmm.

It turns out using V-Flip is only useful when rendering on a mesh suraface. I was alternating between rendering on a surface and rendering through a ViewportContainer, which I suppose isn't the same and thus the issue. Switching the setting on/off appropriatley fixes the problem; a very subtle problem as it got it's wires crossed and flipped it horizontally which isn't as obvious.

### Rigidbody2D and the applied_force/torque properties

What would you assume was the purpose of such properties when you see them on the Rigidbody2D object with the doc vaguely describing them as "The body's total applied force."? Let's say you were also aware of a method called apply_centeral_force(Vector2) on the same object? I assumed they displayed the force that was applied during the **last** frame. You might assume, more closer to the mark, that the afforementioned method is just a helper function to adjust your force vector for centeral alignment and that the properties hold what force is **to be** applied the next frame. Chances are, if you assumed that, you've had not worked with the 3D counterpart of the Rigidbody, which doesn't have such properties (only the methods). The reason I'm writing down all this is that I spent the last few hours horribly confused as I had no idea that you had to clear the properties every frame or else they would retain the applied force from the last frame. Which is unlike anything I've worked before.

### BUG: acceleration limiter

The acceleration limit seems to be errenous on the larger craft but works observably right on the test fighter. Could that editor range limit on a Rigidbody's mass be actually a thing? I had worked assuming it's there for a reason (using multipliers and stuff) for a while but after retrieveing the mass in code noticing it wasn't clamped, I decided it was a helpful guide or a bug. Limiting mass doesn't make sense anyhow. But this enigmatic bug makes me wonder. But say, even massive crafts under the "mass limit" are exhibiting the limit; I might have typed this all down for no reason.
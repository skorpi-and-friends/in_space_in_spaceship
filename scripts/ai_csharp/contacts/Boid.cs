using System;
using Godot;

namespace ISIS {
    public class Boid : ScanPresence {
        public override void _EnterTree() {
            base._EnterTree();
            AddToGroup("Boid");
        }

        // TODO: return a Rigidbody?
        public PhysicsBody GetBody() {
            return (PhysicsBody) GetPresenceOwner();
        }
    }
}
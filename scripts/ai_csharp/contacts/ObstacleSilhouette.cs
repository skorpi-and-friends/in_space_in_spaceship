using System;
using Godot;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS {
    public class ObstacleSilhouette : Area {
        public ObstaclePresence Presence { get; private set; }

        [Export] public Real Radius { get; private set; }
        private CollisionShape _collisionSphere;

        public override void _EnterTree() {
            base._EnterTree();
            _collisionSphere = GetNode<CollisionShape>("Sphere");
            System.Diagnostics.Debug.Assert(_collisionSphere != null);

            Radius = ((SphereShape) _collisionSphere.Shape).Radius;
        }
        public override void _Ready() {
            base._Ready();
            Presence = GetParent<ObstaclePresence>();
            System.Diagnostics.Debug.Assert(Presence != null);
        }
    }
}
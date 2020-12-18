using System.Collections.Generic;
using Godot;

using ObjectId = System.Int32;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.Minds {
    public class MasterMind : Node {
        [Signal] public delegate void ContactMade(ScanPresence contact);
        [Signal] public delegate void ContactLost(ScanPresence contact);
        public List<ScanPresence> MasterContactList { get; } = new List<ScanPresence>();

        // public Dictionary<ObjectId, ScanPresence> ObjectToPresenceMap { get; } = new Dictionary<ObjectId, ScanPresence>();

        // Called when the node enters the scene tree for the first time.
        public override void _EnterTree() {
            base._Ready();
            AddToGroup("MasterMind");
            var sceneTree = GetTree();
            foreach (var item in sceneTree.GetNodesInGroup("ScanPresence")) {
                RegisterPresence((ScanPresence) item);
            }
            sceneTree.Connect("node_added", this, nameof(NodeAddedToTree));
        }

        private void NodeAddedToTree(Node node) {
            if (node.IsInGroup("ScanPresence")) {
                RegisterPresence((ScanPresence) node);
            }
        }

        private void RegisterPresence(ScanPresence presence) {
            MasterContactList.Add(presence);
            presence.Connect("tree_exiting",
                this,
                nameof(PresenceExitingTree),
                new Godot.Collections.Array { presence });
            EmitSignal(nameof(ContactMade), presence);
        }
        // protected static ObjectId GenerateCraftId(Godot.Node @object) => @object.GetRid().GetId();

        private void PresenceExitingTree(ScanPresence presence) {
            MasterContactList.Remove(presence);
            EmitSignal(nameof(ContactLost), presence);
        }

        // TODO: a hash grid or someother spatial database
        public Boid ClosestBoidTo(ScanPresence presence) {
            var presencePosition = presence.GlobalTransform.origin;
            Boid closestBoid = null;
            float minDistanceYetSquared = Godot.Mathf.Inf;
            foreach (var contact in MasterContactList) {
                if (contact == presence || !(contact is Boid boid))
                    continue;
                var distanceSquared = presencePosition.DistanceSquaredTo(contact.GlobalTransform.origin);
                if (distanceSquared < minDistanceYetSquared) {
                    minDistanceYetSquared = distanceSquared;
                    closestBoid = boid;
                }
            }
            return closestBoid;
        }
    }
}
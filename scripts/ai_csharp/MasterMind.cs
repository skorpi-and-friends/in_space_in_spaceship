using System.Collections.Generic;
using Godot;

public class MasterMind : Node {

    [Signal] public delegate void ContactMade(ScanPresence contact);
    [Signal] public delegate void ContactLost(ScanPresence contact);
    public List<ScanPresence> MasterContactList { get; private set; } = new List<ScanPresence>();

    // Called when the node enters the scene tree for the first time.
    public override void _EnterTree() {
        base._Ready();
        AddToGroup("MasterMind");
        var sceneTree = GetTree();
        foreach (var item in sceneTree.GetNodesInGroup("ScanPresence")) {
            var presence = (ScanPresence) item;
            MasterContactList.Add(presence);
        }
        sceneTree.Connect("node_added", this, nameof(NodeAddedToTree));

    }

    private void NodeAddedToTree(Node node) {
        if (node.IsInGroup("ScanPresence")) {
            var scanPresence = (ScanPresence) node;
            MasterContactList.Add(scanPresence);
            scanPresence.Connect("tree_exiting",
                this,
                nameof(PresenceExitingTree),
                new Godot.Collections.Array { scanPresence });
            EmitSignal(nameof(ContactMade), scanPresence);
        }
    }

    private void PresenceExitingTree(ScanPresence presence) {
        MasterContactList.Remove(presence);
        EmitSignal(nameof(ContactLost), presence);
    }
}
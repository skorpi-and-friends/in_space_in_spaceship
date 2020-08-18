using System;
using Godot;

public class Boid : ScanPresence {

    [Export] private NodePath _bodyPath = new NodePath("..");

    public override void _EnterTree() {
        base._EnterTree();
        AddToGroup("Boid");
    }

    public PhysicsBody GetBody() {
        return (PhysicsBody) GetNode(_bodyPath);
    }
}
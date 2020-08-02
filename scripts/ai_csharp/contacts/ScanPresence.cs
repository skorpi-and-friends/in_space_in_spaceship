using System;
using Godot;

public class ScanPresence : Node {

    public override void _EnterTree() {
        base._EnterTree();
        AddToGroup("ScanPresence");
    }

}
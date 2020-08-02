using System;
using Godot;

public class ScanPresence : Spatial {

    public override void _EnterTree() {
        base._EnterTree();
        AddToGroup("ScanPresence");
    }

}